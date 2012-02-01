using System.ServiceModel;
using System.ServiceModel.Description;
using HAW_Tool.HAW.Depending;
using HAW_Tool.HAW.REST;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace HAWToolTests
{
    
    
    /// <summary>
    ///Dies ist eine Testklasse für "PlanFileTest" und soll
    ///alle PlanFileTest Komponententests enthalten.
    ///</summary>
    [TestClass()]
    public class PlanFileTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Ruft den Testkontext auf, der Informationen
        ///über und Funktionalität für den aktuellen Testlauf bietet, oder legt diesen fest.
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


        /// <summary>
        ///Ein Test für "LoadMongoEvents"
        ///</summary>
        [TestMethod()]
        public void LoadMongoEventsTest()
        {
            PlanFile.Instance.LoadSchedules(new string[]{ "http://www.etech.haw-hamburg.de/Stundenplan/Sem_IuE.txt"});
            PlanFile.Instance.LoadMongoEvents();
        }
    }
}
