using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Windows.Threading;

namespace HAW_Tool.HAW.Native
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

        public CalendarWeek(SeminarGroup @group, XElement Base)
        {
            MBaseElement = Base;

            var xAttribute = Base.Attribute("number");
            if (xAttribute != null) _mNumber = Convert.ToInt32(xAttribute.Value);

            var attribute = Base.Attribute("year");
            if (attribute != null) _mYear = Convert.ToInt32(attribute.Value);

            SeminarGroup = @group;
        }

        public int Week { get { return _mNumber; } }
        public int Year { get { return _mYear; } }

        readonly int _mNumber;
        readonly int _mYear;

        public SeminarGroup SeminarGroup { get; set; }

        public bool IsCurrent
        {
            get
            {
                return (Week == Helper.WeekOfDate(DateTime.Now));
            }
        }

        public bool IsPast
        {
            get
            {
                return Helper.IsWeekPast(Year, Week);
            }
        }

        public string LabelShort
        {
            get
            {
                return String.Format("KW{0}:{1}", _mNumber, _mYear);
            }
        }

        List<Event> _mEvents;
        private IEnumerable<Event> EventInternal
        {
            get
            {
                if (_mEvents == null)
                {
                    Dispatcher.CurrentDispatcher.Invoke(new Action(() =>
                                                                       {
                                                                           _mEvents =
                                                                               new List<Event>(
                                                                                   from p in
                                                                                       MBaseElement.Elements("event")
                                                                                   select new Event(this, p));
                                                                       }));
                }

                return _mEvents;
            }
        }

        private IEnumerable<IGrouping<int, IEvent>> GroupedEvents
        {
            get
            {
                return from IEvent p in EventInternal
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
                var i = 0;
                for (; i < 6; i++)
                {
                    var tEvtGroups = GroupedEvents.Where(p => p.Key == i).ToList();
                    var count = tEvtGroups.Count();
                    var tDay = count == 1 ? Day.CreateDay(tEvtGroups.Single(), this) : Day.CreateDay(i, this);

                    yield return tDay;
                }
                //foreach (IGrouping<int, IEvent> tEventGroup in this.GroupedEvents)
                //{
                //    Day tDay = Day.CreateDay(tEventGroup, this);
                //    DateTime tDate = Helper.StartOfWeek(m_Number, m_Year).AddDays((int)tDay.DOW - 1);
                //    tDay.Date = tDate;
                //    yield return tDay;
                //}
            }
        }

        /*
                private Day GetDayFromToday(int daysAdded)
                {
                    DateTime tToday = DateTime.Now.Date;
                    DateTime tTomorrow = tToday.AddDays(daysAdded);

                    var tQuery = from p in this.Days
                                 where p.Date == tTomorrow.Date
                                 select p;

                    if (tQuery.Count() <= 0) return null;

                    Day tDay = tQuery.Single();
                    return tDay;
                }
        */

        /*
                public Day Today
                {
                    get
                    {
                        return GetDayFromToday(0);
                    }
                }
        */

        /*
                public Day Tomorrow
                {
                    get
                    {
                        return GetDayFromToday(1);
                    }
                }
        */

        public override string ToString()
        {
            return String.Format("KW{0} ({1:D} - {2:D})", _mNumber, Helper.StartOfWeek(_mNumber, _mYear), Helper.EndOfWeek(_mNumber, _mYear)/*.AddDays(-2)*/);
        }
    }
}
