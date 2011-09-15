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

namespace HAW_Tool
{
    /// <summary>
    /// Interaction logic for ErrorMessage.xaml
    /// </summary>
    public partial class ErrorMessage : Window
    {
		#region Constructors (1) 

        public ErrorMessage(string Label, Exception exception)
        {
            InitializeComponent();

            ErrorLabel.Text = Label;
            ErrorMessageText.Text = String.Format("{0}\n\nAT\n\n{1}", exception.Message, exception.StackTrace);
        }

		#endregion Constructors 

		#region Methods (1) 

		// Private Methods (1) 

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

		#endregion Methods 
    }
}
