using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.LiveSite.Data;
using BDO.GWT.Core.Services;
using CMS.ContentEngine;
using CMS.Websites;
using CMS.Websites.Routing;

namespace BDO.GWT.Core.LiveSite.Services;

public interface INewsCarouselService
{
    Task<NewsCarouselDto> GetNewsCarouselAsync<T>(T currentPage, NewsCarouselData data, CancellationToken cancellationToken)
        where T : IWebPageFieldsSource;

    Task<ICollection<NewsPageCardDto>> GetFilteredItemsAsync<T>(T currentPage, HashSet<ContentItemReference> featuredGuids, CardsQuery query, CancellationToken cancellationToken)
        where T : IWebPageFieldsSource;

    Task<ICollection<NewsPageCardDto>> GetAPIFilteredItemsAsync(CardsQuery query, CancellationToken cancellationToken);
}

public class NewsCarouselService : INewsCarouselService
{
    private readonly IFilteringHelper _filteringHelper;
    private readonly INewsArticleCardsService _newsArticleCardsService;
    private readonly IContentService _contentService;
    private readonly IWebsiteChannelContext _websiteChannelContext;
    private readonly IContentQueryExecutor _contentQueryExecutor;

    public NewsCarouselService(IFilteringHelper filteringHelper, INewsArticleCardsService newsArticleCardsService, IContentService contentService, IWebsiteChannelContext websiteChannelContext, IContentQueryExecutor contentQueryExecutor)
    {
        _filteringHelper = filteringHelper;
        _newsArticleCardsService = newsArticleCardsService;
        _contentService = contentService;
        _websiteChannelContext = websiteChannelContext;
        _contentQueryExecutor = contentQueryExecutor;
    }

    public async Task<NewsCarouselDto> GetNewsCarouselAsync<T>(T currentPage, NewsCarouselData data, CancellationToken cancellationToken)
        where T : IWebPageFieldsSource
    {
        var dto = new NewsCarouselDto
        {
            Title = data.Title,
            Description = data.Description,
            Filters = data.FilteringEnabled ? await _newsArticleCardsService.CreateFiltersAsync(data.Filters, null, cancellationToken) : null,
            FilteringTooltipEnabled = await _filteringHelper.IsFilteringTooltipEnabledAsync(data.FilteringEnabled, cancellationToken),
            Items = await GetFilteredItemsAsync(currentPage, data.NodeGuidsOfFeaturedNews.ToHashSet(), new CardsQuery(), cancellationToken),
            BackgroundImageURL = data.BackgroundImageURL,
            BackgroundImageAltText = data.BackgroundImageAltText,
            SeeAllLink = data is { SeeAllLinkText: not null, SeeAllLinkUrl: not null }
                ? new LinkItemDto
                {
                    Title = data.SeeAllLinkText,
                    URL = data.SeeAllLinkUrl,
                    Target = data.SeeAllLinkURLTarget ?? "_self",
                }
                : null,
            TopMargin = data.TopMargin,
            BottomMargin = data.BottomMargin,
            WidgetID = data.WidgetID,
            CurrentPageGuid = currentPage.SystemFields.ContentItemGUID,
            PageType = currentPage.GetType().Name,
        };

        return dto;
    }

    public async Task<ICollection<NewsPageCardDto>> GetFilteredItemsAsync<T>(T currentPage, HashSet<ContentItemReference> featuredGuids, CardsQuery query, CancellationToken cancellationToken)
        where T : IWebPageFieldsSource
    {
        return await BuildAndExecuteQueryAsync(
            currentPage.SystemFields.ContentItemGUID.ToString(),
            currentPage.GetType().Name,
            featuredGuids,
            query,
            cancellationToken);
    }

    public async Task<ICollection<NewsPageCardDto>> GetAPIFilteredItemsAsync(CardsQuery query, CancellationToken cancellationToken)
    {
        return await BuildAndExecuteQueryAsync(
            query.CurrentPageGuid.ToString() ?? string.Empty,
            query.PageType ?? string.Empty,
            new HashSet<ContentItemReference>(),
            query,
            cancellationToken);
    }
    private async Task<ICollection<NewsPageCardDto>> BuildAndExecuteQueryAsync(
        string guid,
        string pageTypeName,
        HashSet<ContentItemReference> featuredGuids,
        CardsQuery query,
        CancellationToken cancellationToken)
    {
        var whereExpressions = new List<Action<WhereParameters>>();

        Action<WhereParameters>? filter = pageTypeName switch
        {
            FilterParameterName.BusinessLine => x => x.WhereContains(nameof(NewsArticle.MetadataBusinessLine), guid),
            FilterParameterName.ServiceArea => x => x.WhereContains(nameof(NewsArticle.MetadataServiceAreas), guid),
            FilterParameterName.Service => x => x.WhereContains(nameof(NewsArticle.MetadataServices), guid),
            FilterParameterName.IndustryCategory => x => x.WhereContains(nameof(NewsArticle.MetadataIndustryCategories), guid),
            FilterParameterName.Industry => x => x.WhereContains(nameof(NewsArticle.MetadataIndustries), guid),
            FilterParameterName.MicrositeMasterPage => x => x.WhereContains(nameof(NewsArticle.MicrositeMasterPages), guid),
            FilterParameterName.MicrositePage => x => x.WhereContains(nameof(NewsArticle.MicrositePages), guid),
            _ => null,
        };
        var allowedPageTypes = new HashSet<string>
        {
            FilterParameterName.BusinessLine,
            FilterParameterName.ServiceArea,
            FilterParameterName.Service,
            FilterParameterName.IndustryCategory,
            FilterParameterName.Industry,
            FilterParameterName.MicrositeMasterPage,
            FilterParameterName.MicrositePage
        };

        bool isOneOfTheseSeven = allowedPageTypes.Contains(pageTypeName);

        if (filter is null && isOneOfTheseSeven)
            return null!;

        if (filter is not null)
            whereExpressions.Add(filter);

        query.CurrentPage ??= 1;
        query.PageSize ??= filter is null ? 10 : 20;

        var sectionNews = (await _contentService.RetrievePagesAsync<SectionNews>(cancellationToken))?
            .OfType<SectionNews>()
            .ToList();

        var paginationDto = await _newsArticleCardsService.GetFilteredItemsAsync(
            sectionNews ?? new List<SectionNews>(),
            _contentService.GetCurrentLanguage(),
            query,
            whereExpressions,
            item => featuredGuids.Any(r => r.Identifier == item.SystemFields.ContentItemGUID),
            cancellationToken);

        return paginationDto.Data;
    }
    private async Task<(HashSet<int> FeaturedPageIds, bool ShowPastEvents)> RetrieveFeaturedPageIdsAsync(
  HashSet<ContentItemReference> featuredGuids,
  CancellationToken cancellationToken)
    {
        if (featuredGuids.Count == 0)
            return (new HashSet<int>(), false);

        var guids = featuredGuids.Select(r => r.Identifier).ToHashSet();

        var builder = new ContentItemQueryBuilder()
            .ForContentType(Event.CONTENT_TYPE_NAME, q => q
                .ForWebsite(_websiteChannelContext.WebsiteChannelName)
                .Where(w => w.WhereIn(nameof(WebPageFields.ContentItemGUID), guids)));

        var results = await _contentQueryExecutor.GetWebPageResult(
     builder,
     container => container.WebPageItemID,
     cancellationToken: cancellationToken);

        return (results.ToHashSet(), false);
    }
}
