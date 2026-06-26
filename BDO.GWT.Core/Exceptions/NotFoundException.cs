using System;

namespace BDO.GWT.Common.Core.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException() { }

    public NotFoundException(string message) : base(message) { }
}
