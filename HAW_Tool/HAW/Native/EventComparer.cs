using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HAW_Tool.HAW.Native
{
    public class EventComparer : IEqualityComparer<IEvent>
    {
        #region IEqualityComparer<IEvent> Members

        public bool Equals(IEvent x, IEvent y)
        {
            return x.Code.Equals(y.Code);
        }

        public int GetHashCode(IEvent obj)
        {
            return obj.Code.GetHashCode();
        }

        #endregion
    }

    public class BaseEventComparer : IEqualityComparer<BaseEvent>
    {
        #region IEqualityComparer<BaseEvent> Members

        public bool Equals(BaseEvent x, BaseEvent y)
        {
            return x.BasicCode.Equals(y.BasicCode);
        }

        public int GetHashCode(BaseEvent obj)
        {
            return obj.BasicCode.GetHashCode();
        }

        #endregion
    }
}
