using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace SeveQsCustomControls
{
    public interface IPriorized : IComparable
    {
        int Priority { get; set; }
        Visibility Visibility { get; set; }
        void OnReplaced(IPriorized by);
    }
}
