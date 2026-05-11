using System;

namespace BDO.GWT.Core.Extensions;

public static class ReadOnlySpanExtensions
{
    public static bool EndsWithAny(this ReadOnlySpan<char> readOnlySpan, StringComparison comparisonType, params string[] values)
    {
        foreach (var value in values)
        {
            if (readOnlySpan.EndsWith(value, comparisonType))
            {
                return true;
            }
        }

        return false;
    }
}