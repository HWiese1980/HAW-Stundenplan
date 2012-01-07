#region Usings

using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using LittleHelpers;

#endregion

namespace SeveQsCustomControls
{
    [TemplatePart(Name = "PART_RasterCanvas", Type = typeof(Canvas))]
    public class RasteredItemsControl : ListBox
    {
        private double RasterWidth
        {
            get { return (double)GetValue(RasterWidthProperty); }
            set { SetValue(RasterWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RasterWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RasterWidthProperty =
            DependencyProperty.Register("RasterWidth", typeof(double), typeof(RasteredItemsControl), new UIPropertyMetadata(0.0D));



        public TimeSpan StartTime
        {
            get { return (TimeSpan)GetValue(StartTimeProperty); }
            set { SetValue(StartTimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StartTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StartTimeProperty =
            DependencyProperty.Register("StartTime", typeof(TimeSpan), typeof(RasteredItemsControl), new UIPropertyMetadata(TimeSpan.Parse("7:00")));



        public TimeSpan EndTime
        {
            get { return (TimeSpan)GetValue(EndTimeProperty); }
            set { SetValue(EndTimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EndTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EndTimeProperty =
            DependencyProperty.Register("EndTime", typeof(TimeSpan), typeof(RasteredItemsControl), new UIPropertyMetadata(TimeSpan.Parse("21:00")));




        // Using a DependencyProperty as the backing store for FullLineThickness.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FullLineThicknessProperty =
            DependencyProperty.Register("FullLineThickness", typeof(double), typeof(RasteredItemsControl),
                                        new UIPropertyMetadata(2.0D, OnRerenderPropertyChanged));

        // Using a DependencyProperty as the backing store for HalfLineThickness.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HalfLineThicknessProperty =
            DependencyProperty.Register("HalfLineThickness", typeof(double), typeof(RasteredItemsControl),
                                        new UIPropertyMetadata(1.0D, OnRerenderPropertyChanged));

        // Using a DependencyProperty as the backing store for QuarterLineThickness.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty QuarterLineThicknessProperty =
            DependencyProperty.Register("QuarterLineThickness", typeof(double), typeof(RasteredItemsControl),
                                        new UIPropertyMetadata(0.5D, OnRerenderPropertyChanged));

        // Using a DependencyProperty as the backing store for Raster.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RasterProperty =
            DependencyProperty.Register("Raster", typeof(double), typeof(RasteredItemsControl),
                                        new UIPropertyMetadata(100.0D, OnRerenderPropertyChanged));

        // Using a DependencyProperty as the backing store for LineBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LineBrushProperty =
            DependencyProperty.Register("LineBrush", typeof(Brush), typeof(RasteredItemsControl),
                                        new UIPropertyMetadata(Brushes.White, OnRerenderPropertyChanged));

        // Using a DependencyProperty as the backing store for HalfLineBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HalfLineBrushProperty =
            DependencyProperty.Register("HalfLineBrush", typeof(Brush), typeof(RasteredItemsControl),
                                        new UIPropertyMetadata(Brushes.White, OnRerenderPropertyChanged));


        // Using a DependencyProperty as the backing store for QuarterLineBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty QuarterLineBrushProperty =
            DependencyProperty.Register("QuarterLineBrush", typeof(Brush), typeof(RasteredItemsControl),
                                        new UIPropertyMetadata(Brushes.White, OnRerenderPropertyChanged));


        // Using a DependencyProperty as the backing store for Header.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(RasteredItemsControl),
                                        new UIPropertyMetadata(String.Empty));

        private Canvas _mRasterCanvas;

        static RasteredItemsControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RasteredItemsControl),
                                                     new FrameworkPropertyMetadata(typeof(RasteredItemsControl)));
        }

        public RasteredItemsControl()
        {
            Loaded += (sender, e) => RenderRaster();
        }

        [Category("Appearance"), Description("Thickness of the full hour line")]
        public double FullLineThickness
        {
            get { return (double)GetValue(FullLineThicknessProperty); }
            set { SetValue(FullLineThicknessProperty, value); }
        }

        [Category("Appearance"), Description("Thickness of the half hour line")]
        public double HalfLineThickness
        {
            get { return (double)GetValue(HalfLineThicknessProperty); }
            set { SetValue(HalfLineThicknessProperty, value); }
        }

        [Category("Appearance"), Description("Thickness of the quarter hour line")]
        public double QuarterLineThickness
        {
            get { return (double)GetValue(QuarterLineThicknessProperty); }
            set { SetValue(QuarterLineThicknessProperty, value); }
        }

        [Category("Layout"), Description("The raster width of a full hour")]
        public double Raster
        {
            get { return (double)GetValue(RasterProperty); }
            set { SetValue(RasterProperty, value); }
        }

        public Brush LineBrush
        {
            get { return (Brush)GetValue(LineBrushProperty); }
            set { SetValue(LineBrushProperty, value); }
        }

        public Brush HalfLineBrush
        {
            get { return (Brush)GetValue(HalfLineBrushProperty); }
            set { SetValue(HalfLineBrushProperty, value); }
        }

        public Brush QuarterLineBrush
        {
            get { return (Brush)GetValue(QuarterLineBrushProperty); }
            set { SetValue(QuarterLineBrushProperty, value); }
        }

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        private static void OnRerenderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement) ((UIElement)d).InvalidateVisual();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == PaddingProperty) OnRerenderPropertyChanged(this, e);
        }

        private HitTestResult GetHit(Point pos)
        {
            var tHit = VisualTreeHelper.HitTest(this, pos);
            if (tHit == null) return null;

            var tBox = tHit.VisualHit.GetParent<CheckBox>();
            if (tBox != null) return null;

            return tHit;
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            var rg = this.GetParent<RasteredGroup>();
            if(rg != null) rg.SelectedItem = (e.AddedItems != null && e.AddedItems.Count > 0) ? e.AddedItems[0] : null;
            base.OnSelectionChanged(e);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _mRasterCanvas = (Canvas)GetTemplateChild("PART_RasterCanvas");
        }

        private void RenderRaster()
        {
            _mRasterCanvas.Children.Clear();
            var tFace = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);

            var cTime = StartTime;

            var quarters = (int) ((EndTime - StartTime).TotalMinutes/15);

            // for (var i = Padding.Left; (i <= (ActualWidth - Padding.Right)); i += (Raster / 4.0F))
            for (var q = 0; q <= quarters; q++)
            {
                var qs = q*(Raster/4.0D);
                if (q == quarters)
                {
                    RasterWidth = qs;
                }

                var i = Padding.Left + qs;

                var tP1 = new Point(i, Padding.Top);
                var tP2 = new Point(i, ActualHeight - Padding.Bottom);
                var tLine = new Line { X1 = tP1.X, X2 = tP2.X, Y1 = tP1.Y, Y2 = tP2.Y };

                if (q % 4 == 0)
                {
                    var tTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, (int)(i / Raster),
                                                0, 0);
                    var tFT = new FormattedText(String.Format("{0:t}", tTime.AddHours(7)), CultureInfo.CurrentUICulture,
                                                FlowDirection.LeftToRight, tFace, 10.0F, Foreground);

                    var j = Math.Max(0.0D, i - (tFT.Width / 2));

                    var tLab = new Label
                                    {
                                        Foreground = Foreground,
                                        FontSize = 12,
                                        FontWeight = FontWeight,
                                        Content = String.Format("{0:t}", tTime.AddHours(7))
                                    };


                    Canvas.SetLeft(tLab, j);

                    tLine.StrokeThickness = FullLineThickness;
                    tLine.Stroke = LineBrush;

                    _mRasterCanvas.Children.Add(tLab);
                }
                else if (q % 2 == 0)
                {
                    tLine.Stroke = HalfLineBrush;
                    tLine.StrokeThickness = HalfLineThickness;
                }
                else
                {
                    tLine.Stroke = QuarterLineBrush;
                    tLine.StrokeThickness = QuarterLineThickness;
                }
                _mRasterCanvas.Children.Add(tLine);
            }
        }
    }
}