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
using System.Reflection;

namespace SeveQsCustomControls
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:SeveQsCustomControls"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:SeveQsCustomControls;assembly=SeveQsCustomControls"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:LineGraph/>
    ///
    /// </summary>
    [TemplatePart(Name = "PART_GraphCanvas", Type = typeof(Canvas))]
    public class LineGraph : Control
    {
        private static void OnRerenderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        public string PropertyPath
        {
            get { return (string)GetValue(PropertyPathProperty); }
            set { SetValue(PropertyPathProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PropertyPath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PropertyPathProperty =
            DependencyProperty.Register("PropertyPath", typeof(string), typeof(LineGraph), new UIPropertyMetadata("", OnRerenderPropertyChanged));

        public IEnumerable<object> DataPoints
        {
            get { return GetValue(DataPointsProperty) as IEnumerable<object>; }
            set { SetValue(DataPointsProperty, value); }
        }

        public Brush GraphBrush
        {
            get { return (Brush)GetValue(GraphBrushProperty); }
            set { SetValue(GraphBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for GraphBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GraphBrushProperty =
            DependencyProperty.Register("GraphBrush", typeof(Brush), typeof(LineGraph), new UIPropertyMetadata(Brushes.White, OnRerenderPropertyChanged));

        public Brush RasterBrush
        {
            get { return (Brush)GetValue(RasterBrushProperty); }
            set { SetValue(RasterBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RasterBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RasterBrushProperty =
            DependencyProperty.Register("RasterBrush", typeof(Brush), typeof(LineGraph), new UIPropertyMetadata(Brushes.White, OnRerenderPropertyChanged));

        // Using a DependencyProperty as the backing store for DataPoints.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataPointsProperty =
            DependencyProperty.Register("DataPoints", typeof(IEnumerable<object>), typeof(LineGraph), new UIPropertyMetadata(default(IEnumerable<object>), OnRerenderPropertyChanged));

        Canvas mGraphCanvas;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            FetchTemplateObjects();
            RenderGraph();
        }

        private void FetchTemplateObjects()
        {
            mGraphCanvas = (Canvas)base.GetTemplateChild("PART_GraphCanvas");
        }

        private void RenderRaster()
        {
            if (mGraphCanvas == null || this.DataPoints == null) return;
            mGraphCanvas.Children.Clear();

            double tSteps = (this.DataPoints != null) ? this.DataPoints.Count() : 0.0;

            for (double i = 0.0; i < this.Width; i += tSteps)
            {
                Line tRasterLine = new Line();
                tRasterLine.X1 = i;
                tRasterLine.X2 = i;
                tRasterLine.Y1 = 0;
                tRasterLine.Y2 = this.Height;
                tRasterLine.Fill = this.RasterBrush;

                mGraphCanvas.Children.Add(tRasterLine);
            }
        }

        private void RenderGraph()
        {
            if (mGraphCanvas == null || this.DataPoints == null) return;
            this.RenderRaster();

            Point tOld = new Point(0, 0);
            double tPoints = (this.DataPoints != null) ? this.DataPoints.Count() : 0.0;

            double tStepSize = this.ActualWidth / tPoints;
            double tStep = 0.0;
            foreach (object tObj in DataPoints)
            {
                PropertyInfo tProp = tObj.GetType().GetProperty(this.PropertyPath);
                if (tProp == null) continue;

                tStep += tStepSize;

                var tVal = tProp.GetValue(tObj, null);

                Line tLine = new Line();
                tLine.X1 = tOld.X;
                tLine.X2 = tStep;

                tLine.Y1 = tOld.Y;
                tLine.Y2 = (double)tVal;

                tOld.Y = (double)tVal;
                tOld.X = tStep;
            }
        }

        public LineGraph()
        {
            Loaded += (s, d) =>
                {
                    RenderGraph();
                };
        }

        static LineGraph()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LineGraph), new FrameworkPropertyMetadata(typeof(LineGraph)));
        }
    }
}
