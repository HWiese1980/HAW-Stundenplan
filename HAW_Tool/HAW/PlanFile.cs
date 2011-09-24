using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Text.RegularExpressions;
using System.Net;
using System.Xml.Linq;
using System.IO.IsolatedStorage;
using System.Xml;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using HAW_Tool.Aspects;
using System.Collections.Specialized;
using System.ComponentModel;
using LittleHelpers;
using DDayEvent = DDay.iCal.Event;
// using DDay.iCal.DataTypes;
using DDay.iCal;
// using System.Net.Mail;
using System.Xml.Serialization;
using HAW_Tool.HAW.REST;
using System.Diagnostics;
using HAW_Tool.Comparers;
using System.Threading;
using HAW_Tool.Properties;
using System.Globalization;

namespace HAW_Tool.HAW
{
    public class PlanFile : INotifyPropertyChanged, IIsCurrent
    {
        #region Fields (16)

        static WebClient m_Client = new WebClient();
        IEnumerable<XElement> m_CurrentWeeks;
        int m_CurrentYear = 0;
        XDocument m_Doc = null;
        string m_FullContent = "";
        private iCalendar m_iCal;
        XElement m_Semgroups = new XElement("semgrps"), m_CurrentSemGroup;
        List<RESTEvent> m_StoredEvents;
        List<IEvent> mAllEvents;
        Dictionary<string, Dictionary<string, Change>> mChanges = new Dictionary<string, Dictionary<string, Change>>();
        List<SeminarGroup> mGroups;
        static PlanFile mInstance;
        static List<BaseEvent> mKnownBaseEvents;
        static List<IEvent> mKnownEvents;
        ObservableCollection<ChangeInfo> mLocalChanges = new ObservableCollection<ChangeInfo>();
        ObservableCollection<ChangeInfo> mPublishedChanges = new ObservableCollection<ChangeInfo>();

        #endregion Fields

        #region Constructors (1)

        private PlanFile()
        {
            Init_ObligatoryRegexPatternXDoc();
            Init_EventXDoc();
        }

        #endregion Constructors

        #region Properties (15)

        public IEnumerable<IEvent> AllEvents
        {
            get
            {
                if (mAllEvents == null)
                {
                    Parallel<SeminarGroup> tPar = new Parallel<SeminarGroup>();

                    mAllEvents = new List<IEvent>();
                    tPar.ForEach(SeminarGroups, (grp) =>
                    {
                        var qryX = from weeks in grp.CalendarWeeks
                                   from days in weeks.Days
                                   from IEvent evt in days.Events
                                   orderby evt.Code ascending
                                   select evt;

                        lock (mAllEvents)
                        {
                            mAllEvents.AddRange(qryX);
                        }

                        //Console.WriteLine(@"AllEvents initialization with {0} done", grp);
                    });

                    tPar.WaitForAll();
                }
                return mAllEvents.OrderBy(p => p.Code);
            }
        }

        public IEnumerable<ChangeInfo> Changes
        {
            get
            {
                return from evt in mChanges
                       select new ChangeInfo() { EventHash = evt.Key, Event = GetEventByCode(evt.Key), EventChanges = new List<Change>(evt.Value.Values) };
            }
        }

        public IEnumerable<IWeek> CoveredWeeks
        {
            get
            {
                var tWeeks = from p in SeminarGroups
                             from q in p.CalendarWeeks
                             select q;

                var tWeeksDistinct = tWeeks.Distinct(new WeekEqualityComparer());
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
                var ret = from evt in this.KnownBaseEvents
                          where evt.Groups.Count() > 1
                          select evt;
                return ret.ToArray();
            }
        }

        internal iCalendar iCalendar { get { return m_iCal; } }

        public static PlanFile Instance
        {
            get
            {
                return mInstance;
            }
        }

