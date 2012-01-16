using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Timers;
using LittleHelpers;
using System.Text.RegularExpressions;
using System.IO;
using System.Windows;
using RedBranch.Hammock;

namespace HAW_Tool.HAW.Depending
{
    public class PlanFile : DependencyObject
    {
        private static object _couchDbLock = new object();

        private PlanFile()
        {
            CouchConnection = new Connection(new Uri("http://seveq.de:5984"));
            SeminarGroups = new ObservableCollection<SeminarGroup>();
            ReplacedSchoolEvents = new ObservableCollection<Event>();
            CouchDBEvents = new ObservableCollection<CouchDBEventInfo>();

            var refreshCouchDBTimer = new Timer {Interval = (10000)};
            // alle 10 Sekunden CouchDB refreshen
            refreshCouchDBTimer.Elapsed += (x, y) => Dispatcher.Invoke(new Action(LoadCouchEvents));
            refreshCouchDBTimer.Enabled = true;
        }

        private void ResetCouchDBEvents()
        {
            CouchDBEvents.Clear();
            foreach (var evt in GetAllModifiedEvents())
            {
                if(!evt.IsDirty) evt.Reset(); // nur auf Ursprungsdaten zurücksetzen, wenn keine vom Benutzer vorgenommenen Änderungen vorliegen
            }
        }

        private IEnumerable<Event> GetAllModifiedEvents()
        {
            return from s in SeminarGroups
                   from c in s.CalendarWeeks
                   from d in c.Days
                   from e in d.Events
                   where e.Source == EventSource.School
                   where e.IsDirty
                   select e;
        }

        static PlanFile()
        {
            Instance = new PlanFile();
            LineFilters = new List<StringFilter>
                               {
                                   new StringFilter("version", @"\w+?\s+(?<Semester>.*?)\s*Vers\.\s*(?<Version>\d+?\.\d+?)\s+?vom\s+?(?<Datum>.+)"),
                                   new StringFilter("gruppe", @"Semestergruppe\s+(?<Gruppe>.*?)$"),
                                   new StringFilter("kw", @"^(?:\d+(?:-|,\s+|$))+$"),
                                   new StringFilter("veranstaltung", @"^(?<Code>.+?),(?<Tutor>.*?),(?<Room>.+?),(?<DayOfWeek>\w+?),(?<Start>\d{1,2}\:\d{1,2}),(?<End>\d{1,2}\:\d{1,2})$")
                               };

        }

