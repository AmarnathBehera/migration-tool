using System;

namespace BDO.GWT.Common.Exceptions;

public class InvalidStateException : Exception
{
    public InvalidStateException() { }

    public InvalidStateException(string message) : base(message) { }
}
