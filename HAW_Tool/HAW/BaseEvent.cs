using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LittleHelpers;

namespace HAW_Tool.HAW
{
    public class BaseEvent : XElementContainer
    {
        string mCode = "";
        GroupID mSelectedGroup = GroupID.Empty;
        List<GroupID> mGroups = new List<GroupID>();

        public string Semester
        {
            get
            {
                int i;
                for (i = 0; i < this.BasicCode.Length; i++)
                {
                    var tEvts = from evt in PlanFile.Instance.KnownBaseEvents
                                where evt.BasicCode.Substring(0, i) == this.BasicCode.Substring(0, i)
                                select evt;

                    if (tEvts.Count() <= 0) break;
                }

                return this.BasicCode.Substring(0, i);
            }
        }

        public string BasicCode
        {
            get
            {
                return mCode;
            }
            set
            {
                mCode = value;
            }
        }

        public IEnumerable<GroupID> Groups
        {
            get
            {
                return mGroups;
            }
        }

        public GroupID Group
        {
            get
            {
                return mSelectedGroup;
            }
            set
            {
                mSelectedGroup = value;
            }
        }

        public BaseEvent(IEvent Evt)
        {
            mCode = Evt.BasicCode;
            mGroups.Add(GroupID.Empty);
            mGroups.AddRange(PlanFile.Instance.GetEventGroups(Evt));
            mSelectedGroup = GroupID.Empty;
        }

        public new string Label
        {
            get
            {
                return mCode;
            }
        }

        public override string ToString()
        {
            return mCode;
        }
    }
}
