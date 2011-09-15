using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HAW_Tool.HAW
{
    public interface IWeek : IIsCurrent
    {
        IEnumerable<Day> Days { get; }
        SeminarGroup SeminarGroup { get; set; }
        bool IsPast { get; }
        int Week { get; }
        int Year { get; }
        string Label { get; }
        string LabelShort { get; }
    }
}
