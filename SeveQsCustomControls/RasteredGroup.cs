using System;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;

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
    ///     <MyNamespace:RasteredGroup/>
    ///
    /// </summary>
    public class RasteredGroup : ContentControl, INotifyPropertyChanged
    {
        static RasteredGroup()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RasteredGroup), new FrameworkPropertyMetadata(typeof(RasteredGroup)));
        }

        #region INotifyPropertyChanged Members

        private void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(RasteredGroup), new UIPropertyMetadata(null, OnSelectedItemChanged));

        static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var oldItem = e.OldValue as ISelectable;
            var newItem = e.NewValue as ISelectable;

            Console.WriteLine("++++ RasteredGroup ++++");
            Console.WriteLine("Unselecting {0} - Selecting {1}", oldItem, newItem);
            Console.WriteLine("+++++++++++++++++++++++");

            if (oldItem != null) oldItem.IsSelected = false;
            if (newItem != null) newItem.IsSelected = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
