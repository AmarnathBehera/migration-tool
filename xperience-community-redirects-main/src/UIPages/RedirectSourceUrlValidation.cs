using System.Linq;

using BDO;
using CMS.DataEngine;

namespace XperienceCommunity.Redirects.UIPages;

internal static class RedirectSourceUrlValidation
{
    internal static bool SourceUrlExists(IInfoProvider<RedirectInfo> redirectInfoProvider, string? sourceUrl, int? excludedRedirectId = null)
    {
        if (string.IsNullOrWhiteSpace(sourceUrl))
        {
            return false;
        }

        try
        {
            var query = redirectInfoProvider.Get()
                .WhereEquals(nameof(RedirectInfo.RedirectSourceUrl), sourceUrl.ToLowerInvariant());

            if (excludedRedirectId.HasValue)
            {
                query = query.WhereNotEquals(nameof(RedirectInfo.RedirectID), excludedRedirectId.Value);
            }

            return query.TopN(1).Any();
        }
        catch
        {
            return false;
        }
    }
}