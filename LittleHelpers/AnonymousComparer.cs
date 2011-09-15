using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LittleHelpers
{
    public class AnonymousComparer<T> : IEqualityComparer<T>
    {
        readonly Func<T, T, bool> equals;
        readonly Func<T, int> getHash;

        public AnonymousComparer(Func<T, T, bool> equals, Func<T, int> getHash)
        {
            this.equals = equals;
            this.getHash = getHash;
        }

        #region IEqualityComparer<T> Members

        public bool Equals(T x, T y)
        {
            return this.equals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return this.getHash(obj);
        }

        #endregion
    }
}
