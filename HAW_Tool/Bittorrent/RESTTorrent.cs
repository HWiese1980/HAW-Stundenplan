using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace HAW_Tool.Bittorrent
{
    [DataContract(Name="RESTtorrent")]
    public class RESTTorrent
    {
        [DataMember(Name="id")]
        public int ID
        {
            get; set;
        }


    }
}
