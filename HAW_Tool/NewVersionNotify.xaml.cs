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
using System.Windows.Shapes;
using System.Diagnostics;

namespace HAW_Tool
{
    /// <summary>
    /// Interaction logic for NewVersionNotify.xaml
    /// </summary>
    public partial class NewVersionNotify : Window
    {
        public NewVersionNotify()
        {
            InitializeComponent();
        }

        private void CloseMeClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OpenLinkClick(object sender, RoutedEventArgs e)
        {
            Hyperlink tLnk = (Hyperlink)sender;
            Process.Start(new ProcessStartInfo(tLnk.NavigateUri.ToString()));
        }
    }
}
