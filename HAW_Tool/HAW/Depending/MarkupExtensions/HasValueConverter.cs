using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Markup;

namespace HAW_Tool.HAW.Depending.MarkupExtensions
{
    public class HasValueConverter : MarkupExtension, IValueConverter
    {
        public string PropertyPath { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return false;

            var pathParts = PropertyPath.Split('/');
            foreach(var pathPart in pathParts)
            {
                var prop = value.GetType().GetProperty(pathPart);
                if (prop == null)
                {
                    break;
                }
                value = prop.GetValue(value, null);
            }
            return (value != null);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public HasValueConverter()
        {
        }
    }
}
