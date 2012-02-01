using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace HAWToolTests
{
    [TestClass]
    public class MongoDBTest
    {
        [TestMethod]
        public void TestDate()
        {
            MongoServer s = MongoServer.Create("mongodb://seveq.de");
            MongoDatabase d = s.GetDatabase("TestDate");
            var c = d.GetCollection<ContainsDateTime>("DateTestColl");
            c.Insert(new ContainsDateTime { Date = DateTime.Now });
            
            var c2 = d.GetCollection<ContainsDateTime>("DateTestColl");
            var dts = c2.FindAllAs<ContainsDateTime>();
            foreach(var dt in dts)
            {
                Assert.AreEqual(DateTime.Now.Date, dt.Date.Date);
            }
        }

    }

    [BsonIgnoreExtraElements]
    class ContainsDateTime
    {
        [BsonElement("date")]
        public DateTime Date { get; set; }
    }
}
