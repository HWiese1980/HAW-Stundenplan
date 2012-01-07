using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using System.Windows.Data;

namespace HAW_Tool.Converters
{
    class BindingLogger : MarkupExtension, IValueConverter
    {
        public BindingLogger()
        {
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Console.WriteLine("-> Binding Log: {0} - Value: {1} ({2} -> {3})", parameter, value, value.GetType().Name, targetType.Name);
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Console.WriteLine("<- Binding Log: {0} - Value: {1} ({2} -> {3})", parameter, value, value.GetType().Name, targetType.Name);
            return value;
        }
    }
}
