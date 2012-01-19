using System;
using System.ServiceModel;

namespace HAW_Tool.HAW.REST
{
    public class HAWClient : ClientBase<IHAWRest>, IHAWRest
    {
        #region IHAWRest Members

        public string Version()
        {
            return Channel.Version();
        }

        public string[] Schedules()
        {
            return Channel.Schedules();
        }

/*
        public RESTEvent[] Events()
        {
            return Channel.Events();
        }

        public RESTEvent[] HAWEvents()
        {
            return base.Channel.HAWEvents();
        }
*/

        public void ReportException(Exception exp)
        {
            Channel.ReportException(exp);
        }

        #endregion
    }
}