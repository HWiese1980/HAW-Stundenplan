#region Usings

using System;
using System.Collections.Specialized;
using HAW_Tool.HAW.Depending;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeveQsCustomControls;

#endregion

namespace HAWToolTests
{
    ///<summary>
    ///  Dies ist eine Testklasse für "DayTest" und soll
    ///  alle DayTest Komponententests enthalten.
    ///</summary>
    [TestClass]
    public class DayTest
    {
        ///<summary>
        ///  Ruft den Testkontext auf, der Informationen
        ///  über und Funktionalität für den aktuellen Testlauf bietet, oder legt diesen fest.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Zusätzliche Testattribute

        // 
        //Sie können beim Verfassen Ihrer Tests die folgenden zusätzlichen Attribute verwenden:
        //
        //Mit ClassInitialize führen Sie Code aus, bevor Sie den ersten Test in der Klasse ausführen.
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Mit ClassCleanup führen Sie Code aus, nachdem alle Tests in einer Klasse ausgeführt wurden.
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Mit TestInitialize können Sie vor jedem einzelnen Test Code ausführen.
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Mit TestCleanup können Sie nach jedem einzelnen Test Code ausführen.
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //

        #endregion

        ///<summary>
        ///  Ein Test für "Day-Konstruktor"
        ///</summary>
        [TestMethod]
        public void DayConstructorTest()
        {
            var target = new Day();
            Assert.IsNotNull(target.Events);
        }

        ///<summary>
        ///  Ein Test für "Events_CollectionChanged"
        ///</summary>
        [TestMethod]
        [DeploymentItem("HAW_Tool.exe")]
        public void EventsCollectionChangedTest()
        {
            var target = new Day_Accessor();
            object sender = target.Events;
            var evt = new Event();
            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new[] {evt});
            target.EventsCollectionChanged(sender, e);
            Assert.IsNotNull(evt.Day);
            Assert.AreEqual(target.Date, evt.Day.Date);
        }

        ///<summary>
        ///  Ein Test für "IsSpanOccupiedByOthers"
        ///</summary>
        [TestMethod]
        [DeploymentItem("HAW_Tool.exe")]
        public void IsSpanOccupiedByOthersTest()
        {
            var target = new Day_Accessor();

            var a = new Event
                        {
                            CalendarWeek = 1,
                            From = TimeSpan.Parse("08:10"),
                            Till = TimeSpan.Parse("10:00")
                        };

            var b = new Event
                        {
                            CalendarWeek = 1,
                            From = TimeSpan.Parse("09:10"),
                            Till = TimeSpan.Parse("11:25")
                        };

            var c = new Event
                        {
                            CalendarWeek = 1,
                            From = TimeSpan.Parse("14:00"),
                            Till = TimeSpan.Parse("16:00")
                        };


            target.Events.Add(a);
            target.Events.Add(b);
            target.Events.Add(c);

            var actualA = target.IsSpanOccupiedByOthers(a, a.Row);
            var actualB = target.IsSpanOccupiedByOthers(b, b.Row);
            var actualC = target.IsSpanOccupiedByOthers(c, c.Row);
            Assert.AreEqual(true, actualA);
            Assert.AreEqual(true, actualB);
            Assert.AreEqual(false, actualC);
        }

        ///<summary>
        ///  Ein Test für "RecalculateRowIndex"
        ///</summary>
        [TestMethod]
        [DeploymentItem("HAW_Tool.exe")]
        public void RecalculateRowIndexTest()
        {
            var target = new Day_Accessor();

            var a = new Event
            {
                CalendarWeek = 1,
                From = TimeSpan.Parse("08:10"),
                Till = TimeSpan.Parse("10:00")
            };

            var b = new Event
            {
                CalendarWeek = 1,
                From = TimeSpan.Parse("09:10"),
                Till = TimeSpan.Parse("11:25")
            };

            var c = new Event
            {
                CalendarWeek = 1,
                From = TimeSpan.Parse("14:00"),
                Till = TimeSpan.Parse("16:00")
            };


            target.Events.Add(a);
            target.Events.Add(b);
            target.Events.Add(c);

            target.RecalculateRowIndex(a);

            Assert.AreEqual(a.Row, 1);
            Assert.AreEqual(b.Row, 0);
            Assert.AreEqual(c.Row, 0);

            c.From = TimeSpan.Parse("09:30");
            c.Till = TimeSpan.Parse("10:30");

            target.RecalculateRowIndex(c);
            Assert.AreEqual(a.Row, 1);
            Assert.AreEqual(b.Row, 0);
            Assert.AreEqual(c.Row, 2);

            target.RecalculateRowIndex(b);
            Assert.AreEqual(a.Row, 1);
            Assert.AreEqual(b.Row, 0);
            Assert.AreEqual(c.Row, 0);
        }

