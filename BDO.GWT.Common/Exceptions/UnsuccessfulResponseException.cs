using System;

namespace BDO.GWT.Common.Exceptions;

public class UnsuccessfulResponseException : Exception
{
    public UnsuccessfulResponseException(string message) : base(message) { }
}
