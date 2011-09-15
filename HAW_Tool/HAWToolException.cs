using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HAW_Tool
{
    public class HAWToolException : Exception
    {
        public HAWToolException(string Text)
            : base(Text)
        {

        }
    }
}