        ///<summary>
        ///  Ein Test für "RecalculateRowIndexAll"
        ///</summary>
        [TestMethod]
        public void RecalculateRowIndexAllTest()
        {
            var target = new Day(); // TODO: Passenden Wert initialisieren
            target.RecalculateRowIndexAll();
            Assert.Inconclusive("Eine Methode, die keinen Wert zurückgibt, kann nicht überprüft werden.");
        }

        ///<summary>
        ///  Ein Test für "RemoveAllCouchDBEvents"
        ///</summary>
        [TestMethod]
        public void RemoveAllCouchDBEventsTest()
        {
            var target = new Day(); // TODO: Passenden Wert initialisieren
            var hashInfo = string.Empty; // TODO: Passenden Wert initialisieren
            target.RemoveAllCouchDBEvents(hashInfo);
            Assert.Inconclusive("Eine Methode, die keinen Wert zurückgibt, kann nicht überprüft werden.");
        }

        ///<summary>
        ///  Ein Test für "ResetRowIndex"
        ///</summary>
        [TestMethod]
        [DeploymentItem("HAW_Tool.exe")]
        public void ResetRowIndexTest()
        {
            var target = new Day_Accessor(); // TODO: Passenden Wert initialisieren
            Event e = null; // TODO: Passenden Wert initialisieren
            var expected = false; // TODO: Passenden Wert initialisieren
            bool actual;
            actual = target.ResetRowIndex(e);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Überprüfen Sie die Richtigkeit dieser Testmethode.");
        }

        ///<summary>
        ///  Ein Test für "ToString"
        ///</summary>
        [TestMethod]
        public void ToStringTest()
        {
            var target = new Day(); // TODO: Passenden Wert initialisieren
            var expected = string.Empty; // TODO: Passenden Wert initialisieren
            string actual;
            actual = target.ToString();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Überprüfen Sie die Richtigkeit dieser Testmethode.");
        }

        ///<summary>
        ///  Ein Test für "evt_TimeChanged"
        ///</summary>
        [TestMethod]
        [DeploymentItem("HAW_Tool.exe")]
        public void evt_TimeChangedTest()
        {
            var target = new Day_Accessor(); // TODO: Passenden Wert initialisieren
            object sender = null; // TODO: Passenden Wert initialisieren
            EventArgs e = null; // TODO: Passenden Wert initialisieren
            target.evt_TimeChanged(sender, e);
            Assert.Inconclusive("Eine Methode, die keinen Wert zurückgibt, kann nicht überprüft werden.");
        }

        ///<summary>
        ///  Ein Test für "DOW"
        ///</summary>
        [TestMethod]
        public void DOWTest()
        {
            var target = new Day(); // TODO: Passenden Wert initialisieren
            var expected = new DayOfWeek(); // TODO: Passenden Wert initialisieren
            DayOfWeek actual;
            target.DOW = expected;
            actual = target.DOW;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Überprüfen Sie die Richtigkeit dieser Testmethode.");
        }

        ///<summary>
        ///  Ein Test für "Date"
        ///</summary>
        [TestMethod]
        public void DateTest()
        {
            var target = new Day(); // TODO: Passenden Wert initialisieren
            var expected = new DateTime(); // TODO: Passenden Wert initialisieren
            DateTime actual;
            target.Date = expected;
            actual = target.Date;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Überprüfen Sie die Richtigkeit dieser Testmethode.");
        }

        ///<summary>
        ///  Ein Test für "Week"
        ///</summary>
        [TestMethod]
        [DeploymentItem("HAW_Tool.exe")]
        public void WeekTest()
        {
            var target = new Day_Accessor(); // TODO: Passenden Wert initialisieren
            CalendarWeek expected = null; // TODO: Passenden Wert initialisieren
            CalendarWeek actual;
            target.Week = expected;
            actual = target.Week;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Überprüfen Sie die Richtigkeit dieser Testmethode.");
        }
    }
}