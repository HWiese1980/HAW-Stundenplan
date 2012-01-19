using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HAW_Tool.HAW.Depending
{
    class CouchDBInfoComparer : IEqualityComparer<CouchDBEventInfo>
    {
        public bool Equals(CouchDBEventInfo x, CouchDBEventInfo y)
        {
            return (x.EventInfoHash.Equals(y.EventInfoHash) & x.TimeStamp.Equals(y.TimeStamp)); 
        }

        public int GetHashCode(CouchDBEventInfo obj)
        {
            return obj.EventInfoHash.GetHashCode() + obj.TimeStamp.GetHashCode();
        }
    }
}
