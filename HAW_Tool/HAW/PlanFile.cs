using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml.Linq;
using DDay.iCal;
using DDay.iCal.Serialization.iCalendar;

using HAW_Tool.Comparers;
using HAW_Tool.HAW.REST;
using LittleHelpers;
using RedBranch.Hammock;
using DDayEvent = DDay.iCal.Event;
using Settings = HAW_Tool.Properties.Settings;
using HAW_Tool.HAW.CouchDB;

// using DDay.iCal.DataTypes;
// using System.Net.Mail;

namespace HAW_Tool.HAW
{
    public class PlanFile : INotifyPropertyChanged, IIsCurrent
    {
        #region Fields (16)

        private static WebClient _mClient = new WebClient();
        private static List<BaseEvent> _mKnownBaseEvents;
        private static List<IEvent> _mKnownEvents;
        private List<IEvent> _mAllEvents;

        private Dictionary<string, Dictionary<string, Change>> _mChanges =
            new Dictionary<string, Dictionary<string, Change>>();

        private List<SeminarGroup> _mGroups;
        private ObservableCollection<ChangeInfo> _mPublishedChanges = new ObservableCollection<ChangeInfo>();
        private XElement _mCurrentSemGroup;
        private IEnumerable<XElement> _mCurrentWeeks;
        private int _mCurrentYear;
        private XDocument _mDoc;
        private string _mFullContent = "";
        private XElement _mSemgroups = new XElement("semgrps");
        private List<RESTEvent> _mStoredEvents;

        #endregion Fields

        #region Constructors (1)

        private PlanFile()
        {
            LocalChanges = new ObservableCollection<ChangeInfo>();
            IsNotifyingChanges = true;
            Init_ObligatoryRegexPatternXDoc();
            Init_EventXDoc();
        }

        #endregion Constructors

        #region Properties (15)

        public IEnumerable<IEvent> AllEvents
        {
            get
            {
                if (_mAllEvents == null)
                {
                    var tPar = new Parallel<SeminarGroup>();

                    _mAllEvents = new List<IEvent>();
                    tPar.ForEach(SeminarGroups, grp =>
                                                    {
                                                        var qryX = from weeks in grp.CalendarWeeks
                                                                   from days in weeks.Days
                                                                   from IEvent evt in days.Events
                                                                   orderby evt.Code ascending
                                                                   select evt;

                                                        lock (_mAllEvents)
                                                        {
                                                            _mAllEvents.AddRange(qryX);
                                                        }

                                                        //Console.WriteLine(@"AllEvents initialization with {0} done", grp);
                                                    });

                    tPar.WaitForAll();
                }
                return _mAllEvents.OrderBy(p => p.Code);
            }
        }

        public IEnumerable<ChangeInfo> Changes
        {
            get
            {
                return from evt in _mChanges
                       select
                           new ChangeInfo
                               {
                                   EventHash = evt.Key,
                                   Event = GetEventByCode(evt.Key),
                                   EventChanges = new List<Change>(evt.Value.Values)
                               };
            }
        }

        public IEnumerable<IWeek> CoveredWeeks
        {
            get
            {
                IEnumerable<IWeek> tWeeks = from p in SeminarGroups
                                            from q in p.CalendarWeeks
                                            select q;

                IEnumerable<IWeek> tWeeksDistinct = tWeeks.Distinct(new WeekEqualityComparer());
                return tWeeksDistinct;
            }
        }

        public IEnumerable<IEvent> DisabledEvents
        {
            get
            {
                return from evt in AllEvents
                       where !evt.IsEnabled
                       select evt;
            }
        }

        public IEnumerable<BaseEvent> GroupedBaseEvents
        {
            get
            {
                IEnumerable<BaseEvent> ret = from evt in KnownBaseEvents
                                             where evt.Groups.Count() > 1
                                             select evt;
                return ret.ToArray();
            }
        }

        // ReSharper disable InconsistentNaming
        private iCalendar iCalendar { get; set; }
        // ReSharper restore InconsistentNaming

        public static PlanFile Instance { get; private set; }

