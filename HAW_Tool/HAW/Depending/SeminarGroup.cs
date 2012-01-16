using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using Newtonsoft.Json;

namespace HAW_Tool.HAW.Depending
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class SeminarGroup : DependencyObject
    {
        public SeminarGroup()
        {
            CalendarWeeks = new ObservableCollection<CalendarWeek>();
        }

        [JsonProperty]
        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Name.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register("Name", typeof(string), typeof(SeminarGroup), new UIPropertyMetadata(""));

        public string Version
        {
            get { return (string)GetValue(VersionProperty); }
            set { SetValue(VersionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Version.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VersionProperty =
            DependencyProperty.Register("Version", typeof(string), typeof(SeminarGroup), new UIPropertyMetadata(""));


        public DateTime LastUpdated
        {
            get { return (DateTime)GetValue(LastUpdatedProperty); }
            set { SetValue(LastUpdatedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LastUpdated.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LastUpdatedProperty =
            DependencyProperty.Register("LastUpdated", typeof(DateTime), typeof(SeminarGroup), new UIPropertyMetadata(DateTime.MinValue));

        public ObservableCollection<CalendarWeek> CalendarWeeks { get; set; }

        public Day GetDayByDate(DateTime date)
        {
            var day = (from cw in CalendarWeeks
                      from dy in cw.Days
                      where dy.Date.Date == date.Date
                      select dy).FirstOrDefault();
            return day;
        }
    }
}
