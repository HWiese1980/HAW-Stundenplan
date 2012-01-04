using System;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace HAW_Tool.HAW
{
    [DataContract]
    [XmlInclude(typeof(TimeSpan))]
    public class Change
    {
        DateTime _mTimeStamp = DateTime.Now;

        string _mProperty;

        [DataMember]
        [XmlAttribute("property")]
        public string Property
        {
            get { return _mProperty; }
            set { _mProperty = value; }
        }

        object _mOldValue;

        [DataMember]
        [XmlElement("oldvalue")]
        public object OldValue
        {
            get { return _mOldValue; }
            set { _mOldValue = ConvertValue(value); }
        }

        object _mNewValue;

        [DataMember]
        [XmlElement("newvalue")]
        public object NewValue
        {
            get { return _mNewValue; }
            set { _mNewValue = ConvertValue(value); }
        }

        [DataMember]
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
