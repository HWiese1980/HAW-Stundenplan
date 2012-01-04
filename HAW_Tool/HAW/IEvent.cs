using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HAW_Tool.HAW
{
    public interface IEvent
    {
        string Hash { get; }
        string Code { get; }
        string BasicCode { get; set; }
        string SeminarGroup { get; }
        string Room { get; }
        string Tutor { get; }
        int Priority { get; }
        int DayOfWeek { get; }
        DateTime Date { get; }
        IEnumerable<DateTime> OtherDates { get; }
        TimeSpan From { get; set; }
        TimeSpan Till { get; set;  }
        Day Day { get; set; }
        int RowIndex { get; }
        GroupID Group { get; set; }
        bool IsEnabled { get; set; }
        bool IsObligatory { get; }
    }
}
