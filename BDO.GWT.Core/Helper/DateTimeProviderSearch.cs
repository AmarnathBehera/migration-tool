using System;
using System.Collections.Generic;
using System.Text;

namespace BDO.GWT.Core.Helpers
{
    public interface IDateTimeProviderSearch
    {
        DateTime Now { get; }
    }

    public class DateTimeProviderSearch : IDateTimeProviderSearch
    {
        public DateTime Now => DateTime.Now;
    }
}
