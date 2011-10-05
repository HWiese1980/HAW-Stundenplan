using System;
using System.Collections.Generic;

namespace LittleHelpers
{
    public class AnonymousComparer<T> : IEqualityComparer<T>
    {
        readonly Func<T, T, bool> _equals;
        readonly Func<T, int> _getHash;

        public AnonymousComparer(Func<T, T, bool> equals, Func<T, int> getHash)
        {
            _equals = equals;
            _getHash = getHash;
        }

        #region IEqualityComparer<T> Members

        public bool Equals(T x, T y)
        {
            return _equals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return _getHash(obj);
        }

        #endregion
    }
}
