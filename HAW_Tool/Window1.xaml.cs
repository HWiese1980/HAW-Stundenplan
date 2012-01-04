using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using HAW_Tool.HAW;
using System.ComponentModel;
using SeveQsCustomControls;
using System.Reflection;
using System.Printing;
using LittleHelpers;
using System.Threading;
using Microsoft.Win32;
using HAW_Tool.HAW.REST;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using HAW_Tool.Properties;

// Version 0.0.1: {48119271-97D3-4E03-9D11-58A18F31D5D0}
// Version 0.9.1: {95BE38A7-8FAC-4226-839D-247E3463E474}

namespace HAW_Tool
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        private const int WmSyscommand = 0x112;
        private HwndSource _mHWnd;

        private enum ResizeDirection
        {
            Left = 1,
            Right = 2,
            Top = 3,
            TopLeft = 4,
            TopRight = 5,
            Bottom = 6,
            BottomLeft = 7,
            BottomRight = 8,
        }

        private void ResizeWindow(ResizeDirection direction)
        {
            SendMessage(_mHWnd.Handle, WmSyscommand, (IntPtr)(61440 + direction), IntPtr.Zero);
        }

        public Window1()
        {
            InitializeComponent();

            SourceInitialized += SourceInitializedHandler;

            PlanFile.StatusMessageChanged += PlanFileStatusChanged;
            PlanFile.StatusProgressChanged += PlanFileProgressChanged;

            Loaded += StartUp;
            Closing += ClosingAppSaveSettings;
        }

        private void SourceInitializedHandler(object sender, EventArgs e)
        {
            _mHWnd = PresentationSource.FromVisual((Visual)sender) as HwndSource;
        }

        public static Window1 MainWindow
        {
            get
            {
                if (Application.Current == null) return null;
                return (Window1)Application.Current.MainWindow;
            }
        }

        public string UserName
        {
            get
            {
                var tSet = new Settings();
                if (tSet.Username != String.Empty) return tSet.Username;
                return Environment.UserName;
            }
            set
            {
                var tSet = new Settings { Username = value };
                tSet.Save();
            }
        }

        private void PlanFileStatusChanged(object sender, ValueEventArgs<string> e)
        {
            Status = e.Value;
        }

        private void PlanFileProgressChanged(object sender, ValueEventArgs<int> e)
        {
            ProgressValue = e.Value;
        }

        private void StartUp(object sender, EventArgs e)
        {
            LoadSettings();
            Thread.Sleep(1000);
            LoadPlanFile();
        }

        private void LoadSettings()
        {
            var tWrk = new BackgroundWorker();

            tWrk.DoWork += (x, y) =>
            {
                try
                {
                    string tVerStr = new HAWClient().Version();
                    if (tVerStr == null) return;

                    var tVer = new Version(tVerStr);
                    var tMe = Assembly.GetExecutingAssembly().GetName().Version;
                    if (tMe.IsNewerThan(tVer))
                    {
                        Dispatcher.Invoke(new Action(() => new NewVersionNotify().ShowDialog()));
                    }
                }
                catch (Exception)
                { }
            };

            tWrk.RunWorkerAsync();

            version.Content = String.Format("Version {0}", Assembly.GetExecutingAssembly().GetName().Version);

            var tSet = new Settings();
            SetValue(IsGroupFilterActiveProperty, tSet.FilterByGroup);
        }

        void ClosingAppSaveSettings(object sender, EventArgs e)
        {
            if (PlanFile.Instance != null) PlanFile.Instance.SaveSettings();
        }

        public bool IsGroupFilterActive
        {
            get { return (bool)GetValue(IsGroupFilterActiveProperty); }
            set { SetValue(IsGroupFilterActiveProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsGroupFilterActive.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsGroupFilterActiveProperty =
            DependencyProperty.Register("IsGroupFilterActive", typeof(bool), typeof(Window1), new UIPropertyMetadata(false, (d, o) =>
            {
                var tSet = new Settings { FilterByGroup = (bool)o.NewValue };
                tSet.Save();
            }));

        private string Status
        {
            set
            {
                try
                {
                    Dispatcher.Invoke(new Action(() =>
                        {
                            MainStatus.Content = value;
                        }));
                }
                catch { }
            }
        }

        private int ProgressValue
        {
            set
            {
                Dispatcher.Invoke(new Action(() =>
                    {
                        if (progressStatusBar.Visibility == Visibility.Visible)
                        {
                            progressStatusBar.Value = value;
                        }
                    }));
            }
        }

        private bool Progress
        {
            set
            {
                Dispatcher.Invoke(new Action(() =>
                    {
                        progressStatusBar.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                    }));
            }
        }


        /*
                private void Button_Click(object sender, RoutedEventArgs e)
                {
                    LoadPlanFile();
                }
        */

        private void LoadPlanFile()
        {
            var tWrk = new BackgroundWorker();
            tWrk.DoWork += (tWrkSender, tWrkE) =>
            {
                Status = "Lade Stundenplan. Dauert 'nen Augenblick.";
                Progress = true;
                PlanFile.Initialize();

                string[] urls;
                if (LittleHelpers.Helper.HasInternetConnection)
                {
                    urls = new HAWClient().Schedules();
                    new Settings().ScheduleFiles = String.Join(" ", urls);
                }
                else
                {
                    urls = new Settings().ScheduleFiles.Split(' ');
                }

                foreach (var tURL in urls)
                {
                    try
                    {
                        if (tURL != String.Empty && Uri.IsWellFormedUriString(tURL, UriKind.Absolute))
                        {
                            Status = "Lade URL: " + tURL;
                            PlanFile.LoadTxt(new Uri(tURL));
                        }
                        else
                        {
                            MessageBox.Show(String.Format("URL {0} ist nicht gültig", tURL));
                        }
                    }
                    catch
                    {
                        MessageBox.Show(String.Format("Ein Fehler trat beim Parsen der URL {0} auf", tURL));
                        tWrkE.Cancel = true;
                    }
                }

                // PlanFile.LoadGoogle();

                PlanFile.Instance.LoadComplete();
            };
            tWrk.RunWorkerCompleted += (tWrkSender, tWrkE) =>
            {
                if (!tWrkE.Cancelled)
                {
                    Progress = false;
                    Status = "Fertig!";
                    DataContext = PlanFile.Instance;
                }
            };

            tWrk.RunWorkerAsync();
        }

        private void PrintWeekButtonPressed(object sender, RoutedEventArgs e)
        {

            var tDlg = new PrintDialog { PrintQueue = LocalPrintServer.GetDefaultPrintQueue() };
            tDlg.PrintTicket = tDlg.PrintQueue.DefaultPrintTicket;
            tDlg.PrintTicket.PageOrientation = PageOrientation.Landscape;
            tDlg.PrintTicket.PageResolution = new PageResolution(2400, 2400);
            tDlg.PageRangeSelection = PageRangeSelection.AllPages;
            tDlg.UserPageRangeEnabled = true;

            if ((bool)tDlg.ShowDialog())
            {

                var tBut = sender as DependencyObject;
                var tPrintVis = tBut.GetParent<Grid>().GetChildren<RasteredGroup>().First();

                var tCaps = tDlg.PrintQueue.GetPrintCapabilities(tDlg.PrintTicket);
                Debug.Assert(tCaps.PageImageableArea != null, "tCaps.PageImageableArea != null");
                var scale = Math.Min(tCaps.PageImageableArea.ExtentWidth / tPrintVis.ActualWidth,
                    tCaps.PageImageableArea.ExtentHeight / tPrintVis.ActualHeight);

                var tBrush = new VisualBrush(tPrintVis);

                var tGrid = new Border
                                {
                                    Width = tPrintVis.ActualWidth,
                                    Height = tPrintVis.ActualHeight,
                                    HorizontalAlignment = HorizontalAlignment.Stretch,
                                    VerticalAlignment = VerticalAlignment.Stretch
                                };

                var tRect = new Rectangle
                                {
                                    Margin = new Thickness(30.0D),
                                    //Width = tPrintVis.ActualWidth,
                                    //Height = tPrintVis.ActualHeight,
                                    HorizontalAlignment = HorizontalAlignment.Stretch,
                                    VerticalAlignment = VerticalAlignment.Stretch,
                                    Fill = tBrush
                                };

                tGrid.Child = tRect;
                tGrid.LayoutTransform = new ScaleTransform(scale, scale);


                var sz = new Size(tCaps.PageImageableArea.ExtentWidth, tCaps.PageImageableArea.ExtentHeight);
                tGrid.Measure(sz);
                tGrid.Arrange(new Rect(new Point(tCaps.PageImageableArea.OriginWidth, tCaps.PageImageableArea.OriginHeight), sz));

                try
                {
                    tDlg.PrintVisual(tGrid, "HAW Stundenplan Tool Wochenausdruck");
                }
                catch (Exception exp)
                {
                    MessageBox.Show(String.Format("Beim Drucken trat ein Fehler auf: {0}", exp.Message), "Drucken", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RasteredItemsControlSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Console.WriteLine(@"Selection changed: {0}", e.AddedItems.Count);
            if (e.AddedItems.Count <= 0)
                _mSelectedEvent = default(Event);
            else
                _mSelectedEvent = (IEvent)e.AddedItems[0];
        }

        private void CbIsFilteredClick(object sender, RoutedEventArgs e)
        {
            foreach (var tCtl in this.GetChildren<RasteredItemsControl>()) RefreshFiltering(tCtl);
        }

        private void RefreshFiltering(RasteredItemsControl ctl)
        {
            ctl.Items.Filter = obj =>
            {
                var tObj = obj as IEvent;
                if (!IsGroupFilterActive) return true;

                var tBaseEvents = from evt in PlanFile.Instance.KnownBaseEvents
                                  where evt.BasicCode == tObj.BasicCode && (evt.Groups.Count() <= 1 || evt.Group == GroupID.Empty || evt.Group == tObj.Group || (!tObj.Group.IsSingleGroup && tObj.Group.Value.Split('+').Contains(evt.Group.Value)))
                                  select evt;

                return (tBaseEvents.Count() > 0);
            };
        }

        private void RasteredItemsControlLoaded(object sender, RoutedEventArgs e)
        {
            var tCtl = sender as RasteredItemsControl;
            if (tCtl == null) return;

            RefreshFiltering(tCtl);
        }

        private void ExportEventsClick(object sender, RoutedEventArgs e)
        {
            if (PlanFile.Instance == null)
            {
                MessageBox.Show("Bevor du irgendwas exportieren kannst, musst du natürlich erstmal etwas laden!");
                return;
            }

            var tGrp = SemGroups.SelectedItem as SeminarGroup;
            if (tGrp == null)
            {
                if (MessageBox.Show("Eine Seminargruppe muss schon ausgewählt sein. Was soll sonst exportiert werden? Alle Semestergruppen? Das willst du nicht, glaub mir!\nOder doch?", "Alles exportieren?", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.No) return;
            }

            var tDlg = new SaveFileDialog {Filter = "iCalendar Datei (*.ics)|*.ics"};

            if ((bool)tDlg.ShowDialog())
            {
                PlanFile.Instance.ExportAs(tGrp, ExportType.iCal, tDlg.FileName);
            }
        }

        IEvent _mSelectedEvent = default(IEvent);
        private void SelectedEventShowHideClick(object sender, RoutedEventArgs e)
        {
            if (_mSelectedEvent == default(Event))
            {
                MessageBox.Show("Ein Event muss schon ausgewählt sein, damit alle anderen ein- bzw. ausgeblendet werden können.");
                return;
            }

            string tTag = ((FrameworkElement)sender).Tag.ToString();
            var tEvents = from evt in PlanFile.Instance.AllEvents
                          where evt.Code == _mSelectedEvent.Code
                          select evt;
            foreach (Event tEvt in tEvents)
            {
                tEvt.IsEnabled = (tTag == "Show");
            }
        }

        /*

        TimeSpan? _tOldFrom, _tOldTill;

        private const double MGrid = 5.0D;

        private void EventDragging(object sender, ValueEventArgs<DraggingItem> e)
        {
            if (!(_tOldFrom.HasValue | _tOldTill.HasValue)) return;

            var minute = (int)(Math.Round(e.Value.Distance / MGrid) * MGrid);

            var tEvt = e.Value.Item.Content as IEvent;
            if (tEvt is RESTEvent) return;

            var tDist = new TimeSpan(0, minute, 0);
            Debug.Assert(tEvt != null, "tEvt != null");
            Debug.Assert(_tOldFrom != null, "_tOldFrom != null");
            tEvt.From = _tOldFrom.Value.Add(tDist);
            Debug.Assert(_tOldTill != null, "_tOldTill != null");
            tEvt.Till = _tOldTill.Value.Add(tDist);
        }

        private void EventDraggingStart(object sender, ValueEventArgs<ListBoxItem> e)
        {
            var tEvt = e.Value.Content as IEvent;
            Debug.Assert(tEvt != null, "tEvt != null");
            _tOldFrom = tEvt.From;
            _tOldTill = tEvt.Till;
        }

        private void EventDraggingEnd(object sender, EventArgs e)
        {
            _tOldFrom = null; _tOldTill = null;
        }

        private void CopyHashClick(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(_mSelectedEvent.Hash);
        }
        */

        #region Window Resize

        private void ResizeInit(object sender, MouseButtonEventArgs e)
        {
            var rect = sender as Rectangle;
            if (rect == null) return;

            switch (rect.Tag.ToString())
            {
                case "N": ResizeWindow(ResizeDirection.Top); break;
                case "NE": ResizeWindow(ResizeDirection.TopRight); break;
                case "E": ResizeWindow(ResizeDirection.Right); break;
                case "SE": ResizeWindow(ResizeDirection.BottomRight); break;
                case "S": ResizeWindow(ResizeDirection.Bottom); break;
                case "SW": ResizeWindow(ResizeDirection.BottomLeft); break;
                case "W": ResizeWindow(ResizeDirection.Left); break;
                case "NW": ResizeWindow(ResizeDirection.TopLeft); break;
            }
        }

        #endregion

        private void TabControlFadeSelection(object sender, SelectionChangedEventArgs e)
        {
            return;

            //if (tabControl.ActualHeight <= 0.0 || tabControl.ActualWidth <= 0.0) return;


            //RenderTargetBitmap tBmp = new RenderTargetBitmap((int)tabControl.ActualWidth, (int)tabControl.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            //tBmp.Render(tabControl);
            //tabFadeBorder.Background = new ImageBrush(tBmp);

            //tabFadeBorder.Visibility = Visibility.Visible;

            //Storyboard tBoard = Window1.MainWindow.FindResource("tabFadeStoryboard") as Storyboard;
            //tBoard.Begin();
        }

        private void FramelessTitleBarDoubleClick(object sender, RoutedEventArgs e)
        {
            switch (WindowState)
            {
                case WindowState.Normal: WindowState = WindowState.Maximized; break;
                case WindowState.Maximized: WindowState = WindowState.Normal; break;
            }
        }

        private void DonateButtonClick(object sender, RoutedEventArgs e)
        {
            string url = "";
            const string business = "7.e.q@syncro-community.de";
            const string description = "HAW%20Stundenplantool%20Spende";
            const string country = "DE";
            const string currency = "EUR";

            url += "https://www.paypal.com/cgi-bin/webscr" +
                "?cmd=" + "_donations" +
                "&business=" + business +
                "&lc=" + country +
                "&item_name=" + description +
                "&currency_code=" + currency +
                "&bn=" + "PP%2dDonationsBF";

            Process.Start(url);
        }

        private void MailMe(object sender, RoutedEventArgs e)
        {

        }

        private void TabControlFadeSetBackground(object sender, MouseButtonEventArgs e)
        {
        }

        private void TabFadeCompleted(object sender, EventArgs e)
        {
            tabFadeBorder.Visibility = Visibility.Collapsed;
        }
    }
}
