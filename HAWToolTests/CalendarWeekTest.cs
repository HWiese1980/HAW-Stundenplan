#region Usings

using System;
using System.Threading.Tasks;
using HAW_Tool.HAW.Depending;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace HAWToolTests
{
    [TestClass]
    public class CalendarWeekTest
    {
        [TestMethod]
        public void TestInitializationOneWeek()
        {
            var cw = new CalendarWeek(1, 2012);
            Console.WriteLine("TestInitializationOneWeek done");
        }

        [TestMethod]
        public void TestInitializationBunchOfWeeks()
        {
            Parallel.For(0, 55, i => { var cw = new CalendarWeek(1, 2012); });
            Console.WriteLine("TestInitializationBunchOfWeeks done");
        }

        [TestMethod]
        public void TestInitializationBunchOfYears()
        {
            Parallel.For(2000, 2020, j => Parallel.For(0, 55, i => { var cw = new CalendarWeek(i, j); }));
            Console.WriteLine("TestInitializationBunchOfYears done");
        }

        ///<summary>
        ///  Ein Test für "CalendarWeek-Konstruktor"
        ///</summary>
        [TestMethod]
        public void CalendarWeekConstructorTest()
        {
            var target = new CalendarWeek(1, 2012);
            Assert.IsNotNull(target);
            Assert.IsNotNull(target.Days);
        }

        ///<summary>
        ///  Ein Test für "GetDateOfWeekday"
        ///</summary>
        [TestMethod]
        public void GetDateOfWeekdayTest()
        {
            var target = new CalendarWeek(1, 2012); 
            var day = 2;
            var expected = new DateTime(2012, 1, 4, 0, 0, 0); 
            DateTime actual;
            actual = target.GetDateOfWeekday(day);
            Assert.AreEqual(expected, actual);
        }

        ///<summary>
        ///  Ein Test für "ToString"
        ///</summary>
        [TestMethod]
        public void ToStringTest()
        {
            var target = new CalendarWeek(1, 2012);
            var expected = "[1:2012]";
            string actual;
            actual = target.ToString();
            Assert.AreEqual(expected, actual);
        }

        ///<summary>
        ///  Ein Test für "Days"
        ///</summary>
        [TestMethod]
        public void DaysTest()
        {
            var target = new CalendarWeek(1, 2012);
            ;
            Assert.IsNotNull(target.Days);
            Assert.AreEqual(7, target.Days.Count);
        }

        ///<summary>
        ///  Ein Test für "IsCurrent"
        ///</summary>
        [TestMethod]
        public void IsCurrentTest()
        {
            var target = new CalendarWeek(1, 2012);
            Assert.IsFalse(target.IsCurrent);
        }

        ///<summary>
        ///  Ein Test für "Label"
        ///</summary>
        [TestMethod]
        public void LabelTest()
        {
            var target = new CalendarWeek(1, 2012); // TODO: Passenden Wert initialisieren
            Assert.AreEqual("KW-1", target.LabelShort);
        }

        ///<summary>
        ///  Ein Test für "LabelShort"
        ///</summary>
        [TestMethod]
        public void LabelShortTest()
        {
            var target = new CalendarWeek(1, 2012); // TODO: Passenden Wert initialisieren
            Assert.AreEqual("KW-1", target.LabelShort);
        }
    }
}