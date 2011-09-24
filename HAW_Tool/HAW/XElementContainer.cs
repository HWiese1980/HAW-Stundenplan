using System.Xml.Linq;
using System.ComponentModel;

namespace HAW_Tool.HAW
{
    public abstract class XElementContainer : LabeledItem, INotifyPropertyChanged
    {
        protected XElement MBaseElement;

        protected void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
