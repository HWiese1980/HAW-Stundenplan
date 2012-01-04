using System.Windows.Controls;
using System.Windows;

namespace HAW_Tool.HAW
{
    public class EventItemTemplateSelector : DataTemplateSelector
    {
        /*
        public override System.Windows.DataTemplate SelectTemplate(object item, System.Windows.DependencyObject container)
        {
            
            var tMetroTemplate = (DataTemplate)Application.Current.MainWindow.FindResource("MetroEventTemplate");
            var tEventTmpl = (DataTemplate) Application.Current.MainWindow.FindResource("EventItemTemplate");
            var tRESTEventTmpl = (DataTemplate) Application.Current.MainWindow.FindResource("RESTEventItemTemplate");

            if (item is Event) return tEventTmpl;
            if (item is RESTEvent) return tRESTEventTmpl;

            return base.SelectTemplate(item, container);
        }
         */

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var iface = item.GetType().GetInterface("IEvent");
            if(iface != null)
                return (DataTemplate) Application.Current.MainWindow.FindResource("EventItemTemplate");

            return base.SelectTemplate(item, container);
        }
    }
}
