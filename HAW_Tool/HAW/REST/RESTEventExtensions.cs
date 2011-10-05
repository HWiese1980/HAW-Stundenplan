using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HAW_Tool.Bittorrent;

namespace HAW_Tool.HAW.REST
{
    public static class RESTEventExtensions
    {
		#region Methods (1) 

		// Public Methods (1) 

        public static void AddFile(this IEvent me, RESTTorrent torrent)
        {
            var tCnt = new HAWClient();
            tCnt.AddFile(torrent);
        }

		#endregion Methods 
    }
}
