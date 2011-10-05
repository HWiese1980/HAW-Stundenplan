using System;
using System.ServiceModel;
using System.ServiceModel.Web;
using HAW_Tool.Bittorrent;
using HAW_Tool.Aspects;

namespace HAW_Tool.HAW.REST
{
    [ServiceContract]
    public interface IHAWRest
    {
		#region Operations (5) 

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "addfile")]
        string AddFile(RESTTorrent torrent);

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json, UriTemplate = "events")]
        RESTEvent[] Events();

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json, UriTemplate = "hawevents")]
        RESTEvent[] HAWEvents();

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "login")]
        RESTUserData Login(RESTUserData data);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.Bare, RequestFormat=WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "torrents")]
        RESTTorrent[] Torrents(string eventHash);

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json, UriTemplate = "schedules")]
        string[] Schedules();

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json, UriTemplate = "version")]
        string Version();

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "exceptionreport")]
        void ReportException(Exception exp);

		#endregion Operations 
    }


    [WCFExceptionHandling]
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

        public RESTEvent[] Events()
        {
            return Channel.Events();
        }

        public RESTEvent[] HAWEvents()
        {
            return base.Channel.HAWEvents();
        }

        public RESTTorrent[] Torrents(string eventHash)
        {
            return base.Channel.Torrents(eventHash);
        }

        public RESTUserData Login(RESTUserData data)
        {
            return (base.Channel.Login(data));
        }

        public string AddFile(RESTTorrent torrent)
        {
            return (base.Channel.AddFile(torrent));
        }

        public void ReportException(Exception exp)
        {
            base.Channel.ReportException(exp);
        }

        #endregion
    }
}
