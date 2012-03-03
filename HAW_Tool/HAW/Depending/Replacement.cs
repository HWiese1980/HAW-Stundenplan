using System;
using MongoDB.Bson.Serialization.Attributes;
using SeveQsCustomControls;

namespace HAW_Tool.HAW.Depending
{
    [BsonIgnoreExtraElements]
    public class Replacement : IKeyedObject
    {
        public Event Event { get; set; }
        public DateTime Timestamp { get; set; }

        #region Implementation of IKeyedObject

        public string Key
        {
            get { return Event.HashInfo; }
        }

        #endregion
    }
}