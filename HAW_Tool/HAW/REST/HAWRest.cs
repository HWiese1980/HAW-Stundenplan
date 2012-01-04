using System;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace HAW_Tool.HAW.REST
{
    [ServiceContract]
    public interface IHAWRest
    {
		#region Operations (5) 

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json, UriTemplate = "events")]
        RESTEvent[] Events();

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json, UriTemplate = "hawevents")]
        RESTEvent[] HAWEvents();

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
}
