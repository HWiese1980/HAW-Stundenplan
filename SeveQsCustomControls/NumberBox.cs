using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LittleHelpers;

namespace SeveQsCustomControls
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:SeveQsCustomControls"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:SeveQsCustomControls;assembly=SeveQsCustomControls"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:NumberBox/>
    ///
    /// </summary>
    public class NumberBox : HeaderedContentControl
    {
        static NumberBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumberBox), new FrameworkPropertyMetadata(typeof(NumberBox)));

            Helper.InstallCommand(ref mChangeValueCommand, "IncreaseValueCommand", typeof(Button), CommandJunction, (s, e) => { e.CanExecute = true; });
        }

        private static readonly RoutedCommand mChangeValueCommand;
        public static RoutedCommand ChangeValueCommand
        {
            get { return mChangeValueCommand; }
        }

        private static void CommandJunction(object sender, ExecutedRoutedEventArgs e)
        {
            // RoutedCommand tCmd = e.Command as RoutedCommand;
            Control tSource = e.Source as Control;
            NumberBox tBox = tSource.GetParent<NumberBox>();
            switch (tSource.Tag as String)
            {
                case "Increase": tBox.Value += tBox.SmallChange; break;
                case "Decrease": tBox.Value -= tBox.SmallChange; break;
                default: break;
            }
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(NumberBox), new FrameworkPropertyMetadata(new PropertyChangedCallback(CheckValue)));
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register("Maximum", typeof(double), typeof(NumberBox), new FrameworkPropertyMetadata(10.0D));
        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register("Minimum", typeof(double), typeof(NumberBox), new FrameworkPropertyMetadata(0.0D));
        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public static readonly DependencyProperty LargeChangeProperty = DependencyProperty.Register("LargeChange", typeof(double), typeof(NumberBox), new FrameworkPropertyMetadata(3.0));
        public double LargeChange
        {
            get { return (double)GetValue(LargeChangeProperty); }
            set { SetValue(LargeChangeProperty, value); }
        }

        public static readonly DependencyProperty SmallChangeProperty = DependencyProperty.Register("SmallChange", typeof(double), typeof(NumberBox), new FrameworkPropertyMetadata(1.0));
        public double SmallChange
        {
            get { return (double)GetValue(SmallChangeProperty); }
            set { SetValue(SmallChangeProperty, value); }
        }

        private static void CheckValue(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            NumberBox tBox = sender as NumberBox;

            Console.WriteLine("NumberBox - Value Changed: {0} -> {1}", e.OldValue, e.NewValue);

            if ((double)e.NewValue > tBox.Maximum) tBox.Value = tBox.Maximum;
            if ((double)e.NewValue < tBox.Minimum) tBox.Value = tBox.Minimum;
        }


    }
}
