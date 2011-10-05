using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using HAW_Tool.HAW.REST;

namespace HAW_Tool.HAW
{
    public class EventItemTemplateSelector : DataTemplateSelector
    {
        public override System.Windows.DataTemplate SelectTemplate(object item, System.Windows.DependencyObject container)
        {
            var tMetroTemplate = (DataTemplate)Application.Current.MainWindow.FindResource("MetroEventTemplate");
            var tEventTmpl = (DataTemplate) Application.Current.MainWindow.FindResource("EventItemTemplate");
            var tRESTEventTmpl = (DataTemplate) Application.Current.MainWindow.FindResource("RESTEventItemTemplate");

            if (item is Event) return tMetroTemplate;
            if (item is RESTEvent) return tRESTEventTmpl;

            return base.SelectTemplate(item, container);
        }
    }
}
