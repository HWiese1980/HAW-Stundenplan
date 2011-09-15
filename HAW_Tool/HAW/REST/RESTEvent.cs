using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using HAW_Tool.Bittorrent;

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

        static DateTime unixEpoch;

        #endregion Fields

        #region Constructors (1)

        static RESTEvent()
        {
            unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0);
        }

        #endregion Constructors

        #region Properties (34)

        [DataMember(Name = "basiccode")]
        public string BasicCode { get; set; }

        [IgnoreDataMember]
        public string Code
        {
            get
            {
                StringBuilder tBld = new StringBuilder();

                tBld.Append(BasicCode);

                if (this.Group != GroupID.Empty) tBld.AppendFormat("/{0:00}", this.Group);
                return tBld.ToString();
            }
        }

        [DataMember(Name = "creator")]
        public string Creator { get; set; }

        [DataMember(Name = "tstamp")]
        private string UnixTimeStamp { get; set; }

        [IgnoreDataMember]
        public DateTime LastChangedDate
        {
            get
            {
                DateTime tTime = unixEpoch.AddSeconds(Convert.ToInt32(UnixTimeStamp));
                return tTime;
            }
            set
            {
                int tSeconds = (int)((TimeSpan)(value - unixEpoch)).TotalSeconds;
                this.UnixTimeStamp = tSeconds.ToString();
            }
        }

        [IgnoreDataMember]
        public DateTime Date { get; set; }

        [IgnoreDataMember]
        public Day Day { get; set; }

        [IgnoreDataMember]
        public int DayOfWeek
        {
            get
            {
                return (int)Date.DayOfWeek;
            }
        }

        [IgnoreDataMember]
        public IEnumerable<RESTTorrent> Files
        {
            get;
            set;
        }

        [IgnoreDataMember]
        public TimeSpan From { get; set; }



        [DataMember(Name = "group")]
        private string JsonGroup
        {
            get
            {
                return (Group != null) ? Group.Value : String.Empty;
            }
            set
            {
                this.Group = new GroupID(value);
            }
        }

        public GroupID Group
        { get; set; }

        [IgnoreDataMember]
        public string Hash { get; set; }

        [DataMember(Name = "id")]
        public int ID
        {
            get;
            set;
        }

        [DataMember(Name = "further_inf")]
        public string Info { get; set; }

        [IgnoreDataMember]
        public bool IsEnabled
        {
            get { return true; }
            set { }
        }

        [IgnoreDataMember]
        public bool IsObligatory { get; set; }

        [DataMember(Name = "date")]
        private string JsonDate
        {
            get { return this.Date.ToShortDateString(); }
            set { this.Date = DateTime.Parse(value); }
        }

        string mYWDCode = "";
        [DataMember(Name = "year_week_day")]
        private string JsonYearWeekDayCode
        {
            get
            {
                return mYWDCode;
            }
            set
            {
                mYWDCode = value;
                this.Date = Helper.ParseYearWeekDayCode(mYWDCode);
            }
        }

        [DataMember(Name = "from")]
        private string JsonFrom
        {
            get { return this.From.ToString(); }
            set { this.From = TimeSpan.Parse(value); }
        }



        [DataMember(Name = "isobligatory")]
        private string JsonIsObligatory
        {
            get { return (this.IsObligatory) ? "1" : "0"; }
            set { this.IsObligatory = (value == "1"); }
        }

        [DataMember(Name = "takesplace")]
        private string JsonTakesPlace
        {
            get { return (this.TakesPlace) ? "1" : "0"; }
            set { this.TakesPlace = (value == "1"); }
        }

        [DataMember(Name = "till")]
        private string JsonTill
        {
            get { return this.Till.ToString(); }
            set { this.Till = TimeSpan.Parse(value); }
        }

        [DataMember(Name = "type")]
        private string JsonType { get; set; }

        public int Priority
        {
            get
            {
                switch (this.Type)
                {
                    case EntryType.Test: return 2;
                    case EntryType.Exam: return 1;
                }

                if (this.IsObligatory) return 3;

                return 0;
            }
        }

        [IgnoreDataMember]
        public ReplacementType ReplacementType
        {
            get
            {
                if (this.Replaces == null) return ReplacementType.None;

                switch (this.Replaces.ToLower())
                {
                    case "single": return ReplacementType.Single;
                    case "some": return ReplacementType.Some;
                    case "all": return ReplacementType.All;
                    case "time": return ReplacementType.Time;
                    default: return ReplacementType.None;
                }
            }
        }

        [DataMember(Name = "repeats")]
        public int RepeatsEveryXWeeks { get; set; }

        [IgnoreDataMember]
        public IEnumerable<DateTime> OtherDates
        {
            get
            {
                if (RepeatsEveryXWeeks == 0) return new DateTime[] { };

                List<DateTime> tDates = new List<DateTime>();

                int tThisWeek = Helper.WeekOfDate(this.Date);
                int tWeekCount = PlanFile.Instance.CoveredWeeks.Last().Week - tThisWeek;
                for (int i = 0; i < tWeekCount; i += RepeatsEveryXWeeks)
                {
                    DateTime tDate = this.Date.AddDays(i * 7);
                    tDates.Add(tDate);
                }

                return tDates;
            }
        }

        [DataMember(Name = "replaces")]
        public string Replaces { get; set; }

        [DataMember(Name = "replaces_end_week")]
        public int ReplacesEndWeek
        {
            get
            {
                int weekCount = Helper.WeekCount(this.WeekRefYear);
                if (mRplStartWeek <= weekCount) weekCount = 0;

                return mRplEndWeek - weekCount;
            }
            set
            {
                mRplEndWeek = value;
            }
        }

        [DataMember(Name = "replaces_start_week")]
        public int ReplacesStartWeek
        {
            get
            {
                int weekCount = Helper.WeekCount(this.WeekRefYear);
                if (mRplStartWeek <= weekCount) weekCount = 0;

                return mRplStartWeek - weekCount;
            }
            set
            {
                mRplStartWeek = value;
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
        public bool TakesPlace { get; set; }

        [IgnoreDataMember]
        public TimeSpan Till { get; set; }

        [DataMember(Name = "tutor")]
        public string Tutor { get; set; }

        [IgnoreDataMember]
        public EntryType Type
        {
            get { return (EntryType)Enum.Parse(typeof(EntryType), this.JsonType); }
            set { JsonType = Enum.GetName(typeof(EntryType), value); }
        }

        [IgnoreDataMember]
        public string TypeCode
        {
            get
            {
                switch (this.Type)
                {
                    case EntryType.Exam: return ("[Klausur]");
                    case EntryType.Test: return ("[Test]");
                    case EntryType.Probe: return ("[Probekl.]");
                    default: return "";
                }
            }
        }

        [DataMember(Name = "week_reference_year")]
        private int WeekRefYear { get; set; }

        #endregion Properties

        #region Methods (3)

        // Public Methods (1) 

        public bool IsReplacementFor(IEvent evt)
        {
            switch (this.ReplacementType)
            {
                case ReplacementType.All:
                    return (evt.BasicCode == this.BasicCode);

                case ReplacementType.Some:
                    {
                        DateTime tStart = Helper.StartOfWeek(this.ReplacesStartWeek, this.Date.Year);
                        DateTime tEnd = Helper.EndOfWeek(this.ReplacesEndWeek, this.Date.Year);
                        bool bSameCode = (evt.BasicCode == this.BasicCode);
                        bool bSameDOW = (evt.Date.DayOfWeek == this.Date.DayOfWeek);
                        bool bIsBetween = evt.Date.IsBetween(tStart, tEnd);
                        return (bSameCode & bSameDOW & bIsBetween);
                    }

                case ReplacementType.Single:
                    return (evt.BasicCode == this.BasicCode && evt.Date == this.Date);
                case ReplacementType.Time:
                    {
                        DateTime astart = (this.Date + this.From), aend = (this.Date + this.Till);
                        DateTime bstart = (evt.Date + evt.From), bend = (evt.Date + evt.Till);
                        return Helper.PeriodsOverlap(astart, aend, bstart, bend);
                    }
                case ReplacementType.None:
                default: return false;
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

        int mRplStartWeek, mRplEndWeek;
    }
}
