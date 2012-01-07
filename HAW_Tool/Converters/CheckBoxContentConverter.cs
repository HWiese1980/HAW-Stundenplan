using System;
using System.Windows.Markup;
using System.Windows.Data;

namespace HAW_Tool.Converters
{
    public class CheckBoxContentConverter : MarkupExtension, IValueConverter
    {
        public CheckBoxContentConverter()
        {}

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public string CheckedText { private get; set; }
        public string UncheckedText { private get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is bool)) return null;
            var v = (bool) value;
            return (v) ? CheckedText : UncheckedText;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
