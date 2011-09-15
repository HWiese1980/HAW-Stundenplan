using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LittleHelpers
{
    public class ValueEventArgs<T> : EventArgs
    {
        T mValue = default(T);
        public T Value { get { return mValue; } set { mValue = value; } }
    }

}
