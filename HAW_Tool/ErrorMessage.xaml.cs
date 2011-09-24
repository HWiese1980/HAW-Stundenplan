using System;
using System.Windows;

namespace HAW_Tool
{
    /// <summary>
    /// Interaction logic for ErrorMessage.xaml
    /// </summary>
    public partial class ErrorMessage
    {
		#region Constructors (1) 

        public ErrorMessage(string label, Exception exception)
        {
            InitializeComponent();

            ErrorLabel.Text = label;
            ErrorMessageText.Text = String.Format("{0}\n\nAT\n\n{1}", exception.Message, exception.StackTrace);
        }

		#endregion Constructors 

		#region Methods (1) 

		// Private Methods (1) 

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

		#endregion Methods 
    }
}
