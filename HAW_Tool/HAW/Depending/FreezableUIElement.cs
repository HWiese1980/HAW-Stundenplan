using System.Windows;

namespace HAW_Tool.HAW.Depending
{
    public abstract class FreezableUIElement : UIElement
    {
        //public bool IsDirty
        //{
        //    get { return (bool)GetValue(IsDirtyProperty); }
        //    set { SetValue(IsDirtyProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for IsDirty.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty IsDirtyProperty =
        //    DependencyProperty.Register("IsDirty", typeof(bool), typeof(Freezable), new UIPropertyMetadata(false));

        public abstract bool IsDirty { get; set; }

        public void Freeze()
        {
            IsDirty = false;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if(e.Property.Name != "IsDirty" && 
                e.Property.Name != "IsSelected" &&
                e.Property.Name != "Row") 
                IsDirty = true;
            base.OnPropertyChanged(e);
        }
    }
}
