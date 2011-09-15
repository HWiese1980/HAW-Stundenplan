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
using HAW_Tool.HAW;
using System.ComponentModel;
using SeveQsCustomControls;
using System.IO.IsolatedStorage;
using System.Xml.Linq;
using System.Xml;
using System.Reflection;
using System.Printing;
using System.Net;
using LittleHelpers;
using SysVersion = System.Version;
using System.Threading;
using Microsoft.Win32;
using HAW_Tool.HAW.REST;
using System.Windows.Media.Animation;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using MyHelper = HAW_Tool.HAW.Helper;
using HAW_Tool.Properties;

// Version 0.0.1: {48119271-97D3-4E03-9D11-58A18F31D5D0}
// Version 0.9.1: {95BE38A7-8FAC-4226-839D-247E3463E474}

namespace HAW_Tool
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private const int WM_SYSCOMMAND = 0x112;
        private HwndSource mHWnd;

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

        private void ResizeWindow(ResizeDirection Direction)
        {
            Console.WriteLine("Resizing: {0} => {1}", Direction, mHWnd.Handle);
            SendMessage(mHWnd.Handle, WM_SYSCOMMAND, (IntPtr)(61440 + Direction), IntPtr.Zero);
        }

        public Window1()
        {
            InitializeComponent();

            this.SourceInitialized += SourceInitializedHandler;

            PlanFile.StatusMessageChanged += PlanFileStatusChanged;
            PlanFile.StatusProgressChanged += PlanFileProgressChanged;

            Loaded += StartUp;
            Closing += ClosingAppSaveSettings;
        }

        private void SourceInitializedHandler(object sender, EventArgs e)
        {
            mHWnd = PresentationSource.FromVisual((Visual)sender) as HwndSource;
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
                HAW_Tool.Properties.Settings tSet = new HAW_Tool.Properties.Settings();
                if (tSet.Username != String.Empty) return tSet.Username;
                return Environment.UserName;
            }
            set
            {
                HAW_Tool.Properties.Settings tSet = new HAW_Tool.Properties.Settings();
                tSet.Username = value;
                tSet.Save();
            }
        }

        private void PlanFileStatusChanged(object sender, ValueEventArgs<string> e)
        {
            this.Status = e.Value;
        }

        private void PlanFileProgressChanged(object sender, ValueEventArgs<int> e)
        {
            this.ProgressValue = e.Value;
        }

        private void StartUp(object sender, EventArgs e)
        {
            LoadSettings();
            Thread.Sleep(1000);
            LoadPlanFile();
        }

        private void LoadSettings()
        {
            BackgroundWorker tWrk = new BackgroundWorker();

            tWrk.DoWork += (x, y) =>
            {
                try
                {
                    string tVerStr = new HAWClient().Version();
                    if (tVerStr == null) return;

                    Version tVer = new Version(tVerStr);
                    Version tMe = Assembly.GetExecutingAssembly().GetName().Version;
                    if (tMe.IsNewerThan(tVer))
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            new NewVersionNotify().ShowDialog();
                        }));
                    }
                }
                catch { }
            };

            tWrk.RunWorkerAsync();

            version.Content = String.Format("Version {0}", Assembly.GetExecutingAssembly().GetName().Version);

            Properties.Settings tSet = new HAW_Tool.Properties.Settings();
            SetValue(IsGroupFilterActiveProperty, tSet.FilterByGroup);
        }

        private void ClosingAppSaveSettings(object sender, EventArgs e)
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
                Properties.Settings tSet = new HAW_Tool.Properties.Settings();
                tSet.FilterByGroup = (bool)o.NewValue;
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
                        if (value)
                        {
                            progressStatusBar.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            progressStatusBar.Visibility = Visibility.Collapsed;
                        }
                    }));
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LoadPlanFile();
        }

        private void LoadPlanFile()
        {
            BackgroundWorker tWrk = new BackgroundWorker();
            tWrk.DoWork += (tWrkSender, tWrkE) =>
            {
                this.Status = "Lade Stundenplan. Dauert 'nen Augenblick.";
                this.Progress = true;
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

                foreach (string tURL in urls)
                {
                    try
                    {
                        if (tURL != String.Empty && Uri.IsWellFormedUriString(tURL, UriKind.Absolute))
                        {
                            this.Status = "Lade URL: " + tURL;
                            PlanFile.LoadTXT(new Uri(tURL));
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

                PlanFile.Instance.LoadComplete();
            };
            tWrk.RunWorkerCompleted += (tWrkSender, tWrkE) =>
            {
                if (!tWrkE.Cancelled)
                {
                    this.Progress = false;
                    this.Status = "Fertig!";
                    this.DataContext = PlanFile.Instance;
                }
            };

            tWrk.RunWorkerAsync();
        }

        private void PrintWeekButtonPressed(object sender, RoutedEventArgs e)
        {

            PrintDialog tDlg = new PrintDialog();
            tDlg.PrintQueue = LocalPrintServer.GetDefaultPrintQueue();
            tDlg.PrintTicket = tDlg.PrintQueue.DefaultPrintTicket;
            tDlg.PrintTicket.PageOrientation = PageOrientation.Landscape;
            tDlg.PrintTicket.PageResolution = new PageResolution(2400, 2400);
            tDlg.PageRangeSelection = PageRangeSelection.AllPages;
            tDlg.UserPageRangeEnabled = true;

            if ((bool)tDlg.ShowDialog())
            {

                DependencyObject tBut = sender as DependencyObject;
                FrameworkElement tPrintVis = tBut.GetParent<Grid>().GetChildren<RasteredGroup>().First();

                PrintCapabilities tCaps = tDlg.PrintQueue.GetPrintCapabilities(tDlg.PrintTicket);
                double scale = Math.Min(tCaps.PageImageableArea.ExtentWidth / tPrintVis.ActualWidth,
                    tCaps.PageImageableArea.ExtentHeight / tPrintVis.ActualHeight);

                VisualBrush tBrush = new VisualBrush(tPrintVis);

                Border tGrid = new Border()
                {
                    Width = tPrintVis.ActualWidth,
                    Height = tPrintVis.ActualHeight,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };

                Rectangle tRect = new Rectangle()
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


                Size sz = new Size(tCaps.PageImageableArea.ExtentWidth, tCaps.PageImageableArea.ExtentHeight);
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

        private void RasteredItemsControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Console.WriteLine("Selection changed: {0}", e.AddedItems.Count);
            if (e.AddedItems.Count <= 0)
                mSelectedEvent = default(Event);
            else
                mSelectedEvent = (IEvent)e.AddedItems[0] ?? default(IEvent);
        }

        private void CBIsFiltered_Click(object sender, RoutedEventArgs e)
        {
            foreach (RasteredItemsControl tCtl in this.GetChildren<RasteredItemsControl>()) RefreshFiltering(tCtl);
        }

        private void RefreshFiltering(RasteredItemsControl Ctl)
        {
            Ctl.Items.Filter = (obj) =>
            {
                IEvent tObj = obj as IEvent;
                if (!this.IsGroupFilterActive) return true;

                var tBaseEvents = from evt in PlanFile.Instance.KnownBaseEvents
                                  where evt.BasicCode == tObj.BasicCode && (evt.Groups.Count() <= 1 || evt.Group == GroupID.Empty || evt.Group == tObj.Group || (!tObj.Group.IsSingleGroup && tObj.Group.Value.Split('+').Contains(evt.Group.Value)))
                                  select evt;

                return (tBaseEvents.Count() > 0);
            };
        }

        private void RasteredItemsControl_Loaded(object sender, RoutedEventArgs e)
        {
            RasteredItemsControl tCtl = sender as RasteredItemsControl;
            if (tCtl == null) return;

            RefreshFiltering(tCtl);
        }

        private void ResetChangesClick(object sender, RoutedEventArgs e)
        {
            Control tCtl = sender as Control;
            string tHash = tCtl.Tag as String;

            PlanFile.Instance.ResetChanges(tHash);
        }

        private void ExportEventsClick(object sender, RoutedEventArgs e)
        {
            if (PlanFile.Instance == null)
            {
                MessageBox.Show("Bevor du irgendwas exportieren kannst, musst du natürlich erstmal etwas laden!");
                return;
            }

            SeminarGroup tGrp = SemGroups.SelectedItem as SeminarGroup;
            if (tGrp == null)
            {
                if (MessageBox.Show("Eine Seminargruppe muss schon ausgewählt sein. Was soll sonst exportiert werden? Alle Semestergruppen? Das willst du nicht, glaub mir!\nOder doch?", "Alles exportieren?", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.No) return;
            }

            SaveFileDialog tDlg = new SaveFileDialog();
            tDlg.Filter = "iCalendar Datei (*.ics)|*.ics";

            if ((bool)tDlg.ShowDialog())
            {
                PlanFile.Instance.ExportAs(tGrp, ExportType.iCal, tDlg.FileName);
            }
        }

        IEvent mSelectedEvent = default(IEvent);
        private void selectedEventShowHide_Click(object sender, RoutedEventArgs e)
        {
            if (mSelectedEvent == default(Event))
            {
                MessageBox.Show("Ein Event muss schon ausgewählt sein, damit alle anderen ein- bzw. ausgeblendet werden können.");
                return;
            }

            string tTag = ((FrameworkElement)sender).Tag.ToString();
            var tEvents = from evt in PlanFile.Instance.AllEvents
                          where evt.Code == mSelectedEvent.Code
                          select evt;
            foreach (Event tEvt in tEvents)
            {
                tEvt.IsEnabled = (tTag == "Show");
            }
        }

        private void MouseDownDragStart(object sender, MouseButtonEventArgs e)
        {
        }

        TimeSpan? tOldFrom = null, tOldTill = null;

        double mGrid = 5.0D;

        private void EventDragging(object sender, ValueEventArgs<DraggingItem> e)
        {
            if (!(tOldFrom.HasValue | tOldTill.HasValue)) return;

            int minute = (int)(Math.Round(e.Value.Distance / mGrid) * mGrid);

            IEvent tEvt = e.Value.Item.Content as IEvent;
            if (tEvt is RESTEvent) return;

            TimeSpan tDist = new TimeSpan(0, minute, 0);
            tEvt.From = tOldFrom.Value.Add(tDist);
            tEvt.Till = tOldTill.Value.Add(tDist);
        }

        private void EventDraggingStart(object sender, ValueEventArgs<ListBoxItem> e)
        {
            IEvent tEvt = e.Value.Content as IEvent;
            tOldFrom = tEvt.From;
            tOldTill = tEvt.Till;
        }

        private void EventDraggingEnd(object sender, EventArgs e)
        {
            tOldFrom = null; tOldTill = null;
        }

        private void CopyHashClick(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(mSelectedEvent.Hash);
        }

        #region Window Resize

        private void Resize_Init(object sender, MouseButtonEventArgs e)
        {
            Rectangle rect = sender as Rectangle;
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
                default: break;
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

        private void FramelessTitleBar_DoubleClick(object sender, RoutedEventArgs e)
        {
            switch (this.WindowState)
            {
                case WindowState.Normal: this.WindowState = WindowState.Maximized; break;
                case WindowState.Maximized: this.WindowState = WindowState.Normal; break;
            }
        }

        private void DonateButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string url = "";
            string business = "7.e.q@syncro-community.de";
            string description = "HAW%20Stundenplantool%20Spende";
            string country = "DE";
            string currency = "EUR";

            url += "https://www.paypal.com/cgi-bin/webscr" +
                "?cmd=" + "_donations" +
                "&business=" + business +
                "&lc=" + country +
                "&item_name=" + description +
                "&currency_code=" + currency +
                "&bn=" + "PP%2dDonationsBF";

            System.Diagnostics.Process.Start(url);
        }

        private void MailMe(object sender, RoutedEventArgs e)
        {

        }

        private void TabControlFadeSetBackground(object sender, MouseButtonEventArgs e)
        {
        }

        private void tabFadeCompleted(object sender, EventArgs e)
        {
            tabFadeBorder.Visibility = Visibility.Collapsed;
        }
    }
}
