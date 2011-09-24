using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Windows;
using System.Globalization;
using HAW_Tool.HAW.REST;
using LittleHelpers;

namespace HAW_Tool.HAW
{
    public class SeminarGroup : XElementContainer, IIsCurrent
    {
		#region Fields (4) 

        DateTime m_LastUpdated;
        string m_Name;
        string m_Version;
        List<CalendarWeek> mWeeks;

		#endregion Fields 

		#region Properties (12) 

        private IEnumerable<IWeek> AdditionalWeeks
        {
            get
            {
                AnonymousComparer<RESTWeek> tCmp = new AnonymousComparer<RESTWeek>((x, y) => { return (x.Week == y.Week & x.Year == y.Year); }, (x) => { return String.Format("{0}:{1}", x.Week, x.Year).GetHashCode(); });

                var weeks = (from p in PlanFile.Instance.StoredEvents
                             where p.SeminarGroup == this.FullName
                             let tWeek = Helper.WeekOfDate(p.Date)
                             where !this.AlreadyHasHAWWeek(tWeek, p.Date.Year)
                             select new RESTWeek() { SeminarGroup = this, Week = tWeek, Year = p.Date.Year }).Distinct(tCmp).Cast<IWeek>();


                return weeks;
            }
        }

        public IEnumerable<IEvent> AllEvents
        {
            get
            {
                return from weeks in this.CalendarWeeks
                       from days in weeks.Days
                       from IEvent evt in days.Events
                       select evt;
            }
        }

        public IEnumerable<IWeek> CalendarWeeks
        {
            get
            {
                int currentWeek = Helper.WeekOfDate(DateTime.Now);



                var tCalWeeks = from p in this.CalenderWeeksInternal.Cast<IWeek>().Concat(this.AdditionalWeeks)
                                orderby p.Week
                                orderby p.Year
                                // where p.Year == DateTime.Now.Year
                                where !p.IsPast
                                select (IWeek)p;

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
                if (mWeeks == null)
                {
                    mWeeks = new List<CalendarWeek>(from p in MBaseElement.Elements("kw")
                                                    select new CalendarWeek(this, p));
                }
                return mWeeks;
            }
        }

        public IWeek Current
        {
            get
            {
                var tCurr = from p in this.CalendarWeeks
                            where Helper.StartOfWeek(p.Week, p.Year) <= DateTime.Now
                                & DateTime.Now <= Helper.EndOfWeek(p.Week, p.Year)
                            select p;

                if (tCurr.Count() <= 0) return null;

                return tCurr.Single();
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
            get
            {
                return String.Format("{0}", m_Name);
            }
        }

        public IEnumerable<string> GroupedEvents
        {
            get
            {
                var gEvts = from week in this.CalenderWeeksInternal
                       from day in week.Days
                       from Event evt in day.Events
                       where evt is Event
                            where evt.Group != GroupID.Empty
                       group evt by evt.BasicCode into groupedEvt
                       select groupedEvt.Key;
                return gEvts.ToArray();
            }
        }

        public DateTime LastUpdated { get { return m_LastUpdated; } }

        public new string ToolTip
        {
            get
            {
                return String.Format("{0} Version: {1} vom {2:d}", m_Name, m_Version, m_LastUpdated);
            }
        }

        public string Version { get { return m_Version; } }

		#endregion Properties 

		#region Methods (2) 

		// Public Methods (1) 

        public static implicit operator SeminarGroup(XElement element)
        {
            return new SeminarGroup()
            {
                MBaseElement = element,
                m_Name = element.Attribute("name").Value,
                m_Version = element.Attribute("version").Value,
                m_LastUpdated = DateTime.Parse(PlanFile.CleanDateString(element.Attribute("lastupdate").Value))
            };
        }
		// Private Methods (1) 

        private bool AlreadyHasHAWWeek(int Week, int Year)
        {
            foreach (CalendarWeek tInternalWeek in this.CalenderWeeksInternal)
            {
                int tAdd = (Year > tInternalWeek.Year) ? Helper.WeekCount(tInternalWeek.Year) : 0;
                if (tInternalWeek.Week == Week + tAdd & (tAdd > 0 || tInternalWeek.Year == Year)) return true;
            }
            return false;
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
