using System;
using System.Collections.Generic;
using System.Text;

namespace BDO.GWT.Core.Helpers
{
    public interface IDateTimeProvider
    {
        DateTime Now { get; }
    }

    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime Now => DateTime.Now;
    }
}