        public SeminarGroup SelectedSeminarGroup
        {
            get { return (SeminarGroup)GetValue(SelectedSeminarGroupProperty); }
            set { SetValue(SelectedSeminarGroupProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedSeminarGroup.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedSeminarGroupProperty =
            DependencyProperty.Register("SelectedSeminarGroup", typeof(SeminarGroup), typeof(PlanFile), new UIPropertyMetadata(null, SeminarGroupChanged));

        static void SeminarGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Console.WriteLine("Seminar Group selected: {0}", ((SeminarGroup)e.NewValue).Name);
        }


        private static readonly List<StringFilter> LineFilters;

        public static PlanFile Instance { get; set; }

        public static event EventHandler<ValueEventArgs<int>> StatusProgressChanged;

        public static void OnStatusProgressChanged(int value)
        {
            var args = new ValueEventArgs<int> { Value = value };
            if (StatusProgressChanged != null) StatusProgressChanged(null, args);
        }

        public static event EventHandler<ValueEventArgs<string>> StatusMessageChanged;

        private static void OnStatusMessageChanged(string format, params object[] parameter)
        {
            var args = new ValueEventArgs<string> { Value = string.Format(format, parameter) };
            if (StatusMessageChanged != null) StatusMessageChanged(null, args);
        }

        internal void LoadSchedule(string schedule)
        {
            OnStatusMessageChanged("Lade {0}", schedule);
            if (!Uri.IsWellFormedUriString(schedule, UriKind.Absolute))
                return;

            try
            {
                string content = new WebClient().DownloadString(schedule);
                ParseContent(content);
            }
            catch (Exception exp)
            {
                OnStatusMessageChanged("Laden von {0} fehlgeschlagen: {1}", schedule.Substring(schedule.Length - 20),
                                       exp.Message);
                Debugger.Break();
            }
        }

        public Connection CouchConnection { get; private set; }

        internal void LoadCouchEvents()
        {
            if (LittleHelpers.Helper.IsInDesignModeStatic || SeminarGroups == null || SeminarGroups.Count <= 0) return;

            if (SelectedSeminarGroup == null) SelectedSeminarGroup = SeminarGroups.FirstOrDefault();

            lock (_couchDbLock)
            {
                var couchS = CouchConnection.CreateSession("haw_events");
                var cdbDocs = couchS.ListDocuments();

                ResetCouchDBEvents();

                foreach (var cdbDoc in cdbDocs)
                {
                    if (!cdbDoc.Id.StartsWith("couchdbeventinfo")) continue;

                    var evtdoc = couchS.Load<CouchDBEventInfo>(cdbDoc.Id);
                    evtdoc.Event.Source = EventSource.CouchDB;

                    CouchDBEvents.Add(evtdoc);
                }

                CleanUpCouchDBEvents();
                ReplaceSchoolEvents();
            }
        }

        private void ReplaceSchoolEvents()
        {
            foreach (var evtinfo in CouchDBEvents)
            {
                var originalEvent = GetEventByHashInfo(evtinfo.EventInfoHash);
                if (originalEvent.IsDirty) continue;

                var itemDay = GetDayByDate(evtinfo.Event.Date.Date, evtinfo.Event.SeminarGroup.Name);
                
                itemDay.RemoveAllCouchDBEvents(evtinfo.EventInfoHash);
                itemDay.Events.Add(evtinfo.Event);
                evtinfo.Event.CleanUp();

                originalEvent.Visibility = Visibility.Hidden;
            }
        }

        void CleanUpCouchDBEvents()
        {
// ReSharper disable AccessToModifiedClosure
            var dirtyHashes = new List<string>();
            foreach (var item in CouchDBEvents)
            {
                if (!dirtyHashes.Contains(item.EventInfoHash)) dirtyHashes.Add(item.EventInfoHash);
            }

            foreach (var infohash in dirtyHashes)
            {
                var eventsByStamp = CouchDBEvents.Where(evt => evt.EventInfoHash == infohash).OrderByDescending(evt => evt.TimeStamp);

                while (true)
                {
                    var older = eventsByStamp.Skip(1).ToArray();
                    if (older.Length <= 0)
                    {
                        var newestEvent = eventsByStamp.FirstOrDefault();
                        if(newestEvent != null) Console.WriteLine(@"{0} already clean", newestEvent.Event.Code);
                        break;
                    }

                    for (int i = 0; i < older.Length; i++)
                    {
                        CouchDBEvents.Remove(older[i]);
                    }
                }
            }
// ReSharper restore AccessToModifiedClosure
        }

        public Day GetDayByDate(DateTime date, string seminargroupname)
        {
            var days = from s in SeminarGroups
                       where s.Name == seminargroupname
                       from c in s.CalendarWeeks
                       from d in c.Days
                       where d.Date.Date == date.Date
                       select d;
            return days.FirstOrDefault();
        }

        public Event GetEventByHashInfo(string hashInfo)
        {
            var evt = from s in SeminarGroups
                      from c in s.CalendarWeeks
                      from d in c.Days
                      from e in d.Events
                      where e.Source == EventSource.School
                      where e.HashInfo == hashInfo
                      select e;

            return evt.FirstOrDefault();
        }

        /*
         *                
         *      case "version":
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

         * 
         * 
         * 
         * 
         *             var tCode = new XElement("code",
                                     match.Groups["Code"].Value.ConvertUmlauts(UmlautConvertDirection.ToCrossWordFormat));
            var tTutor = new XElement("dozent", match.Groups["Tutor"].Value);
            var tRoom = new XElement("raum", match.Groups["Room"].Value);
            var tDay = new XElement("tag", match.Groups["DayOfWeek"].Value);
            var tFrom = new XElement("von", match.Groups["Start"].Value);
            var tTill = new XElement("bis", match.Groups["End"].Value);

            var tEvent = new XElement("event",
                                      tCode, tTutor, tRoom, tDay, tFrom, tTill);

            return tEvent;

         * */
        #region Parser Helper Methods
        public static string CleanDateString(string dateString)
        {
            var rgxClearDate = new Regex(@"\d{1,2}\.\d{1,2}\.\d{1,4}");
            dateString = rgxClearDate.Match(dateString).Value;
            return dateString;
        }

        private DateTime ParseDate(string cleanDate)
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
                if (DateTime.TryParseExact(cleanDate, dateFormat, CultureInfo.CurrentCulture, style, out temp))
                    return temp;
            }

            return DateTime.MinValue;
        }

