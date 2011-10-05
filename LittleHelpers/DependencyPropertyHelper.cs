using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Threading;
using System.Windows.Threading;

namespace LittleHelpers
{
    /// <summary>
    /// Gives you a place to host a DependencyProperty without having the owner 
    /// inherit from DependencyObject
    /// </summary>
    /// <typeparam name="T">The type of DependencyProperty</typeparam>
    public class DependencyPropertyHelper<T> : DependencyObject
    {
        #region AddBinding

        public static void AddBinding(string path, object source, BindingMode mode,
            DependencyObject target, DependencyProperty property)
        {
            AddBinding(path, source, mode, target, property, null);
        }

        public static void AddBinding(string path, object source, BindingMode mode,
            DependencyObject target, DependencyProperty property,
            IValueConverter converter)
        {
            var binding = new Binding(path) {Source = source, Mode = mode, Converter = converter};
            BindingOperations.SetBinding(target, property, binding);
        }

        #endregion

        #region ValueChanged Event Handler

        private static void OnValueChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var helper = (d as DependencyPropertyHelper<T>);
            Debug.Assert(helper != null, "helper != null");
            if (helper.ValueChanged != null)
                helper.ValueChanged(d, e);
        }

        #endregion

        #region ValueChanged Event

        /// <summary>
        /// This event is tied to the DependencyProperty Value changed event.
        /// </summary>
        public event DependencyPropertyChangedEventHandler ValueChanged;

        #endregion

        #region Value

        public readonly static DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof(T), typeof(DependencyPropertyHelper<T>),
            new PropertyMetadata(OnValueChanged));

        /// <summary>
        /// This is a DependencyProperty
        /// </summary>
        public T Value
        {
            get
            {
                try
                {
                    if (!Dispatcher.CheckAccess())
                        return (T)Application.Current.Dispatcher.Invoke(
                            DispatcherPriority.Background,
                            (DispatcherOperationCallback)delegate
                        {
                            return GetValue(ValueProperty);
                        }, ValueProperty);
                    return (T)GetValue(ValueProperty);
                }
                catch
                {
                    return (T)ValueProperty.DefaultMetadata.DefaultValue;
                }
            }
            set
            {
                if (!Dispatcher.CheckAccess())
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                        (SendOrPostCallback)delegate { SetValue(ValueProperty, value); },
                        value);
                else
                    SetValue(ValueProperty, value);
            }
        }

        #endregion
    }
}
