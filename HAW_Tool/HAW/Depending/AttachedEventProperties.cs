using System.Windows;

namespace HAW_Tool.HAW.Depending
{
    public class AttachedEventProperties
    {
        public static readonly DependencyProperty IsModifiedProperty =
            DependencyProperty.RegisterAttached("IsModified", typeof (bool), typeof (AttachedEventProperties), new PropertyMetadata(default(bool)));

        public static void SetIsModified(UIElement element, bool value)
        {
            element.SetValue(IsModifiedProperty, value);
        }

        public static bool GetIsModified(UIElement element)
        {
            return (bool) element.GetValue(IsModifiedProperty);
        }
    }
}
