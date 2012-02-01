using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using HAW_Tool.HAW.Depending;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HAWToolTests
{
    /// <summary>
    /// Zusammenfassungsbeschreibung für EventTest
    /// </summary>
    [TestClass]
    public class EventTest
    {
        public EventTest()
        {
            //
            // TODO: Konstruktorlogik hier hinzufügen
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Ruft den Textkontext mit Informationen über
        ///den aktuellen Testlauf sowie Funktionalität für diesen auf oder legt diese fest.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Zusätzliche Testattribute
        //
        // Sie können beim Schreiben der Tests folgende zusätzliche Attribute verwenden:
        //
        // Verwenden Sie ClassInitialize, um vor Ausführung des ersten Tests in der Klasse Code auszuführen.
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Verwenden Sie ClassCleanup, um nach Ausführung aller Tests in einer Klasse Code auszuführen.
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Mit TestInitialize können Sie vor jedem einzelnen Test Code ausführen. 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Mit TestCleanup können Sie nach jedem einzelnen Test Code ausführen.
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestSetDate()
        {
            var grp = new SeminarGroup();
            var week1 = new CalendarWeek(1, 2012);
            var week2 = new CalendarWeek(2, 2012);
            grp.CalendarWeeks.Add(week1);
            grp.CalendarWeeks.Add(week2);

            

            Assert.IsNotNull(grp.CalendarWeeks);
            Assert.IsTrue(grp.CalendarWeeks.Count >= 1);
            Assert.AreEqual(week1.Days.Count, 7);

            var evt = new Event();
            var day1 = week1.Days.Last();
            var day2 = week2.Days.First();

            day1.Events.Add(evt);
            Assert.AreEqual(day1.Events.Count, 1);
            Assert.AreEqual(day2.Events.Count, 0);

            Assert.IsNotNull(evt.Day);
            Assert.IsNotNull(evt.Date);
            Assert.AreEqual("Day Saturday of Week [1:2012] -> Date 08.01.2012 00:00:00", evt.Day.ToString());

            evt.Date = evt.Date.AddDays(1);
            Assert.AreEqual(day1.Events.Count, 0);
            Assert.AreEqual(day2.Events.Count, 1);

            Assert.IsNotNull(evt.Day);
            Assert.IsNotNull(evt.Date);
            Assert.AreEqual("Day Sunday of Week [2:2012] -> Date 09.01.2012 00:00:00", evt.Day.ToString());
        }
    }
}
