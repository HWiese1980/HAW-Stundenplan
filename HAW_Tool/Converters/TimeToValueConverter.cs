using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Markup;

namespace HAW_Tool.Converters
{
    public class TimeToValueConverter : MarkupExtension, IValueConverter
    {
        public TimeToValueConverter()
        {
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public TimeSpan MinTime { private get; set; }
        public double SizeOfSlider { private get; set; }
        public TimeSpan MaxTime { private get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var time = (TimeSpan) value;
            var diff = time - MinTime;
            var timeDeltaPerStep = (MaxTime - MinTime).TotalSeconds/SizeOfSlider;

            var steps = (long)(diff.TotalSeconds / timeDeltaPerStep);


            return steps;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture)
        {
            var steps = (long) value;
            var timeDeltaPerStep = (MaxTime - MinTime).TotalSeconds / SizeOfSlider;

            var diff = timeDeltaPerStep * steps;
            var add = TimeSpan.FromSeconds(diff);

            var time = MinTime.Add(add);


            return time;
        }
    }
}
