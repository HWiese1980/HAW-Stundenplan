using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Xml.Linq;
using HAW_Tool.HAW.Native;
using RedBranch.Hammock;
using SeveQsCustomControls;

namespace HAW_Tool.HAW.Depending
{
    public class Event : FreezableUIElement, IEvent, ISelectable
    {
        public event EventHandler TimeChanged;



        public override bool IsDirty
        {
            get { return (bool)GetValue(IsDirtyProperty); }
            set { SetValue(IsDirtyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsDirty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsDirtyProperty =
            DependencyProperty.Register("IsDirty", typeof(bool), typeof(Event), new UIPropertyMetadata(false));



        public void OnTimeChanged(EventArgs e)
        {
            var handler = TimeChanged;
            if (handler != null) handler(this, e);
        }

        public override string ToString()
        {
            return string.Format("Event {0}", Code);
        }

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsSelected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(Event), new UIPropertyMetadata(false));



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

        static object CoerceTime(DependencyObject d, object value)
        {
            var ts = (TimeSpan) value;
            var minutes = ts.Minutes;
            var minutesLeft = minutes%5;
            var tsub = new TimeSpan(0, 0, minutesLeft, 0);

            return ts.Subtract(tsub);
        }

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
            DependencyProperty.Register("Date", typeof(DateTime), typeof(Event), new UIPropertyMetadata(DateTime.MinValue, DateChanged));

        static void DateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (PlanFile.Instance.SelectedSeminarGroup == null) return;

            var oldDate = (DateTime) e.OldValue;
            var newDate = (DateTime) e.NewValue;

            var oldDay = PlanFile.Instance.SelectedSeminarGroup.GetDayByDate(oldDate);
            var newDay = PlanFile.Instance.SelectedSeminarGroup.GetDayByDate(newDate);

            var evt = (Event) d;

            oldDay.Events.Remove(evt);
            newDay.Events.Add(evt);

            evt.Day = newDay;
        }



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
            DependencyProperty.Register("From", typeof(TimeSpan), typeof(Event), new UIPropertyMetadata(default(TimeSpan), OnDepTimeChanged, CoerceTime));

        public TimeSpan Till
        {
            get { return (TimeSpan)GetValue(TillProperty); }
            set { SetValue(TillProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Till.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TillProperty =
            DependencyProperty.Register("Till", typeof(TimeSpan), typeof(Event), new UIPropertyMetadata(default(TimeSpan), OnDepTimeChanged, CoerceTime));


        private static void OnDepTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var evt = (Event) d;
            evt.OnTimeChanged(new EventArgs());
        }

        

        public Day Day
        {
            get { return (Day)GetValue(DayProperty); }
            set { SetValue(DayProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Day.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DayProperty =
            DependencyProperty.Register("Day", typeof(Day), typeof(Event), new UIPropertyMetadata(default(Day)));



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



        public bool TakesPlace
        {
            get { return (bool)GetValue(TakesPlaceProperty); }
            set { SetValue(TakesPlaceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TakesPlace.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TakesPlaceProperty =
            DependencyProperty.Register("TakesPlace", typeof(bool), typeof(Event), new UIPropertyMetadata(true));


        
    }
}