        public IEnumerable<BaseEvent> KnownBaseEvents
        {
            get
            {
                if (mKnownBaseEvents == null)
                {
                    OnStatusMessageChanged("Initialisiere Liste bekannter Basis Events");

                    UniqueList<BaseEvent> tEvts = new UniqueList<BaseEvent>();
                    Parallel<IEvent> tPar = new Parallel<IEvent>();

                    tPar.ForEach(this.KnownEvents, (evt) =>
                        {
                            lock (tEvts)
                            {
                                BaseEvent tBaseEvt = new BaseEvent(evt);
                                tEvts.Add(tBaseEvt, new BaseEventComparer());
                            }
                        });

                    tPar.WaitForAll();

                    mKnownBaseEvents = new List<BaseEvent>(tEvts);
                    OnStatusMessageChanged("Bekannte Basis Events: {0}", mKnownBaseEvents.Count);
                }
                return mKnownBaseEvents;
            }
        }

        public IEnumerable<IEvent> KnownEvents
        {
            get
            {
                if (mKnownEvents == null)
                {
                    OnStatusMessageChanged("Initialisiere Liste bekannter Events");

                    var tEvents = this.AllEvents;
                    tEvents = tEvents.Distinct<IEvent>(new EventComparer());
                    mKnownEvents = new List<IEvent>(tEvents);
                }
                return mKnownEvents;
            }
        }

        public ObservableCollection<ChangeInfo> LocalChanges
        {
            get { return mLocalChanges; }
            set { mLocalChanges = value; }
        }

        public IEnumerable<string> ObligatoryRegexPatterns { get; set; }

        public ObservableCollection<ChangeInfo> PublishedChanges
        {
            get { return mPublishedChanges; }
        }

        public IEnumerable<SeminarGroup> SeminarGroups
        {
            get
            {
                if (mGroups == null)
                {
                    mGroups = new List<SeminarGroup>(from p in m_Doc.Element("semgrps").Elements("semgrp")
                                                     select (SeminarGroup)p);
                }
                return mGroups;
            }
        }

        public IEnumerable<RESTEvent> StoredEvents
        {
            get { return m_StoredEvents; }
        }

        public RESTUserData Userdata { get; set; }

        #endregion Properties

        #region Delegates and Events (2)

        // Events (2) 

        public static event EventHandler<ValueEventArgs<string>> StatusMessageChanged;
        public static event EventHandler<ValueEventArgs<int>> StatusProgressChanged;

        #endregion Delegates and Events

        #region Methods (33)

        // Public Methods (10) 

        public void ExportAs(SeminarGroup Group, ExportType XType, string Filename)
        {
            switch (XType)
            {
                case ExportType.iCal:
                    {
                        this.Export_iCal(Group, Filename);
                        break;
                    }
            }
        }

        public Event GetEventByCode(string Hash)
        {
            var tOut = from p in AllEvents where p is Event && p.Hash == Hash select (Event)p;
            if (tOut.Count() <= 0) return default(Event);
            return tOut.First();
        }

        public IEnumerable<GroupID> GetEventGroups(IEvent Event)
        {
            var tGrp = (from events in PlanFile.Instance.KnownEvents
                        where events.BasicCode == Event.BasicCode && events.Group != GroupID.Empty
                        orderby events.Group ascending
                        select events.Group).ToList();
            for (int i = 0; i < tGrp.Count; i++)
            {
                if (tGrp[i].Value.Contains('+'))
                {
                    var new_grps = from p in tGrp[i].Value.Split('+') select new GroupID(p);
                    tGrp.RemoveAt(i);
                    tGrp.AddRange(new_grps);
                }
            }
            return tGrp.Distinct(new KeyEqualityComparer<GroupID>(p => p.Value));
        }

        public bool HasChangesByHash(string Hash)
        {
            return mChanges.ContainsKey(Hash);
        }

        public bool HasReplacements(IEvent evt, Day Day)
        {
            var bydayandcode = from p in StoredEvents
                               where (Day.Week == null || p.SeminarGroup == Day.Week.SeminarGroup.FullName)
                               where p.IsReplacementFor(evt)
                               select p;
            bool bret = (bydayandcode.Count() > 0);
            return bret;
        }

