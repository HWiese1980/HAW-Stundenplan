using System.Windows;

namespace HAW_Tool.WPF
{
    public class ConverterWrapper : DependencyObject
    {


        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(ConverterWrapper), new UIPropertyMetadata(null));


    }
}
