using System;

namespace BDO.GWT.Common.Exceptions;

public class MissingSectionPageException : Exception
{
    public MissingSectionPageException(string message) : base(message) { }
}
