

namespace BDO.GWT.Core.Services;

using BDO.GWT.Core.DTOs.Pages;
using CMS.Websites;
using CMS.Websites.Routing;
using global::BDO.GWT.Core.DTOs;
using global::BDO.GWT.Core.Utilities;
using Kentico.Content.Web.Mvc;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;



public interface IRssSectionPageService
{
    Task<RssSectionDto> GetRssSectionDtoAsync(CancellationToken cancellationToken);
}

public class RssSectionPageService : IRssSectionPageService
{
    private readonly IContentService _contentService;
    private readonly ICultureContext _cultureContext;
    private readonly IWebsiteChannelContext _websiteChannelContext;
    private readonly IWebPageUrlRetriever _webPageUrlRetriever;

    public RssSectionPageService( IContentService contentService, ICultureContext cultureContext, IWebsiteChannelContext websiteChannelContext, IWebPageUrlRetriever webPageUrlRetriever)
    {
        _contentService = contentService;
        _cultureContext = cultureContext;
        _websiteChannelContext = websiteChannelContext;
        _webPageUrlRetriever = webPageUrlRetriever;

    }

    public async Task<RssSectionDto> GetRssSectionDtoAsync( CancellationToken cancellationToken)
    {
        var currentPage = await _contentService.RetrieveCurrentPageAsync<SectionRss>(
                new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 4 }, cancellationToken);
        

        if(currentPage == null)  {
            return new RssSectionDto();
        }



        var currentTreePath = currentPage.SystemFields.WebPageItemTreePath;

        var rssFeeds = (await _contentService.GetPagesAsync<Rss>(
            new RetrievePagesParameters
            {
                ChannelName = _websiteChannelContext.WebsiteChannelName,
                LanguageName = _cultureContext.CurrentLanguageTag,
                LinkedItemsMaxLevel = 0,
            },
            q => q.Where(where => where
                // Equivalent to q.Where(x => x.RssPublic)
                .WhereTrue(nameof(Rss.RssPublic))
                // Equivalent to q.Path(NodeAliasPath, PathTypeEnum.Children) — direct children only
                .WhereLike(nameof(IWebPageContentQueryDataContainer.WebPageItemTreePath), $"{currentTreePath}/%")
                .WhereNotLike(nameof(IWebPageContentQueryDataContainer.WebPageItemTreePath), $"{currentTreePath}/%/%")),
             RetrievalCacheSettings.CacheDisabled,
            cancellationToken)).ToArray();

        var relativeUrlByWebPageItemId = new Dictionary<int, string>();
        foreach (var rssFeed in rssFeeds)
        {
            try
            {
                var url = await _webPageUrlRetriever.Retrieve(
                    rssFeed.SystemFields.WebPageItemID,
                    _cultureContext.CurrentLanguageTag,
                    cancellationToken: cancellationToken);

                if (!string.IsNullOrEmpty(url?.RelativePath))
                    relativeUrlByWebPageItemId[rssFeed.SystemFields.WebPageItemID] = url.RelativePath;
            }
            catch (Exception ex) { }
        }
        var dto = new RssSectionDto
        {
            Title = currentPage.MetadataTitle,
            Description = HtmlUtility.RemoveTildeFromUrlTypeHtmlAttributes(currentPage.MetadataDescription),
            Links = rssFeeds
        .Where(rssFeed => relativeUrlByWebPageItemId.ContainsKey(rssFeed.SystemFields.WebPageItemID))
        .Select(rssFeed => new LinkItemDto
        {
            Title = rssFeed.RssFeedTitle,
            URL = relativeUrlByWebPageItemId[rssFeed.SystemFields.WebPageItemID],
            Target = "_self",
        })
        .ToArray(),
        };

        return dto;
    }
}
