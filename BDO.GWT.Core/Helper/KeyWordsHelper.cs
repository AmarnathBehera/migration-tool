using System;
using System.Collections.Generic;
using System.Text;

namespace BDO.GWT.Core.Helper
{
    public static class KeywordsHelper
    {
        public static List<string> ParseKeywords(string keywords)
        {
            var result = new List<string>();
            var inQuotes = false;
            var currentTag = new StringBuilder();

            foreach (var ch in keywords)
            {
                if (!inQuotes && ch == ',')
                {
                    result.Add(currentTag.ToString().Trim());
                    currentTag.Clear();
                }
                else if (ch == '\"')
                {
                    inQuotes = !inQuotes;
                }
                else
                {
                    currentTag.Append(ch);
                }
            }

            if (currentTag.Length > 0)
            {
                result.Add(currentTag.ToString().Trim());
            }

            return result;
        }
    }
}
