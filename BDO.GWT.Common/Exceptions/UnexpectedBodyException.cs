using System;

namespace BDO.GWT.Common.Exceptions;

public class UnexpectedBodyException : Exception
{
    public UnexpectedBodyException(string message) : base(message) { }
}
