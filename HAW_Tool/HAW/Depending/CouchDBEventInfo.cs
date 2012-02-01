using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace HAW_Tool.HAW.Depending
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [BsonIgnoreExtraElements]
    public class CouchDBEventInfo
    {
        [JsonProperty("timestamp")]
        [BsonElement("timestamp")]
        public DateTime TimeStamp { get; set; }

        [JsonProperty("eventinfohash")]
        [BsonElement("eventinfohash")]
        public string EventInfoHash { get; set; }

        [JsonProperty("event")]
        [BsonElement("event")]
        public Event Event { get; set; }
    }
}
