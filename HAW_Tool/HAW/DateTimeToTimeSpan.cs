using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using HAW_Tool.WPF;

namespace HAW_Tool.HAW
{
    public class DateTimeToTimeSpan : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is DateTime)) throw new ArgumentException("Value must be of type DateTime");

            DateTime tVal = (DateTime)value;
            TimeSpan tRet = tVal.TimeOfDay;

            return tRet;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is TimeSpan) && !(parameter is DateTime) && !(parameter is ConverterWrapper)) throw new ArgumentException("Value and Parameter must be of type TimeSpan and DateTime");

            TimeSpan tVal = (TimeSpan)value;
            DateTime tParm = (parameter is ConverterWrapper) ? (DateTime)((ConverterWrapper)parameter).Value : (DateTime)parameter;

            DateTime tRet = new DateTime(tParm.Year, tParm.Month, tParm.Day, tVal.Hours, tVal.Minutes, tVal.Seconds);
            return tRet;


        }

        #endregion
    }
}
