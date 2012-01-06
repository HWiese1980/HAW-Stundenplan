using System;
using System.Collections.Generic;
using System.Linq;
using LittleHelpers;
using HAW_Tool.HAW.Native;
using Helper = HAW_Tool.HAW.Native.Helper;

namespace HAW_Tool.HAW.REST
{
    public class RESTWeek : IWeek
    {
        #region IWeek Members

        public IEnumerable<Day> Days
        {
            get
            {
                AnonymousComparer<RESTEvent> tDateCompare = new AnonymousComparer<RESTEvent>((a, b) => { return a.Date == b.Date; }, (a) => { return a.GetHashCode(); });

                var sEvents = (from p in PlanFile.Instance.StoredEvents
                               where p.SeminarGroup == this.SeminarGroup.FullName
                               let tEventWeek = Helper.WeekOfDate(p.Date)
                               let tAddWeek = (p.Date.Year > this.Year) ? Helper.WeekCount(this.Year) : 0
                               where (tEventWeek + tAddWeek) == this.Week
                               group (IEvent)p by p.DayOfWeek into q
                               select q);

                foreach (IGrouping<int, IEvent> grp in sEvents)
                {
                    Day tDay = Day.CreateDay(grp, this);
                    tDay.Week = this;

                    yield return tDay;
                }
            }
        }

        public SeminarGroup SeminarGroup { get; set; }

        public int Week { get; set; }
        public int Year { get; set; }

        #endregion

        #region IWeek Members

        public bool IsPast
        {
            get
            {
                // return Helper.IsWeekPast(this.Year, this.Week);
                return false;
            }
        }

        public bool IsCurrent
        {
            get
            {
                return (this.Week == Helper.WeekOfDate(DateTime.Now));
            }
        }

        public string Label
        {
            get
            {
                return String.Format("KW{0} ({1:D} - {2:D})", this.Week, Helper.StartOfWeek(this.Week, this.Year), Helper.EndOfWeek(this.Week, this.Year).AddDays(-2));
            }
        }

        #endregion

        #region IWeek Members


        public string LabelShort
        {
            get { return String.Format("KW{0}:{1}", this.Week, this.Year); }
        }

        #endregion
    }
}
