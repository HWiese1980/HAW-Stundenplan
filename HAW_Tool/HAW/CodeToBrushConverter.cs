using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using System.Windows.Media;

namespace HAW_Tool.Converters
{
    class CodeToBrushConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string[] tCodes = value.ToString().Split('/');

            List<Color> tColors = new List<Color>();

            foreach (string tCode in tCodes)
            {
                object res = null;
                res = Application.Current.TryFindResource(tCode);
                tColors.Add((res != null && res is Color) ? (Color)res : Colors.Beige);
            }

            LinearGradientBrush tBrush = new LinearGradientBrush();
            for (int i = 0; i < tColors.Count; i++)
            {
                double tPos = i / tColors.Count;
                Color tCol = tColors[i];
                tBrush.GradientStops.Add(new GradientStop(tCol, tPos));
            }

            return tBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
