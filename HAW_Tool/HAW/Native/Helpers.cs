using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using LHelper = LittleHelpers.Helper;
using System.Globalization;
using DDayEvent = DDay.iCal.Event;
using HAW_Tool.HAW.REST;
using System.Xml.Linq;

namespace HAW_Tool.HAW.Native
{
    public static class Helper
    {
        public static readonly Dictionary<string,int> DaysOfWeek = new Dictionary<string, int>();
        static Helper()
        {
            DaysOfWeek.Add("Mo", 1);
            DaysOfWeek.Add("Di", 2);
            DaysOfWeek.Add("Mi", 3);
            DaysOfWeek.Add("Do", 4);
            DaysOfWeek.Add("Fr", 5);
            DaysOfWeek.Add("Sa", 6);
            DaysOfWeek.Add("So", 0);
        }
        #region Properties (1)

        public static double MaxDayWidth
        {
            get
            {
                if (LHelper.IsInDesignModeStatic) return 500.0D;

                var findResource = Application.Current.FindResource("RasterH");
                if (findResource != null)
                {
                    double rasterH = ((double)findResource * 24.0D) - 1.0D;
                    return rasterH;
                }
                throw new Exception("No RasterH defined. Need!");
            }
        }

        #endregion Properties

        #region Methods (15)

        // Public Methods (15) 

        public static bool CheckElements(XElement elm, params string[] elementNames)
        {
            return elementNames.Select(elementName => elm.Element(elementName)).All(subelement => subelement != null);
        }

        public static DDay.iCal.Event AsDDayEvent(this IEvent Event)
        {
            var tEvt = new DDayEvent
                           {
                               DTStart = new DDay.iCal.iCalDateTime(Event.Date + Event.From),
                               DTEnd = new DDay.iCal.iCalDateTime(Event.Date + Event.Till)
                           };

            if (Event is RESTEvent)
                tEvt.Summary = String.Format("{0} {1}", ((RESTEvent)Event).TypeCode, Event.Code);
            else if (Event is Event)
            {
                string evtCode = (Event.Group != GroupID.Empty) ? string.Format("{0} Gruppe: {1}", Event.BasicCode, Event.Group) : Event.BasicCode;
                tEvt.Summary = evtCode;
            }

            tEvt.Location = String.Format("Raum {0}", Event.Room);
            tEvt.Priority = Event.Priority;
            tEvt.Description = String.Format("Raum {0} - Dozent: {1}", Event.Room, Event.Tutor);
            return tEvt;
        }

        public static DateTime ParseYearWeekDayCode(string ywd)
        {
            if (ywd == null) throw new ArgumentNullException("ywd");

            var tYWD = ywd.Split(':');
            var year = Convert.ToInt32(tYWD[0]);
            var week = Convert.ToInt32(tYWD[1]);
            var day = tYWD[2];

            var tDate = FirstDayOfFirstWeek(year).AddDays((week - 1) * 7).AddDays(WeekHelper.DOW[day]);

            return tDate;
        }

        public static bool IsWeekPast(int year, int week)
        {
            return (EndOfWeek(week, year) < DateTime.Now.AddDays(-28));
        }

        public static DateTime DayOfWeekToDateTime(int year, int week, int day)
        {
            DateTime tFirst = StartOfWeek(week, year);
            tFirst = tFirst.AddDays(day);
            return tFirst;
        }

        public static DateTime DayOfYearToDateTime(this int dayOfYear)
        {
            if (dayOfYear < 1 || dayOfYear > 365) throw new ArgumentOutOfRangeException("dayOfYear");
            DateTime start = new DateTime(DateTime.Now.Year, 1, 1).AddDays(-1);
            DateTime end = start.AddDays(dayOfYear);
            return end;
        }

        public static DateTime Earliest(params DateTime[] dates)
        {
            var t = from p in dates
                    orderby p ascending
                    select p;

            return t.First();
        }

        public static TimeSpan Earliest(params TimeSpan[] times)
        {
            var t = from p in times
                    orderby p ascending
                    select p;

            return t.First();
        }

        public static DateTime EndOfWeek(int week, int year)
        {
            var tStartOfWeek = StartOfWeek(week, year);
            while (tStartOfWeek.DayOfWeek != DayOfWeek.Friday) tStartOfWeek = tStartOfWeek.AddDays(1);
            return tStartOfWeek;
        }

        private static DateTime FirstDayOfFirstWeek(int year)
        {
            var tStart = new DateTime(year, 1, 4);
            while (tStart.DayOfWeek != DayOfWeek.Monday) tStart = tStart.AddDays(-1);
            return tStart;
        }

        public static bool IsBetween(this DateTime me, DateTime first, DateTime last)
        {
            return (first <= me & me <= last);
        }

        public static bool IsNumeric(this string what)
        {
            int iTemp;
            return Int32.TryParse(what, out iTemp);
        }

        public static DateTime Latest(params DateTime[] dates)
        {
            var t = from p in dates
                    orderby p descending
                    select p;

            return t.First();
        }

        public static TimeSpan Latest(params TimeSpan[] times)
        {
            var t = from p in times
                    orderby p descending
                    select p;

            return t.First();
        }

        public static bool PeriodsOverlap(DateTime aStart, DateTime aEnd, DateTime bStart, DateTime bEnd)
        {
            var bOverlap = false;
            bOverlap |= aStart.IsBetween(bStart, bEnd);
            bOverlap |= aEnd.IsBetween(bStart, bEnd);
            bOverlap |= bStart.IsBetween(aStart, aEnd);
            bOverlap |= bEnd.IsBetween(aStart, aEnd);

            return bOverlap;
        }

        public static DateTime StartOfWeek(int week, int year)
        {
            DateTime tFirstWeekStart = FirstDayOfFirstWeek(year);
            for (var i = 1; i < week; i++)
            {
                tFirstWeekStart = tFirstWeekStart.AddDays(1);
                while (tFirstWeekStart.DayOfWeek != DayOfWeek.Monday) tFirstWeekStart = tFirstWeekStart.AddDays(1);
            }
            return tFirstWeekStart;
        }

        public static int WeekCount(int year)
        {
            var firstJan = new DateTime(year, 1, 1);
            var lastDec = new DateTime(year, 12, 31);
            return (firstJan.DayOfWeek == DayOfWeek.Thursday | lastDec.DayOfWeek == DayOfWeek.Thursday) ? 53 : 52;
        }

        public static int WeekOfDate(DateTime date)
        {
            int tWeek = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            return tWeek;
        }

        #endregion Methods
    }
}
