using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using LittleHelpers;
using System.Globalization;
using System.Windows.Controls.Primitives;

using LHelper = LittleHelpers.Helper;
using SeveQsCustomControls;
using System.ComponentModel;
using System.Windows.Shapes;

namespace SeveQsCustomControls
{
    public class DraggingItem
    {
		#region Properties (2) 

        public double Distance { get; set; }

        public ListBoxItem Item { get; set; }

		#endregion Properties 
    }

    [TemplatePart(Name = "PART_RasterCanvas", Type = typeof(Canvas))]
    public class RasteredItemsControl : ListBox
    {
        private static void OnRerenderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement) ((UIElement)d).InvalidateVisual();
        }

        protected override void OnPreviewMouseWheel(System.Windows.Input.MouseWheelEventArgs e)
        {
            ScrollViewer tView = this.GetParent<ScrollViewer>();
            if (tView == null) { e.Handled = false; return; }

            double x = (double)e.Delta;
            double y = tView.VerticalOffset;

            tView.ScrollToVerticalOffset(y - x);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == PaddingProperty) OnRerenderPropertyChanged(this, e);
        }



        [Category("Appearance"), Description("Thickness of the full hour line")]
        public double FullLineThickness
        {
            get { return (double)GetValue(FullLineThicknessProperty); }
            set { SetValue(FullLineThicknessProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FullLineThickness.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FullLineThicknessProperty =
            DependencyProperty.Register("FullLineThickness", typeof(double), typeof(RasteredItemsControl), new UIPropertyMetadata(2.0D, OnRerenderPropertyChanged));

        [Category("Appearance"), Description("Thickness of the half hour line")]
        public double HalfLineThickness
        {
            get { return (double)GetValue(HalfLineThicknessProperty); }
            set { SetValue(HalfLineThicknessProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HalfLineThickness.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HalfLineThicknessProperty =
            DependencyProperty.Register("HalfLineThickness", typeof(double), typeof(RasteredItemsControl), new UIPropertyMetadata(1.0D, OnRerenderPropertyChanged));

        [Category("Appearance"), Description("Thickness of the quarter hour line")]
        public double QuarterLineThickness
        {
            get { return (double)GetValue(QuarterLineThicknessProperty); }
            set { SetValue(QuarterLineThicknessProperty, value); }
        }

        // Using a DependencyProperty as the backing store for QuarterLineThickness.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty QuarterLineThicknessProperty =
            DependencyProperty.Register("QuarterLineThickness", typeof(double), typeof(RasteredItemsControl), new UIPropertyMetadata(0.5D, OnRerenderPropertyChanged));

        [Category("Layout"), Description("The raster width of a full hour")]
        public double Raster
        {
            get { return (double)GetValue(RasterProperty); }
            set { SetValue(RasterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Raster.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RasterProperty =
            DependencyProperty.Register("Raster", typeof(double), typeof(RasteredItemsControl), new UIPropertyMetadata(100.0D, OnRerenderPropertyChanged));

        public Brush LineBrush
        {
            get { return (Brush)GetValue(LineBrushProperty); }
            set { SetValue(LineBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LineBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LineBrushProperty =
            DependencyProperty.Register("LineBrush", typeof(Brush), typeof(RasteredItemsControl), new UIPropertyMetadata(Brushes.White, OnRerenderPropertyChanged));

        public Brush HalfLineBrush
        {
            get { return (Brush)GetValue(HalfLineBrushProperty); }
            set { SetValue(HalfLineBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HalfLineBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HalfLineBrushProperty =
            DependencyProperty.Register("HalfLineBrush", typeof(Brush), typeof(RasteredItemsControl), new UIPropertyMetadata(Brushes.White, OnRerenderPropertyChanged));



        public Brush QuarterLineBrush
        {
            get { return (Brush)GetValue(QuarterLineBrushProperty); }
            set { SetValue(QuarterLineBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for QuarterLineBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty QuarterLineBrushProperty =
            DependencyProperty.Register("QuarterLineBrush", typeof(Brush), typeof(RasteredItemsControl), new UIPropertyMetadata(Brushes.White, OnRerenderPropertyChanged));


        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Header.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(RasteredItemsControl), new UIPropertyMetadata(String.Empty));


        static RasteredItemsControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RasteredItemsControl), new FrameworkPropertyMetadata(typeof(RasteredItemsControl)));
        }

        protected override void OnPreviewMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            Point newPoint = e.GetPosition(this);
            HitTestResult tHit = VisualTreeHelper.HitTest(this, newPoint);
            if (tHit == null) return;

            CheckBox tBox = tHit.VisualHit.GetParent<CheckBox>();
            if (tBox != null) return;

            ListBoxItem tItem = tHit.VisualHit.GetParent<ListBoxItem>();
            if (tItem != null)
            {
                e.Handled = true;
            }
        }

        private HitTestResult GetHit(Point pos)
        {
            HitTestResult tHit = VisualTreeHelper.HitTest(this, pos);
            if (tHit == null) return null;

            CheckBox tBox = tHit.VisualHit.GetParent<CheckBox>();
            if (tBox != null) return null;

            return tHit;
        }

        protected override void OnPreviewMouseLeftButtonUp(System.Windows.Input.MouseButtonEventArgs e)
        {
            bBeginDragging = false;

            if (bDragging && mDraggingItem != null)
            {
                bDragging = false;
                mDraggingItem = null;
                e.Handled = true;
                OnEndDragging();
                return;
            }


            Point newPoint = e.GetPosition(this);
            HitTestResult tHit = GetHit(newPoint);
            if (tHit == null) return;

            ListBoxItem tItem = tHit.VisualHit.GetParent<ListBoxItem>();
            if (tItem != null)
            {
                tItem.IsSelected = !tItem.IsSelected;
                e.Handled = true;
            }
        }

        protected override void OnPreviewMouseDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            mDragStart = e.GetPosition(this);
            HitTestResult tHit = GetHit(mDragStart);
            if (tHit == null) return;

            mDraggingItem = tHit.VisualHit.GetParent<ListBoxItem>();
            if (mDraggingItem != null)
            {
                bBeginDragging = true;
                OnStartDragging();
            }

            base.OnPreviewMouseDown(e);
        }

        ListBoxItem mDraggingItem = null;
        Point mDragStart;
        bool bBeginDragging = false, bDragging = false;

        public event EventHandler<ValueEventArgs<DraggingItem>> Dragging;

        public event EventHandler<ValueEventArgs<ListBoxItem>> StartDragging;
        public event EventHandler EndDragging;

        void OnStartDragging()
        {
            if (StartDragging != null) StartDragging(this, new ValueEventArgs<ListBoxItem>() { Value = mDraggingItem });
        }

        void OnEndDragging()
        {
            if (EndDragging != null) EndDragging(this, new EventArgs());
        }

        protected override void OnPreviewMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            Point newPoint = e.GetPosition(this);
            double xDist = newPoint.X - mDragStart.X;
            if (bBeginDragging && Math.Abs(xDist) > 5.0D)
            {
                bBeginDragging = false;
                bDragging = true;
            }

            if (!bDragging) return;

            try
            {

                OnDragging(xDist);
            }
            finally
            {
                base.OnPreviewMouseMove(e);
            }
        }

        private void OnDragging(double xDist)
        {
            if (Dragging != null) Dragging(this, new ValueEventArgs<DraggingItem>() { Value = new DraggingItem() { Distance = xDist, Item = mDraggingItem } });
        }

        public RasteredItemsControl()
        {
            Loaded += (sender, e) =>
            {
                RenderRaster();
            };
        }


        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var tRIC = (from p in this.GetParent<RasteredGroup>().GetChildren<RasteredItemsControl>() where p != this select p);
                foreach (RasteredItemsControl tCtl in tRIC)
                {
                    tCtl.SelectedItem = null;
                }
            }
            base.OnSelectionChanged(e);

            RasteredGroup tGrp = this.GetParent<RasteredGroup>();
            if (tGrp != null) tGrp.OnSelectedItemChanged();
        }

        Canvas mRasterCanvas;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            mRasterCanvas = (Canvas)base.GetTemplateChild("PART_RasterCanvas");
        }

        private void RenderRaster()
        {
            mRasterCanvas.Children.Clear();
            Typeface tFace = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);

//             Pen p;

            for (double i = Padding.Left; i < (this.ActualWidth - Padding.Right); i += (this.Raster / 4.0F))
            {
                Point tP1 = new Point(i, Padding.Top), tP2 = new Point(i, this.ActualHeight - Padding.Bottom);
                Line tLine = new Line();
                tLine.X1 = tP1.X;
                tLine.X2 = tP2.X;
                tLine.Y1 = tP1.Y;
                tLine.Y2 = tP2.Y;

                if (i % 4 == 0.0D)
                {
                    DateTime tTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, (int)(i / this.Raster), 0, 0);
                    FormattedText tFT = new FormattedText(String.Format("{0:t}", tTime.AddHours(7)), CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, tFace, 10.0F, Foreground);

                    double j = Math.Max(0.0D, i - (tFT.Width / 2));

                    Label tLab = new Label();
                    tLab.Foreground = Foreground;
                    tLab.FontSize = 12;
                    tLab.FontWeight = FontWeight;

                    tLab.Content = String.Format("{0:t}", tTime.AddHours(7));

                    Canvas.SetLeft(tLab, j);

                    tLine.StrokeThickness = FullLineThickness;
                    tLine.Stroke = LineBrush;

                    mRasterCanvas.Children.Add(tLab);

                }
                else if (i % 2 == 0.0D)
                {
                    tLine.Stroke = HalfLineBrush;
                    tLine.StrokeThickness = HalfLineThickness;
                }
                else
                {
                    tLine.Stroke = QuarterLineBrush;
                    tLine.StrokeThickness = QuarterLineThickness;
                }
                mRasterCanvas.Children.Add(tLine);
            }
        }
    }
}
