using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using System.Windows.Data;

namespace HAW_Tool.Converters
{
    public class CompareConverter : MarkupExtension, IValueConverter
    {
        public CompareConverter()
        {}

        public int Value { get; set; }

        public CompareConverterOperator Operator { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if(!(value is int)) throw new Exception("Value must be integer");
            switch(Operator)
            {
                case CompareConverterOperator.Equal:
                    return (int)value == Value;
                case CompareConverterOperator.LessThan:
                    return (int)value < Value;
                case CompareConverterOperator.GreaterThan:
                    return (int)value > Value;
                case CompareConverterOperator.LessOrEqual:
                    return (int)value <= Value;
                case CompareConverterOperator.GreaterOrEqual:
                    return (int)value >= Value;
                case CompareConverterOperator.NotEqual:
                    return (int)value != Value;
                default:
                    throw new Exception("Wrong operator provided");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public enum CompareConverterOperator
    {
        LessThan,
        GreaterThan,
        Equal,
        LessOrEqual,
        GreaterOrEqual,
        NotEqual
    }
}
