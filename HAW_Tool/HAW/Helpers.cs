using System;
using System.Linq;
using System.Windows;
using LHelper = LittleHelpers.Helper;
using System.Globalization;
using DDayEvent = DDay.iCal.Event;
using HAW_Tool.HAW.REST;
using System.Xml.Linq;

namespace HAW_Tool.HAW
{
    public static class Helper
    {
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

        public static DateTime ParseYearWeekDayCode(string YWD)
        {
            if (YWD == null) throw new ArgumentNullException("YWD");

            string[] ywd = YWD.Split(':');
            int year = Convert.ToInt32(ywd[0]);
            int week = Convert.ToInt32(ywd[1]);
            string day = ywd[2];

            DateTime tDate = FirstDayOfFirstWeek(year).AddDays((week - 1) * 7).AddDays(WeekHelper.DOW[day]);

            return tDate;
        }

        public static bool IsWeekPast(int Year, int Week)
        {
            return (Helper.EndOfWeek(Week, Year) < DateTime.Now.AddDays(-28));
        }

        public static DateTime DayOfWeekToDateTime(int Year, int Week, int Day)
        {
            DateTime tFirst = StartOfWeek(Week, Year);
            tFirst = tFirst.AddDays(Day);
            return tFirst;
        }

        public static DateTime DayOfYearToDateTime(this int DayOfYear)
        {
            if (DayOfYear < 1 || DayOfYear > 365) throw new ArgumentOutOfRangeException("Value must be 1 to 365");
            DateTime start = new DateTime(DateTime.Now.Year, 1, 1).AddDays(-1);
            DateTime end = start.AddDays(DayOfYear);
            return end;
        }

        public static DateTime Earliest(params DateTime[] Dates)
        {
            var t = from p in Dates
                    orderby p ascending
                    select p;

            return t.First();
        }

        public static TimeSpan Earliest(params TimeSpan[] Times)
        {
            var t = from p in Times
                    orderby p ascending
                    select p;

            return t.First();
        }

        public static DateTime EndOfWeek(int Week, int Year)
        {
            DateTime tStartOfWeek = StartOfWeek(Week, Year);
            while (tStartOfWeek.DayOfWeek != DayOfWeek.Friday) tStartOfWeek = tStartOfWeek.AddDays(1);
            return tStartOfWeek;
        }

        public static DateTime FirstDayOfFirstWeek(int Year)
        {
            DateTime tStart = new DateTime(Year, 1, 4);
            while (tStart.DayOfWeek != DayOfWeek.Monday) tStart = tStart.AddDays(-1);
            return tStart;
        }

        public static bool IsBetween(this DateTime me, DateTime First, DateTime Last)
        {
            return (First <= me & me <= Last);
        }

        public static bool IsNumeric(this string What)
        {
            int iTemp = 0;
            return Int32.TryParse(What, out iTemp);
        }

        public static DateTime Latest(params DateTime[] Dates)
        {
            var t = from p in Dates
                    orderby p descending
                    select p;

            return t.First();
        }

        public static TimeSpan Latest(params TimeSpan[] Times)
        {
            var t = from p in Times
                    orderby p descending
                    select p;

            return t.First();
        }

        public static bool PeriodsOverlap(DateTime AStart, DateTime AEnd, DateTime BStart, DateTime BEnd)
        {
            bool bOverlap = false;
            bOverlap |= AStart.IsBetween(BStart, BEnd);
            bOverlap |= AEnd.IsBetween(BStart, BEnd);
            bOverlap |= BStart.IsBetween(AStart, AEnd);
            bOverlap |= BEnd.IsBetween(AStart, AEnd);

            return bOverlap;
        }

        public static DateTime StartOfWeek(int Week, int Year)
        {
            DateTime tFirstWeekStart = FirstDayOfFirstWeek(Year);
            for (int i = 1; i < Week; i++)
            {
                tFirstWeekStart = tFirstWeekStart.AddDays(1);
                while (tFirstWeekStart.DayOfWeek != DayOfWeek.Monday) tFirstWeekStart = tFirstWeekStart.AddDays(1);
            }
            return tFirstWeekStart;
        }

        public static int WeekCount(int Year)
        {
            DateTime firstJan = new DateTime(Year, 1, 1);
            DateTime lastDec = new DateTime(Year, 12, 31);
            return (firstJan.DayOfWeek == DayOfWeek.Thursday | lastDec.DayOfWeek == DayOfWeek.Thursday) ? 53 : 52;
        }

        public static int WeekOfDate(DateTime Date)
        {
            int tWeek = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(Date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            return tWeek;
        }

        #endregion Methods
    }
}
