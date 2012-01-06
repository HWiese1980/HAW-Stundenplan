using System.Collections.Generic;

namespace HAW_Tool.HAW.Native.Comparers
{
    class WeekEqualityComparer : IEqualityComparer<IWeek>
    {
        #region IEqualityComparer<IWeek> Members

        public bool Equals(IWeek x, IWeek y)
        {
            return (x.Year == y.Year & x.Week == y.Week);
        }

        public int GetHashCode(IWeek obj)
        {
            return ((IWeek)obj).Week.GetHashCode();
        }

        #endregion
    }
}
