using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Channels;

namespace HAW_Tool.HAW.CouchDB
{
    class CouchDBMapper : WebContentTypeMapper
    {
        public override WebContentFormat GetMessageFormatForContentType(string contentType)
        {
            if(contentType.StartsWith("text/plain"))
            {
                return WebContentFormat.Json;
            }
            return WebContentFormat.Default;
        }
    }
}
