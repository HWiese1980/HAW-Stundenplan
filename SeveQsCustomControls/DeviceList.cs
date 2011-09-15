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
using System.Windows.Controls.Primitives;

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
    ///     <MyNamespace:DeviceList/>
    ///
    /// </summary>
    public class DeviceList : ListBox
    {
        public event RoutedEventHandler AddDevice, RemoveDevice, MountDevice, UnmountDevice, LoadTCFavs;

        static DeviceList()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DeviceList), new FrameworkPropertyMetadata(typeof(DeviceList)));

            Helper.InstallCommand(ref mAddDeviceCommand, "AddDeviceCommand", typeof(UIElement), OnCommand, OnCanExecuteCommand);
            Helper.InstallCommand(ref mRemoveDeviceCommand, "RemoveDeviceCommand", typeof(UIElement), OnCommand, OnCanExecuteCommand);
            Helper.InstallCommand(ref mMountDeviceCommand, "MountDeviceCommand", typeof(UIElement), OnCommand, OnCanExecuteCommand);
            Helper.InstallCommand(ref mUnmountDeviceCommand, "UnmountDeviceCommand", typeof(UIElement), OnCommand, OnCanExecuteCommand);
            Helper.InstallCommand(ref mLoadTCFavsCommand, "LoadTCFavsCommand", typeof(UIElement), OnCommand, OnCanExecuteCommand);
        }

        ItemsPresenter mPresenter;
        public DeviceList()
        {
        }

        private static RoutedCommand mAddDeviceCommand;
        private static RoutedCommand mRemoveDeviceCommand;
        private static RoutedCommand mMountDeviceCommand;
        private static RoutedCommand mUnmountDeviceCommand;
        private static RoutedCommand mLoadTCFavsCommand;

        public static RoutedCommand AddDeviceCommand { get { return mAddDeviceCommand; } }
        public static RoutedCommand RemoveDeviceCommand { get { return mRemoveDeviceCommand; } }
        public static RoutedCommand MountDeviceCommand { get { return mMountDeviceCommand; } }
        public static RoutedCommand UnmountDeviceCommand { get { return mUnmountDeviceCommand; } }
        public static RoutedCommand LoadTCFavsCommand { get { return mLoadTCFavsCommand; } }


        private static void OnCommand(object sender, ExecutedRoutedEventArgs e)
        {
            RoutedCommand tCmd = e.Command as RoutedCommand;
            DeviceList tDevList = (sender as Control).GetParent<DeviceList>();

            tDevList.OnFireEvent(tCmd.Name);

            e.Handled = true;
        }

        private void OnFireEvent(string Command)
        {
            switch (Command)
            {
                case "AddDeviceCommand": if (AddDevice != null) AddDevice(this, new RoutedEventArgs()); break;
                case "RemoveDeviceCommand": if (RemoveDevice != null) RemoveDevice(this, new RoutedEventArgs()); break;
                case "MountDeviceCommand": if (MountDevice != null) MountDevice(this, new RoutedEventArgs()); break;
                case "UnmountDeviceCommand": if (UnmountDevice != null) UnmountDevice(this, new RoutedEventArgs()); break;
                case "LoadTCFavsCommand": if (LoadTCFavs != null) LoadTCFavs(this, new RoutedEventArgs()); break;
                default: break;
            }
        }

        private static void OnCanExecuteCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            RoutedCommand tCmd = e.Command as RoutedCommand;
            switch (tCmd.Name)
            {
                default: break;
            }
        }

    }
}
