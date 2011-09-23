using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Windows;
using System.Security.Cryptography;
using HAW_Tool.Aspects;
using LittleHelpers;
using DDayEvent = DDay.iCal.Event;
// using DDayEvent = DDay.iCal.Event;
using DDay.iCal;
using System.Text.RegularExpressions;
using HAW_Tool.Bittorrent;
using HAW_Tool.HAW.REST;

namespace HAW_Tool.HAW
{
    [Notifying("HasChanges")]
    public class Event : XElementContainer, IComparable<Event>, INotifyValueChanged, INotificationEnabled, IEvent
    {
        #region Fields (10)

        private bool bEnabled = true;
        private bool bOff = true;
        private EventCode m_Code;
        private DateTime m_Date;
        private int m_KW = 0;
        private string m_Room;
        private string m_Tag;
        //[NotifyingProperty(true, "Till", "Info")]
        //public TimeSpan TillTime
        //{
        //    get { return m_Till.TimeOfDay; }
        //    set
        //    {
        //        DateTime tNewTill = new DateTime(Till.Year, Till.Month, Till.Day, value.Hours, value.Minutes, value.Seconds);
        //        if (tNewTill < From) return;
        //        Till = tNewTill;
        //    }
        //}
        private string m_Tutor;
        private Day mDay = null;
        private string mHash = String.Empty;

        #endregion Fields

        #region Constructors (1)

        public Event(CalendarWeek KW, XElement BaseElement)
        {
            MBaseElement = BaseElement;

            m_Code = ParseCode(BaseElement.Element("code").Value);

            m_Tutor = BaseElement.Element("dozent").Value;
            m_Room = BaseElement.Element("raum").Value;
            m_Tag = BaseElement.Element("tag").Value;

            TimeSpan tFrom = DateTime.Parse(BaseElement.Element("von").Value).TimeOfDay;
            TimeSpan tTill = DateTime.Parse(BaseElement.Element("bis").Value).TimeOfDay;


            m_KW = KW.Week;

            int tDOW = WeekHelper.DOW[m_Tag.ToLower()];

            DateTime tDT = Helper.DayOfWeekToDateTime(KW.Year, KW.Week, tDOW);
            m_Date = tDT;

            m_From = tFrom;
            m_Till = tTill;

            ReGenerateHash();
        }

        #endregion Constructors

        #region Properties (22)

        [NotifyingProperty("Code", "Info", "IsObligatory")]
        public string BasicCode
        {
            get { return m_Code.Code; }
            set { m_Code.Code = value; }
        }

        public string Code
        {
            get
            {
                if (m_Code.Group != GroupID.Empty) return String.Format("{0}/{1:00}", m_Code.Code, m_Code.Group);
                return m_Code.Code;
            }
        }

        [NotifyingProperty]
        public DateTime Date
        {
            get { return m_Date.Date; }
            private set { m_Date = value; }
        }

        public IEnumerable<DateTime> OtherDates
        {
            get { return new DateTime[] {}; }
        }

        public Day Day
        {
            get { return mDay; }
            set
            {
                mDay = value;
                CalculateRowIndex();
            }
        }

        public int DayOfWeek
        {
            get { return WeekHelper.DOW[m_Tag.ToLower()]; }
        }

        public IEnumerable<RESTTorrent> Files { get; set; }

        [NotifyingProperty]
        public TimeSpan From
        {
            get { return m_From; }
            set
            {
                IEvent occupiedBy = Day.MinuteOccupiedBy((uint) value.TotalMinutes);
                if (occupiedBy == null || occupiedBy == this)
                    m_From = value;
            }
        }

        [NotifyingProperty("Code", "Info", "IsObligatory")]
        public GroupID Group
        {
            get { return m_Code.Group; }
            set
            {
                m_Code.Group = value;
                //OnPropertyChanged("Code");
            }
        }

        public bool HasChanges
        {
            get { return PlanFile.Instance.HasChangesByHash(this.Hash); }
        }

        public string Hash
        {
            get
            {
                if (mHash == String.Empty)
                {
                    ReGenerateHash();
                }
                return mHash;
            }
        }

        private byte[] ID
        {
            get { return Encoding.ASCII.GetBytes(this.Info); }
        }

        public string Info
        {
            get { return String.Format("{0};{1};{2}", this.Code, this.m_KW, this.DayOfWeek); }
        }

