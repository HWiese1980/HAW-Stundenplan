using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace HAW_Tool.HAW
{
    public class CalendarWeek : XElementContainer, IWeek
    {
        //         public static implicit operator CalendarWeek(XElement Element)
        //         {
        //             return new CalendarWeek()
        //             {
        //                 m_BaseElement = Element,
        //                 m_Number = Convert.ToInt32(Element.Attribute("number").Value),
        //                 m_Year = Convert.ToInt32(Element.Attribute("year").Value),
        //             };
        //         }

        public CalendarWeek(SeminarGroup Group, XElement Base)
        {
            m_BaseElement = Base;
            m_Number = Convert.ToInt32(Base.Attribute("number").Value);
            m_Year = Convert.ToInt32(Base.Attribute("year").Value);
            this.SeminarGroup = Group;
        }

        public int Week { get { return m_Number; } }
        public int Year { get { return m_Year; } }

        int m_Number = 0;
        int m_Year = 0;

        public SeminarGroup SeminarGroup { get; set; }

        public bool IsCurrent
        {
            get
            {
                return (this.Week == Helper.WeekOfDate(DateTime.Now));
            }
        }

        public bool IsPast
        {
            get
            {
                return Helper.IsWeekPast(this.Year, this.Week);
            }
        }

        public string LabelShort
        {
            get
            {
                return String.Format("KW{0}:{1}", m_Number, m_Year);
            }
        }

        List<Event> mEvents;
        private IEnumerable<Event> EventInternal
        {
            get
            {
                if (mEvents == null)
                {
                    mEvents = new List<Event>(from p in m_BaseElement.Elements("event")
                                              select new Event(this, p));

                }
                return mEvents;
            }
        }

        private IEnumerable<IGrouping<int, IEvent>> GroupedEvents
        {
            get
            {
                return from IEvent p in this.EventInternal
                       orderby p.Till
                       orderby p.From
                       orderby p.DayOfWeek
                       group p by p.DayOfWeek into q
                       select q;
            }
        }

        public IEnumerable<Day> Days
        {
            get
            {
                foreach (IGrouping<int, IEvent> tEventGroup in this.GroupedEvents)
                {
                    Day tDay = Day.CreateDay(tEventGroup, this);
                    DateTime tDate = Helper.StartOfWeek(m_Number, m_Year).AddDays((int)tDay.DOW - 1);
                    tDay.Date = tDate;
                    yield return tDay;
                }
            }
        }

        private Day GetDayFromToday(int DaysAdded)
        {
            DateTime tToday = DateTime.Now.Date;
            DateTime tTomorrow = tToday.AddDays(DaysAdded);

            var tQuery = from p in this.Days
                         where p.Date == tTomorrow.Date
                         select p;

            if (tQuery.Count() <= 0) return null;

            Day tDay = tQuery.Single();
            return tDay;
        }

        public Day Today
        {
            get
            {
                return GetDayFromToday(0);
            }
        }

        public Day Tomorrow
        {
            get
            {
                return GetDayFromToday(1);
            }
        }

        public override string ToString()
        {
            return String.Format("KW{0} ({1:D} - {2:D})", m_Number, Helper.StartOfWeek(m_Number, m_Year), Helper.EndOfWeek(m_Number, m_Year)/*.AddDays(-2)*/);
        }
    }
}
