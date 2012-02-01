#region Usings

using System;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using SeveQsCustomControls;

#endregion

namespace HAW_Tool.HAW.Depending
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [BsonIgnoreExtraElements]
    public class Event : EventControlBase, IEvent, ISelectable, IPriorized
    {
        private int _calWeek;
        private string _code;
        private DateTime _date;
        private Day _day;
        private int _dow;
        private TimeSpan _from;
        private GroupID _group;
        private bool _isObligatory;
        private bool _isReplaced;
        private string _loadedHashInfo = String.Empty;
        private string _room;
        private SeminarGroup _semGrp;
        private string _shortCode;
        private EventSource _source = EventSource.School;
        private bool _takesPlace = true;
        private TimeSpan _till;
        private string _tutor;

        [JsonProperty]
        [BsonElement("hashinfo")]
        public override string HashInfo
        {
            get
            {
                return ((Source == EventSource.CouchDB) | (Source == EventSource.MongoDB)
                            ? _loadedHashInfo
                            : String.Format("{0};{1};{2};{3};{4};{5}",
                                            GetOriginalValue<string>("Code"),
                                            GetOriginalValue<string>("Room"),
                                            GetOriginalValue<Int32>("CalendarWeek"),
                                            GetOriginalValue<Int32>("DayOfWeek"),
                                            GetOriginalValue<TimeSpan>("From"),
                                            GetOriginalValue<TimeSpan>("Till"))
                       );
            }
            set { _loadedHashInfo = value; }
        }

        [JsonProperty]
        [BsonElement("calendarweek")]
        public int CalendarWeek
        {
            get { return _calWeek; }
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

        [JsonProperty]
        [BsonElement("code")]
        public string Code
        {
            get { return _code; }
            set
            {
                _code = value;
                OnPropertyChanged("Code");
                ParseCode(_code);
            }
        }

        [BsonIgnore]
        public string ShortCode
        {
            get { return _shortCode; }
            set
            {
                _shortCode = value;
                OnPropertyChanged("ShortCode");
            }
        }

        [JsonProperty]
        [BsonElement("seminargroupname")]
        public string SeminarGroupName
        {
            get { return SeminarGroup.Name; }
            set { SeminarGroup = PlanFile.Instance.SeminarGroups.Where(grp => grp.Name == value).FirstOrDefault(); }
        }

        [BsonIgnore]
        public SeminarGroup SeminarGroup
        {
            get { return _semGrp; }
            set
            {
                _semGrp = value;
                SetDayByDate();
                OnPropertyChanged("SeminarGroup");
            }
        }

        [BsonIgnore]
        public EventSource Source
        {
            get { return _source; }
            set
            {
                _source = value;
                OnPropertyChanged("Source");
            }
        }

        [JsonProperty]
        [BsonElement("room")]
        public string Room
        {
            get { return _room; }
            set
            {
                _room = value;
                OnPropertyChanged("Room");
            }
        }

        [JsonProperty]
        [BsonElement("tutor")]
        public string Tutor
        {
            get { return _tutor; }
            set
            {
                _tutor = value;
                OnPropertyChanged("Tutor");
            }
        }

        [BsonIgnore]
        public int DayOfWeek
        {
            get { return _dow; }
            private set
            {
                if (_dow == value) return;
                _dow = value;
                OnPropertyChanged("DayOfWeek");
            }
        }

        [JsonProperty]
        [BsonElement("date")]
        [BsonDateTimeOptions(DateOnly = true)]
        public DateTime Date
        {
            get { return _date; }
            set
            {
                if(PlanFile.Instance.Logging) Console.WriteLine("[{0}] Date {1} -> {2}", this, _date, value);
                
                _date = value;
                SetDayByDate();
                OnPropertyChanged("Date");
            }
        }

        [BsonIgnore]
        public Day Day
        {
            get { return _day; }
            private set
            {
                if (_day != null && _day.Date.Date == value.Date.Date) return;
                _day = value;
                OnPropertyChanged("Day");
            }
        }

        [BsonIgnore]
        public GroupID Group
        {
            get { return _group; }
            set
            {
                _group = value;
                OnPropertyChanged("Group");
            }
        }

        [JsonProperty]
        [BsonElement("isobligatory")]
        public bool IsObligatory
        {
            get { return _isObligatory; }
            set
            {
                _isObligatory = value;
                OnPropertyChanged("IsObligatory");
            }
        }

        [JsonProperty]
        [BsonElement("takesplace")]
        public bool TakesPlace
        {
            get { return _takesPlace; }
            set
            {
                _takesPlace = value;
                OnPropertyChanged("TakesPlace");
            }
        }

        [BsonIgnore]
        public bool IsReplaced
        {
            get { return _isReplaced; }
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

        #region IEvent Members

        [JsonProperty]
        public TimeSpan From
        {
            get { return _from; }
            set
            {
                _from = value;
                OnPropertyChanged("From");
                OnTimeChanged();
            }
        }

        [JsonProperty]
        public TimeSpan Till
        {
            get { return _till; }
            set
            {
                _till = value;
                OnPropertyChanged("Till");
                OnTimeChanged();
            }
        }

        #endregion

        public void SetDayByDate()
        {
            if (IsInitializing || SeminarGroup == null) return;

            var oldDay = Day;

            var newDay = SeminarGroup.GetDayByDate(Date);
            if (newDay == null || oldDay == newDay) return;

            if(PlanFile.Instance.Logging)
            {
                Console.WriteLine("{0} moving from {1} to Date {2} ({3} -> {4})", this, (oldDay != null) ? oldDay.Date.ToShortDateString() : "(null)", _date, oldDay, newDay);
            }

            if (oldDay != null) oldDay.Events.Remove(this);
            newDay.Events.Add(this);

            Day = newDay;
            DayOfWeek = (int)Day.DOW;
        }

        public event EventHandler TimeChanged;

        public void OnTimeChanged(EventArgs e = null)
        {
            var handler = TimeChanged;
            if (handler != null) handler(this, e);
        }

        public override string ToString()
        {
            return string.Format("Event {0} {1} from {2} till {3}", Code, Source, From, Till);
        }

        public int CompareTo(object obj)
        {
            var other = (Event) obj;
            return HashInfo.CompareTo(other.HashInfo);
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

        public override void CleanUp()
        {
            if ((Source != EventSource.CouchDB) && (Source != EventSource.MongoDB))
            {
                HashInfo = String.Empty;
            }
            base.CleanUp();
        }

        public override void Reset()
        {
            base.Reset();
            IsReplaced = false;
        }

        protected override void OnGotDirty()
        {
            if (Source == EventSource.CouchDB || Source == EventSource.MongoDB)
            {
                var originalEvent = PlanFile.Instance.GetEventByHashInfo(HashInfo);
                if (originalEvent != null)
                {
                    originalEvent.IsDirty = true;
                }
            }
        }


        /*
        internal void Save()
        {
            Session s = PlanFile.Instance.CouchConnection.CreateSession("haw_events");

            var cdbi = new CouchDBEventInfo { Event = this, EventInfoHash = HashInfo, TimeStamp = DateTime.Now };

            s.Save(cdbi);
            IsDirty = false;
        }
         * */

        internal void Save()
        {
            var db = PlanFile.Instance.MongoConnection.GetDatabase("HAWEvents");
            var coll = db.GetCollection<CouchDBEventInfo>("CouchDBEvents");
            var obj = new CouchDBEventInfo { Event = this, EventInfoHash = HashInfo, TimeStamp = DateTime.Now };
            coll.Insert(obj);
            CleanUp();
        }

        #region Implementation of IPriorized

        public int Priority
        {
            get { return (int) Source; }
            set { throw new NotImplementedException(); }
        }

        public void OnReplaced(IPriorized @by)
        {
            Reset();
        }

        #endregion
    }
}