        public void ImportGroupSettings()
        {
            try
            {
                Properties.Settings tSettings = new HAW_Tool.Properties.Settings();
                if (tSettings.HAWSettingsXML == "") return;

                XDocument tSettingsDoc = XDocument.Parse(tSettings.HAWSettingsXML);

                XElement tHawSettings = tSettingsDoc.Element("hawtoolsettings");
                XElement tGrpSettings = tHawSettings.Element("groupsettings");
                XElement tChgSettings = tHawSettings.Element("evtchanges");

                foreach (XElement tElm in tGrpSettings.Elements("group"))
                {
                    string tBaseCode = tElm.Attribute("code").Value;
                    BaseEvent tEvt = GetBaseEventByCode(tBaseCode);
                    if (tEvt != null)
                        tEvt.Group = new GroupID(tElm.Attribute("groupno").Value);
                }

                foreach (XElement tElm in tChgSettings.Elements("changeinfo"))
                {
                    ChangeInfo tInf = ChangeInfo.FromXML(tElm);
                    mLocalChanges.Add(tInf);
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
            mInstance = new PlanFile();
        }

        public void LoadComplete()
        {
            ImportGroupSettings();
        }

        public static void LoadTXT(Uri URL)
        {
            string tDestPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string tFile = Path.Combine(tDestPath, URL.Segments.Last());

            if (LittleHelpers.Helper.HasInternetConnection)
            {
                try
                {
                    WebClient tCnt = new WebClient();
                    tCnt.DownloadFile(URL, tFile);
                }
                catch (Exception exp)
                {
                    MessageBox.Show(String.Format("Konnte Stundenplan {0} nicht herunterladen: {1}", URL.AbsoluteUri, exp.Message), "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            if (!File.Exists(tFile))
            {
                MessageBox.Show(String.Format("Lokale Stundenplan-Datei {0} existiert nicht.", tFile), "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            mInstance.LoadFile(tFile);
        }

        public void Login(string Username, string Password)
        {
            HAWClient tClnt = new HAWClient();
            Userdata = tClnt.Login(new RESTUserData() { Username = Username, Password = Password });
        }
        // Private Methods (19) 

        private static string ConfigFilePath(string CfgFile)
        {
            string cfgPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            cfgPath = Path.Combine(cfgPath, "ConfigFiles");
            cfgPath = Path.Combine(cfgPath, CfgFile);
            return cfgPath;
        }

        private DDay.iCal.Event ConvertToDDayEvt(IEvent tEvt)
        {
            return (DDayEvent)tEvt;
        }

        private XElement CreateEventElement(Match match)
        {
            XElement tCode = new XElement("code", match.Groups["Code"].Value.ConvertUmlauts(UmlautConvertDirection.ToCrossWordFormat));
            XElement tTutor = new XElement("dozent", match.Groups["Tutor"].Value);
            XElement tRoom = new XElement("raum", match.Groups["Room"].Value);
            XElement tDay = new XElement("tag", match.Groups["DayOfWeek"].Value);
            XElement tFrom = new XElement("von", match.Groups["Start"].Value);
            XElement tTill = new XElement("bis", match.Groups["End"].Value);

            XElement tEvent = new XElement("event",
                tCode, tTutor, tRoom, tDay, tFrom, tTill);

            return tEvent;
        }

        private void Export_iCal(SeminarGroup Group, string Filename)
        {
            m_iCal = new iCalendar();

            if (Group != null)
            {
                FetchDDayEventsFromSeminarGroup(Group);
            }
            else
            {
                foreach (SeminarGroup tgrp in SeminarGroups)
                {
                    OnStatusMessageChanged("Hole Events aus Gruppe {0}", tgrp.Label);
                    FetchDDayEventsFromSeminarGroup(tgrp);
                }
            }

            OnStatusMessageChanged("Alle Events in iCal Events umgewandelt... Export nach {0} startet", Filename);

            DDay.iCal.Serialization.iCalendar.iCalendarSerializer tSer = new DDay.iCal.Serialization.iCalendar.iCalendarSerializer(m_iCal);
            tSer.Serialize(Filename);

            OnStatusMessageChanged("Export nach {0} abgeschlossen.", Filename);
        }

        private void FetchDDayEventsFromSeminarGroup(SeminarGroup Group)
        {
            foreach (IEvent tEvt in Group.EnabledEvents)
            {
                var tBase = from evt in this.KnownBaseEvents
                            where evt.BasicCode == tEvt.BasicCode
                            && (tEvt.Group == GroupID.Empty || tEvt.Group == evt.Group)
                            select evt;

                if (!Window1.MainWindow.IsGroupFilterActive || tBase.Count() > 0)
                {
                    m_iCal.Events.Add(tEvt.AsDDayEvent());
                }
                else
                    OnStatusMessageChanged("Event {0} beim Export aufgrund von Gruppenfiltern ignoriert", tEvt.Code);
            }
        }

        private BaseEvent GetBaseEventByCode(string Code)
        {
            var x = from p in KnownBaseEvents where p.BasicCode == Code select p;
            if (x.Count() <= 0) return null;
            return x.Single();
        }

        private void Init_EventXDoc()
        {
            m_Doc = new XDocument(new XDeclaration("1.0", Encoding.UTF8.WebName, "yes")); ;

            m_Doc.Add(m_Semgroups);
        }

        private void Init_ObligatoryRegexPatternXDoc()
        {
            Settings tSet = new Settings();
            StringCollection tColl = tSet.ObligatoryRegexPatterns;
            var tCollStrings = from String p in tColl select p;

            this.ObligatoryRegexPatterns = tCollStrings.ToArray();
        }

        private void LoadFile(string Filename)
        {
            LoadStoredEvents();


            try
            {
                var buff = File.ReadAllBytes(Filename);
                m_FullContent = Encoding.Default.GetString(buff);
                this.ParseFileTXT();
            }
            catch (Exception e)
            {
                MessageBox.Show(String.Format("Ein Fehler ist bei der Verarbeitung der Datei {0} aufgetreten: {1}", Filename, e.Message));
                return;
            }
        }

        private void LoadStoredEvents()
        {
            try
            {
                HAWClient tCnt = new HAWClient();
                // var hawevts = tCnt.HAWEvents();
                var evts = tCnt.Events();

                // List<RESTEvent> hawEvents = new List<RESTEvent>(hawevts);

                //foreach(RESTEvent tEvt in evts)
                //{
                //    hawEvents.RemoveAll(p => p.Date == tEvt.Date && p.BasicCode == tEvt.BasicCode); 
                //}

                m_StoredEvents = new List<RESTEvent>(evts/*.Concat(hawEvents)*/);
            }
            catch (Exception e)
            {
                MessageBox.Show("Beim Laden zusätzlicher Daten vom Server des Entwicklers trat folgender Fehler auf:\n\n  {0}\n\nBitte teile dies dem Entwickler mit.\nKontakt-Infos gibt's auf der Homepage http://blog.seveq.de.\n\nDanke!", e.Message);
                m_StoredEvents = new List<RESTEvent>();
            }
        }

        private void OnStatusMessageChanged(string Format, params object[] Params)
        {
            String newStatus = String.Format(Format, Params);
            // Console.WriteLine(newStatus);

            if (StatusMessageChanged != null) StatusMessageChanged(this, new ValueEventArgs<string>() { Value = newStatus });
        }

        private void OnStatusProgressChanged(int percent)
        {
            if (StatusProgressChanged != null) StatusProgressChanged(this, new ValueEventArgs<int>() { Value = percent });
        }

        private void ParseFileTXT()
        {
            string t_Full_wo_returns = m_FullContent.Replace("\r", "");
            string[] lines = t_Full_wo_returns.Split('\n');

            Dictionary<Regex, string> t_Regexes = new Dictionary<Regex, string>();
            // [a-zA-Z]*?\s+(?<Semester>.*?)\s*Vers\.\s*(?<Version>\d+?\.\d+?)\s+?vom\s+?(?<Datum>.+)
            // t_Regexes.Add(new Regex(@"Stundenplan\s+?(?<Semester>.*?)\(Vers\.\s*?(?<Version>\d+?\.\d+?)\s+?vom(?<Datum>\s+?.+?)\)"), "version");
            t_Regexes.Add(new Regex(@"\w+?\s+(?<Semester>.*?)\s*Vers\.\s*(?<Version>\d+?\.\d+?)\s+?vom\s+?(?<Datum>.+)"), "version");
            t_Regexes.Add(new Regex(@"Semestergruppe\s+(?<Gruppe>.*?)$"), "gruppe");
            t_Regexes.Add(new Regex(@"^(?:\d+(?:-|,\s+|$))+$"), "kw");
            t_Regexes.Add(new Regex(@"^(?<Code>.+?),(?<Tutor>.*?),(?<Room>.+?),(?<DayOfWeek>\w+?),(?<Start>\d{1,2}\:\d{1,2}),(?<End>\d{1,2}\:\d{1,2})$"), "event");

            string[] tLines = (from p in lines where p != String.Empty select p).ToArray();

            int tLineCount = tLines.Length;

            for (int i = 0; i < tLineCount; i++)
            {
                string line = tLines[i];

                var tMatches = from p in t_Regexes.Keys
                               where p.IsMatch(line)
                               select p;

                foreach (Regex tRgx in tMatches)
                {
                    Match tMatch = tRgx.Match(line);
                    ParseLine(t_Regexes[tRgx], tMatch);
                }
            }
        }

        private IEnumerable<XElement> ParseKW(string KWLine)
        {
            List<XElement> tElm = new List<XElement>();

            var t_wBlocks = from string p in KWLine.Split(',')
                            select p.Trim();

            Regex tFromTo = new Regex(@"^(?<from>\d+)-(?<to>\d+)$");
            foreach (string tBlock in t_wBlocks)
            {
                if (tFromTo.IsMatch(tBlock))
                {
                    Match tMatch = tFromTo.Match(tBlock);
                    int tStart = int.Parse(tMatch.Groups["from"].Value);
                    int tEnd = int.Parse(tMatch.Groups["to"].Value);

                    for (int i = tStart; i <= tEnd; i++)
                    {
                        tElm.Add(new XElement("kw", new XAttribute("year", m_CurrentYear), new XAttribute("number", i)));
                    }
                }
                else
                {
                    tElm.Add(new XElement("kw", new XAttribute("year", m_CurrentYear), new XAttribute("number", tBlock)));
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

                        m_CurrentYear = date.Year;
                        m_CurrentSemGroup = new XElement("semgrp",
                            new XAttribute("version", match.Groups["Version"].Value),
                            new XAttribute("lastupdate", match.Groups["Datum"].Value));
                        break;
                    }
                case "gruppe":
                    {
                        Group grp = match.Groups["Gruppe"];
                        m_CurrentSemGroup.Add(new XAttribute("name", grp.Value));
                        m_Semgroups.Add(m_CurrentSemGroup);
                        break;
                    }
                case "kw":
                    {
                        m_CurrentWeeks = ParseKW(match.Value);
                        m_CurrentSemGroup.Add(m_CurrentWeeks);
                        break;
                    }
                case "event":
                    {
                        foreach (XElement tWeek in m_CurrentWeeks)
                        {
                            tWeek.Add(CreateEventElement(match));
                        }
                        break;
                    }
            }
        }

        public static string CleanDateString(string dateString)
        {
            Regex rgxClearDate = new Regex(@"\d{1,2}\.\d{1,2}\.\d{1,4}");
            dateString = rgxClearDate.Match(dateString).Value; 
            return dateString;
        }

        DateTime ParseDate(string dateString)
        {
            string[] dateFormats = new string[] {
                "dd.MM.yyyy",
                "dd.MM.yy"
            };

            foreach (string dateFormat in dateFormats)
            {
                DateTime temp;
                DateTimeStyles style = DateTimeStyles.AllowWhiteSpaces;
                if (DateTime.TryParseExact(dateString, dateFormat, CultureInfo.CurrentCulture, style, out temp)) return temp;
            }

            return DateTime.MinValue;
        }

        void RefreshChangesFrom(IEnumerable<ChangeInfo> Changes)
        {
            foreach (ChangeInfo tInf in Changes)
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

        void RefreshChangesFromLocal()
        {
            RefreshChangesFrom(mLocalChanges);
        }

        void RefreshChangesFromRemote()
        {
            RefreshChangesFrom(mPublishedChanges);
        }

        private static void SetPropertyBase64(object TheObject, string Property, string Base64String)
        {
            if (TheObject == null) return;
            PropertyInfo tProp = TheObject.GetType().GetProperty(Property);
            if (tProp == null) throw new ArgumentException(String.Format("Die Property {0} ist in dem Objekt des Typs {1} nicht enthalten", Property, TheObject.GetType()));

            MemoryStream tStrm = new MemoryStream(Convert.FromBase64String(Base64String));
            BinaryFormatter tBin = new BinaryFormatter();

            tStrm.Seek(0, SeekOrigin.Begin);

            object tVal = tBin.Deserialize(tStrm);
            tProp.SetValue(TheObject, tVal, null);
        }
        // Internal Methods (4) 

        internal void AddChange(Event Evt, string Property, object OldValue, object NewValue)
        {
            if (!mChanges.ContainsKey(Evt.Hash)) mChanges.Add(Evt.Hash, new Dictionary<string, Change>());
            Dictionary<string, Change> tChange = mChanges[Evt.Hash];

            if (tChange.ContainsKey(Property))
            {
                if (tChange[Property].OldValue.Equals(NewValue))
                    tChange.Remove(Property);
                else
                    tChange[Property].NewValue = NewValue;

                if (tChange.Count == 0)
                    mChanges.Remove(Evt.Hash);
            }
            else
            {
                Change tNew = new Change(Property, OldValue, NewValue);
                tChange.Add(Property, tNew);
            }

            OnPropetyChanged("Changes");
        }

        internal Change GetChange(string EventHash, string tPropName)
        {
            var tChanges = from inf in mPublishedChanges
                           where inf.EventHash == EventHash
                           from chg in inf.EventChanges
                           where chg.Property == tPropName
                           orderby chg.Timestamp ascending
                           select chg;

            return (tChanges.Count() > 0) ? tChanges.Last() : null;
        }

        internal void ResetChanges(string tHash)
        {
            if (!mChanges.ContainsKey(tHash)) return;

            PlanFile.Instance.IsNotifyingChanges = false;

            Change[] tList = mChanges[tHash].Values.ToArray();
            Event tEvt = GetEventByCode(tHash);
            int tCount = tList.Count();
            for (int i = 0; i < tCount; i++)
            {
                Change tChange = tList[i];
                PropertyInfo tProp = typeof(Event).GetProperty(tChange.Property);
                tProp.SetValue(tEvt, tChange.OldValue, null);
                tEvt.OnValueChanged(tChange.Property);
            }

            mChanges.Remove(tHash);

            OnPropetyChanged("Changes");

            tEvt.OnValueChanged("HasChanges");
            PlanFile.Instance.IsNotifyingChanges = true;
        }

        internal void SaveSettings()
        {
            Properties.Settings tSettings = new HAW_Tool.Properties.Settings();

            XDocument tDoc = new XDocument();
            XElement tHAWSettingsElm = new XElement("hawtoolsettings");
            tHAWSettingsElm.Add(new XElement("groupsettings", from elm in PlanFile.Instance.GroupedBaseEvents
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



        #region INotificationEnabled Members

        bool bNotifyingChanges = true;
        public bool IsNotifyingChanges
        {
            get { return bNotifyingChanges; }
            set { bNotifyingChanges = value; }
        }

        #endregion

        #region INotifyPropertyChanged Members

        private void OnPropetyChanged(string Prop)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(Prop));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region IIsCurrent Members

        public bool IsCurrent
        {
            get { return false; }
        }

        #endregion
    }

    public enum ExportType
    {
        iCal,
        Plain
    }
}
