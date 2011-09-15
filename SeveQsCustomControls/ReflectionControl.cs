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
using System.ComponentModel;

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
    ///     <MyNamespace:ReflectionControl/>
    ///
    /// </summary>
    public class ReflectionControl : Decorator
    {
        VisualBrush mReflection;
        LinearGradientBrush mOpacityMask;


        static void RedrawVisual(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement tFW = d as FrameworkElement;
            if (tFW != null) tFW.InvalidateVisual();
        }

        public ReflectionControl()
        {
            VerticalAlignment = VerticalAlignment.Center;
            HorizontalAlignment = HorizontalAlignment.Center;

            mOpacityMask = new LinearGradientBrush();
            mOpacityMask.StartPoint = new Point(0, 0);
            mOpacityMask.EndPoint = new Point(0, 1);
            mOpacityMask.GradientStops.Add(new GradientStop(Colors.Black, 0));
            mOpacityMask.GradientStops.Add(new GradientStop(Colors.Transparent, 0.6));

            mReflection = new VisualBrush();
            mReflection.Stretch = Stretch.None;
            mReflection.TileMode = TileMode.None;
            mReflection.Transform = new ScaleTransform(1, -1);
        }


        [Category("Reflection")]
        public double ReflectionOpacity
        {
            get { return (double)GetValue(ReflectionOpacityProperty); }
            set { SetValue(ReflectionOpacityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ReflectionOpacity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ReflectionOpacityProperty =
            DependencyProperty.Register("ReflectionOpacity", typeof(double), typeof(ReflectionControl), new UIPropertyMetadata(0.3D, RedrawVisual));

        [Category("Brushes")]
        public Brush ReflectionMask
        {
            get { return (Brush)GetValue(ReflectionMaskProperty); }
            set { SetValue(ReflectionMaskProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ReflectionMask.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ReflectionMaskProperty =
            DependencyProperty.Register("ReflectionMask", typeof(Brush), typeof(ReflectionControl), new UIPropertyMetadata(default(SolidColorBrush), RedrawVisual));

        protected override Size MeasureOverride(Size constraint)
        {
            if (Child == null)
                return new Size(0, 0);

            Child.Measure(constraint);
            return new Size(Child.DesiredSize.Width, Child.DesiredSize.Height * 2);
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            if (Child == null)
                return new Size(0, 0);

            Child.Arrange(new Rect(0, 0, arrangeSize.Width, arrangeSize.Height / 2));
            return arrangeSize;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            drawingContext.PushOpacityMask(ReflectionMask);
            drawingContext.PushOpacity(ReflectionOpacity);

            mReflection.Visual = Child;

            ((ScaleTransform)mReflection.Transform).CenterY = 3 * ActualHeight / 4;
            ((ScaleTransform)mReflection.Transform).CenterX = ActualWidth / 2;

            drawingContext.DrawRectangle(mReflection, null, new Rect(0, ActualHeight / 2, ActualWidth, ActualHeight / 2));

            drawingContext.Pop();
            drawingContext.Pop();
        }
    }
}
