﻿using System;
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
using RedBranch.Hammock;
using SeveQsCustomControls;

using MongoDB.Driver;

namespace HAW_Tool.HAW.Depending
{
    public class PlanFile : NotifyingObject
    {
        public event EventHandler Loaded;

        private Dictionary<Event, Event> _mongoDBOriginalEventMap = new Dictionary<Event, Event>();
        private static readonly object CouchDbLock = new object();

        public void RefreshTimer(bool On)
        {
            _refreshCouchDBTimer.Enabled = On;
        }

        private readonly Timer _refreshCouchDBTimer;

        private PlanFile()
        {

            CouchConnection = new Connection(new Uri("http://seveq.de:5984"));
            MongoConnection = MongoServer.Create("mongodb://seveq.de");

            SeminarGroups = new ThreadSafeObservableCollection<SeminarGroup>();
            ReplacedSchoolEvents = new ThreadSafeObservableCollection<Event>();
            CouchDBEvents = new ThreadSafeObservableCollection<CouchDBEventInfo>();

            CouchDBEvents.CollectionChanged += CleanUpDays;

            _refreshCouchDBTimer = new Timer { Interval = (2000) };
            _refreshCouchDBTimer.Elapsed += (x, y) => LoadCouchEvents();
        }

        private void CleanUpDays(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach(var item in e.NewItems.Cast<CouchDBEventInfo>())
                {
                    item.Event.Source = EventSource.MongoDB;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems.Cast<CouchDBEventInfo>())
                {
                    var day = item.Event.Day;
                    if (day == null) continue;

                    day.Events.Remove(item.Event);

                    var originalEvent = GetEventByHashInfo(item.EventInfoHash);
                    if (originalEvent != null)
                    {
                        originalEvent.Reset();
                        // originalEvent.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private void ResetCouchDBEvents()
        {
            CouchDBEvents.Clear();
        }

        private IEnumerable<Event> GetAllEvents()
        {
            return from s in SeminarGroups
                   from c in s.CalendarWeeks
                   from d in c.Days
                   from e in d.Events
                   where e.Source == EventSource.School
                   // where e.IsDirty
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

        //public SeminarGroup SelectedSeminarGroup
        //{
        //    get { return (SeminarGroup)GetValue(SelectedSeminarGroupProperty); }
        //    set { SetValue(SelectedSeminarGroupProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for SelectedSeminarGroup.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty SelectedSeminarGroupProperty =
        //    DependencyProperty.Register("SelectedSeminarGroup", typeof(SeminarGroup), typeof(PlanFile), new UIPropertyMetadata(null, SeminarGroupChanged));

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
                // lock (_loadScheduleLock)
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
                Debugger.Break();
            }

        }

        public MongoServer MongoConnection { get; private set; }
        
        public void LoadMongoEvents()
        {
            if (Helper.IsInDesignModeStatic || SeminarGroups == null || SeminarGroups.Count <= 0) return;
            if (SelectedSeminarGroup == null) SelectedSeminarGroup = SeminarGroups.FirstOrDefault();

            try
            {
                MongoDatabase db = MongoConnection.GetDatabase("HAWEvents");
                var coll = db.GetCollection<CouchDBEventInfo>("CouchDBEvents");
                var checkedHashes = new List<string>();

                CouchDBEvents.Clear();

                foreach(var evt in coll.FindAllAs<CouchDBEventInfo>())
                {
                    var hash = evt.EventInfoHash;
                    if (checkedHashes.Contains(hash))
                    {
                        Console.WriteLine("Hash {0} already checked", hash);
                        continue;
                    }
                    
                    var qry = new QueryDocument("eventinfohash", evt.EventInfoHash);
                    var evtofhash = coll.FindAs<CouchDBEventInfo>(qry).SetSortOrder("timestamp");
                    Console.WriteLine("Found {0} events for hash {1}", evtofhash.Count(), hash);
                    Console.WriteLine("TimeStamps: {0}",
                                      string.Join("; ",
                                                  evtofhash.Select(
                                                      p => string.Format("[{0} - {1}]", p.EventInfoHash, p.TimeStamp)).
                                                      ToArray()));
                    
                    var newest = evtofhash.Last();
                    Console.WriteLine("Newest is from {0}", newest.TimeStamp);

                    evt.Event.Source = EventSource.MongoDB;

                    var alreadyLoadedNewerItems = from p in CouchDBEvents
                                                  where p.EventInfoHash == newest.EventInfoHash
                                                  where p.TimeStamp >= newest.TimeStamp
                                                  select p;

                    if(alreadyLoadedNewerItems.Count() <= 0)
                    {
                        CouchDBEvents.Add(newest);
                        checkedHashes.Add(newest.EventInfoHash);
                    }
                    else
                    {
                        Console.WriteLine("CouchDBEvents Collection already contains an event with hash {0} that is newer than {1}", newest.EventInfoHash, newest.TimeStamp);
                    }
                }

                foreach(var couchdbevent in CouchDBEvents) couchdbevent.Event.SetDayByDate();

                // ReplaceSchoolEvents();
            }
            catch (Exception)
            {
                return;
            }
        }


        public Connection CouchConnection { get; private set; }

        public bool Logging { get; set; }

        internal void LoadCouchEvents()
        {
//             if (Helper.IsInDesignModeStatic || SeminarGroups == null || SeminarGroups.Count <= 0) return;
//             if (SelectedSeminarGroup == null) SelectedSeminarGroup = SeminarGroups.FirstOrDefault();
// 
//             // lock (CouchDbLock)
//             {
//                 Console.WriteLine("Lade CouchDB Events");
//                 try
//                 {
//                     var tWrk = new BackgroundWorker();
//                     var couchS = CouchConnection.CreateSession("haw_events");
//                     IList<Document> cdbDocs = null;
//                     tWrk.DoWork += (x, y) =>
//                     {
//                         cdbDocs = couchS.ListDocuments();
//                     };
//                     tWrk.RunWorkerCompleted += (x, y) =>
//                     {
//                         // ResetCouchDBEvents();
// 
//                         var buffer = new List<CouchDBEventInfo>();
// 
//                         foreach (var cdbDoc in cdbDocs)
//                         {
//                             if (!cdbDoc.Id.StartsWith("couchdbeventinfo")) continue;
// 
//                             var evtdoc = couchS.Load<CouchDBEventInfo>(cdbDoc.Id);
//                             evtdoc.Event.Source = EventSource.CouchDB;
// 
//                             buffer.Add(evtdoc);
//                         }
// 
//                         // if (buffer.Count <= 0) return;
// 
//                         // gucken, ob items aus der bisherigen CouchDBListe nicht mehr in der neuen sind
// 
//                         var comparer = new CouchDBInfoComparer();
// 
//                         var removed = CouchDBEvents.Except(buffer, comparer).ToArray();
//                         if (removed.Length > 0)
//                         {
//                             Console.WriteLine("Removing {0} items from the CouchDB List", removed.Length);
//                             CouchDBEvents.RemoveItems(removed);
//                         }
// 
//                         var added = buffer.Except(CouchDBEvents, comparer).ToArray();
//                         if (added.Length > 0)
//                         {
//                             Console.WriteLine("Adding {0} items to the CouchDB List", added.Length);
//                             CouchDBEvents.AddItems(added);
//                         }
// 
//                         CleanUpCouchDBEvents();
//                         ReplaceSchoolEvents();
//                     };
// 
//                     tWrk.RunWorkerAsync();
//                 }
//                 catch
//                 {
//                     return;
//                 }
//             }
        }

        private void ReplaceSchoolEvents()
        {
            Logging = true;
            foreach (var evtinfo in CouchDBEvents)
            {
                var originalEvent = GetEventByHashInfo(evtinfo.EventInfoHash);
                if (originalEvent != null)
                {
                    originalEvent.Reset();
                    originalEvent.IsReplaced = true;
                    originalEvent.Visibility = Visibility.Hidden;
                    originalEvent.CleanUp();

                    evtinfo.EventInfoHash = originalEvent.HashInfo;

                    _mongoDBOriginalEventMap[evtinfo.Event] = originalEvent;
                }

                evtinfo.Event.EndCleanInit();
                evtinfo.Event.SetDayByDate();
                evtinfo.Event.CleanUp();
            }
            Logging = false;
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
                        if (newestEvent != null) Console.WriteLine(@"{0} already clean", newestEvent.Event.Code);
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
            // var tElm = new List<CalendarWeek>();

            IEnumerable<string> tWBlocks = from string p in kwLine.Split(',')
                                           select p.Trim();

            var tFromTo = new Regex(@"^(?<from>\d+)-(?<to>\d+)$");
            var weeks = new List<int>();

            foreach (string tBlock in tWBlocks)
            {

                if (tFromTo.IsMatch(tBlock))
                {
                    Match tMatch = tFromTo.Match(tBlock);
                    int tStart = int.Parse(tMatch.Groups["from"].Value);
                    int tEnd = int.Parse(tMatch.Groups["to"].Value);


                    for (int i = tStart; i <= tEnd; i++)
                    {
                        weeks.Add(i);
                    }

                }
                else
                {
                    try
                    {
                        weeks.Add(int.Parse(tBlock));
                    }
                    catch
                    {
                        Console.WriteLine(@"Invalid Week Block: {0}", tBlock);
                    }
                }

            }

            return from c in weeks
                   select new CalendarWeek(c, year);
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

                            currentSeminarGroup.CalendarWeeks.AddItems(currentWeeks);
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
                                    CalendarWeek cw1 = cw;
                                    //var day =(Day)Dispatcher.Invoke(new Func<Day>(() => cw2.Days.Where(d => d.DOW == (DayOfWeek)(iDOW)).FirstOrDefault()));
                                    var day = cw1.Days.Where(d => d.DOW == (DayOfWeek)(iDOW)).FirstOrDefault();
                                    if (day == null) break;

                                    try
                                    {
                                        from = TimeSpan.ParseExact(values["Start"], "g", CultureInfo.CurrentCulture);
                                        till = TimeSpan.ParseExact(values["End"], "g", CultureInfo.CurrentCulture);
                                    }
                                    catch
                                    {
                                        Debugger.Break();
                                    }

                                    SeminarGroup @group = currentSeminarGroup;
                                    CalendarWeek cw2 = cw;
                                    var newEvent = new Event
                                                       {
                                                           SeminarGroup = @group,
                                                           Code = values["Code"],
                                                           From = from,
                                                           Till = till,
                                                           Date = cw2.GetDateOfWeekday(iDOW),
                                                           Tutor = values["Tutor"],
                                                           CalendarWeek = cw2.Week,
                                                           Room = values["Room"],
                                                       };
                                    // newEvent.Day.Events.Add(newEvent);
                                    newEvent.EndCleanInit();
                                    newEvent.SetDayByDate();
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
        public ThreadSafeObservableCollection<CouchDBEventInfo> CouchDBEvents { get; set; }
        public ThreadSafeObservableCollection<Event> ReplacedSchoolEvents { get; set; }

        #endregion



        internal void LoadSchedules(string[] schedules)
        {
            var workers = new List<BackgroundWorker>();
            var tcheck = new Timer(100);

            tcheck.Elapsed += (x, y) =>
            {
                if (workers.Count <= 0)
                {
                    tcheck.Enabled = false;
                    LoadMongoEvents();
                    OnAllSchedulesLoaded();
                }
            };

            // Parallel.ForEach(schedules, LoadSchedule);

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

        public event EventHandler AllSchedulesLoaded;

        public void OnAllSchedulesLoaded()
        {
            EventHandler handler = AllSchedulesLoaded;
            if (handler != null) handler(this, default(EventArgs));
        }

        public Event GetEventFromMongoEvent(Event eventContext)
        {
            return _mongoDBOriginalEventMap[eventContext];
        }
    }
}
