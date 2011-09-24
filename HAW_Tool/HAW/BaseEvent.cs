using System.Collections.Generic;
using System.Linq;

namespace HAW_Tool.HAW
{
    public class BaseEvent : XElementContainer
    {
        string _mCode = "";
        GroupID _mSelectedGroup = GroupID.Empty;
        readonly List<GroupID> _mGroups = new List<GroupID>();

        public string Semester
        {
            get
            {
                int iCodeLength;
                for (iCodeLength = 0; iCodeLength < BasicCode.Length; iCodeLength++)
                {
                    var length = iCodeLength;
                    var tEvts = from evt in PlanFile.Instance.KnownBaseEvents
                                where evt.BasicCode.Substring(0, length) == BasicCode.Substring(0, length)
                                select evt;

                    if (tEvts.Count() <= 0) break;
                }

                return BasicCode.Substring(0, iCodeLength);
            }
        }

        public string BasicCode
        {
            get
            {
                return _mCode;
            }
            set
            {
                _mCode = value;
            }
        }

        public IEnumerable<GroupID> Groups
        {
            get
            {
                return _mGroups;
            }
        }

        public GroupID Group
        {
            get
            {
                return _mSelectedGroup;
            }
            set
            {
                _mSelectedGroup = value;
            }
        }

        public BaseEvent(IEvent evt)
        {
            _mCode = evt.BasicCode;
            _mGroups.Add(GroupID.Empty);
            _mGroups.AddRange(PlanFile.Instance.GetEventGroups(evt));
            _mSelectedGroup = GroupID.Empty;
        }

        public new string Label
        {
            get
            {
                return _mCode;
            }
        }

        public override string ToString()
        {
            return _mCode;
        }
    }
}
