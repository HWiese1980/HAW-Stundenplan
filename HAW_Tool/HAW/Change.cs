using System;
using System.Xml.Serialization;

namespace HAW_Tool.HAW
{
    [XmlInclude(typeof(TimeSpan))]
    public class Change
    {
        DateTime _mTimeStamp = DateTime.Now;

        string _mProperty;

        [XmlAttribute("property")]
        public string Property
        {
            get { return _mProperty; }
            set { _mProperty = value; }
        }

        readonly object _mOldValue;

        [XmlElement("oldvalue")]
        public object OldValue
        {
            get { return _mOldValue; }
/*
            set { _mOldValue = ConvertValue(value); }
*/
        }

        object _mNewValue;

        [XmlElement("newvalue")]
        public object NewValue
        {
            get { return _mNewValue; }
            set { _mNewValue = ConvertValue(value); }
        }

        [XmlAttribute("timestamp")]
        public string Timestamp
        {
            get { return _mTimeStamp.ToString(); }
            set { _mTimeStamp = DateTime.Parse(value); }
        }


        private object ConvertValue(object value)
        {
            switch (_mProperty)
            {
                case "From":
                case "Till":
                    {
                        if (value is TimeSpan)
                            return ((TimeSpan)value).ToString();
                        
                        if (value is String)
                            return TimeSpan.Parse(value.ToString());
                        
                        return null;
                    }

                default: return value;
            }

        }

        //string mChangedBy = Window1.MainWindow.UserName;
        //[XmlAttribute("changedby")]
        //public string ChangedBy
        //{
        //    get
        //    {
        //        return mChangedBy;
        //    }
        //    set
        //    {
        //        mChangedBy = value;
        //    }
        //}

        public Change(string property, object oldValue, object newValue)
        {
            _mProperty = property; _mOldValue = oldValue; _mNewValue = newValue;
        }
    }
}
