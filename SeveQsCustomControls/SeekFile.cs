using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
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
    ///     "Add Reference"->"Projects"->[Select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:CustomControl1/>
    ///
    /// </summary>
    public class SeekFile : HeaderedContentControl
    {
        private static RoutedCommand _mBrowseButtonCommand;

        public static RoutedCommand BrowseButtonCommand
        {
            get
            {
                return _mBrowseButtonCommand;
            }
        }

        static SeekFile()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SeekFile), new FrameworkPropertyMetadata(typeof(SeekFile)));

            Helper.InstallCommand(ref _mBrowseButtonCommand, "BrowseButtonCommand", typeof(SeekFile), OnBrowseButtonCommand, OnBrowseButtonCanExecute);
        }

        private static void OnBrowseButtonCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var tSF = sender as SeekFile;
            if (tSF != null) tSF.OnBrowseButton();
        }

        private static void OnBrowseButtonCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void OnBrowseButton()
        {
            var tDlg = new OpenFileDialog {Filter = Filter};
            if(!(bool)tDlg.ShowDialog()) return;

            Content = tDlg.FileName;
        }

        public static readonly DependencyProperty FilterProperty = DependencyProperty.Register("Filter", typeof(string), typeof(SeekFile));

        public string Filter
        {
            get { return (String)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }
    }
}
