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
    ///     <MyNamespace:FramelessTitleBar/>
    ///
    /// </summary>
    [TemplatePart(Name = "PART_Title", Type = typeof(Label))]
    public class FramelessTitleBar : Control
    {
        static RoutedCommand mMinCmd, mMaxCmd, mCloseCmd;

        public static RoutedCommand MinimizeCommand
        {
            get { return mMinCmd; }
        }

        public static RoutedCommand MaximizeCommand
        {
            get { return mMaxCmd; }
        }

        public static RoutedCommand CloseCommand
        {
            get { return mCloseCmd; }
        }

        static FramelessTitleBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FramelessTitleBar), new FrameworkPropertyMetadata(typeof(FramelessTitleBar)));

            Helper.InstallCommand(ref mMinCmd, "MinimizeCommand", typeof(Button), OnCommand, OnCanExecuteCommand);
            Helper.InstallCommand(ref mMaxCmd, "MaximizeCommand", typeof(Button), OnCommand, OnCanExecuteCommand);
            Helper.InstallCommand(ref mCloseCmd, "CloseCommand", typeof(Button), OnCommand, OnCanExecuteCommand);
        }

        protected override void OnPreviewMouseDoubleClick(MouseButtonEventArgs e)
        {
            RaiseDoubleClickEvent();
            e.Handled = true;
        }

        private void RaiseDoubleClickEvent()
        {
            RoutedEventArgs e = new RoutedEventArgs(FramelessTitleBar.DoubleClickEvent);
            RaiseEvent(e);
        }

        public static readonly RoutedEvent DoubleClickEvent = EventManager.RegisterRoutedEvent("DoubleClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FramelessTitleBar));

        public event RoutedEventHandler DoubleClick
        {
            add { AddHandler(DoubleClickEvent, value); }
            remove { RemoveHandler(DoubleClickEvent, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            Label tLab = base.GetTemplateChild("PART_Title") as Label;
            if (Helper.IsInDesignModeStatic)
            {
                tLab.Content = "Designmode - no Window Title available";
                return;
            }

            Binding tBind = new Binding();
            tBind.Source = this.GetWindow();
            tBind.Path = new PropertyPath("Title");
            tBind.Mode = BindingMode.OneWay;

            tLab.SetBinding(ContentControl.ContentProperty, tBind);
        }

        private static void OnCommand(object sender, ExecutedRoutedEventArgs e)
        {
            RoutedCommand tCmd = e.Command as RoutedCommand;
            FramelessTitleBar tBar = (sender as DependencyObject).GetParent<FramelessTitleBar>();

            switch (tCmd.Name)
            {
                case "CloseCommand": tBar.OnClose(); break;
                case "MinimizeCommand": tBar.SetWindowState(WindowState.Minimized); break;
                case "MaximizeCommand": tBar.SetWindowState(WindowState.Maximized); break;
                default: break;
            }
        }

        Point mOldWindowPosition = new Point();

        private void SetWindowState(WindowState windowState)
        {
            Window tWnd = this.GetWindow();

            if (windowState == WindowState.Maximized & tWnd.WindowState == WindowState.Maximized)
            {
                tWnd.WindowState = WindowState.Normal;
                tWnd.Left = mOldWindowPosition.X;
                tWnd.Top = mOldWindowPosition.Y;
                return;
            }
            else
            {
                mOldWindowPosition.X = tWnd.Left;
                mOldWindowPosition.Y = tWnd.Top;
            }

            tWnd.WindowState = windowState;
        }

        private void OnClose()
        {
            Window tWnd = this.GetWindow();
            if (tWnd == Application.Current.MainWindow) Application.Current.Shutdown();

            tWnd.Close();
        }

        private static void OnCanExecuteCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            RoutedCommand tCmd = e.Command as RoutedCommand;
            switch (tCmd.Name)
            {
                default: e.CanExecute = true; break;
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            Window tWindow = this.GetWindow();
            tWindow.DragMove();
        }
    }
}
