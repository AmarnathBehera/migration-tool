using System;
using System.Collections.Generic;
using System.Text;

namespace BDO.GWT.Core.Entities.Search
{
    public class SuggestResult<T>
        where T : SearchDocument
    {
        public string Text { get; set; } = null!;

        public T Document { get; set; } = null!;
    }
}
