using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Xml.Linq;
using HAW_Tool.HAW.Native;

namespace HAW_Tool.HAW.Depending
{
    public class Event : DependencyObject, IEvent
    {
/*
        public Event(IWeek kw, XElement baseElement)
        {
            var code = ParseCode(baseElement.Element("code").Value);

            Tutor = baseElement.Element("dozent").Value;
            Room = baseElement.Element("raum").Value;
            var tag = baseElement.Element("tag").Value;

            var tFrom = DateTime.Parse(baseElement.Element("von").Value).TimeOfDay;
            var tTill = DateTime.Parse(baseElement.Element("bis").Value).TimeOfDay;


            CalendarWeek = kw.Week;

            var tDOW = WeekHelper.DOW[tag.ToLower()];

            var tDT = Helper.DayOfWeekToDateTime(kw.Year, kw.Week, tDOW);
            Date = tDT;

            From = tFrom;
            Till = tTill;

        }*/

        private EventCode ParseCode(string xmlCode)
        {
            var tParts = xmlCode.Split('/');
            if (tParts.Count() > 1 && GroupID.IsValidGroup(tParts[1]))
            {
                return new EventCode { Code = tParts[0], Group = new GroupID(tParts[1]) };
            }
            return new EventCode { Code = xmlCode, Group = GroupID.Empty };
        }

        private byte[] ID
        {
            get { return Encoding.ASCII.GetBytes(Info); }
        }

        public string Info
        {
            get { return String.Format("{0};{1};{2}", Code, CalendarWeek, DayOfWeek); }
        }

        private void ReGenerateHash()
        {
            var tMD5 = MD5.Create();
            Hash = Convert.ToBase64String(tMD5.ComputeHash(ID), Base64FormattingOptions.InsertLineBreaks);
        }

        static void HashInvalidatingPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var ev = (Event) obj;
            ev.ReGenerateHash();
        }

        public int CalendarWeek
        {
            get { return (int)GetValue(CalendarWeekProperty); }
            set { SetValue(CalendarWeekProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CalendarWeek.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CalendarWeekProperty =
            DependencyProperty.Register("CalendarWeek", typeof(int), typeof(Event), new UIPropertyMetadata(0, HashInvalidatingPropertyChanged));



        public string Hash
        {
            get { return (string)GetValue(HashProperty); }
            private set { SetValue(HashProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Hash.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HashProperty =
            DependencyProperty.Register("Hash", typeof(string), typeof(Event), new UIPropertyMetadata(""));



        public string Code
        {
            get { return (string)GetValue(CodeProperty); }
            set { SetValue(CodeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Code.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CodeProperty =
            DependencyProperty.Register("Code", typeof(string), typeof(Event), new UIPropertyMetadata("", HashInvalidatingPropertyChanged));



        public string BasicCode
        {
            get { return (string)GetValue(BasicCodeProperty); }
            set { SetValue(BasicCodeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BasicCode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BasicCodeProperty =
            DependencyProperty.Register("BasicCode", typeof(string), typeof(Event), new UIPropertyMetadata(""));



        public string SeminarGroup
        {
            get { return (string)GetValue(SeminarGroupProperty); }
            set { SetValue(SeminarGroupProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SeminarGroup.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SeminarGroupProperty =
            DependencyProperty.Register("SeminarGroup", typeof(string), typeof(Event), new UIPropertyMetadata(""));



        public string Room
        {
            get { return (string)GetValue(RoomProperty); }
            set { SetValue(RoomProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Room.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RoomProperty =
            DependencyProperty.Register("Room", typeof(string), typeof(Event), new UIPropertyMetadata(""));




        public string Tutor
        {
            get { return (string)GetValue(TutorProperty); }
            set { SetValue(TutorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Tutor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TutorProperty =
            DependencyProperty.Register("Tutor", typeof(string), typeof(Event), new UIPropertyMetadata(""));



        public int Priority
        {
            get { return (int)GetValue(PriorityProperty); }
            set { SetValue(PriorityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Priority.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PriorityProperty =
            DependencyProperty.Register("Priority", typeof(int), typeof(Event), new UIPropertyMetadata(0));




        public int DayOfWeek
        {
            get { return (int)GetValue(DayOfWeekProperty); }
            set { SetValue(DayOfWeekProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DayOfWeek.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DayOfWeekProperty =
            DependencyProperty.Register("DayOfWeek", typeof(int), typeof(Event), new UIPropertyMetadata(0, HashInvalidatingPropertyChanged));



        public DateTime Date
        {
            get { return (DateTime)GetValue(DateProperty); }
            set { SetValue(DateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Date.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DateProperty =
            DependencyProperty.Register("Date", typeof(DateTime), typeof(Event), new UIPropertyMetadata(DateTime.MinValue));



        public IEnumerable<DateTime> OtherDates
        {
            get { return (IEnumerable<DateTime>)GetValue(OtherDatesProperty); }
            set { SetValue(OtherDatesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OtherDates.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OtherDatesProperty =
            DependencyProperty.Register("OtherDates", typeof(IEnumerable<DateTime>), typeof(Event), new UIPropertyMetadata(default(IEnumerable<DateTime>)));



        public TimeSpan From
        {
            get { return (TimeSpan)GetValue(FromProperty); }
            set { SetValue(FromProperty, value); }
        }

        // Using a DependencyProperty as the backing store for From.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FromProperty =
            DependencyProperty.Register("From", typeof(TimeSpan), typeof(Event), new UIPropertyMetadata(default(TimeSpan)));



        public TimeSpan Till
        {
            get { return (TimeSpan)GetValue(TillProperty); }
            set { SetValue(TillProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Till.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TillProperty =
            DependencyProperty.Register("Till", typeof(TimeSpan), typeof(Event), new UIPropertyMetadata(default(TimeSpan)));




        public Day Day
        {
            get { return (Day)GetValue(DayProperty); }
            set { SetValue(DayProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Day.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DayProperty =
            DependencyProperty.Register("Day", typeof(Day), typeof(Event), new UIPropertyMetadata(default(Day)));



        public int RowIndex
        {
            get { return (int)GetValue(RowIndexProperty); }
            set { SetValue(RowIndexProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RowIndex.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RowIndexProperty =
            DependencyProperty.Register("RowIndex", typeof(int), typeof(Event), new UIPropertyMetadata(0));



        public GroupID Group
        {
            get { return (GroupID)GetValue(GroupProperty); }
            set { SetValue(GroupProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Group.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GroupProperty =
            DependencyProperty.Register("Group", typeof(GroupID), typeof(Event), new UIPropertyMetadata(default(GroupID)));



        public bool IsEnabled
        {
            get { return (bool)GetValue(IsEnabledProperty); }
            set { SetValue(IsEnabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.Register("IsEnabled", typeof(bool), typeof(Event), new UIPropertyMetadata(true));




        public bool IsObligatory
        {
            get { return (bool)GetValue(IsObligatoryProperty); }
            set { SetValue(IsObligatoryProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsObligatory.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsObligatoryProperty =
            DependencyProperty.Register("IsObligatory", typeof(bool), typeof(Event), new UIPropertyMetadata(false));
        
    }
}