        private IEnumerable<CalendarWeek> ParseKW(string kwLine, int year)
        {
            var tElm = new List<CalendarWeek>();

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
                        var cw = new CalendarWeek { Week = i, Year = year };
                        tElm.Add(cw);
                    }
                }
                else
                {
                    var cw = new CalendarWeek { Week = int.Parse(tBlock), Year = year };
                    tElm.Add(cw);
                }
            }
            return tElm;
        }


        #endregion


        private void ParseContent(string content)
        {
            // Zeile für Zeile einlesen
            var sr = new StringReader(content);
            string line = "";

            SeminarGroup currentSeminarGroup = null;
            CalendarWeek[] currentWeeks = null;
            int currentYear = 0;

            while (null != (line = sr.ReadLine()))
            {
                var filter = GetLineFilter(line);
                if (filter == null) continue;

                var values = filter.GetValues(line);

                OnStatusMessageChanged("Filter {0} -> hat {1} Werte", filter.Name, values.Count);

                switch (filter.Name)
                {
                    case "version":
                        {
                            string cleanDate = CleanDateString(values["Datum"]);
                            var date = ParseDate(cleanDate);

                            currentYear = date.Year;
                            currentSeminarGroup = new SeminarGroup { Version = values["Version"], LastUpdated = date };

                            break;
                        }
                    case "gruppe":
                        {
                            if (currentSeminarGroup != null)
                            {
                                currentSeminarGroup.Name = values["Gruppe"];
                                SeminarGroups.Add(currentSeminarGroup);
                            }
                            break;
                        }
                    case "kw":
                        {
                            currentWeeks = ParseKW(values["0"], currentYear).ToArray();
                            foreach (var cw in currentWeeks)
                            {
                                cw.InitializeDays();
                                currentSeminarGroup.CalendarWeeks.Add(cw);
                            }
                            break;
                        }
                    case "veranstaltung":
                        {
                            if (currentWeeks != null)
                                foreach (var cw in currentWeeks)
                                {
                                    TimeSpan from = TimeSpan.MinValue, till = TimeSpan.MinValue;
                                    var s_dow = values["DayOfWeek"];
                                    var i_dow = Native.Helper.DaysOfWeek[s_dow] - 1;
                                    var day = cw.Days.Where(d => d.DOW == (DayOfWeek)(i_dow)).FirstOrDefault();

                                    if (day == null)
                                    {
                                        Debugger.Break();
                                    }

                                    try
                                    {
                                        from = TimeSpan.ParseExact(values["Start"], "g",
                                                                                                          CultureInfo.CurrentCulture);
                                        till = TimeSpan.ParseExact(values["End"], "g",
                                                                      CultureInfo.CurrentCulture);
                                    }
                                    catch
                                    {
                                        Debugger.Break();
                                    }

                                    if (day == null) throw new Exception("Day is null! Verify why!");

                                    var evt = new Event();

                                    evt.BeginCleanInit();

                                    evt.Code = values["Code"];
                                    evt.Date = cw.GetDateOfWeekday(i_dow);
                                    evt.From = from;
                                    evt.Till = till;
                                    evt.Tutor = values["Tutor"];
                                    evt.DayOfWeek = i_dow;
                                    evt.Day = day;
                                    evt.CalendarWeek = cw.Week;
                                    evt.Room = values["Room"];
                                    evt.SeminarGroup = currentSeminarGroup;

                                    evt.EndCleanInit();

                                    evt.Day.Events.Add(evt);
                                    evt.CleanUp();
                                }
                            break;
                        }
                }
            }

            OnStatusMessageChanged("Fertig");
        }

        private StringFilter GetLineFilter(string line)
        {
            var filter = LineFilters.Where(flt => flt.FilterMatches(line)).FirstOrDefault();
            return filter;
        }


        #region Daten

        public ObservableCollection<SeminarGroup> SeminarGroups { get; set; }
        public ObservableCollection<CouchDBEventInfo> CouchDBEvents { get; set; }
        public ObservableCollection<Event> ReplacedSchoolEvents { get; set; }

        #endregion
    }
}
