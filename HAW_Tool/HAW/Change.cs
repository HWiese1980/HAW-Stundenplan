using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Xml.Serialization;

namespace HAW_Tool.HAW
{
    public enum ChangeState
    {
        Local,
        Published
    }

    [XmlRoot("changeinfo")]
    public class ChangeInfo
    {
        public static XElement AsXML(ChangeInfo Info)
        {
            XmlSerializer tSer = new XmlSerializer(typeof(ChangeInfo));
            MemoryStream tStrm = new MemoryStream();

            tSer.Serialize(tStrm, Info);

            tStrm.Seek(0, SeekOrigin.Begin);

            StreamReader tRdr = new StreamReader(tStrm);
            string tXML = tRdr.ReadToEnd();

            tStrm.Close();

            return XElement.Parse(tXML);
        }

        public static ChangeInfo FromXML(XElement XML)
        {
            MemoryStream tStrm = new MemoryStream(Encoding.Default.GetBytes(XML.ToString()));
            XmlSerializer tSer = new XmlSerializer(typeof(ChangeInfo));
            ChangeInfo tRet = (ChangeInfo)tSer.Deserialize(tStrm);

            return tRet;
        }

        string mHash = "";

        [XmlAttribute("hash")]
        public string EventHash
        {
            get
            {
                return mHash;
            }
            set
            {
                mHash = value;
                Event = (Event)PlanFile.Instance.GetEventByCode(value);
            }
        }

        [XmlElement("change")]
        public List<Change> EventChanges { get; set; }

        [XmlIgnore]
        public Event Event { get; set; }

        [XmlIgnore]
        public ChangeState ChangeState { get; set; }

        [XmlIgnore]
        public string Info
        {
            get
            {
                return String.Format("{0} - {1}", Event.Code, EventChanges.Count);
            }
        }

        //public static IEnumerable<ChangeInfo> Combine(IEnumerable<ChangeInfo> Changes)
        //{

        //}

        public void Cleanup()
        {
            List<Change> tCleanedChanges = new List<Change>();

            foreach (Change tChg in EventChanges)
            {
                var tOthers = from chg in EventChanges
                              where chg.Property == tChg.Property
                              orderby chg.Timestamp ascending
                              select chg;

                if (tCleanedChanges.Count(p => p.Property == tOthers.Last().Property) <= 0)
                    tCleanedChanges.Add(tOthers.Last());
            }

            EventChanges = tCleanedChanges;
        }
    }

    [XmlInclude(typeof(TimeSpan))]
    public class Change
    {
        DateTime mTimeStamp = DateTime.Now;

        string mProperty;

        [XmlAttribute("property")]
        public string Property
        {
            get { return mProperty; }
            set { mProperty = value; }
        }

        object mOldValue;

        [XmlElement("oldvalue")]
        public object OldValue
        {
            get { return mOldValue; }
            set { mOldValue = ConvertValue(value); }
        }

        object mNewValue;

        [XmlElement("newvalue")]
        public object NewValue
        {
            get { return mNewValue; }
            set { mNewValue = ConvertValue(value); }
        }

        [XmlAttribute("timestamp")]
        public string Timestamp
        {
            get { return mTimeStamp.ToString(); }
            set { mTimeStamp = DateTime.Parse(value); }
        }


        private object ConvertValue(object Value)
        {
            switch (mProperty)
            {
                case "From":
                case "Till":
                    {
                        if (Value is TimeSpan)
                            return ((TimeSpan)Value).ToString();
                        else if (Value is String)
                            return TimeSpan.Parse(Value.ToString());
                        else
                            return null;
                    }

                default: return Value;
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

        public Change(string Property, object OldValue, object NewValue)
        {
            mProperty = Property; mOldValue = OldValue; mNewValue = NewValue;
        }

        public Change()
        {
        }
    }
}
