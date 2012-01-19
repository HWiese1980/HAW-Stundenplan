using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace HAW_Tool.HAW.Depending
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class CouchDBEventInfo
    {
        [JsonProperty("timestamp")]
        public DateTime TimeStamp { get; set; }

        [JsonProperty("eventinfohash")]
        public string EventInfoHash { get; set; }

        [JsonProperty("event")]
        public Event Event { get; set; }
    }
}
