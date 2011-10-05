using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LittleHelpers
{
    public class ValueEventArgs<T> : EventArgs
    {
        public T Value { get; set; }
    }

}