        [NotifyingProperty]
        public bool IsEnabled
        {
            get { return bEnabled; }
            set { bEnabled = value; }
        }

        public bool IsObligatory
        {
            get
            {
                if (PlanFile.Instance.ObligatoryRegexPatterns == null) return false;

                int matchingCount = (from p in PlanFile.Instance.ObligatoryRegexPatterns
                                     let q = new Regex(p)
                                     where q.IsMatch(Code)
                                     select p).Count();

                return (matchingCount > 0);
                // return (Code.Contains("P") && Code.Contains("/"));
            }
        }

        public int Priority
        {
            get
            {
                if (this.IsObligatory) return 3;
                return 0;
            }
        }

        [NotifyingProperty("Info")]
        public string Room
        {
            get { return m_Room; }
            set { m_Room = value; }
        }

        public int RowIndex { get; set; }

        public string SeminarGroup
        {
            get
            {
                Regex tRgx = new Regex(@"(.*?)-.*");
                if (!tRgx.IsMatch(BasicCode)) return "n/a";
                return tRgx.Match(BasicCode).Groups[1].Value;
            }
        }

        [NotifyingProperty]
        public bool TakesPlace
        {
            get { return bOff; }
            set { bOff = value; }
        }

        [NotifyingProperty]
        public TimeSpan Till
        {
            get { return m_Till; }
            set
            {
                IEvent occupiedBy = Day.MinuteOccupiedBy((uint) value.TotalMinutes);
                if (occupiedBy == null || occupiedBy == this)
                    m_Till = value;
            }
        }

        public new string ToolTip
        {
            get
            {
                return String.Format("{0} - Von {1:t} bis {2:t} im Raum {3} [{4}]", this.Code, this.From, this.Till,
                                     this.Room, this.BasicCode);
            }
        }

        [NotifyingProperty("Info")]
        public string Tutor
        {
            get { return m_Tutor; }
            set { m_Tutor = value; }
        }

        #endregion Properties

        #region Methods (4)

        // Public Methods (1) 

        public override string ToString()
        {
            return this.Code;
        }

        // Private Methods (3) 

        private void CalculateRowIndex()
        {
            if (Day == null) return;

            DateTime astart = this.Date + this.From, aend = this.Date + this.Till;
            foreach (IEvent tEvt in this.Day.Events.Where(p => p.RowIndex == 0))
            {
                if (tEvt == this) continue;

                DateTime bstart = tEvt.Date + tEvt.From, bend = tEvt.Date + tEvt.Till;

                if (Helper.PeriodsOverlap(astart, aend, bstart, bend)) this.RowIndex++;
            }
        }

        private EventCode ParseCode(string XMLCode)
        {
            string[] tParts = XMLCode.Split('/');
            if (tParts != null && tParts.Count() > 1 && GroupID.IsValidGroup(tParts[1]))
            {
                return new EventCode() {Code = tParts[0], Group = new GroupID(tParts[1])};
            }
            return new EventCode() {Code = XMLCode, Group = GroupID.Empty};
        }

        private void ReGenerateHash()
        {
            MD5 tMD5 = MD5.Create();
            mHash = Convert.ToBase64String(tMD5.ComputeHash(this.ID), Base64FormattingOptions.InsertLineBreaks);
        }

        #endregion Methods

        internal class EventCode
        {
            private string _code;

            public string Code
            {
                get { return _code.ConvertUmlauts(UmlautConvertDirection.FromCrossWordFormat); }
                set { _code = value.ConvertUmlauts(UmlautConvertDirection.ToCrossWordFormat); }
            }

            public GroupID Group { get; set; }
        }

        private TimeSpan m_From, m_Till;

        #region INotifyValueChanged Members

        public void OnValueChanging(string Property, object OldValue, object NewValue)
        {
            if (!OldValue.Equals(NewValue)) PlanFile.Instance.AddChange(this, Property, OldValue, NewValue);
        }

        public void OnValueChanged(string Property)
        {
            OnPropertyChanged(Property);
        }

        #endregion

        #region IComparable<Event> Members

        public int CompareTo(Event other)
        {
            return this.Hash.CompareTo(other.Hash);
        }

        #endregion

        #region INotificationEnabled Members

        public bool IsNotifyingChanges
        {
            get { return PlanFile.Instance.IsNotifyingChanges; }
        }

        #endregion
    }
}