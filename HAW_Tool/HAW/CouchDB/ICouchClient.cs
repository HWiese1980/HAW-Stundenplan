using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace HAW_Tool.HAW.CouchDB
{
    [ServiceContract]
    public interface ICouchClient
    {
        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json, UriTemplate = "/changes/_design/AllChanges/_view/AllChanges")]
        CouchDBResponse[] GetChanges();

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "/changes")]
        CouchDBResponse PutChange(CouchDocChange c);
    }

    public class CouchClient : ClientBase<ICouchClient>, ICouchClient
    {
        public CouchDBResponse[] GetChanges()
        {
            return Channel.GetChanges();
        }

        public CouchDBResponse PutChange(CouchDocChange c)
        {
            return Channel.PutChange(c);
        }
    }
}
