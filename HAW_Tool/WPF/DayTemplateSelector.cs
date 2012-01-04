using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HAW_Tool.HAW;
using System.Windows;
using System.Windows.Controls;

namespace HAW_Tool.WPF
{
    public class DayTemplateSelector : DataTemplateSelector
    {
        public DataTemplate EmptyDayTemplate { get; set; }
        public DataTemplate NotEmptyDayTemplate { get; set; }

        public override System.Windows.DataTemplate SelectTemplate(object item, System.Windows.DependencyObject container)
        {
            Day d = (Day)item;
            return d.Events.Count() > 0 ? NotEmptyDayTemplate : EmptyDayTemplate;
        }

    }
}
