using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace HAW_Tool.HAW.CouchDB
{
    [DataContract]
    public class CouchDBResponse
    {
        [DataMember(Name = "ok")]
        public bool OK { get; set; }

        [DataMember(Name = "error")]
        public string ErrorMessage { get; set; }

        [DataMember(Name = "reason")]
        public string ReasonObject { get; set; }

        [DataMember(Name = "id")]
        public string ID { get; set; }

        [DataMember(Name = "rev")]
        public string Revision { get; set; }

        [DataMember(Name = "value")]
        public dynamic Value { get; set; }
    }
}