        public IEnumerable<BaseEvent> KnownBaseEvents
        {
            get
            {
                if (_mKnownBaseEvents == null)
                {
                    OnStatusMessageChanged("Initialisiere Liste bekannter Basis Events");

                    var tEvts = new UniqueList<BaseEvent>();
                    var tPar = new Parallel<IEvent>();

                    tPar.ForEach(KnownEvents, evt =>
                                                  {
                                                      lock (tEvts)
                                                      {
                                                          var tBaseEvt = new BaseEvent(evt);
                                                          tEvts.Add(tBaseEvt, new BaseEventComparer());
                                                      }
                                                  });

                    tPar.WaitForAll();

                    _mKnownBaseEvents = new List<BaseEvent>(tEvts);
                    OnStatusMessageChanged("Bekannte Basis Events: {0}", _mKnownBaseEvents.Count);
                }
                return _mKnownBaseEvents;
            }
        }

        public IEnumerable<IEvent> KnownEvents
        {
            get
            {
                if (_mKnownEvents == null)
                {
                    OnStatusMessageChanged("Initialisiere Liste bekannter Events");

                    IEnumerable<IEvent> tEvents = AllEvents;
                    tEvents = tEvents.Distinct(new EventComparer());
                    _mKnownEvents = new List<IEvent>(tEvents);
                }
                return _mKnownEvents;
            }
        }

        public ObservableCollection<ChangeInfo> LocalChanges { get; set; }

        public IEnumerable<string> ObligatoryRegexPatterns { get; set; }

        public ObservableCollection<ChangeInfo> PublishedChanges
        {
            get { return _mPublishedChanges; }
        }

        public IEnumerable<SeminarGroup> SeminarGroups
        {
            get
            {
                var xSemGroupsElement = _mDoc.Element("semgrps");
                if (xSemGroupsElement != null) return _mGroups ?? (_mGroups = new List<SeminarGroup>(from p in xSemGroupsElement.Elements("semgrp")
                                                                                                     select (SeminarGroup)p));
                throw new Exception("semgrps");
            }
        }

        public IEnumerable<RESTEvent> StoredEvents
        {
            get { return _mStoredEvents; }
        }

        public RESTUserData Userdata { get; set; }

        #endregion Properties

        // Events (2) 

        public bool IsNotifyingChanges { get; set; }

        #region IIsCurrent Members

