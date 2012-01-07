using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace HAW_Tool.HAW.Depending
{
    public class RowIndex : DependencyObject
    {
        public static readonly DependencyProperty RowProperty =
            DependencyProperty.RegisterAttached("Row", typeof (Int32), typeof (RowIndex), new PropertyMetadata(default(Int32), RowIndexChanged, CoerceIndex));

        private static object CoerceIndex(DependencyObject d, object basevalue)
        {
            var newval = (int) basevalue;
            return (Math.Max(0, Math.Min(5, newval)));
        }

        private static void RowIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Console.WriteLine("Event {0} RowIndex changed from {1} to {2}", d, e.OldValue, e.NewValue);
        }

        public static void SetRow(UIElement element, Int32 value)
        {
            element.SetValue(RowProperty, value);
        }

        public static Int32 GetRow(UIElement element)
        {
            return (Int32) element.GetValue(RowProperty);
        }
    }
}
