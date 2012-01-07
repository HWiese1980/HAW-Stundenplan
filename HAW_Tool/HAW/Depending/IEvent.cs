using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HAW_Tool.HAW.Depending
{
    interface IEvent
    {
        TimeSpan From { get; set; }
        TimeSpan Till { get; set; }
    }
}
