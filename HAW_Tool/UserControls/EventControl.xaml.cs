using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HAW_Tool.HAW.Depending;
using LittleHelpers;
using SeveQsCustomControls;

namespace HAW_Tool.UserControls
{
    /// <summary>
    /// Interaktionslogik für EventControl.xaml
    /// </summary>
    public partial class EventControl : UserControl
    {
        private static void OnDepPropChg(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
        }

        public Brush EventBorderBackground
        {
            get { return (Brush)GetValue(EventBorderBackgroundProperty); }
            set { SetValue(EventBorderBackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EventBorderBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EventBorderBackgroundProperty =
            DependencyProperty.Register("EventBorderBackground", typeof(Brush), typeof(EventControl), new UIPropertyMetadata(new SolidColorBrush(Color.FromArgb(0xB2, 0x00, 0x00, 0x00)), OnDepPropChg));



        public Brush EventBorderBrush
        {
            get { return (Brush)GetValue(EventBorderBrushProperty); }
            set { SetValue(EventBorderBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EventBorderBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EventBorderBrushProperty =
            DependencyProperty.Register("EventBorderBrush", typeof(Brush), typeof(EventControl), new UIPropertyMetadata(new SolidColorBrush(Color.FromArgb(0x76, 0x47, 0x47, 0x47)), OnDepPropChg));



        public double EventBorderOpacity
        {
            get { return (double)GetValue(EventBorderOpacityProperty); }
            set { SetValue(EventBorderOpacityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EventBorderOpacity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EventBorderOpacityProperty =
            DependencyProperty.Register("EventBorderOpacity", typeof(double), typeof(EventControl), new UIPropertyMetadata(1.0D, OnDepPropChg));



        public Brush BackBorderBackground
        {
            get { return (Brush)GetValue(BackBorderBackgroundProperty); }
            set { SetValue(BackBorderBackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BackBorderBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackBorderBackgroundProperty =
            DependencyProperty.Register("BackBorderBackground", typeof(Brush), typeof(EventControl), new UIPropertyMetadata(new SolidColorBrush(Color.FromArgb(0xFF, 0x53, 0x6B, 0x8E)), OnDepPropChg));



        public Brush BackBorderBrush
        {
            get { return (Brush)GetValue(BackBorderBrushProperty); }
            set { SetValue(BackBorderBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BackBorderBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackBorderBrushProperty =
            DependencyProperty.Register("BackBorderBrush", typeof(Brush), typeof(EventControl), new UIPropertyMetadata(Brushes.DarkBlue, OnDepPropChg));



        public Visibility StrikeOutVisibility
        {
            get { return (Visibility)GetValue(StrikeOutVisibilityProperty); }
            set { SetValue(StrikeOutVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StrikeOutVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StrikeOutVisibilityProperty =
            DependencyProperty.Register("StrikeOutVisibility", typeof(Visibility), typeof(EventControl), new UIPropertyMetadata(Visibility.Hidden, OnDepPropChg));



        public double RasterWidth
        {
            get { return (double)GetValue(RasterWidthProperty); }
            set { SetValue(RasterWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RasterWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RasterWidthProperty =
            DependencyProperty.Register("RasterWidth", typeof(double), typeof(EventControl), new UIPropertyMetadata(0.0D));



        public EventControl()
        {
            DataContextChanged += EventControl_DataContextChanged;
            InitializeComponent();
        }

        void EventControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var evt = (Event)DataContext;
            _evtDelta = evt.Till - evt.From;
        }

        private double TimePerPixel
        {
            get
            {
                var ric = this.GetParent<RasteredItemsControl>();
                var minTime = ric.StartTime;
                var maxTime = ric.EndTime;
                var width = ric.ActualWidth;
                var minutesPerPixel = (maxTime - minTime).TotalMinutes / width;
                return minutesPerPixel;
            }
        }

        private void LeftDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            var minutesPerPixel = TimePerPixel;

            var evt = (Event)DataContext;
            evt.From = evt.From.Add(TimeSpan.FromMinutes(minutesPerPixel * e.HorizontalChange));
            _evtDelta = evt.Till - evt.From;
        }

        private void RightDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            var minutesPerPixel = TimePerPixel;
            
            var evt = (Event)DataContext;
            evt.Till = evt.Till.Add(TimeSpan.FromMinutes(minutesPerPixel * e.HorizontalChange));
            _evtDelta = evt.Till - evt.From;
        }

        private TimeSpan _evtDelta;
        private void BothDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            var minutesPerPixel = TimePerPixel;
            var evt = (Event) DataContext;

            var ts = TimeSpan.FromMinutes(minutesPerPixel*e.HorizontalChange);
            evt.From = evt.From.Add(ts);
            evt.Till = evt.From.Add(_evtDelta);
        }

        private void MyEventControl_Loaded(object sender, RoutedEventArgs e)
        {
            var evt = (Event) DataContext;
            evt.Day.RecalculateRowIndexAll();
        }
    }
}
