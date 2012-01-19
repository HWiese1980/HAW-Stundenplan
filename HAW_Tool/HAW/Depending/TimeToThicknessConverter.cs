using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace HAW_Tool.HAW.Depending
{
    public class TimeToThicknessConverter : IMultiValueConverter
    {
        public double Multiplier { private get; set; }

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (LittleHelpers.Helper.IsInDesignModeStatic) return null;

            var tEvents = from p in values
                          where p is Event
                          select p;

            Event tEvt;
            try
            {
                tEvt = (Event)tEvents.Single();
            }
            catch
            {
                Console.WriteLine(@"No event found in this Object");
                return 0.0D;
            }

            TimeSpan tStart = tEvt.From;
            double tCoord = (tStart.Hours - 7) * Multiplier;
            double tMinutesStart = tStart.Minutes / 60.0F;
            tCoord += tMinutesStart * Multiplier;

            var propName = (string)parameter;
            Thickness? tn = null;
            switch(propName.ToLower())
            {
                case "left":
                    tn = new Thickness(tCoord, 0, 0, 0);
                    break;
            }

            return tn ?? new Thickness(0, 0, 0, 0);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
