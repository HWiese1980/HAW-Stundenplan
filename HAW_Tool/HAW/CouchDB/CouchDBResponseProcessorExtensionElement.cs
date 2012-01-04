using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Configuration;

namespace HAW_Tool.HAW.CouchDB
{
    class CouchDBResponseProcessorExtensionElement : BehaviorExtensionElement
    {
        public override Type BehaviorType
        {
            get { return typeof(CouchDBResponseProcessor); }
        }

        protected override object CreateBehavior()
        {
            return new CouchDBResponseProcessor();
        }
    }
}
