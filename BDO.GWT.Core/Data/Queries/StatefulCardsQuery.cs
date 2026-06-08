using System;
using System.Collections.Generic;
using System.Text;

namespace BDO.GWT.Core.Data.Queries
{
    public class StatefulCardsQuery
    {
        public Guid State { get; set; }
        public IDictionary<string, string> Filters { get; set; } = new Dictionary<string, string>();

        public int? CurrentPage { get; set; }

        public int? PageSize { get; set; }

        public string? Order { get; set; }
        public bool? ComingSoon { get; set; }
      

    }
}
