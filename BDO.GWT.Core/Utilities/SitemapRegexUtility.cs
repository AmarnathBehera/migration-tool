using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace BDO.GWT.Core.Utilities
{
    internal static partial class SitemapRegex
    {
        [GeneratedRegex(@"-[a-z0-9]{8}$")]
        internal static partial Regex SuffixRegex();
    }
}
