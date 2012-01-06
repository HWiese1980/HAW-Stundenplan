using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HAW_Tool.HAW.Native.WPF
{
    public class DayTemplateSelector : DataTemplateSelector
    {
        public DataTemplate EmptyDayTemplate { get; set; }
        public DataTemplate NotEmptyDayTemplate { get; set; }

        public override System.Windows.DataTemplate SelectTemplate(object item, System.Windows.DependencyObject container)
        {
            var d = (Day)item;
            return d.Events.Count() > 0 ? NotEmptyDayTemplate : EmptyDayTemplate;
        }

    }
}
