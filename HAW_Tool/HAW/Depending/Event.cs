#region Usings

using System;
using System.Linq;
using LittleHelpers;
using MongoDB.Bson.Serialization.Attributes;
using SeveQsCustomControls;

#endregion

namespace HAW_Tool.HAW.Depending
{
    // [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [BsonIgnoreExtraElements]
    public class Event : EventControlBase, IEvent, ISelectable, IKeyedObject
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
        private EventSource _source;
        private bool _takesPlace = true;
        private TimeSpan _till;
        private string _tutor;

        // [JsonProperty]
        [BsonElement("hashinfo")]
        public override string HashInfo
        {
            get
            {
                return (Source == EventSource.ReplacementDB
                            ? _loadedHashInfo
                            : String.Join(";", new[]
                                                   {
                                                       GetOriginalValue<string>("Code"),
                                                       GetOriginalValue<string>("Room"),
                                                       GetOriginalValue<Int32>("CalendarWeek").ToString(),
                                                       GetOriginalValue<Int32>("DayOfWeek").ToString(),
                                                       GetOriginalValue<TimeSpan>("From").ToString(),
                                                       GetOriginalValue<TimeSpan>("Till").ToString(),
                                                   })
                       );
            }
            set { _loadedHashInfo = value; }
        }

        // [JsonProperty]
        [BsonElement("week")]
        public int CalendarWeek
        {
            get { return _calWeek; }
            set
            {
                _calWeek = value;
                OnPropertyChanged("CalendarWeek");
            }
        }

        // [JsonProperty]
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

        [BsonIgnore]
        public SeminarGroup SeminarGroup
        {
            get { return _semGrp; }
            set
            {
                _semGrp = value;
                OnPropertyChanged("SeminarGroup");
            }
        }

        [BsonElement("seminargroup")]
        public string SeminarGroupName
        {
            get { return _semGrp.Name; }
            set { _semGrp = PlanFile.Instance.SeminarGroups.Where(p => p.Name == value).SingleOrDefault(); }
        }

        [BsonIgnore]
        [BsonDefaultValue(EventSource.ReplacementDB)]
        public EventSource Source
        {
            get { return _source; }
            set
            {
                _source = value;
                OnPropertyChanged("Source");
            }
        }

        // [JsonProperty]
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

        //[JsonProperty]
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

        [BsonElement("dow")]
        public int DayOfWeek
        {
            get { return _dow; }
            set
            {
                _dow = value;
                OnPropertyChanged("DayOfWeek");
            }
        }

        //[JsonProperty]
        [BsonElement("date")]
        [BsonDateTimeOptions(DateOnly = true)]
        public DateTime Date
        {
            get { return _date; }
            set
            {
                _date = value;
                OnPropertyChanged("Date");
            }
        }

        [BsonIgnore]
        public Day Day
        {
            get { return _day; }
            set
            {
                _day = value;
                DayChanged(_day);
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

        //[JsonProperty]
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

        //[JsonProperty]
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

        //[JsonProperty]
        [BsonElement("from")]
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

        //[JsonProperty]
        [BsonElement("till")]
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


        public void OnTimeChanged(EventArgs e = null)
        {
            if (Day != null) Day.RecalculateRowIndex(this);
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

        public override void CleanUp()
        {
            if (Source != EventSource.ReplacementDB)
            {
                HashInfo = String.Empty;
            }
            base.CleanUp();
        }

        private void DayChanged(Day newDay)
        {
            if (newDay == null) return;
            DayOfWeek = (int)newDay.DOW;
        }

        public override void Reset()
        {
            base.Reset();
            IsReplaced = false;
        }

        protected override void OnGotDirty()
        {
            if (Source == EventSource.ReplacementDB)
            {
                var originalEvent = PlanFile.Instance.GetEventByHashInfo(HashInfo);
                if (originalEvent != null)
                {
                    originalEvent.IsDirty = true;
                }
            }
        }

        public event EventHandler<ValueEventArgs<Replacement>> EventSaved;

        private void OnEventSaved(object sender, ValueEventArgs<Replacement> e)
        {
            if (EventSaved != null) EventSaved(sender, e);
        }



        public void Save()
        {
            var r = new Replacement { Event = this, Timestamp = DateTime.Now };
            PlanFile.Instance.MongoStore(r);
            Reset();

            OnEventSaved(this, new ValueEventArgs<Replacement> { Value = r });
        }

        #region Implementation of IKeyedObject

        public string Key
        {
            get { return HashInfo; }
        }

        #endregion
    }
}