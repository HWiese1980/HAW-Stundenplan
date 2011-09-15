using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HAW_Tool
{
    public class UniqueList<T> : List<T>
    {
        public new void Add(T obj)
        {
            if (this.Contains(obj)) return;
            base.Add(obj);
        }

        public void Add(T obj, IEqualityComparer<T> Comparer)
        {
            if (this.Contains(obj, Comparer)) return;
            base.Add(obj);
        }
    }
}
