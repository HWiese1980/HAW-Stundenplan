using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace HAW_Tool.HAW
{
    class ToStringConverter : IValueConverter
    {
        #region IValueConverter Members

        public string Format { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is TimeSpan) value = DateTime.Now.Date + (TimeSpan)value;
            
            string ret = String.Format(Format, value);
            return ret;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
