using System;
using System.Collections.Generic;
using System.Text;

namespace BDO.GWT.Core.Entities.Search
{
    public class SuggestSearchResponse<T>
      where T : SearchDocument
    {
        public List<SuggestResult<T>> Results { get; set; } = new();
    }
}
