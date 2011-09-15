using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Windows;
using HAW_Tool.HAW.REST;

namespace HAW_Tool.HAW
{
    public class Day : XElementContainer
    {
		#region Fields (7) 

        bool bInitlialized = false;
        bool bRecalculatingOccupiedRanges = false;
        DateTime m_Date;
        DayOfWeek? m_DOW = null;
        List<IEvent> m_Events;
        HAWClient m_HAWClient = new HAWClient();
        IEvent[] mOccupiedMinutes;

		#endregion Fields 

		#region Constructors (1) 

        private Day(int DayNo, IEnumerable<IEvent> Events, IWeek Week)
        {
            int minutes = (int)(TimeSpan.Parse("21:00") - TimeSpan.Parse("07:00")).TotalMinutes;
            this.Week = Week;
            mOccupiedMinutes = new IEvent[minutes];

            m_DOW = (DayOfWeek)DayNo + 1;
            m_Events = new List<IEvent>(Events);
            m_Date = Events.First().Date;
        }

		#endregion Constructors 

		#region Properties (8) 

        public DateTime Date
        {
            get { return m_Date; }
            set { m_Date = value; }
        }

        public DayOfWeek? DOW
        {
            get { return m_DOW; }
        }

        public IEnumerable<IEvent> Events
        {
            get
            {
                InitializeEvents();
                RecalculateOccupiedRanges();

                var todays = from p in StoredEvents
                             where p.SeminarGroup == this.Week.SeminarGroup.FullName
                             where p.OtherDates.Contains(this.Date.Date) | p.Date == this.Date & (p.Replaces ?? "") == ""
                             select p;

                var exams = from p in todays
                            where p.Type == EntryType.Exam
                            select p;

                // int tExmCount = exams.Count();

                var ret = (from p in m_Events
                           where !PlanFile.Instance.HasReplacements(p, this)
                           select (IEvent)p);

                List<IEvent> replacements = (from p in m_Events
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
                                  where p.SeminarGroup == this.Week.SeminarGroup.FullName
                                  where p.Date == this.Date
                                  select p;
                return (additionals.Count() > 0);
            }
        }

        public bool HasReplacements
        {
            get
            {
                var replacements = from p in m_Events
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
                    double rasterW = (double)Application.Current.FindResource("RasterH");
                    TimeToCoord tConv = new TimeToCoord() { AsWidth = false, Multiplier = rasterW };
                    TimeToCoord tWConv = new TimeToCoord() { AsWidth = true, Multiplier = rasterW };

                    var tLatest = from p in this.Events
                                  orderby p.From descending
                                  select p;

                    double tPos = (double)tConv.Convert(new object[] { tLatest.First().From, tLatest.First() }, null, null, CultureInfo.CurrentCulture);
                    tPos += (double)tWConv.Convert(new object[] { tLatest.First().From, tLatest.First() }, null, null, CultureInfo.CurrentCulture);

                    return tPos + rasterW;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: {0}", e.Message);
                    return 0;
                }
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

        public static Day CreateDay(IGrouping<int, IEvent> EventGroup, IWeek Week)
        {
            Day tDay = new Day(EventGroup.Key, EventGroup, Week);
            return tDay;
        }

        public bool IsInOccupiedRanges(IEvent tEvt)
        {
            int minutes = (int)(tEvt.Till - tEvt.From).TotalMinutes; // Länge des Events in Minuten
            int frommin = (int)(tEvt.From - TimeSpan.Parse("07:00")).TotalMinutes; // Start des Events ab 07:00 Uhr in Minuten

            for (int min = frommin; min < frommin + minutes; min++)
            {
                if (mOccupiedMinutes[min] != null && mOccupiedMinutes[min] != tEvt) return true;
            }
            return false;
        }

        public IEvent MinuteOccupiedBy(uint Minute)
        {
            uint fromminute = Minute - (uint)TimeSpan.Parse("07:00").TotalMinutes;
            return mOccupiedMinutes[fromminute];
        }

        public override string ToString()
        {
            return String.Format("{0}", WeekHelper.WD[(int)m_DOW.Value - 1]);
        }
		// Private Methods (3) 

        private bool ContainsCode(string p)
        {
            var evt = from q in m_Events where q.BasicCode == p select q;
            return evt.Count() > 0;
        }

        private void InitializeEvents()
        {
            if (bInitlialized) return;

            bInitlialized = true;
            foreach (IEvent evt in Events.Where(p => p.Day == null)) evt.Day = this;
        }

        private void RecalculateOccupiedRanges()
        {
            if (bRecalculatingOccupiedRanges) return;
            bRecalculatingOccupiedRanges = true;

            mOccupiedMinutes = new IEvent[mOccupiedMinutes.Length];

            foreach (IEvent tEvt in Events.Where(p => p.RowIndex == 0))
            {
                int minutes = (int)(tEvt.Till - tEvt.From).TotalMinutes; // Länge des Events in Minuten
                int frommin = (int)(tEvt.From - TimeSpan.Parse("07:00")).TotalMinutes; // Start des Events ab 07:00 Uhr in Minuten

                for (int min = frommin; min < frommin + minutes; min++)
                {
                    mOccupiedMinutes[min] = tEvt;
                }
            }
            bRecalculatingOccupiedRanges = false;
        }

		#endregion Methods 

		#region Nested Classes (1) 


        class Range
        {
		#region Fields (2) 

            public DateTime Date;
            public int RowIndex = 0;

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
		#endregion Nested Classes 
    }
}
