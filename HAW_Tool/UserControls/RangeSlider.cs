using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using HAW_Tool.Converters;

namespace HAW_Tool.UserControls
{
    /// <summary>
    /// Führen Sie die Schritte 1a oder 1b und anschließend Schritt 2 aus, um dieses benutzerdefinierte Steuerelement in einer XAML-Datei zu verwenden.
    ///
    /// Schritt 1a) Verwenden des benutzerdefinierten Steuerelements in einer XAML-Datei, die im aktuellen Projekt vorhanden ist.
    /// Fügen Sie dieses XmlNamespace-Attribut dem Stammelement der Markupdatei 
    /// an der Stelle hinzu, an der es verwendet werden soll:
    ///
    ///     xmlns:MyNamespace="clr-namespace:HAW_Tool.UserControls"
    ///
    ///
    /// Schritt 1b) Verwenden des benutzerdefinierten Steuerelements in einer XAML-Datei, die in einem anderen Projekt vorhanden ist.
    /// Fügen Sie dieses XmlNamespace-Attribut dem Stammelement der Markupdatei 
    /// an der Stelle hinzu, an der es verwendet werden soll:
    ///
    ///     xmlns:MyNamespace="clr-namespace:HAW_Tool.UserControls;assembly=HAW_Tool.UserControls"
    ///
    /// Darüber hinaus müssen Sie von dem Projekt, das die XAML-Datei enthält, einen Projektverweis
    /// zu diesem Projekt hinzufügen und das Projekt neu erstellen, um Kompilierungsfehler zu vermeiden:
    ///
    ///     Klicken Sie im Projektmappen-Explorer mit der rechten Maustaste auf das Zielprojekt und anschließend auf
    ///     "Verweis hinzufügen"->"Projekte"->[Navigieren Sie zu diesem Projekt, und wählen Sie es aus.]
    ///
    ///
    /// Schritt 2)
    /// Fahren Sie fort, und verwenden Sie das Steuerelement in der XAML-Datei.
    ///
    ///     <MyNamespace:RangeSlider/>
    ///
    /// </summary>
    [TemplatePart(Name = "PART_LeftThumb", Type = typeof(Thumb))]
    [TemplatePart(Name = "PART_RightThumb", Type = typeof(Thumb))]
    public class RangeSlider : Control
    {
        private TimeToValueConverter cvt = new TimeToValueConverter();

        static RangeSlider()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RangeSlider), new FrameworkPropertyMetadata(typeof(RangeSlider)));
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            cvt.SizeOfSlider = ActualWidth;
            base.OnRenderSizeChanged(sizeInfo);
        }

        private TimeSpan RightPosToTime()
        {
            var pos = ActualWidth - (double)_rightThumb.GetValue(Canvas.RightProperty);
            var perc = pos / ActualWidth;

            return PercToTime(perc);
        }

        private TimeSpan LeftPosToTime()
        {
            var pos = (double)_leftThumb.GetValue(Canvas.LeftProperty);
            var perc = pos / ActualWidth;

            return PercToTime(perc);
        }

        private TimeSpan PercToTime(double perc)
        {
            var timeDelta = (MaxTime - MinTime).TotalMilliseconds;
            var timePerc = timeDelta * perc;
            var ret = TimeSpan.FromMilliseconds(timePerc) + MinTime;
            return ret;
        }

        private Thumb _leftThumb, _rightThumb;

        public override void OnApplyTemplate()
        {
            _leftThumb = base.GetTemplateChild("PART_LeftThumb") as Thumb ?? new Thumb();
            _rightThumb = base.GetTemplateChild("PART_RightThumb") as Thumb ?? new Thumb();

            _leftThumb.SetBinding(Canvas.LeftProperty, new Binding("LeftThumbPos") { Source = this });
            _rightThumb.SetBinding(Canvas.RightProperty, new Binding("RightThumbPos") { Source = this });

            _leftThumb.DragDelta += DragDeltaLeft;
            _rightThumb.DragDelta += DragDeltaRight;

            base.OnApplyTemplate();
        }

        private void DragDeltaRight(object sender, DragDeltaEventArgs e)
        {
            RightThumbPos -= e.HorizontalChange;
        }

        private void DragDeltaLeft(object sender, DragDeltaEventArgs e)
        {
            LeftThumbPos += e.HorizontalChange;
        }

        public double LeftThumbPos
        {
            get { return (double)GetValue(LeftThumbPosProperty); }
            private set { SetValue(LeftThumbPosProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LeftThumbPos.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LeftThumbPosProperty =
            DependencyProperty.Register("LeftThumbPos", typeof(double), typeof(RangeSlider), new UIPropertyMetadata(0.0D, LeftPosChanged));

        private static void LeftPosChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var r = (RangeSlider) d;
            r.LeftTime = r.LeftPosToTime();
            Console.WriteLine("Left Time: {0}", r.LeftTime);
        }

        public double RightThumbPos
        {
            get { return (double)GetValue(RightThumbPosProperty); }
            private set { SetValue(RightThumbPosProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RightThumbPos.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RightThumbPosProperty =
            DependencyProperty.Register("RightThumbPos", typeof(double), typeof(RangeSlider), new UIPropertyMetadata(0.0D, RightPosChanged));

        private static void RightPosChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var r = (RangeSlider)d;
            r.RightTime = r.RightPosToTime();
            Console.WriteLine("Right Time: {0}", r.RightTime);
        }

        public TimeSpan MinTime
        {
            get { return (TimeSpan)GetValue(MinTimeProperty); }
            set { SetValue(MinTimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MinTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinTimeProperty =
            DependencyProperty.Register("MinTime", typeof(TimeSpan), typeof(RangeSlider), new UIPropertyMetadata(TimeSpan.Parse("7:00"), MinTimeChanged));

        private static void MinTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            throw new NotImplementedException();
        }


        public TimeSpan MaxTime
        {
            get { return (TimeSpan)GetValue(MaxTimeProperty); }
            set { SetValue(MaxTimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxTimeProperty =
            DependencyProperty.Register("MaxTime", typeof(TimeSpan), typeof(RangeSlider), new UIPropertyMetadata(TimeSpan.Parse("21:00"), MaxTimeChanged));

        private static void MaxTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            throw new NotImplementedException();
        }


        public TimeSpan LeftTime
        {
            get { return (TimeSpan)GetValue(LeftTimeProperty); }
            set { SetValue(LeftTimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LeftTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LeftTimeProperty =
            DependencyProperty.Register("LeftTime", typeof(TimeSpan), typeof(RangeSlider), new UIPropertyMetadata(TimeSpan.Parse("7:00")));




        public TimeSpan RightTime
        {
            get { return (TimeSpan)GetValue(RightTimeProperty); }
            set { SetValue(RightTimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RightTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RightTimeProperty =
            DependencyProperty.Register("RightTime", typeof(TimeSpan), typeof(RangeSlider), new UIPropertyMetadata(TimeSpan.Parse("21:00")));

        
        
    }
}
