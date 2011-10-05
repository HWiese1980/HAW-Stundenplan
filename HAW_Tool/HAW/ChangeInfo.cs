using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace HAW_Tool.HAW
{
    [XmlRoot("changeinfo")]
    public class ChangeInfo
    {
        public static XElement AsXML(ChangeInfo info)
        {
            var tSer = new XmlSerializer(typeof(ChangeInfo));
            var tStrm = new MemoryStream();

            tSer.Serialize(tStrm, info);

            tStrm.Seek(0, SeekOrigin.Begin);

            var tRdr = new StreamReader(tStrm);
            var tXML = tRdr.ReadToEnd();

            tStrm.Close();

            return XElement.Parse(tXML);
        }

        public static ChangeInfo FromXML(XElement xml)
        {
            var tStrm = new MemoryStream(Encoding.Default.GetBytes(xml.ToString()));
            var tSer = new XmlSerializer(typeof(ChangeInfo));
            var tRet = (ChangeInfo)tSer.Deserialize(tStrm);

            return tRet;
        }

        string _mHash = "";

        [XmlAttribute("hash")]
        public string EventHash
        {
            get
            {
                return _mHash;
            }
            set
            {
                _mHash = value;
                Event = PlanFile.Instance.GetEventByCode(value);
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
            var tCleanedChanges = new List<Change>();

            foreach (var tChg in EventChanges)
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
}