using System.Windows;

namespace HAW_Tool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
		#region Methods (2) 

		// Protected Methods (1) 

        protected override void OnStartup(StartupEventArgs e)
        {
#if !DEBUG
            // Application.Current.DispatcherUnhandledException += ExceptionHandling;
#endif
            base.OnStartup(e);
        }
		// Private Methods (1) 

/*
        void ExceptionHandling(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ErrorMessage tMsg = new ErrorMessage("Ein Fehler trat während der Ausführung des Programms auf. Es muss beendet werden. Bitte wende dich mit folgender Fehlermeldung an den Entwickler!", e.Exception);
            tMsg.ShowDialog();

            if (Helper.HasInternetConnection)
            {
                HAWClient tCnt = new HAWClient();
                tCnt.ReportException(e.Exception);
            }

            Application.Current.Shutdown(1);
        }
*/

		#endregion Methods 
    }
}
