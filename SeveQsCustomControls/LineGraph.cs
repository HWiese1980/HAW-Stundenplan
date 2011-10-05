#region Usings

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

#endregion

namespace SeveQsCustomControls
{
    ///<summary>
    ///  Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    ///  Step 1a) Using this custom control in a XAML file that exists in the current project.
    ///  Add this XmlNamespace attribute to the root element of the markup file where it is 
    ///  to be used:
    ///
    ///  xmlns:MyNamespace="clr-namespace:SeveQsCustomControls"
    ///
    ///
    ///  Step 1b) Using this custom control in a XAML file that exists in a different project.
    ///  Add this XmlNamespace attribute to the root element of the markup file where it is 
    ///  to be used:
    ///
    ///  xmlns:MyNamespace="clr-namespace:SeveQsCustomControls;assembly=SeveQsCustomControls"
    ///
    ///  You will also need to add a project reference from the project where the XAML file lives
    ///  to this project and Rebuild to avoid compilation errors:
    ///
    ///  Right click on the target project in the Solution Explorer and
    ///  "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    ///  Step 2)
    ///  Go ahead and use your control in the XAML file.
    ///
    ///  <MyNamespace:LineGraph />
    ///</summary>
    [TemplatePart(Name = "PART_GraphCanvas", Type = typeof (Canvas))]
    public class LineGraph : Control
    {
        // Using a DependencyProperty as the backing store for PropertyPath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PropertyPathProperty =
            DependencyProperty.Register("PropertyPath", typeof (string), typeof (LineGraph),
                                        new UIPropertyMetadata("", OnRerenderPropertyChanged));

        // Using a DependencyProperty as the backing store for GraphBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GraphBrushProperty =
            DependencyProperty.Register("GraphBrush", typeof (Brush), typeof (LineGraph),
                                        new UIPropertyMetadata(Brushes.White, OnRerenderPropertyChanged));

        // Using a DependencyProperty as the backing store for RasterBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RasterBrushProperty =
            DependencyProperty.Register("RasterBrush", typeof (Brush), typeof (LineGraph),
                                        new UIPropertyMetadata(Brushes.White, OnRerenderPropertyChanged));

        // Using a DependencyProperty as the backing store for DataPoints.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataPointsProperty =
            DependencyProperty.Register("DataPoints", typeof (IEnumerable<object>), typeof (LineGraph),
                                        new UIPropertyMetadata(default(IEnumerable<object>), OnRerenderPropertyChanged));

        private Canvas _mGraphCanvas;

        static LineGraph()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (LineGraph),
                                                     new FrameworkPropertyMetadata(typeof (LineGraph)));
        }

        public LineGraph()
        {
            Loaded += (s, d) => RenderGraph();
        }

        public string PropertyPath
        {
            get { return (string) GetValue(PropertyPathProperty); }
            set { SetValue(PropertyPathProperty, value); }
        }

        public IEnumerable<object> DataPoints
        {
            get { return GetValue(DataPointsProperty) as IEnumerable<object>; }
            set { SetValue(DataPointsProperty, value); }
        }

        public Brush GraphBrush
        {
            get { return (Brush) GetValue(GraphBrushProperty); }
            set { SetValue(GraphBrushProperty, value); }
        }

        public Brush RasterBrush
        {
            get { return (Brush) GetValue(RasterBrushProperty); }
            set { SetValue(RasterBrushProperty, value); }
        }

        private static void OnRerenderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            FetchTemplateObjects();
            RenderGraph();
        }

        private void FetchTemplateObjects()
        {
            _mGraphCanvas = (Canvas) GetTemplateChild("PART_GraphCanvas");
        }

        private void RenderRaster()
        {
            if (_mGraphCanvas == null || DataPoints == null) return;
            _mGraphCanvas.Children.Clear();

            double tSteps = (DataPoints != null) ? DataPoints.Count() : 0.0;

            for (double i = 0.0; i < Width; i += tSteps)
            {
                var tRasterLine = new Line {X1 = i, X2 = i, Y1 = 0, Y2 = Height, Fill = RasterBrush};

                _mGraphCanvas.Children.Add(tRasterLine);
            }
        }

        private void RenderGraph()
        {
            if (_mGraphCanvas == null || DataPoints == null) return;
            RenderRaster();

            var tOld = new Point(0, 0);
            double tPoints = (DataPoints != null) ? DataPoints.Count() : 0.0;

            double tStepSize = ActualWidth/tPoints;
            double tStep = 0.0;
            foreach (var tObj in DataPoints)
            {
                var tProp = tObj.GetType().GetProperty(PropertyPath);
                if (tProp == null) continue;

                tStep += tStepSize;

                var tVal = tProp.GetValue(tObj, null);

                //var tLine = new Line { X1 = tOld.X, X2 = tStep, Y1 = tOld.Y, Y2 = (double) tVal };


                tOld.Y = (double) tVal;
                tOld.X = tStep;
            }
        }
    }
}