using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SeveQsCustomControls
{
    public class SpecialException : Exception
    {
        public SpecialException(string message)
            : base(message)
        { }
    }
}
