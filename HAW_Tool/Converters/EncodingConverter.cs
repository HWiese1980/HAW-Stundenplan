using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using LittleHelpers;
using System.IO;

namespace HAW_Tool.Converters
{
    public class EncodingConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string tParam = (string)parameter;
            string[] tEncodingStrings = tParam.Split(';');

            var tValidEncodings = from p in Encoding.GetEncodings()
                                  select p.GetEncoding().WebName;



            var tEncodings = from p in tEncodingStrings
                             select Encoding.GetEncoding(p);
            
            Queue<Encoding> tEncodingQueue = new Queue<Encoding>(tEncodings);

            string tVal = value as string;
            while (true)
            {
                if (tEncodingQueue.Count <= 1) break;

                Encoding tFrom = tEncodingQueue.Dequeue();
                Encoding tTo = tEncodingQueue.Peek();

                tVal = tVal.ConvertEncoding(tFrom, tTo);
            }

            return tVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
