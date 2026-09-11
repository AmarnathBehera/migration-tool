using BDO.GWT.Core.DTOs.Pages;
using CMS.Websites;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Core.Services;
public interface INewsGroupPageService
{
    Task<NewsGroupDto> GetNewsGroupDtoAsync(NewsGroup currentPage, CancellationToken cancellationToken);
}
public class NewsGroupPageService : INewsGroupPageService
{
    private readonly IContentService _contentService;
    private readonly IWebPageUrlResolver _webPageUrlResolver;

    public NewsGroupPageService(IContentService contentService, IWebPageUrlResolver webPageUrlResolver)
    {
        _contentService = contentService;
        _webPageUrlResolver = webPageUrlResolver;
    }
    public async Task<NewsGroupDto> GetNewsGroupDtoAsync(NewsGroup currentPage, CancellationToken cancellationToken)
    {
        var dto = new NewsGroupDto
        {
            Title = currentPage.NewsGroupTitle,
            Description = currentPage.Description,
            Links = await GetNewsGroupItemDto(currentPage, cancellationToken),
        };

        return dto;
    }
    private async Task<NewsGroupItemDto[]> GetNewsGroupItemDto(NewsGroup currentPage, CancellationToken cancellationToken)
    {
        var newsArticles = (await _contentService.GetPagesAsync<NewsArticle>(
           new RetrievePagesParameters { },
           query => query
               .OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder))
               .Where(w => w.WhereEquals("WebPageItemParentID", currentPage.SystemFields.WebPageItemID)),
           RetrievalCacheSettings.CacheDisabled,
           cancellationToken
       )).ToArray();
     
       var orderedNewsArticles = newsArticles.OrderByDescending(x => x.SystemFields.ContentItemCommonDataLastPublishedWhen);

        var result = orderedNewsArticles.Select(newsArticle => new NewsGroupItemDto
        {
            Title = newsArticle.MetadataTitle,
            URL = _webPageUrlResolver.GetRelativePath(newsArticle),
            PublishFrom = newsArticle.SystemFields.ContentItemCommonDataLastPublishedWhen,
        }).ToArray();

        return result;
    }
}
