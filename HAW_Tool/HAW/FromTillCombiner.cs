using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace HAW_Tool.HAW
{
    internal class FromTillCombiner : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return values.FirstOrDefault();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            DateTime tFrom = ((DateTime)value).Date.AddHours(8);
            DateTime tTill = tFrom.AddHours(1.5);

            return new object[] { tFrom, tTill };
        }

        #endregion
    }
}
