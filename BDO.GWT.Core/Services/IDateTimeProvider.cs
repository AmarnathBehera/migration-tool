using System;

namespace BDO.GWT.Common.Core.Services;

public interface IDateTimeProvider
{
    DateTime Now { get; }
}

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime Now => DateTime.Now;
}
