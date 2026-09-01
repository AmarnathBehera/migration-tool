using System;
using System.Collections.Generic;
using System.Text;

namespace BDO.GWT.Core.Exceptions
{
    public class MissingTypeMapException : Exception
    {
        public MissingTypeMapException(string message) : base(message) { }
    }
}
