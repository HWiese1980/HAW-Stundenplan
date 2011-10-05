using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using HAW_Tool.HAW.REST;

namespace HAW_Tool.HAW
{
    public class Day : XElementContainer
    {
        #region Fields (7)

        bool _bInitlialized;
        bool _bRecalculatingOccupiedRanges;
        readonly List<IEvent> _mEvents;
/*
        HAWClient _mHawClient = new HAWClient();
*/
        IEvent[] _mOccupiedMinutes;

        #endregion Fields

        #region Constructors (1)

        private Day(int dayNo, IEnumerable<IEvent> events, IWeek week)
            : this(dayNo, week)
        {
            Debug.Assert(events != null, "events != null");
            var ev = events.ToList();
            _mEvents.AddRange(ev);
            Date = ev.First().Date;
        }

        private Day(int dayNo, IWeek week)
        {
            var minutes = (int)(TimeSpan.Parse("21:00") - TimeSpan.Parse("07:00")).TotalMinutes;
            Week = week;
            _mOccupiedMinutes = new IEvent[minutes];

            DOW = (DayOfWeek)dayNo + 1;
            _mEvents = new List<IEvent>();
            Date = Helper.DayOfWeekToDateTime(week.Year, week.Week, dayNo);
        }

        #endregion Constructors

        #region Properties (8)

        public DateTime Date { get; set; }

        public DayOfWeek? DOW { get; set; }

        public IEnumerable<IEvent> Events
        {
            get
            {
                InitializeEvents();
                RecalculateOccupiedRanges();

                var todays = (from p in StoredEvents
                             where p.SeminarGroup == Week.SeminarGroup.FullName
                             where p.OtherDates.Contains(Date.Date) | p.Date == Date & (p.Replaces ?? "") == ""
                             select p).ToList();

                // int tExmCount = exams.Count();

                var ret = (from p in _mEvents
                           where !PlanFile.Instance.HasReplacements(p, this)
                           select p);

                List<IEvent> replacements = (from p in _mEvents
                                             from q in StoredEvents
                                             where q.IsReplacementFor(p)
                                             select (IEvent)q).ToList();

                var all = ret.Concat(todays.Cast<IEvent>()).Concat(replacements);

                return all;
            }
        }

        public bool HasAdditions
        {
            get
            {
                var additionals = from p in StoredEvents
                                  where p.SeminarGroup == Week.SeminarGroup.FullName
                                  where p.Date == Date
                                  select p;
                return (additionals.Count() > 0);
            }
        }


        public bool HasReplacements
        {
            get
            {
                var replacements = from p in _mEvents
                                   where PlanFile.Instance.HasReplacements(p, this)
                                   select p;
                return (replacements.Count() > 0);
            }
        }


        public double MinWidth
        {
            get
            {
                try
                {
                    object rasterH = Application.Current.FindResource("RasterH");
                    if (rasterH != null)
                    {
                        var rasterW = (double)rasterH;
                        var tConv = new TimeToCoord { AsWidth = false, Multiplier = rasterW };
                        var tWConv = new TimeToCoord { AsWidth = true, Multiplier = rasterW };

                        var tLatest = from p in Events
                                      orderby p.From descending
                                      select p;

                        var tPos = (double)tConv.Convert(new object[] { tLatest.First().From, tLatest.First() }, null, null, CultureInfo.CurrentCulture);
                        tPos += (double)tWConv.Convert(new object[] { tLatest.First().From, tLatest.First() }, null, null, CultureInfo.CurrentCulture);

                        return tPos + rasterW;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(@"Error: {0}", e.Message);
                    return 0;
                }

                return 0;
            }
        }


        private IEnumerable<RESTEvent> StoredEvents
        {
            get
            {
                return PlanFile.Instance.StoredEvents;
            }
        }

        public IWeek Week { get; set; }

        #endregion Properties

        #region Methods (7)

        // Public Methods (4) 

        public static Day CreateDay(IGrouping<int, IEvent> eventGroup, IWeek week)
        {
            var tDay = new Day(eventGroup.Key, eventGroup, week);
            return tDay;
        }

        public static Day CreateDay(int day, IWeek week)
        {
            var tDay = new Day(day, week);
            return tDay;
        }

        public bool IsInOccupiedRanges(IEvent tEvt)
        {
            var minutes = (int)(tEvt.Till - tEvt.From).TotalMinutes; // Länge des Events in Minuten
            var frommin = (int)(tEvt.From - TimeSpan.Parse("07:00")).TotalMinutes; // Start des Events ab 07:00 Uhr in Minuten

            for (var min = frommin; min < frommin + minutes; min++)
            {
                if (_mOccupiedMinutes[min] != null && _mOccupiedMinutes[min] != tEvt) return true;
            }
            return false;
        }

        public IEvent MinuteOccupiedBy(uint minute)
        {
            uint fromminute = minute - (uint)TimeSpan.Parse("07:00").TotalMinutes;
            return _mOccupiedMinutes[fromminute];
        }

        public override string ToString()
        {
            Debug.Assert(DOW != null, "DOW != null");
            return String.Format("{0}", WeekHelper.WD[(int)DOW.Value - 1]);
        }

        // Private Methods (3) 

/*
        private bool ContainsCode(string p)
        {
            var evt = from q in _mEvents where q.BasicCode == p select q;
            return evt.Count() > 0;
        }
*/

        private void InitializeEvents()
        {
            if (_bInitlialized) return;

            _bInitlialized = true;
            foreach (IEvent evt in Events.Where(p => p.Day == null)) evt.Day = this;
        }

        private void RecalculateOccupiedRanges()
        {
            if (_bRecalculatingOccupiedRanges) return;
            _bRecalculatingOccupiedRanges = true;

            _mOccupiedMinutes = new IEvent[_mOccupiedMinutes.Length];

            foreach (IEvent tEvt in Events.Where(p => p.RowIndex == 0))
            {
                var minutes = (int)(tEvt.Till - tEvt.From).TotalMinutes; // Länge des Events in Minuten
                var frommin = (int)(tEvt.From - TimeSpan.Parse("07:00")).TotalMinutes; // Start des Events ab 07:00 Uhr in Minuten

                for (int min = frommin; min < frommin + minutes; min++)
                {
                    _mOccupiedMinutes[min] = tEvt;
                }
            }
            _bRecalculatingOccupiedRanges = false;
        }

        #endregion Methods

        #region Nested Classes (1)


/*
        class Range
        {
            #region Fields (2)

            private DateTime Date;
            private int RowIndex = 0;

            #endregion Fields

            #region Methods (1)

            // Public Methods (1) 

            public bool IsInRange(DateTime Value, int Index)
            {
                if (Index != RowIndex) return false;

                return Value.IsBetween(Date.Date + From, Date.Date + To);
            }

            #endregion Methods




            public TimeSpan From, To;
        }
*/
        #endregion Nested Classes
    }
}