        public bool IsCurrent
        {
            get { return false; }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public static event EventHandler<ValueEventArgs<string>> StatusMessageChanged;
        public static event EventHandler<ValueEventArgs<int>> StatusProgressChanged;

        private void OnPropetyChanged(string prop)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        #region Methods (33)

        // Public Methods (10) 

        public void ExportAs(SeminarGroup @group, ExportType xType, string filename)
        {
            switch (xType)
            {
                case ExportType.iCal:
                    {
                        Export_iCal(@group, filename);
                        break;
                    }
            }
        }

        public Event GetEventByCode(string hash)
        {
            var tOut = (from p in AllEvents where p is Event && p.Hash == hash select (Event)p).ToList();
            return tOut.Count <= 0 ? default(Event) : tOut.First();
        }

        public IEnumerable<GroupID> GetEventGroups(IEvent Event)
        {
            if (Event == null) throw new ArgumentNullException("Event");
            var tGrp = (from events in Instance.KnownEvents
                        where events.BasicCode == Event.BasicCode && events.Group != GroupID.Empty
                        orderby events.Group ascending
                        select events.Group).ToList();

            for (var i = 0; i < tGrp.Count; i++)
            {
                if (tGrp[i].Value.Contains('+'))
                {
                    var newGrps = from p in tGrp[i].Value.Split('+') select new GroupID(p);
                    tGrp.RemoveAt(i);
                    tGrp.AddRange(newGrps);
                }
            }

            return tGrp.Distinct(new KeyEqualityComparer<GroupID>(p => p.Value));
        }

        public bool HasChangesByHash(string hash)
        {
            return _mChanges.ContainsKey(hash);
        }

        public bool HasReplacements(IEvent evt, Day day)
        {
            var bydayandcode = from p in StoredEvents
                               where
                                   (day.Week == null ||
                                    p.SeminarGroup == day.Week.SeminarGroup.FullName)
                               where p.IsReplacementFor(evt)
                               select p;
            bool bret = (bydayandcode.Count() > 0);
            return bret;
        }

        public void ImportGroupSettings()
        {
            try
            {
                var tSettings = new Settings();
                if (tSettings.HAWSettingsXML == "") return;

                var tSettingsDoc = XDocument.Parse(tSettings.HAWSettingsXML);

                var tHawSettings = tSettingsDoc.Element("hawtoolsettings");

                Debug.Assert(tHawSettings != null, "tHawSettings != null");
                var tGrpSettings = tHawSettings.Element("groupsettings");
                var tChgSettings = tHawSettings.Element("evtchanges");

                Debug.Assert(tGrpSettings != null, "tGrpSettings != null");
                foreach (var tElm in tGrpSettings.Elements("group"))
                {
                    Debug.Assert(tElm != null, "tElm != null");
                    var xCodeAttribute = tElm.Attribute("code");
                    if (xCodeAttribute == null) continue;

                    var tBaseCode = xCodeAttribute.Value;
                    var tEvt = GetBaseEventByCode(tBaseCode);
                    if (tEvt == null) continue;

                    var xGroupAttribute = tElm.Attribute("groupno");
                    if (xGroupAttribute != null)
                        tEvt.Group = new GroupID(xGroupAttribute.Value);
                }

                Debug.Assert(tChgSettings != null, "tChgSettings != null");
                foreach (var tInf in tChgSettings.Elements("changeinfo").Select(ChangeInfo.FromXML))
                {
                    LocalChanges.Add(tInf);
                }

                //foreach (ChangeInfo tInf in this.FetchChanges())
                //{
                //    mPublishedChanges.Add(tInf);
                //}

                OnStatusMessageChanged("Importiere lokal gespeicherte Änderungen...");
                RefreshChangesFromLocal();

                OnStatusMessageChanged("Importiere öffentlich gespeicherte Änderungen...");
                RefreshChangesFromRemote();

                OnStatusMessageChanged("Alle Einstellungen importiert.");
            }
            catch (Exception exp)
            {
                OnStatusMessageChanged("Beim Importieren der Einstellungen trat ein Fehler auf: {0}", exp.Message);
            }
        }

        public static void Initialize()
        {
            Instance = new PlanFile();
        }

        public void LoadComplete()
        {
            ImportGroupSettings();
        }

        /* Google API ist noch sehr unkomfortabel
        public static void LoadGoogle()
        {
            string googleAccountName = new Settings().GoogleUsername;
            string googleAccountPass = new Settings().GooglePassword;

            var provider = new NativeApplicationClient(GoogleAuthenticationServer.Description)
                               {
                                   ClientIdentifier = "346627111399.apps.googleusercontent.com",
                                   ClientSecret = "BICWvhYP5IWTuRWKcnJpzqzo"
                               };

            Google.Apis.Authentication.IAuthenticator auth = new OAuth2Authenticator<NativeApplicationClient>(provider, GetAuthorization);
            var service = new CalendarService(auth);

            var events = service.Events.List("j61u4pk4c8frc6sfs9omo1hr0s@group.calendar.google.com").Fetch();



        }

        private static IAuthorizationState GetAuthorization(NativeApplicationClient arg)
        {
            IAuthorizationState state = new AuthorizationState(new[] { CalendarService.Scopes.Calendar.GetStringValue() });
            state.Callback = new Uri(NativeApplicationClient.OutOfBandCallbackUrl);
            Uri authUri = arg.RequestUserAuthorization(state);
            Process.Start(authUri.ToString());
            Console.Write(@"Authorization Code: ");
            string authCode = "4/5PJ7ojy-oUImHIWA3F6VccFOUIiw";
            Console.WriteLine();

            return arg.ProcessUserAuthorization(authCode, state);
        }
         * 
         */

        public static void LoadTxt(Uri url)
        {
            string tDestPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string tFile = Path.Combine(tDestPath, url.Segments.Last());

            if (LittleHelpers.Helper.HasInternetConnection)
            {
                try
                {
                    var tCnt = new WebClient();
                    tCnt.DownloadFile(url, tFile);
                }
                catch (Exception exp)
                {
                    MessageBox.Show(
                        String.Format("Konnte Stundenplan {0} nicht herunterladen: {1}", url.AbsoluteUri, exp.Message),
                        "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            if (!File.Exists(tFile))
            {
                MessageBox.Show(String.Format("Lokale Stundenplan-Datei {0} existiert nicht.", tFile), "Fehler",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Instance.LoadFile(tFile);
        }

        public void Login(string username, string password)
        {
            var tClnt = new HAWClient();
            Userdata = tClnt.Login(new RESTUserData { Username = username, Password = password });
        }

        // Private Methods (19) 

        private static string ConfigFilePath(string cfgFile)
        {
            string cfgPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            Debug.Assert(cfgPath != null, "cfgPath != null");
            cfgPath = Path.Combine(cfgPath, "ConfigFiles");
            cfgPath = Path.Combine(cfgPath, cfgFile);
            return cfgPath;
        }

        private DDayEvent ConvertToDDayEvt(IEvent tEvt)
        {
            return (DDayEvent)tEvt;
        }

        private XElement CreateEventElement(Match match)
        {
            var tCode = new XElement("code",
                                     match.Groups["Code"].Value.ConvertUmlauts(UmlautConvertDirection.ToCrossWordFormat));
            var tTutor = new XElement("dozent", match.Groups["Tutor"].Value);
            var tRoom = new XElement("raum", match.Groups["Room"].Value);
            var tDay = new XElement("tag", match.Groups["DayOfWeek"].Value);
            var tFrom = new XElement("von", match.Groups["Start"].Value);
            var tTill = new XElement("bis", match.Groups["End"].Value);

            var tEvent = new XElement("event",
                                      tCode, tTutor, tRoom, tDay, tFrom, tTill);

            return tEvent;
        }

        private void Export_iCal(SeminarGroup @group, string filename)
        {
            iCalendar = new iCalendar();

            if (@group != null)
            {
                FetchDDayEventsFromSeminarGroup(@group);
            }
            else
            {
                foreach (SeminarGroup tgrp in SeminarGroups)
                {
                    OnStatusMessageChanged("Hole Events aus Gruppe {0}", tgrp.Label);
                    FetchDDayEventsFromSeminarGroup(tgrp);
                }
            }

            OnStatusMessageChanged("Alle Events in iCal Events umgewandelt... Export nach {0} startet", filename);

            var tSer = new iCalendarSerializer();
            tSer.Serialize(iCalendar, filename);

            OnStatusMessageChanged("Export nach {0} abgeschlossen.", filename);
        }

        private void FetchDDayEventsFromSeminarGroup(SeminarGroup @group)
        {
            foreach (IEvent tEvt in @group.EnabledEvents)
            {
                var tBase = from evt in KnownBaseEvents
                            where evt.BasicCode == tEvt.BasicCode
                                  && (tEvt.Group == GroupID.Empty || tEvt.Group == evt.Group)
                            select evt;

                if (!Window1.MainWindow.IsGroupFilterActive || tBase.Count() > 0)
                {
                    iCalendar.Events.Add(tEvt.AsDDayEvent());
                }
                else
                    OnStatusMessageChanged("Event {0} beim Export aufgrund von Gruppenfiltern ignoriert", tEvt.Code);
            }
        }

        private BaseEvent GetBaseEventByCode(string code)
        {
            var x = (from p in KnownBaseEvents where p.BasicCode == code select p).ToList();
            return x.Count != 1 ? null : x.Single();
        }

        private void Init_EventXDoc()
        {
            _mDoc = new XDocument(new XDeclaration("1.0", Encoding.UTF8.WebName, "yes"));
            _mDoc.Add(_mSemgroups);
        }

        private void Init_ObligatoryRegexPatternXDoc()
        {
            var tSet = new Settings();
            var tColl = tSet.ObligatoryRegexPatterns;
            var tCollStrings = from String p in tColl select p;

            ObligatoryRegexPatterns = tCollStrings.ToArray();
        }

        private void LoadFile(string filename)
        {
            LoadStoredEvents();


            try
            {
                byte[] buff = File.ReadAllBytes(filename);
                _mFullContent = Encoding.Default.GetString(buff);
                ParseFileTxt();
            }
            catch (Exception e)
            {
                MessageBox.Show(String.Format("Ein Fehler ist bei der Verarbeitung der Datei {0} aufgetreten: {1}",
                                              filename, e.Message));
                return;
            }
        }

        private void LoadStoredEvents()
        {
            try
            {
                var tCnt = new HAWClient();
                // var hawevts = tCnt.HAWEvents();
                RESTEvent[] evts = tCnt.Events();

                // List<RESTEvent> hawEvents = new List<RESTEvent>(hawevts);

                //foreach(RESTEvent tEvt in evts)
                //{
                //    hawEvents.RemoveAll(p => p.Date == tEvt.Date && p.BasicCode == tEvt.BasicCode); 
                //}

                _mStoredEvents = new List<RESTEvent>(evts /*.Concat(hawEvents)*/);
            }
            catch (Exception e)
            {
                MessageBox.Show(
                    "Beim Laden zusätzlicher Daten vom Server des Entwicklers trat folgender Fehler auf:\n\n  {0}\n\nBitte teile dies dem Entwickler mit.\nKontakt-Infos gibt's auf der Homepage http://blog.seveq.de.\n\nDanke!",
                    e.Message);
                _mStoredEvents = new List<RESTEvent>();
            }
        }

        private void OnStatusMessageChanged(string format, params object[] Params)
        {
            String newStatus = String.Format(format, Params);
            // Console.WriteLine(newStatus);

            if (StatusMessageChanged != null)
                StatusMessageChanged(this, new ValueEventArgs<string> { Value = newStatus });
        }

        private void OnStatusProgressChanged(int percent)
        {
            if (StatusProgressChanged != null) StatusProgressChanged(this, new ValueEventArgs<int> { Value = percent });
        }

        private void ParseFileTxt()
        {
            string tFullWoReturns = _mFullContent.Replace("\r", "");
            string[] lines = tFullWoReturns.Split('\n');

            var tRegexes = new Dictionary<Regex, string>
                               {
                                   {
                                       new Regex(
                                       @"\w+?\s+(?<Semester>.*?)\s*Vers\.\s*(?<Version>\d+?\.\d+?)\s+?vom\s+?(?<Datum>.+)")
                                       , "version"
                                       },
                                   {new Regex(@"Semestergruppe\s+(?<Gruppe>.*?)$"), "gruppe"},
                                   {new Regex(@"^(?:\d+(?:-|,\s+|$))+$"), "kw"},
                                   {
                                       new Regex(
                                       @"^(?<Code>.+?),(?<Tutor>.*?),(?<Room>.+?),(?<DayOfWeek>\w+?),(?<Start>\d{1,2}\:\d{1,2}),(?<End>\d{1,2}\:\d{1,2})$")
                                       , "event"
                                       }
                               };
            // [a-zA-Z]*?\s+(?<Semester>.*?)\s*Vers\.\s*(?<Version>\d+?\.\d+?)\s+?vom\s+?(?<Datum>.+)
            // t_Regexes.Add(new Regex(@"Stundenplan\s+?(?<Semester>.*?)\(Vers\.\s*?(?<Version>\d+?\.\d+?)\s+?vom(?<Datum>\s+?.+?)\)"), "version");

            var tLines = (from p in lines where p != String.Empty select p).ToArray();

            var tLineCount = tLines.Length;

            for (var i = 0; i < tLineCount; i++)
            {
                string line = tLines[i];

                var tMatches = from p in tRegexes.Keys
                               where p.IsMatch(line)
                               select p;

                foreach (var tRgx in tMatches)
                {
                    var tMatch = tRgx.Match(line);
                    ParseLine(tRegexes[tRgx], tMatch);
                }
            }
        }

        private IEnumerable<XElement> ParseKW(string kwLine)
        {
            var tElm = new List<XElement>();

            IEnumerable<string> tWBlocks = from string p in kwLine.Split(',')
                                           select p.Trim();

            var tFromTo = new Regex(@"^(?<from>\d+)-(?<to>\d+)$");
            foreach (string tBlock in tWBlocks)
            {
                if (tFromTo.IsMatch(tBlock))
                {
                    Match tMatch = tFromTo.Match(tBlock);
                    int tStart = int.Parse(tMatch.Groups["from"].Value);
                    int tEnd = int.Parse(tMatch.Groups["to"].Value);

                    for (int i = tStart; i <= tEnd; i++)
                    {
                        tElm.Add(new XElement("kw", new XAttribute("year", _mCurrentYear), new XAttribute("number", i)));
                    }
                }
                else
                {
                    tElm.Add(new XElement("kw", new XAttribute("year", _mCurrentYear), new XAttribute("number", tBlock)));
                }
            }
            return tElm.AsEnumerable();
        }

        private void ParseLine(string type, Match match)
        {
            switch (type)
            {
                case "version":
                    {
                        string dateString = CleanDateString(match.Groups["Datum"].Value.Trim());

                        DateTime date = ParseDate(dateString);

                        _mCurrentYear = date.Year;
                        _mCurrentSemGroup = new XElement("semgrp",
                                                         new XAttribute("version", match.Groups["Version"].Value),
                                                         new XAttribute("lastupdate", match.Groups["Datum"].Value));
                        break;
                    }
                case "gruppe":
                    {
                        Group grp = match.Groups["Gruppe"];
                        _mCurrentSemGroup.Add(new XAttribute("name", grp.Value));
                        _mSemgroups.Add(_mCurrentSemGroup);
                        break;
                    }
                case "kw":
                    {
                        _mCurrentWeeks = ParseKW(match.Value);
                        _mCurrentSemGroup.Add(_mCurrentWeeks);
                        break;
                    }
                case "event":
                    {
                        foreach (XElement tWeek in _mCurrentWeeks)
                        {
                            tWeek.Add(CreateEventElement(match));
                        }
                        break;
                    }
            }
        }

        public static string CleanDateString(string dateString)
        {
            var rgxClearDate = new Regex(@"\d{1,2}\.\d{1,2}\.\d{1,4}");
            dateString = rgxClearDate.Match(dateString).Value;
            return dateString;
        }

        private static DateTime ParseDate(string dateString)
        {
            var dateFormats = new[]
                                  {
                                      "dd.MM.yyyy",
                                      "dd.MM.yy"
                                  };

            foreach (string dateFormat in dateFormats)
            {
                DateTime temp;
                const DateTimeStyles style = DateTimeStyles.AllowWhiteSpaces;
                if (DateTime.TryParseExact(dateString, dateFormat, CultureInfo.CurrentCulture, style, out temp))
                    return temp;
            }

            return DateTime.MinValue;
        }

        private void RefreshChangesFrom(IEnumerable<ChangeInfo> changes)
        {
            foreach (ChangeInfo tInf in changes)
            {
                Event tEvt = tInf.Event;
                foreach (Change tChange in tInf.EventChanges)
                {
                    PropertyInfo tProp = typeof(Event).GetProperty(tChange.Property);
                    if (tProp == null) continue;

                    tProp.SetValue(tEvt, tChange.NewValue, null);
                }
            }
        }

        private void RefreshChangesFromLocal()
        {
            RefreshChangesFrom(LocalChanges);
        }

        private void RefreshChangesFromRemote()
        {
            RefreshChangesFrom(_mPublishedChanges);
        }

        private static void SetPropertyBase64(object theObject, string property, string base64String)
        {
            if (theObject == null) return;
            var tProp = theObject.GetType().GetProperty(property);
            if (tProp == null)
                throw new ArgumentException(
                    String.Format("Die Property {0} ist in dem Objekt des Typs {1} nicht enthalten", property,
                                  theObject.GetType()));

            var tStrm = new MemoryStream(Convert.FromBase64String(base64String));
            var tBin = new BinaryFormatter();

            tStrm.Seek(0, SeekOrigin.Begin);

            var tVal = tBin.Deserialize(tStrm);
            tProp.SetValue(theObject, tVal, null);
        }

        // Internal Methods (4) 

        internal void AddChange(Event evt, string property, object oldValue, object newValue)
        {
            if (!_mChanges.ContainsKey(evt.Hash)) _mChanges.Add(evt.Hash, new Dictionary<string, Change>());
            var tChange = _mChanges[evt.Hash];

            if (tChange.ContainsKey(property))
            {
                if (tChange[property].OldValue.Equals(newValue))
                    tChange.Remove(property);
                else
                    tChange[property].NewValue = newValue;

                if (tChange.Count == 0)
                    _mChanges.Remove(evt.Hash);
            }
            else
            {
                var tNew = new Change(property, oldValue, newValue);
                tChange.Add(property, tNew);
            }

            OnPropetyChanged("Changes");
        }

        internal void AddCouchDBChange(Event p, string property, object oldValue, object newValue)
        {
            var c = new Connection(new Uri("http://seveq.iriscouch.com"));
            var s = c.CreateSession("changes");
            var change = new CouchDocChange();
            var ch = new Change(property, oldValue, newValue);
            change.Change = ch;
            change.EventHash = p.Hash;
            s.Save(change);

            AddChange(p, property, oldValue, newValue);
        }

        public void LoadCouchDBChanges()
        {
            var c = new Connection(new Uri("http://seveq.iriscouch.com"));
            var s = c.CreateSession("changes");
            foreach(var doc in s.ListDocuments())
            {
                if (doc.Id.StartsWith("_")) continue;
                var cDoc = s.Load<CouchDocChange>(doc.Id);
                var evt = AllEvents.Where(p => p.Hash == cDoc.EventHash).FirstOrDefault();
                if (evt == null) continue;
                AddChange((Event) evt, cDoc.Change.Property, cDoc.Change.OldValue, cDoc.Change.NewValue);
            }

        }

        internal Change GetChange(string eventHash, string tPropName)
        {
            var tChanges = (from inf in _mPublishedChanges
                            where inf.EventHash == eventHash
                            from chg in inf.EventChanges
                            where chg.Property == tPropName
                            orderby chg.Timestamp ascending
                            select chg).ToList();

            return (tChanges.Count > 0) ? tChanges.Last() : null;
        }

        internal void ResetChanges(string tHash)
        {
            if (!_mChanges.ContainsKey(tHash)) return;

            Instance.IsNotifyingChanges = false;

            Change[] tList = _mChanges[tHash].Values.ToArray();
            Event tEvt = GetEventByCode(tHash);
            int tCount = tList.Count();
            for (int i = 0; i < tCount; i++)
            {
                var tChange = tList[i];
                var tProp = typeof(Event).GetProperty(tChange.Property);
                tProp.SetValue(tEvt, tChange.OldValue, null);
                tEvt.OnValueChanged(tChange.Property);
            }

            _mChanges.Remove(tHash);

            OnPropetyChanged("Changes");

            tEvt.OnValueChanged("HasChanges");
            Instance.IsNotifyingChanges = true;
        }

        internal void SaveSettings()
        {
            var tSettings = new Settings();

            var tDoc = new XDocument();
            var tHAWSettingsElm = new XElement("hawtoolsettings");
            tHAWSettingsElm.Add(new XElement("groupsettings", from elm in Instance.GroupedBaseEvents
                                                              select new XElement("group",
                                                                                  new XAttribute("code", elm.BasicCode),
                                                                                  new XAttribute("groupno", elm.Group))));

            //tHAWSettingsElm.Add(new XElement("evtchanges", from key in PlanFile.Instance.mChanges.Keys
            //                                               select new XElement("event", new XAttribute("hash", key),
            //                                                   from change in PlanFile.Instance.mChanges[key].Values
            //                                                   select change.ChangeXML)));

            tHAWSettingsElm.Add(new XElement("evtchanges", from chgInf in Changes
                                                           select ChangeInfo.AsXML(chgInf)));


            tDoc.Add(tHAWSettingsElm);

            tSettings.HAWSettingsXML = tDoc.ToString();
            tSettings.Save();
        }

        #endregion Methods

        /*
        internal void LoadChangesFromCouchDB()
        {
            var cClient = new CouchClient();
            var cd_changes = cClient.GetChanges();
            foreach(var cd_change in cd_changes)
            {
                var evtHash = cd_change.EventHash;
                if(_mChanges.ContainsKey(evtHash))
                {
                    var changes = _mChanges[evtHash];
                    AddChange();
                }
            }
        }
        */
    }

    public enum ExportType
    {
// ReSharper disable InconsistentNaming
        iCal,
// ReSharper restore InconsistentNaming
        Plain
    }
}