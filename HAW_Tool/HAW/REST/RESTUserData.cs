using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace HAW_Tool.HAW.REST
{
    [DataContract(Name="RESTUserData")]
    public class RESTUserData
    {
		#region Properties (2) 

        [DataMember(Name = "password")]
        public string Password { get; set; }

        [DataMember(Name="username")]
        public string Username { get; set; }

		#endregion Properties 
    }
}
