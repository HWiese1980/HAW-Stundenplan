using System;
using System.Linq;
using System.Windows;
using Newtonsoft.Json;
using SeveQsCustomControls;

namespace HAW_Tool.HAW.Depending
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class Event : DirtyableUIElement, IEvent, ISelectable
    {
        public event EventHandler TimeChanged;

        public void OnTimeChanged(EventArgs e)
        {
            var handler = TimeChanged;
            if (handler != null) handler(this, e);
        }

        public override string ToString()
        {
            return string.Format("Event {0} {1}", Code, Source);
        }

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsSelected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(Event), new UIPropertyMetadata(false));

        private void ParseCode(string Code)
        {
            var tParts = Code.Split('/');
            if (tParts.Count() > 1 && GroupID.IsValidGroup(tParts[1]))
            {
                Group = new GroupID(tParts[1]);
            }
            ShortCode = tParts[0];
        }

        private string _loadedHashInfo = String.Empty;

        [JsonProperty]
        public override string HashInfo
        {
            get
            {
                return (
                           !String.IsNullOrEmpty(_loadedHashInfo)
                               ? _loadedHashInfo
                               : String.Format("{0};{1};{2};{3}", Code, Room, CalendarWeek, DayOfWeek)
                               );
            }
            set
            {
                _loadedHashInfo = value;
            }
        }

        static void HashInvalidatingPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var ev = (Event)obj;
            ev.ReGenerateHash();
        }

        [JsonProperty]
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
            var ts = (TimeSpan)value;
            var minutes = ts.Minutes;
            var minutesLeft = minutes % 5;
            var newTime = new TimeSpan(0, ts.Hours, ts.Minutes - minutesLeft, 0, 0);

            return newTime;
        }

        [JsonProperty]
        public string Code
        {
            get { return (string)GetValue(CodeProperty); }
            set { SetValue(CodeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Code.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CodeProperty =
            DependencyProperty.Register("Code", typeof(string), typeof(Event), new UIPropertyMetadata("CODE", CodeChanged));

        private static void CodeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HashInvalidatingPropertyChanged(d, e);

            var evt = (Event) d;
            evt.ParseCode((string) e.NewValue);
        }



        public string ShortCode
        {
            get { return (string)GetValue(ShortCodeProperty); }
            set { SetValue(ShortCodeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShortCode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShortCodeProperty =
            DependencyProperty.Register("ShortCode", typeof (string), typeof (Event), new UIPropertyMetadata("SCODE"));



        // Using a DependencyProperty as the backing store for BasicCode.  This enables animation, styling, binding, etc...


        [JsonProperty]
        public SeminarGroup SeminarGroup
        {
            get { return (SeminarGroup)GetValue(SeminarGroupProperty); }
            set { SetValue(SeminarGroupProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SeminarGroup.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SeminarGroupProperty =
            DependencyProperty.Register("SeminarGroup", typeof (SeminarGroup), typeof (Event),
                                        new UIPropertyMetadata(default(SeminarGroup)));



        public EventSource Source
        {
            get { return (EventSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(EventSource), typeof(Event), new UIPropertyMetadata(EventSource.School));

        [JsonProperty]
        public string Room
        {
            get { return (string)GetValue(RoomProperty); }
            set { SetValue(RoomProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Room.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RoomProperty =
            DependencyProperty.Register("Room", typeof(string), typeof(Event), new UIPropertyMetadata("ROOM"));


        [JsonProperty]
        public string Tutor
        {
            get { return (string)GetValue(TutorProperty); }
            set { SetValue(TutorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Tutor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TutorProperty =
            DependencyProperty.Register("Tutor", typeof(string), typeof(Event), new UIPropertyMetadata("[TUT]"));

        // Using a DependencyProperty as the backing store for Priority.  This enables animation, styling, binding, etc...

        public int DayOfWeek
        {
            get { return (int)GetValue(DayOfWeekProperty); }
            set { SetValue(DayOfWeekProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DayOfWeek.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DayOfWeekProperty =
            DependencyProperty.Register("DayOfWeek", typeof(int), typeof(Event), new UIPropertyMetadata(0, HashInvalidatingPropertyChanged));

        [JsonProperty]
        public DateTime Date
        {
            get { return (DateTime)GetValue(DateProperty); }
            set { SetValue(DateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Date.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DateProperty =
            DependencyProperty.Register("Date", typeof(DateTime), typeof(Event), new UIPropertyMetadata(DateTime.MinValue));


        // Using a DependencyProperty as the backing store for OtherDates.  This enables animation, styling, binding, etc...


        [JsonProperty]
        public TimeSpan From
        {
            get { return (TimeSpan)GetValue(FromProperty); }
            set { SetValue(FromProperty, value); }
        }

        // Using a DependencyProperty as the backing store for From.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FromProperty =
            DependencyProperty.Register("From", typeof(TimeSpan), typeof(Event), new UIPropertyMetadata(TimeSpan.Parse("08:10"), OnDepTimeChanged, CoerceTime));

        [JsonProperty]
        public TimeSpan Till
        {
            get { return (TimeSpan)GetValue(TillProperty); }
            set { SetValue(TillProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Till.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TillProperty =
            DependencyProperty.Register("Till", typeof(TimeSpan), typeof(Event), new UIPropertyMetadata(TimeSpan.Parse("11:25"), OnDepTimeChanged, CoerceTime));


        private static void OnDepTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var evt = (Event)d;
            evt.OnTimeChanged(new EventArgs());
        }



        public Day Day
        {
            get { return (Day)GetValue(DayProperty); }
            set { SetValue(DayProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Day.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DayProperty =
            DependencyProperty.Register("Day", typeof(Day), typeof(Event), new UIPropertyMetadata(default(Day), DayChanged));

        private static void DayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(e.NewValue == null) return;
            var evt = (Event) d;
            var day = (Day) e.NewValue;
            evt.DayOfWeek = (int)day.DOW;
        }

        public GroupID Group
        {
            get { return (GroupID)GetValue(GroupProperty); }
            set { SetValue(GroupProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Group.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GroupProperty =
            DependencyProperty.Register("Group", typeof(GroupID), typeof(Event), new UIPropertyMetadata(null));

        [JsonProperty]
        public bool IsObligatory
        {
            get { return (bool)GetValue(IsObligatoryProperty); }
            set { SetValue(IsObligatoryProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsObligatory.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsObligatoryProperty =
            DependencyProperty.Register("IsObligatory", typeof(bool), typeof(Event), new UIPropertyMetadata(false));

        [JsonProperty]
        public bool TakesPlace
        {
            get { return (bool)GetValue(TakesPlaceProperty); }
            set { SetValue(TakesPlaceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TakesPlace.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TakesPlaceProperty =
            DependencyProperty.Register("TakesPlace", typeof(bool), typeof(Event), new UIPropertyMetadata(true));

        internal void Save()
        {
            var s = PlanFile.Instance.CouchConnection.CreateSession("haw_events");

            var cdbi = new CouchDBEventInfo();
            cdbi.Event = this;
            cdbi.EventInfoHash = HashInfo;
            cdbi.TimeStamp = DateTime.Now;

            s.Save(cdbi);
            Reset();
        }
    }
}
