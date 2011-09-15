using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace SeveQsCustomControls
{
    public class StrikeOut : FrameworkElement
    {


        public float StrikeOutThickness
        {
            get { return (float)GetValue(StrikeOutThicknessProperty); }
            set { SetValue(StrikeOutThicknessProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StrikeOutThickness.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StrikeOutThicknessProperty =
            DependencyProperty.Register("StrikeOutThickness", typeof(float), typeof(StrikeOut), new UIPropertyMetadata(1.5F));



        public Brush StrikeOutBrush
        {
            get { return (Brush)GetValue(StrikeOutBrushProperty); }
            set { SetValue(StrikeOutBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StrikeOutBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StrikeOutBrushProperty =
            DependencyProperty.Register("StrikeOutBrush", typeof(Brush), typeof(StrikeOut), new UIPropertyMetadata(Brushes.Red));

        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            Pen tPOut = new Pen(StrikeOutBrush, StrikeOutThickness);

            drawingContext.DrawLine(tPOut, new System.Windows.Point(this.Margin.Left, this.Margin.Top), new System.Windows.Point(this.ActualWidth - this.Margin.Right, this.ActualHeight - this.Margin.Bottom));
            drawingContext.DrawLine(tPOut, new System.Windows.Point(this.Margin.Left, this.ActualHeight - this.Margin.Bottom), new System.Windows.Point(this.ActualWidth - this.Margin.Right, this.Margin.Top));
        }
    }
}
