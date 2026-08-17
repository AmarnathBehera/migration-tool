using System;
using System.Collections.Generic;
using System.Text;

namespace BDO.GWT.Core.Entities.Search
{
    public class SearchResponse<T>
    where T : SearchDocument
    {
        public long? Count { get; set; }

        public IDictionary<string, IEnumerable<SearchFacet>> Facets { get; set; } = new Dictionary<string, IEnumerable<SearchFacet>>();

        public List<T> Documents { get; set; } = new();
    }
}
