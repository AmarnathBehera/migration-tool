using System;
using System.Collections.Generic;
using System.Text;

namespace BDO.GWT.Core.Exceptions
{
    public class MissingPropertyMapException : Exception
    {
        public MissingPropertyMapException(string message) : base(message) { }
    }

}
