using BDO.GWT.Common.Exceptions;
using BDO.GWT.Core;
using CMS.ContentEngine;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Web.Core.Services;

internal static class SectionInsightsPageRetriever
{
    public static async Task<SectionInsightsPage?> GetSectionInsightsPageAsync(
        IContentRetriever contentRetriever,
        IContentService contentService,
        IWebsiteChannelContext websiteChannelContext,
        CancellationToken cancellationToken)
    {
        int channelId = await contentService.GetCurrentChannelIDChannelInfo(cancellationToken);
        var insightSection = await contentRetriever.RetrievePages<SectionInsightsPage, SectionInsightsPage>(
            new RetrievePagesParameters
            {
                LinkedItemsMaxLevel = 5
            },
            query =>
            {
                query.Where(w =>
                    w.WhereEquals("ContentItemChannelID", channelId));
            },
            RetrievalCacheSettings.CacheDisabled,
            (_, item) => Task.FromResult(item),
            cancellationToken);

        return insightSection.FirstOrDefault();
    }

    public static async Task<SectionInsightsPage> GetRequiredSectionInsightsPageAsync(
        IContentRetriever contentRetriever,
        IWebsiteChannelContext websiteChannelContext, IContentService contentService,
        CancellationToken cancellationToken)
    {
        var page = await GetSectionInsightsPageAsync(contentRetriever, contentService, websiteChannelContext, cancellationToken);

        if (page is null)
        {
            throw new MissingSectionPageException($"No pages have been found of type '{nameof(SectionInsightsPage)}'.");
        }

        return page;
    }
}
