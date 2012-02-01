using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using MongoDB.Bson.Serialization.Attributes;

namespace HAW_Tool.HAW.Depending
{
    public abstract class EventControlBase : DirtyableObject
    {
        #region Control Properties (non-depending)

        private Visibility _visibility;
        [BsonIgnore]
        public Visibility Visibility
        {
            get { return _visibility; }
            set { _visibility = value; OnPropertyChanged("Visibility"); }
        }

        private int _row;
        [BsonIgnore]
        public int Row
        {
            get { return _row; }
            set { _row = value; OnPropertyChanged("Row"); }
        }

        /*
        private int _index;
        public int Index
        {
            get { return _index; }
            set
            {
                _index = value;
                OnPropertyChanged("Index");
            }
        }
        */

        private bool _isExpanded;
        [BsonIgnore]
        public bool IsExpanded
        {
            get
            {
                return _isExpanded;
            }
            set
            {
                _isExpanded = value;
                OnPropertyChanged("IsExpanded");
            }
        }

        
        private bool _isSelected;
        [BsonIgnore]
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }


        private bool _isEnabled = true;
        [BsonIgnore]
        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                _isEnabled = value;
                OnPropertyChanged("IsEnabled");
            }
        }
                        
                        

        #endregion
    }
}
