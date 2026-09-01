using System;
using System.Collections.Generic;
using System.Text;

namespace BDO.GWT.Core.Data.Queries
{
    public class SearchResultQuery : IPaginationQuery
    {
        public string? SearchText { get; set; }

        public string? SortBy { get; set; }

        public string? FilterBy { get; set; }

        public int? PageSize { get; set; }

        public int? CurrentPage { get; set; }
    }
}
