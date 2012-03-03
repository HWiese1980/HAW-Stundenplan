using System;
using System.Linq;
using System.Windows;
using SeveQsCustomControls;

namespace HAW_Tool.HAW.Depending
{
    //[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class SeminarGroup : NotifyingObject, IKeyedObject
    {
        public static readonly DependencyProperty CurrentSeminarGroupProperty =
            DependencyProperty.RegisterAttached("CurrentSeminarGroup", typeof (SeminarGroup), typeof (SeminarGroup), new PropertyMetadata(default(SeminarGroup)));

        public static void SetCurrentSeminarGroup(DependencyObject element, SeminarGroup value)
        {
            element.SetValue(CurrentSeminarGroupProperty, value);
        }

        public static SeminarGroup GetCurrentSeminarGroup(DependencyObject element)
        {
            return (SeminarGroup) element.GetValue(CurrentSeminarGroupProperty);
        }

        public SeminarGroup()
        {
            CalendarWeeks = new ThreadSafeObservableCollection<CalendarWeek>();
            CalendarWeeks.CollectionChanged += (x, y) =>
                                                   {
                                                       foreach (var cw in y.NewItems.Cast<CalendarWeek>())
                                                           cw.SeminarGroup = this;
                                                   };
        }

        private string _name = "";

        //[JsonProperty]
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        //public string Name
        //{
        //    get { return (string)GetValue(NameProperty); }
        //    set { SetValue(NameProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for Name.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty NameProperty =
        //    DependencyProperty.Register("Name", typeof(string), typeof(SeminarGroup), new UIPropertyMetadata(""));

        private string _version = "";
        public string Version
        {
            get { return _version; }
            set
            {
                _version = value;
                OnPropertyChanged("Version");
            }
        }

        //public string Version
        //{
        //    get { return (string)GetValue(VersionProperty); }
        //    set { SetValue(VersionProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for Version.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty VersionProperty =
        //    DependencyProperty.Register("Version", typeof(string), typeof(SeminarGroup), new UIPropertyMetadata(""));

        private DateTime _lastUpdated;
        public DateTime LastUpdated
        {
            get { return _lastUpdated; }
            set
            {
                _lastUpdated = value;
                OnPropertyChanged("LastUpdated");
            }
        }

        //public DateTime LastUpdated
        //{
        //    get { return (DateTime)GetValue(LastUpdatedProperty); }
        //    set { SetValue(LastUpdatedProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for LastUpdated.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty LastUpdatedProperty =
        //    DependencyProperty.Register("LastUpdated", typeof(DateTime), typeof(SeminarGroup), new UIPropertyMetadata(DateTime.MinValue));

        public ThreadSafeObservableCollection<CalendarWeek> CalendarWeeks { get; set; }

        public Day GetDayByDate(DateTime date)
        {
            var day = (from cw in CalendarWeeks
                       from dy in cw.Days
                       where dy.Date.Date == date.Date
                       select dy).FirstOrDefault();
            return day;
        }

        #region Implementation of IKeyedObject

        public string Key
        {
            get { return Name; }
        }

        #endregion

        public override string ToString()
        {
            return Name;
        }
    }
}
