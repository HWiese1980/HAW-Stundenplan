using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SeveQsCustomControls;

namespace HAW_Tool.HAW.Depending
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class CalendarWeek : NotifyingObject
    {
        public CalendarWeek(int week, int year)
        {
            Year = year;
            Week = week;

            Days = new ThreadSafeObservableCollection<Day>();
            Days.CollectionChanged += DaysCollectionChanged;
            InitializeDays();
        }

        private void DaysCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        foreach (var day in e.NewItems.Cast<Day>())
                        {
                            Day day1 = day;
                            day1.Week = this;
                        }
                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (var day in e.OldItems.Cast<Day>())
                        {
                            Day day1 = day;
                            day1.Week = default(CalendarWeek);
                        }
                        break;
                    }
            }
        }

        public bool IsCurrent
        {
            get
            {
                var a = HAWToolHelper.StartOfWeek(Week, Year);
                var b = HAWToolHelper.EndOfWeek(Week, Year);
                return (DateTime.Now.Date >= a & DateTime.Now <= b);
            }
        }

        public void InitializeDays()
        {
            var days = new Day[7];
            //Parallel.For(0, 7, i =>
            //                       {
            //                           var d = new Day
            //                                       {
            //                                           Date = HAWToolHelper.StartOfWeek(Week, Year).AddDays(i),
            //                                           DOW = (DayOfWeek) i
            //                                       };
            //                           days[i] = d;
            //                       });

            var startDate = HAWToolHelper.StartOfWeek(Week, Year);
            for (int i = 0; i < 7; i++)
            {
                var d = new Day
                {
                    Date = startDate.AddDays(i),
                    DOW = (DayOfWeek)i
                };
                days[i] = d;
            }
            Days.AddItems(days);
        }

        private int _year;
        [JsonProperty]
        public int Year
        {
            get
            {
                return _year;
            }
            set
            {
                _year = value;
                OnPropertyChanged("Year");
            }
        }
                        
                        

//         [JsonProperty]
//         public int Week
//         {
//             get { return (int)GetValue(WeekProperty); }
//             set { SetValue(WeekProperty, value); }
//         }
// 
//         // Using a DependencyProperty as the backing store for Week.  This enables animation, styling, binding, etc...
//         public static readonly DependencyProperty WeekProperty =
//             DependencyProperty.Register("Week", typeof(int), typeof(CalendarWeek), new UIPropertyMetadata(0));

        private int _week;
        [JsonProperty]
        public int Week
        {
            get
            {
                return _week;
            }
            set
            {
                _week = value;
                OnPropertyChanged("Week");
            }
        }
                        
                        

//         public SeminarGroup SeminarGroup
//         {
//             get { return (SeminarGroup)GetValue(SeminarGroupProperty); }
//             set { SetValue(SeminarGroupProperty, value); }
//         }
// 
//         // Using a DependencyProperty as the backing store for SeminarGroup.  This enables animation, styling, binding, etc...
//         public static readonly DependencyProperty SeminarGroupProperty =
//             DependencyProperty.Register("SeminarGroup", typeof(SeminarGroup), typeof(CalendarWeek), new UIPropertyMetadata(default(SeminarGroup)));


        private SeminarGroup _semGrp;
        public SeminarGroup SeminarGroup
        {
            get
            {
                return _semGrp;
            }
            set
            {
                _semGrp = value;
                OnPropertyChanged("SeminarGroup");
            }
        }

        public ThreadSafeObservableCollection<Day> Days { get; set; }

        public string LabelShort
        {
            get { return string.Format("KW-{0}", Week); }
        }

        public string Label
        {
            get { return string.Format("Woche {0} vom {1} bis {2}", Week, HAWToolHelper.StartOfWeek(this.Week, this.Year).ToShortDateString(), HAWToolHelper.EndOfWeek(this.Week, this.Year).ToShortDateString()); }
        }

        public DateTime GetDateOfWeekday(int day)
        {
            return HAWToolHelper.StartOfWeek(Week, Year).AddDays(day);
        }

        public string WeekSort { get { return string.Format("{0}.{1}", Year, Week); } }

        public override string ToString()
        {
            return string.Format("[{0}:{1}]", Week, Year);
        }
    }
}
