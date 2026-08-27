using System;
using System.Collections.Generic;
using System.Text;

namespace BDO.GWT.Core.Extensions
{
      public class AzureCognitiveSearchOptions
        {
            public Uri ServiceUrl { get; set; } = null!;

            public string QueryKey { get; set; } = null!;

            public string? IndexSuffix { get; set; }
        }
    
}
