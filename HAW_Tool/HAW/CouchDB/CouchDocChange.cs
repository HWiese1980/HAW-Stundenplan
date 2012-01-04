using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace HAW_Tool.HAW.CouchDB
{
    public class CouchDocChange
    {
        [DataMember(Name="EventHash")]
        public string EventHash { get; set; }

        [DataMember(Name="Change")]
        public Change Change { get; set; }
    }
}
