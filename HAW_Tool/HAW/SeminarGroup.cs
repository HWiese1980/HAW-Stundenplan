using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using HAW_Tool.HAW.REST;
using LittleHelpers;

namespace HAW_Tool.HAW
{
    public class SeminarGroup : XElementContainer, IIsCurrent
    {
        #region Fields (4)

        private DateTime _mLastUpdated;
        private string _mName;
        private string _mVersion;
        private List<CalendarWeek> _mWeeks;

        #endregion Fields

        #region Properties (12)

        private IEnumerable<IWeek> AdditionalWeeks
        {
            get
            {
                var tCmp = new AnonymousComparer<RESTWeek>((x, y) => (x.Week == y.Week & x.Year == y.Year),
                                                           x => String.Format("{0}:{1}", x.Week, x.Year).
                                                                    GetHashCode());

                IEnumerable<IWeek> weeks = (from p in PlanFile.Instance.StoredEvents
                                            where p.SeminarGroup == FullName
                                            let tWeek = Helper.WeekOfDate(p.Date)
                                            where !AlreadyHasHAWWeek(tWeek, p.Date.Year)
                                            select new RESTWeek {SeminarGroup = this, Week = tWeek, Year = p.Date.Year})
                    .Distinct(tCmp).Cast<IWeek>();


                return weeks;
            }
        }

        public IEnumerable<IEvent> AllEvents
        {
            get
            {
                return from weeks in CalendarWeeks
                       from days in weeks.Days
                       from IEvent evt in days.Events
                       select evt;
            }
        }

        public IEnumerable<IWeek> CalendarWeeks
        {
            get
            {
                /*int currentWeek = Helper.WeekOfDate(DateTime.Now);*/


                IEnumerable<IWeek> tCalWeeks = from p in CalenderWeeksInternal.Cast<IWeek>().Concat(AdditionalWeeks)
                                               orderby p.Week
                                               orderby p.Year
                                               // where p.Year == DateTime.Now.Year
                                               where !p.IsPast
                                               select p;

                //int tRefYear = tCalWeeks.First().Year;

                //var tReturnWeeks = from p in tCalWeeks
                //                   let t = (p.Year > tRefYear) ? Helper.WeekCount(DateTime.Now.Year) : 0
                //                   let week = p.Week + t
                //                   where week >= currentWeek
                //                   select p;

                return tCalWeeks;
            }
        }

        private IEnumerable<CalendarWeek> CalenderWeeksInternal
        {
            get
            {
                return _mWeeks ?? (_mWeeks = new List<CalendarWeek>(from p in MBaseElement.Elements("kw")
                                                                    select new CalendarWeek(this, p)));
            }
        }

        public IWeek Current
        {
            get
            {
                List<IWeek> tCurr = (from p in CalendarWeeks
                                     where Helper.StartOfWeek(p.Week, p.Year) <= DateTime.Now
                                           & DateTime.Now <= Helper.EndOfWeek(p.Week, p.Year)
                                     select p).ToList();

                int count = tCurr.Count;
                return count <= 0 ? null : tCurr.Single();
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

        public IEnumerable<IEvent> EnabledEvents
        {
            get
            {
                return from evt in AllEvents
                       where evt.IsEnabled
                       select evt;
            }
        }

        public string FullName
        {
            get { return String.Format("{0}", _mName); }
        }

        public IEnumerable<string> GroupedEvents
        {
            get
            {
                IEnumerable<string> gEvts = from week in CalenderWeeksInternal
                                            from day in week.Days
                                            from IEvent evt in day.Events
                                            where evt is Event
                                            where evt.Group != GroupID.Empty
                                            group evt by evt.BasicCode
                                            into groupedEvt
                                            select groupedEvt.Key;
                return gEvts.ToArray();
            }
        }

        public DateTime LastUpdated
        {
            get { return _mLastUpdated; }
        }

        public new string ToolTip
        {
            get { return String.Format("{0} Version: {1} vom {2:d}", _mName, _mVersion, _mLastUpdated); }
        }

        public string Version
        {
            get { return _mVersion; }
        }

        #endregion Properties

        #region Methods (2)

        // Public Methods (1) 

        public static implicit operator SeminarGroup(XElement element)
        {
            return new SeminarGroup
                       {
// ReSharper disable PossibleNullReferenceException
                           MBaseElement = element,
                           _mName = element.Attribute("name").Value,
                           _mVersion = element.Attribute("version").Value,
                           _mLastUpdated =
                               DateTime.Parse(PlanFile.CleanDateString(element.Attribute("lastupdate").Value))
// ReSharper restore PossibleNullReferenceException
                       };
        }

        // Private Methods (1) 

        private bool AlreadyHasHAWWeek(int week, int year)
        {
            return (from tInternalWeek in CalenderWeeksInternal let tAdd = (year > tInternalWeek.Year) ? Helper.WeekCount(tInternalWeek.Year) : 0 where tInternalWeek.Week == week + tAdd & (tAdd > 0 || tInternalWeek.Year == year) select tInternalWeek).Any();
        }

        #endregion Methods

        #region IIsCurrent Members

        public bool IsCurrent
        {
            get { return false; }
        }

        #endregion
    }
}