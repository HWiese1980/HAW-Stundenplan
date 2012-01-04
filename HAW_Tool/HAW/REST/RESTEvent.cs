using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace HAW_Tool.HAW.REST
{
    public enum EntryType
    {
        Event,
        Exam,
        Test,
        Probe,
        Other
    }

    public enum ReplacementType
    {
        None,
        Single,
        Some,
        Time,
        All
    }

    [DataContract(Name = "RESTevent")]
    public class RESTEvent : IEvent
    {
        #region Fields (1)

        private static DateTime _unixEpoch;

        #endregion Fields

        #region Constructors (1)

        static RESTEvent()
        {
            _unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0);
        }

        #endregion Constructors

        #region Properties (34)

        private string _mYWDCode = "";

        [DataMember(Name = "creator")]
        public string Creator { get; set; }

        [DataMember(Name = "tstamp")]
        private string UnixTimeStamp { get; set; }

        [IgnoreDataMember]
        public DateTime LastChangedDate
        {
            get
            {
                var tTime = _unixEpoch.AddSeconds(Convert.ToInt32(UnixTimeStamp));
                return tTime;
            }
            set
            {
                var tSeconds = (int) (value - _unixEpoch).TotalSeconds;
                UnixTimeStamp = tSeconds.ToString();
            }
        }


        [DataMember(Name = "group")]
        private string JsonGroup
        {
            get { return (Group != null) ? Group.Value : String.Empty; }
            set { Group = new GroupID(value); }
        }

        [DataMember(Name = "id")]
        public int ID { get; set; }

        [DataMember(Name = "further_inf")]
        public string Info { get; set; }

        [DataMember(Name = "date")]
        private string JsonDate
        {
            get { return Date.ToShortDateString(); }
            set { Date = DateTime.Parse(value); }
        }

        [DataMember(Name = "year_week_day")]
        private string JsonYearWeekDayCode
        {
            get { return _mYWDCode; }
            set
            {
                _mYWDCode = value;
                Date = Helper.ParseYearWeekDayCode(_mYWDCode);
            }
        }

        [DataMember(Name = "from")]
        private string JsonFrom
        {
            get { return From.ToString(); }
            set { From = TimeSpan.Parse(value); }
        }


        [DataMember(Name = "isobligatory")]
        private string JsonIsObligatory
        {
            get { return (IsObligatory) ? "1" : "0"; }
            set { IsObligatory = (value == "1"); }
        }

        [DataMember(Name = "takesplace")]
        private string JsonTakesPlace
        {
            get { return (TakesPlace) ? "1" : "0"; }
            set { TakesPlace = (value == "1"); }
        }

        [DataMember(Name = "till")]
        private string JsonTill
        {
            get { return Till.ToString(); }
            set { Till = TimeSpan.Parse(value); }
        }

        [DataMember(Name = "type")]
        private string JsonType { get; set; }

        [IgnoreDataMember]
        public ReplacementType ReplacementType
        {
            get
            {
                if (Replaces == null) return ReplacementType.None;

                switch (Replaces.ToLower())
                {
                    case "single":
                        return ReplacementType.Single;
                    case "some":
                        return ReplacementType.Some;
                    case "all":
                        return ReplacementType.All;
                    case "time":
                        return ReplacementType.Time;
                    default:
                        return ReplacementType.None;
                }
            }
        }

        [DataMember(Name = "repeats")]
        public int RepeatsEveryXWeeks { get; set; }

        [DataMember(Name = "replaces")]
        public string Replaces { get; set; }

        [DataMember(Name = "replaces_end_week")]
        public int ReplacesEndWeek
        {
            get
            {
                int weekCount = Helper.WeekCount(WeekRefYear);
                if (_mRplStartWeek <= weekCount) weekCount = 0;

                return _mRplEndWeek - weekCount;
            }
            set { _mRplEndWeek = value; }
        }

        [DataMember(Name = "replaces_start_week")]
        public int ReplacesStartWeek
        {
            get
            {
                int weekCount = Helper.WeekCount(WeekRefYear);
                if (_mRplStartWeek <= weekCount) weekCount = 0;

                return _mRplStartWeek - weekCount;
            }
            set { _mRplStartWeek = value; }
        }

        [IgnoreDataMember]
        public bool TakesPlace { get; set; }

        [IgnoreDataMember]
        public EntryType Type
        {
            get { return (EntryType) Enum.Parse(typeof (EntryType), JsonType); }
            set { JsonType = Enum.GetName(typeof (EntryType), value); }
        }

        [IgnoreDataMember]
        public string TypeCode
        {
            get
            {
                switch (Type)
                {
                    case EntryType.Exam:
                        return ("[Klausur]");
                    case EntryType.Test:
                        return ("[Test]");
                    case EntryType.Probe:
                        return ("[Probekl.]");
                    default:
                        return "";
                }
            }
        }

        [DataMember(Name = "week_reference_year")]
        private int WeekRefYear { get; set; }

        [DataMember(Name = "basiccode")]
        public string BasicCode { get; set; }

        [IgnoreDataMember]
        public string Code
        {
            get
            {
                var tBld = new StringBuilder();

                tBld.Append(BasicCode);

                if (Group != GroupID.Empty) tBld.AppendFormat("/{0:00}", Group);
                return tBld.ToString();
            }
        }

        [IgnoreDataMember]
        public DateTime Date { get; set; }

        [IgnoreDataMember]
        public Day Day { get; set; }

        [IgnoreDataMember]
        public int DayOfWeek
        {
            get { return (int) Date.DayOfWeek; }
        }

        [IgnoreDataMember]
        public TimeSpan From { get; set; }

        public GroupID Group { get; set; }

        [IgnoreDataMember]
        public string Hash { get; set; }

        [IgnoreDataMember]
        public bool IsEnabled
        {
            get { return true; }
            set { }
        }

        [IgnoreDataMember]
        public bool IsObligatory { get; set; }

        public int Priority
        {
            get
            {
                switch (Type)
                {
                    case EntryType.Test:
                        return 2;
                    case EntryType.Exam:
                        return 1;
                }

                if (IsObligatory) return 3;

                return 0;
            }
        }

        [IgnoreDataMember]
        public IEnumerable<DateTime> OtherDates
        {
            get
            {
                if (RepeatsEveryXWeeks == 0) return new DateTime[] {};

                var tDates = new List<DateTime>();

                int tThisWeek = Helper.WeekOfDate(Date);
                int tWeekCount = PlanFile.Instance.CoveredWeeks.Last().Week - tThisWeek;
                for (int i = 0; i < tWeekCount; i += RepeatsEveryXWeeks)
                {
                    DateTime tDate = Date.AddDays(i*7);
                    tDates.Add(tDate);
                }

                return tDates;
            }
        }

        [DataMember(Name = "room")]
        public string Room { get; set; }

        [IgnoreDataMember]
        public int RowIndex
        {
            get
            {
                if (Day == null) return 0;
                return (Day.IsInOccupiedRanges(this)) ? 1 : 0;
            }
        }

        [DataMember(Name = "seminargroup")]
        public string SeminarGroup { get; set; }

        [IgnoreDataMember]
        public TimeSpan Till { get; set; }

        [DataMember(Name = "tutor")]
        public string Tutor { get; set; }

        #endregion Properties

        #region Methods (3)

        // Public Methods (1) 

        public bool IsReplacementFor(IEvent evt)
        {
            switch (ReplacementType)
            {
                case ReplacementType.All:
                    return (evt.BasicCode == BasicCode);

                case ReplacementType.Some:
                    {
                        DateTime tStart = Helper.StartOfWeek(ReplacesStartWeek, Date.Year);
                        DateTime tEnd = Helper.EndOfWeek(ReplacesEndWeek, Date.Year);
                        bool bSameCode = (evt.BasicCode == BasicCode);
                        bool bSameDOW = (evt.Date.DayOfWeek == Date.DayOfWeek);
                        bool bIsBetween = evt.Date.IsBetween(tStart, tEnd);
                        return (bSameCode & bSameDOW & bIsBetween);
                    }

                case ReplacementType.Single:
                    return (evt.BasicCode == BasicCode && evt.Date == Date);
                case ReplacementType.Time:
                    {
                        DateTime astart = (Date + From), aend = (Date + Till);
                        DateTime bstart = (evt.Date + evt.From), bend = (evt.Date + evt.Till);
                        return Helper.PeriodsOverlap(astart, aend, bstart, bend);
                    }
                default:
                    return false;
            }
        }

        // Private Methods (2) 

        private void FetchTorrents()
        {
            //HAWClient tClient = new HAWClient();
            //this.Files = tClient.Torrents(this.Hash);
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext ctx)
        {
            FetchTorrents();
        }

        #endregion Methods

        private int _mRplEndWeek;
        private int _mRplStartWeek;
    }
}