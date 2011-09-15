﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        string AddFile(RESTTorrent Torrent);

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json, UriTemplate = "events")]
        RESTEvent[] Events();

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json, UriTemplate = "hawevents")]
        RESTEvent[] HAWEvents();

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "login")]
        RESTUserData Login(RESTUserData Data);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.Bare, RequestFormat=WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "torrents")]
        RESTTorrent[] Torrents(string EventHash);

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
    public partial class HAWClient : ClientBase<IHAWRest>, IHAWRest
    {
        #region IHAWRest Members

        public string Version()
        {
            return base.Channel.Version();
        }

        public string[] Schedules()
        {
            return base.Channel.Schedules();
        }

        public RESTEvent[] Events()
        {
            return base.Channel.Events();
        }

        public RESTEvent[] HAWEvents()
        {
            return base.Channel.HAWEvents();
        }

        public RESTTorrent[] Torrents(string EventHash)
        {
            return base.Channel.Torrents(EventHash);
        }

        public RESTUserData Login(RESTUserData Data)
        {
            return (base.Channel.Login(Data));
        }

        public string AddFile(RESTTorrent Torrent)
        {
            return (base.Channel.AddFile(Torrent));
        }

        public void ReportException(Exception exp)
        {
            base.Channel.ReportException(exp);
        }

        #endregion
    }
}