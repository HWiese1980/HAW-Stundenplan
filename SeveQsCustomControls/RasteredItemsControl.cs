#region Usings

using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using LittleHelpers;

#endregion

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

        private bool _bBeginDragging, _bDragging;
        private Point _mDragStart;
        private ListBoxItem _mDraggingItem;
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

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            var tView = this.GetParent<ScrollViewer>();
            if (tView == null)
            {
                e.Handled = false;
                return;
            }

            var x = (double)e.Delta;
            var y = tView.VerticalOffset;

            tView.ScrollToVerticalOffset(y - x);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == PaddingProperty) OnRerenderPropertyChanged(this, e);
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            Point newPoint = e.GetPosition(this);
            HitTestResult tHit = VisualTreeHelper.HitTest(this, newPoint);
            if (tHit == null) return;

            var tBox = tHit.VisualHit.GetParent<CheckBox>();
            if (tBox != null) return;

            var tItem = tHit.VisualHit.GetParent<ListBoxItem>();
            if (tItem != null)
            {
                e.Handled = true;
            }
        }

        private HitTestResult GetHit(Point pos)
        {
            var tHit = VisualTreeHelper.HitTest(this, pos);
            if (tHit == null) return null;

            var tBox = tHit.VisualHit.GetParent<CheckBox>();
            if (tBox != null) return null;

            return tHit;
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            _bBeginDragging = false;

            if (_bDragging && _mDraggingItem != null)
            {
                _bDragging = false;
                _mDraggingItem = null;
                e.Handled = true;
                OnEndDragging();
                return;
            }


            Point newPoint = e.GetPosition(this);
            HitTestResult tHit = GetHit(newPoint);
            if (tHit == null) return;

            var tItem = tHit.VisualHit.GetParent<ListBoxItem>();
            if (tItem != null)
            {
                tItem.IsSelected = !tItem.IsSelected;
                e.Handled = true;
            }
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            _mDragStart = e.GetPosition(this);
            HitTestResult tHit = GetHit(_mDragStart);
            if (tHit == null) return;

            _mDraggingItem = tHit.VisualHit.GetParent<ListBoxItem>();
            if (_mDraggingItem != null)
            {
                _bBeginDragging = true;
                OnStartDragging();
            }

            base.OnPreviewMouseDown(e);
        }

        public event EventHandler<ValueEventArgs<DraggingItem>> Dragging;

        public event EventHandler<ValueEventArgs<ListBoxItem>> StartDragging;
        public event EventHandler EndDragging;

        private void OnStartDragging()
        {
            if (StartDragging != null) StartDragging(this, new ValueEventArgs<ListBoxItem> { Value = _mDraggingItem });
        }

        private void OnEndDragging()
        {
            if (EndDragging != null) EndDragging(this, new EventArgs());
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            Point newPoint = e.GetPosition(this);
            double xDist = newPoint.X - _mDragStart.X;
            if (_bBeginDragging && Math.Abs(xDist) > 5.0D)
            {
                _bBeginDragging = false;
                _bDragging = true;
            }

            if (!_bDragging) return;

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
            if (Dragging != null)
                Dragging(this,
                         new ValueEventArgs<DraggingItem> { Value = new DraggingItem { Distance = xDist, Item = _mDraggingItem } });
        }


        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var tRIC =
                    (from p in this.GetParent<RasteredGroup>().GetChildren<RasteredItemsControl>()
                     where p != this
                     select p);
                foreach (var tCtl in tRIC)
                {
                    tCtl.SelectedItem = null;
                }
            }
            base.OnSelectionChanged(e);

            var tGrp = this.GetParent<RasteredGroup>();
            if (tGrp != null) tGrp.OnSelectedItemChanged();
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

            //             Pen p;

            for (double i = Padding.Left; i < (ActualWidth - Padding.Right); i += (Raster / 4.0F))
            {
                Point tP1 = new Point(i, Padding.Top), tP2 = new Point(i, ActualHeight - Padding.Bottom);
                var tLine = new Line { X1 = tP1.X, X2 = tP2.X, Y1 = tP1.Y, Y2 = tP2.Y };

                if ((int)(i % 4) == 0)
                {
                    var tTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, (int)(i / Raster),
                                             0, 0);
                    var tFT = new FormattedText(String.Format("{0:t}", tTime.AddHours(7)), CultureInfo.CurrentUICulture,
                                                FlowDirection.LeftToRight, tFace, 10.0F, Foreground);

                    double j = Math.Max(0.0D, i - (tFT.Width / 2));

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
                else if ((int)(i % 2) == 0)
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