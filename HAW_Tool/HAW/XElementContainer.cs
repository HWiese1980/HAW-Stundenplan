using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Windows;
using System.ComponentModel;

namespace HAW_Tool.HAW
{
    public abstract class LabeledItem
    {
        public virtual string Label
        {
            get
            {
                return this.ToString();
            }
        }

        public virtual string ToolTip
        {
            get
            {
                return this.Label;
            }
        }
    }

    public abstract class XElementContainer : LabeledItem, INotifyPropertyChanged
    {
        protected XElement m_BaseElement;

        protected void OnPropertyChanged(string Property)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(Property));
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
