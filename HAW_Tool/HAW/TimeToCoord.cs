using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Data;
using LHelper = LittleHelpers.Helper;


namespace HAW_Tool.HAW
{
    public class TimeToCoord : IMultiValueConverter
    {
        private double _mMultiply = 20.0D;

        private bool _mWidth;

        public double Multiplier
        {
            get { return _mMultiply; }
            set { _mMultiply = value; }
        }

        public bool AsWidth
        {
            get { return _mWidth; }
            set { _mWidth = value; }
        }

        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (LHelper.IsInDesignModeStatic) return null;

            IEnumerable<object> tEvents = from p in values
                                          where p is IEvent
                                          select p;

            IEvent tEvt;
            try
            {
                tEvt = (IEvent) tEvents.Single();
            }
            catch
            {
                Console.WriteLine(@"No event found in this Object");
                return 0.0D;
            }

            double tCoord;

            if (parameter == null)
            {
                TimeSpan tStart = tEvt.From, tEnd = tEvt.Till;
                tCoord = (tStart.Hours - 7)*_mMultiply;
                double tMinutesStart = tStart.Minutes/60.0F;
                double tMinutesEnd = tEnd.Minutes/60.0F;
                tCoord += tMinutesStart*_mMultiply;

                if (_mWidth)
                {
                    tCoord = ((tEnd.Hours - 7)*_mMultiply) - tCoord;
                    tCoord += tMinutesEnd*_mMultiply;
                }
            }
            else
            {
                var tPropName = parameter as String;
                if (tPropName == null) throw new ArgumentException("Parameter must be String");
                PropertyInfo tProp = tEvt.GetType().GetProperty(tPropName);
                object tObj = tProp.GetValue(tEvt, null);
                if (!(tObj is int)) throw new ArgumentException("Property described by Parameter must be of type Int");
                var tValue = (int) tObj;

                tCoord = tValue*_mMultiply;
            }

            return tCoord;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}