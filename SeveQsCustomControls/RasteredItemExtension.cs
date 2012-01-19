using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using LittleHelpers;

namespace SeveQsCustomControls
{
    public class RasteredItemExtension : DependencyObject
    {
        public static readonly DependencyProperty RowProperty =
            DependencyProperty.RegisterAttached("Row", typeof (Int32), typeof (RasteredItemExtension), new PropertyMetadata(default(Int32), RowIndexChanged, CoerceIndex));

        private static object CoerceIndex(DependencyObject d, object basevalue)
        {
            var newval = (int) basevalue;
            return (Math.Max(0, Math.Min(5, newval)));
        }

        private static void RowIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        public static void SetRow(UIElement element, Int32 value)
        {
            element.SetValue(RowProperty, value);
        }

        public static Int32 GetRow(UIElement element)
        {
            return (Int32) element.GetValue(RowProperty);
        }

        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.RegisterAttached("IsExpanded", typeof (bool), typeof (RasteredItemExtension), new PropertyMetadata(default(bool)));

        public static void SetIsExpanded(UIElement element, bool value)
        {
            element.SetValue(IsExpandedProperty, value);
        }

        public static bool GetIsExpanded(UIElement element)
        {
            return (bool) element.GetValue(IsExpandedProperty);
        }
    }
}
