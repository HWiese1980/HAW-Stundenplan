using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Reflection;
using System.ComponentModel;

using LittleHelpers;

using HAWHelper = HAW_Tool.HAW.Helper;
using LHelper = LittleHelpers.Helper;
using HAW_Tool.Aspects;


namespace HAW_Tool.HAW
{
    public class TimeToCoord : IMultiValueConverter
    {
        private double mMultiply = 20.0D; public double Multiplier { get { return mMultiply; } set { mMultiply = value; } }
        private bool mWidth = false; public bool AsWidth { get { return mWidth; } set { mWidth = value; } }

        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (LHelper.IsInDesignModeStatic) return null;

            var tEvents = from p in values
                          where p is IEvent
                          select p;

            IEvent tEvt = null;
            try
            {
                tEvt = (IEvent)tEvents.Single();
            }
            catch
            {
                Console.WriteLine("No event found in this Object");
                return 0.0D;
            }

            double tCoord = 0.0D;

            if (parameter == null)
            {
                TimeSpan tStart = tEvt.From, tEnd = tEvt.Till;
                tCoord = (tStart.Hours - 7) * mMultiply;
                double tMinutesStart = tStart.Minutes / 60.0F;
                double tMinutesEnd = tEnd.Minutes / 60.0F;
                tCoord += tMinutesStart * mMultiply;

                if (mWidth)
                {
                    tCoord = ((tEnd.Hours - 7) * mMultiply) - tCoord;
                    tCoord += tMinutesEnd * mMultiply;
                }
            }
            else
            {
                String tPropName = parameter as String;
                if (tPropName == null) throw new ArgumentException("Parameter must be String");
                PropertyInfo tProp = tEvt.GetType().GetProperty(tPropName);
                object tObj = tProp.GetValue(tEvt, null);
                if (!(tObj is int)) throw new ArgumentException("Property described by Parameter must be of type Int");
                int tValue = (int)tObj;

                tCoord = tValue * mMultiply;
            }

            return tCoord;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
