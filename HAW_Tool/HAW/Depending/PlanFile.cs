using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Threading;
using LittleHelpers;
using System.Text.RegularExpressions;
using System.IO;
using System.Windows;
using MongoDB.Driver;
using SeveQsCustomControls;

namespace HAW_Tool.HAW.Depending
{
    public class PlanFile : NotifyingObject
    {
        public static Dispatcher MainDispatcher { get; set; }

        private PlanFile()
        {
            MainDispatcher = Application.Current.MainWindow.Dispatcher;

            SeminarGroups = new ThreadSafeObservableCollection<SeminarGroup>();
            Replacements = new ThreadSafeObservableCollection<Replacement>();
            //Events = new ThreadSafeObservableCollection<Event>();

            //Events.CollectionChanged += Events_CollectionChanged;
        }

        public event EventHandler<ValueEventArgs<Event>> EventSaved;

        private void OnEventSaved(object sender, ValueEventArgs<Event> e)
        {
            if (EventSaved != null) EventSaved(sender, e);
        }

        void Events_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var evt in e.NewItems.Cast<Event>())
                    {
                        evt.EventSaved += evt_EventSaved;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var evt in e.OldItems.Cast<Event>())
                    {
                        evt.EventSaved -= evt_EventSaved;
                    }
                    break;
            }
        }

        void evt_EventSaved(object sender, ValueEventArgs<Replacement> e)
        {
            LoadReplacements();
            OnEventSaved(this, new ValueEventArgs<Event> { Value = (Event)sender });
        }

        //private IEnumerable<Event> GetAllEvents()
        //{
        //    return from e in Events
        //           where e.Source == EventSource.School
        //           select e;
        //}

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

        private SeminarGroup _selectedSeminarGroup;
        public SeminarGroup SelectedSeminarGroup
        {
            get { return _selectedSeminarGroup; }
            set { _selectedSeminarGroup = value; OnPropertyChanged("SelectedSeminarGroup"); }
        }

        //static void SeminarGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    Console.WriteLine(@"Seminar Group selected: {0}", ((SeminarGroup)e.NewValue).Name);
        //}


        private static readonly List<StringFilter> LineFilters;

        public static PlanFile Instance { get; private set; }

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

        private readonly object _loadScheduleLock = new object();
        internal void LoadSchedule(string schedule)
        {
            OnStatusMessageChanged("Lade {0}", schedule);
            if (!Uri.IsWellFormedUriString(schedule, UriKind.Absolute))
                return;

            try
            {
                lock (_loadScheduleLock)
                {
                    Console.WriteLine(@"Lade {0}", schedule);
                    string content = new WebClient().DownloadString(schedule);
                    ParseContent(content);
                }
            }
            catch (Exception exp)
            {
                OnStatusMessageChanged("Laden von {0} fehlgeschlagen: {1}", schedule.Substring(schedule.Length - 20),
                                       exp.Message);
                // Debugger.Break();
            }

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
                                      "dd.MM.yy",
                                      "dd.M.yyyy",
                                      "dd.M.yy"
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
                        //var cw = DispatcherFactory<CalendarWeek>(s =>
                        //{
                        //    s.Week = i;
                        //    s.Year = year;
                        //});
                        tElm.Add(cw);
                    }
                }
                else
                {
                    var cw = new CalendarWeek { Week = int.Parse(tBlock), Year = year };
                    //var cw = DispatcherFactory<CalendarWeek>(s =>
                    //{
                    //    s.Week = int.Parse(tBlock);
                    //    s.Year = year;
                    //});
                    tElm.Add(cw);
                }
            }
            return tElm;
        }


        #endregion

        //public T DispatcherFactory<T>(Action<T> setupData, bool async = false) where T : DependencyObject, new()
        //{
        //    var p = new Func<T>(() => InternalDispatcherFactory(setupData));
        //    if (async)
        //    {
        //        Dispatcher.BeginInvoke(p);
        //        return null;
        //    }
        //    return Dispatcher.Invoke(p) as T;
        //}

        private static T InternalDispatcherFactory<T>(Action<T> setupData) where T : DependencyObject, new()
        {
            var obj = new T();
            setupData(obj);
            return obj;
        }

        private void ParseContent(string content)
        {
            // Zeile für Zeile einlesen

            // content = content.Replace("\r\n", "|");

            var sr = new StringReader(content);
            string line = "";

            var semGroupBlocks = new List<string>();
            StringBuilder bld = null;
            while (null != (line = sr.ReadLine()))
            {
                if (line.StartsWith("Stundenplan"))
                {
                    if (bld != null) semGroupBlocks.Add(bld.ToString());
                    bld = new StringBuilder();
                }
                bld.AppendLine(line);
            }

            var blocksLeft = semGroupBlocks.Count;
            //var opt = new ParallelOptions
            //                          {
            //                              MaxDegreeOfParallelism = 5,
            //                          };

            Parallel.ForEach(semGroupBlocks, /*opt,*/ block =>
                                                 {
                                                     try
                                                     {
                                                         ParseSeminarGroupBlock(block);
                                                     }
                                                     catch (Exception exp)
                                                     {
                                                         Console.WriteLine("");
                                                     }
                                                     finally
                                                     {
                                                         OnStatusMessageChanged("Noch {0} Seminargruppen zu lesen", blocksLeft--);
                                                     }
                                                 });
        }

        private void ParseSeminarGroupBlock(string block)
        {
            string line;
            SeminarGroup currentSeminarGroup = null;
            CalendarWeek[] currentWeeks = null;
            int currentYear = 0;

            var localBlock = block;
            var srLocal = new StringReader(localBlock);
            while (null != (line = srLocal.ReadLine()))
            {
                var filter = GetLineFilter(line);
                if (filter == null) continue;

                var values = filter.GetValues(line);
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
                                SeminarGroup @group = currentSeminarGroup;
                                currentSeminarGroup.Name = values["Gruppe"];
                                SeminarGroups.Add(@group);
                                //Dispatcher.Invoke(new Action(() =>
                                //                                 {
                                //                                     @group.Name = values["Gruppe"];
                                //                                     SeminarGroups.Add(@group);
                                //                                 }));
                            }
                            break;
                        }
                    case "kw":
                        {
                            currentWeeks =
                                ParseKW(values["0"], currentYear).ToArray();
                            foreach (var cw in currentWeeks)
                            {
                                cw.InitializeDays();
                                SeminarGroup @group = currentSeminarGroup;
                                //Dispatcher.Invoke(new Action(() => @group.CalendarWeeks.Add(cw)));
                                @group.CalendarWeeks.Add(cw);
                            }
                            break;
                        }
                    case "veranstaltung":
                        {
                            if (currentWeeks != null)
                                foreach (var cw in currentWeeks)
                                {
                                    TimeSpan from = TimeSpan.MinValue,
                                             till = TimeSpan.MinValue;
                                    var sDOW = values["DayOfWeek"];
                                    var iDOW = HAWToolHelper.DaysOfWeek[sDOW] - 1;
                                    CalendarWeek cw2 = cw;
                                    //var day =(Day)Dispatcher.Invoke(new Func<Day>(() => cw2.Days.Where(d => d.DOW == (DayOfWeek)(iDOW)).FirstOrDefault()));
                                    var day = cw2.Days.Where(d => d.DOW == (DayOfWeek)(iDOW)).FirstOrDefault();
                                    if (day == null)
                                    {
                                        Debugger.Break();
                                    }

                                    try
                                    {
                                        from = TimeSpan.ParseExact(values["Start"], "g", CultureInfo.CurrentCulture);
                                        till = TimeSpan.ParseExact(values["End"], "g", CultureInfo.CurrentCulture);
                                    }
                                    catch
                                    {
                                        Debugger.Break();
                                    }

                                    if (day == null)
                                        throw new Exception(
                                            "Day is null! Verify why!");

                                    SeminarGroup @group = currentSeminarGroup;
                                    CalendarWeek cw1 = cw;
                                    var newEvent = new Event
                                                       {
                                                           Code = values["Code"],
                                                           Date = cw1.GetDateOfWeekday(iDOW),
                                                           From = from,
                                                           Till = till,
                                                           Tutor = values["Tutor"],
                                                           DayOfWeek = iDOW,
                                                           Day = day,
                                                           CalendarWeek = cw1.Week,
                                                           Room = values["Room"],
                                                           SeminarGroup = @group
                                                       };
                                    newEvent.Day.Events.Add(newEvent);
                                    // Events.Add(newEvent);
                                    newEvent.EndCleanInit();
                                    newEvent.CleanUp();
                                }
                            break;
                        }
                }
            }
        }

        private StringFilter GetLineFilter(string line)
        {
            var filter = LineFilters.Where(flt => flt.FilterMatches(line)).FirstOrDefault();
            return filter;
        }


        #region Daten

        public ThreadSafeObservableCollection<SeminarGroup> SeminarGroups { get; set; }
        //public ThreadSafeObservableCollection<Event> Events { get; set; }
        public ThreadSafeObservableCollection<Replacement> Replacements { get; private set; }

        #endregion

        private MongoServer _mongoServer = MongoServer.Create("mongodb://seveq.de");
        public void MongoStore(Replacement rpl)
        {
            var db = _mongoServer.GetDatabase("HAW");
            var coll = db.GetCollection<Replacement>("Replacements");
            coll.Insert(rpl);
        }

        internal void LoadSchedules(string[] schedules)
        {
            var workers = new List<BackgroundWorker>();
            var tcheck = new Timer(250);

            tcheck.Elapsed += (x, y) =>
            {
                if (workers.Count <= 0)
                {
                    tcheck.Enabled = false;
                    LoadReplacements();
                    OnAllSchedulesLoaded();
                }
            };

            foreach (var sched in schedules)
            {
                var twrk = new BackgroundWorker();
                workers.Add(twrk);
                string sched1 = sched;
                twrk.DoWork += (x, y) => LoadSchedule(sched1);
                twrk.RunWorkerCompleted += (x, y) => workers.Remove(twrk);
                twrk.RunWorkerAsync();
            }
            tcheck.Enabled = true;
        }

        private void LoadReplacements()
        {
            /*
            OnStatusMessageChanged("Lade Änderungen am Stundenplan...");
            var db = _mongoServer.GetDatabase("HAW");
            var coll = db.GetCollection<Replacement>("Replacements");
            var items = coll.FindAllAs<Replacement>().ToArray();

            Replacements.Clear();
            Replacements.AddItems(items.Select(p =>
            {
                OnStatusMessageChanged(
                    "Lade Änderungen am Stundenplan... fülle Replacementliste; Änderung: {0}",
                    p.Event);
                p.Event.Source = EventSource.ReplacementDB;
                return p;
            }).ToArray());

            foreach (var item in items)
            {
                OnStatusMessageChanged("Lade Änderungen am Stundenplan... Änderung: {0}", item.Event);
                if (item.Event.SeminarGroup == null) continue;

                var day = item.Event.SeminarGroup.GetDayByDate(item.Event.Date.Date);
                item.Event.Day = day;
                item.Event.Source = EventSource.ReplacementDB;
                day.Events.Add(item.Event);
                item.Event.EndCleanInit();
            }
             * */

            //Events.RemoveItems(Events.Where(p => p.Source == EventSource.ReplacementDB).ToArray());
        //    Events.AddItems(items.Select(p =>
        //                                     {
        //                                         OnStatusMessageChanged(
        //                                             "Lade Änderungen am Stundenplan... platziere Änderungen in Eventliste; Änderung: {0}",
        //                                             p.Event);
        //                                         p.Event.Source = EventSource.ReplacementDB;
        //                                         return p.Event;
        //                                     }).ToArray());
        }

        public event EventHandler AllSchedulesLoaded;

        public void OnAllSchedulesLoaded()
        {
            EventHandler handler = AllSchedulesLoaded;
            if (handler != null) handler(this, default(EventArgs));
        }

        public IEnumerable<Event> GetEventsByHashInfo(string hash)
        {
            return from s in SeminarGroups
                   from c in s.CalendarWeeks
                   from d in c.Days
                   from e in d.Events
                   where e.HashInfo == hash
                   select e;
        }
    }
}
