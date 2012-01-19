using System;
using System.Linq;
using Newtonsoft.Json;
using SeveQsCustomControls;

namespace HAW_Tool.HAW.Depending
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class Event : EventControlBase, IEvent, ISelectable
    {
        public event EventHandler TimeChanged;

        public void OnTimeChanged(EventArgs e = null)
        {
            var handler = TimeChanged;
            if (handler != null) handler(this, e);
        }

        public override string ToString()
        {
            return string.Format("Event {0} {1}", Code, Source);
        }

        private void ParseCode(string code)
        {
            var tParts = code.Split('/');
            if (tParts.Count() > 1 && GroupID.IsValidGroup(tParts[1]))
            {
                Group = new GroupID(tParts[1].Trim());
            }
            ShortCode = tParts[0];
        }

        private string _loadedHashInfo = String.Empty;

        [JsonProperty]
        public override string HashInfo
        {
            get
            {
                return (Source == EventSource.CouchDB
                               ? _loadedHashInfo
                               : String.Format("{0};{1};{2};{3}", 
                               GetOriginalValue<string>("Code"),
                               GetOriginalValue<string>("Room"),
                               GetOriginalValue<Int32>("CalendarWeek"),
                               GetOriginalValue<Int32>("DayOfWeek"))
                               );
            }
            set
            {
                _loadedHashInfo = value;
            }
        }

        public override void CleanUp()
        {
            if(Source != EventSource.CouchDB)
            {
                HashInfo = String.Empty;
            }
            base.CleanUp();
        }

        private int _calWeek;

        [JsonProperty]
        public int CalendarWeek
        {
            get
            {
                return _calWeek;
            }
            set
            {
                _calWeek = value;
                OnPropertyChanged("CalendarWeek");
            }
        }
        
/*
        static object CoerceTime(DependencyObject d, object value)
        {
            var ts = (TimeSpan)value;
            var minutes = ts.Minutes;
            var minutesLeft = minutes % 5;
            var newTime = new TimeSpan(0, ts.Hours, ts.Minutes - minutesLeft, 0, 0);

            return newTime;
        }
*/

        private string _code;
        [JsonProperty]
        public string Code
        {
            get
            {
                return _code;
            }
            set
            {
                _code = value;
                OnPropertyChanged("Code");
                ParseCode(_code);
            }
        }

        private string _shortCode;
        public string ShortCode
        {
            get
            {
                return _shortCode;
            }
            set
            {
                _shortCode = value;
                OnPropertyChanged("ShortCode");
            }
        }

        private SeminarGroup _semGrp;
        [JsonProperty]
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

        private EventSource _source;
        public EventSource Source
        {
            get
            {
                return _source;
            }
            set
            {
                _source = value;
                OnPropertyChanged("Source");
            }
        }

        private string _room;
        [JsonProperty]
        public string Room
        {
            get
            {
                return _room;
            }
            set
            {
                _room = value;
                OnPropertyChanged("Room");
            }
        }

        private string _tutor;
        [JsonProperty]
        public string Tutor
        {
            get
            {
                return _tutor;
            }
            set
            {
                _tutor = value;
                OnPropertyChanged("Tutor");
            }
        }

        private int _dow;
        public int DayOfWeek
        {
            get
            {
                return _dow;
            }
            set
            {
                _dow = value;
                OnPropertyChanged("DayOfWeek");
            }
        }

        private DateTime _date;
        [JsonProperty]
        public DateTime Date
        {
            get
            {
                return _date;
            }
            set
            {
                _date = value;
                OnPropertyChanged("Date");
            }
        }

        private TimeSpan _from;
        [JsonProperty]
        public TimeSpan From
        {
            get
            {
                return _from;
            }
            set
            {
                _from = value;
                OnPropertyChanged("From");
                OnTimeChanged();
            }
        }


        private TimeSpan _till;
        [JsonProperty]
        public TimeSpan Till
        {
            get
            {
                return _till;
            }
            set
            {
                _till = value;
                OnPropertyChanged("Till");
                OnTimeChanged();
            }
        }

        private Day _day;
        public Day Day
        {
            get
            {
                return _day;
            }
            set
            {
                _day = value;
                DayChanged(_day);
                OnPropertyChanged("Day");
            }
        }

        private void DayChanged(Day newDay)
        {
            if (newDay == null) return;
            DayOfWeek = (int)newDay.DOW;
        }

        private GroupID _group;
        public GroupID Group
        {
            get
            {
                return _group;
            }
            set
            {
                _group = value;
                OnPropertyChanged("Group");
            }
        }

        private bool _isObligatory;
        [JsonProperty]
        public bool IsObligatory
        {
            get
            {
                return _isObligatory;
            }
            set
            {
                _isObligatory = value;
                OnPropertyChanged("IsObligatory");
            }
        }

        private bool _takesPlace = true;
        [JsonProperty]
        public bool TakesPlace
        {
            get
            {
                return _takesPlace;
            }
            set
            {
                _takesPlace = value;
                OnPropertyChanged("TakesPlace");
            }
        }

        public override void Reset()
        {
            base.Reset();
            IsReplaced = false;
        }

        private bool _isReplaced;
        public bool IsReplaced
        {
            get
            {
                return _isReplaced;
            }
            set
            {
                if (_isReplaced != value)
                {
                    _isReplaced = value;
                    Console.WriteLine(@"{0} wurde ersetzt: {1}", HashInfo, value);
                    OnPropertyChanged("IsReplaced");
                }
            }
        }
                        
        protected override void OnGotDirty()
        {
            if(Source == EventSource.CouchDB)
            {
                var originalEvent = PlanFile.Instance.GetEventByHashInfo(HashInfo);
                if(originalEvent != null)
                {
                    originalEvent.IsDirty = true;
                }
            }
        }

        internal void Save()
        {
            var s = PlanFile.Instance.CouchConnection.CreateSession("haw_events");

            var cdbi = new CouchDBEventInfo {Event = this, EventInfoHash = HashInfo, TimeStamp = DateTime.Now};

            s.Save(cdbi);
            IsDirty = false;
        }
    }
}
