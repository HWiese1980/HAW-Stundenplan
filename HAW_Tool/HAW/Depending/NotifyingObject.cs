using System.ComponentModel;

namespace HAW_Tool.HAW.Depending
{
    public class NotifyingObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string property)
        {
            if(PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

    }
}
