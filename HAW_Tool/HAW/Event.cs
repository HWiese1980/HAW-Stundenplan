#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using HAW_Tool.Aspects;
using HAW_Tool.Bittorrent;
using LittleHelpers;
using DDayEvent = DDay.iCal.Event;

// using DDayEvent = DDay.iCal.Event;

#endregion

namespace HAW_Tool.HAW
{
    [Notifying("HasChanges")]
    public class Event : XElementContainer, IComparable<Event>, INotifyValueChanged, INotificationEnabled, IEvent
    {
        #region Fields (10)

        private bool _bEnabled = true;
        private bool _bOff = true;
        private EventCode _mCode;
        private DateTime _mDate;
        private Day _mDay;
        private string _mHash = String.Empty;
        private int _mKw;
        private string _mRoom;
        private string _mTag;
        private string _mTutor;

        #endregion Fields

        #region Constructors (1)

        public Event(IWeek kw, XElement baseElement)
        {
            MBaseElement = baseElement;

            Debug.Assert(baseElement != null, "baseElement != null");
// ReSharper disable PossibleNullReferenceException
            _mCode = ParseCode(baseElement.Element("code").Value);

            _mTutor = baseElement.Element("dozent").Value;
            _mRoom = baseElement.Element("raum").Value;
            _mTag = baseElement.Element("tag").Value;

            var tFrom = DateTime.Parse(baseElement.Element("von").Value).TimeOfDay;
            var tTill = DateTime.Parse(baseElement.Element("bis").Value).TimeOfDay;
            // ReSharper restore PossibleNullReferenceException


            _mKw = kw.Week;

            var tDOW = WeekHelper.DOW[_mTag.ToLower()];

            var tDT = Helper.DayOfWeekToDateTime(kw.Year, kw.Week, tDOW);
            _mDate = tDT;

            _mFrom = tFrom;
            _mTill = tTill;

            ReGenerateHash();
        }

        #endregion Constructors

        #region Properties (22)

        public bool HasChanges
        {
            get { return PlanFile.Instance.HasChangesByHash(Hash); }
        }

        private byte[] ID
        {
            get { return Encoding.ASCII.GetBytes(Info); }
        }

        public string Info
        {
            get { return String.Format("{0};{1};{2}", Code, _mKw, DayOfWeek); }
        }

        [NotifyingProperty]
        public bool TakesPlace
        {
            get { return _bOff; }
            set { _bOff = value; }
        }

        public new string ToolTip
        {
            get
            {
                return String.Format("{0} - Von {1:t} bis {2:t} im Raum {3} [{4}]", Code, From, Till,
                                     Room, BasicCode);
            }
        }

        [NotifyingProperty("Code", "Info", "IsObligatory")]
        public string BasicCode
        {
            get { return _mCode.Code; }
            set { _mCode.Code = value; }
        }

        public string Code
        {
            get
            {
                if (_mCode.Group != GroupID.Empty) return String.Format("{0}/{1:00}", _mCode.Code, _mCode.Group);
                return _mCode.Code;
            }
        }

        [NotifyingProperty]
        public DateTime Date
        {
            get { return _mDate.Date; }
            private set { _mDate = value; }
        }

        public IEnumerable<DateTime> OtherDates
        {
            get { return new DateTime[] {}; }
        }

        public Day Day
        {
            get { return _mDay; }
            set
            {
                _mDay = value;
                CalculateRowIndex();
            }
        }

        public int DayOfWeek
        {
            get { return WeekHelper.DOW[_mTag.ToLower()]; }
        }

        public IEnumerable<RESTTorrent> Files { get; set; }

        [NotifyingProperty]
        public TimeSpan From
        {
            get { return _mFrom; }
            set
            {
                var occupiedBy = Day.MinuteOccupiedBy((uint) value.TotalMinutes);
                if (occupiedBy == null || occupiedBy == this)
                    _mFrom = value;
            }
        }

        [NotifyingProperty("Code", "Info", "IsObligatory")]
        public GroupID Group
        {
            get { return _mCode.Group; }
            set
            {
                _mCode.Group = value;
                //OnPropertyChanged("Code");
            }
        }

        public string Hash
        {
            get
            {
                if (_mHash == String.Empty)
                {
                    ReGenerateHash();
                }
                return _mHash;
            }
        }

        [NotifyingProperty]
        public bool IsEnabled
        {
            get { return _bEnabled; }
            set { _bEnabled = value; }
        }

        public bool IsObligatory
        {
            get
            {
                if (PlanFile.Instance.ObligatoryRegexPatterns == null) return false;

                var matchingCount = (from p in PlanFile.Instance.ObligatoryRegexPatterns
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
                if (IsObligatory) return 3;
                return 0;
            }
        }

        [NotifyingProperty("Info")]
        public string Room
        {
            get { return _mRoom; }
            set { _mRoom = value; }
        }

        public int RowIndex { get; set; }

        public string SeminarGroup
        {
            get
            {
                var tRgx = new Regex(@"(.*?)-.*");
                if (!tRgx.IsMatch(BasicCode)) return "n/a";
                return tRgx.Match(BasicCode).Groups[1].Value;
            }
        }

        [NotifyingProperty]
        public TimeSpan Till
        {
            get { return _mTill; }
            set
            {
                var occupiedBy = Day.MinuteOccupiedBy((uint) value.TotalMinutes);
                if (occupiedBy == null || occupiedBy == this)
                    _mTill = value;
            }
        }

        [NotifyingProperty("Info")]
        public string Tutor
        {
            get { return _mTutor; }
            set { _mTutor = value; }
        }

        #endregion Properties

        #region Methods (4)

        // Public Methods (1) 

        public override string ToString()
        {
            return Code;
        }

        // Private Methods (3) 

        private void CalculateRowIndex()
        {
            if (Day == null) return;

            DateTime astart = Date + From, aend = Date + Till;
            foreach (var tEvt in Day.Events.Where(p => p.RowIndex == 0))
            {
                if (tEvt == this) continue;

                DateTime bstart = tEvt.Date + tEvt.From, bend = tEvt.Date + tEvt.Till;

                if (Helper.PeriodsOverlap(astart, aend, bstart, bend)) RowIndex++;
            }
        }

        private EventCode ParseCode(string xmlCode)
        {
            var tParts = xmlCode.Split('/');
            if (tParts.Count() > 1 && GroupID.IsValidGroup(tParts[1]))
            {
                return new EventCode {Code = tParts[0], Group = new GroupID(tParts[1])};
            }
            return new EventCode {Code = xmlCode, Group = GroupID.Empty};
        }

        private void ReGenerateHash()
        {
            var tMD5 = MD5.Create();
            _mHash = Convert.ToBase64String(tMD5.ComputeHash(ID), Base64FormattingOptions.InsertLineBreaks);
        }

        #endregion Methods

        private TimeSpan _mFrom, _mTill;

        #region IComparable<Event> Members

        public int CompareTo(Event other)
        {
            return Hash.CompareTo(other.Hash);
        }

        #endregion

        #region INotificationEnabled Members

        public bool IsNotifyingChanges
        {
            get { return PlanFile.Instance.IsNotifyingChanges; }
        }

        #endregion

        #region INotifyValueChanged Members

        public void OnValueChanging(string property, object oldValue, object newValue)
        {
            if (!oldValue.Equals(newValue)) PlanFile.Instance.AddChange(this, property, oldValue, newValue);
        }

        public void OnValueChanged(string property)
        {
            OnPropertyChanged(property);
        }

        #endregion

        #region Nested type: EventCode

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

        #endregion
    }
}