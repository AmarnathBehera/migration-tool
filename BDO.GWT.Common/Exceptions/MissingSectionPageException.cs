using System;

namespace BDO.GWT.Common.Core.Exceptions;

[Serializable]
public class MissingSectionPageException : Exception
{
    public MissingSectionPageException(string message) : base(message) { }
}
