using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.LiveSite.Data;
using BDO.GWT.Core.Services;
using CMS.ContentEngine;
using CMS.Websites;

namespace BDO.GWT.Core.LiveSite.Services;

public interface INewsCarouselService
{
    Task<NewsCarouselDto> GetNewsCarouselAsync<T>(T currentPage, NewsCarouselData data, CancellationToken cancellationToken)
        where T : IWebPageFieldsSource;

    Task<ICollection<NewsPageCardDto>> GetFilteredItemsAsync<T>(T currentPage, HashSet<Guid> featuredGuids, CardsQuery query, CancellationToken cancellationToken)
        where T : IWebPageFieldsSource;

    Task<ICollection<NewsPageCardDto>> GetAPIFilteredItemsAsync(CardsQuery query, CancellationToken cancellationToken);
}

public class NewsCarouselService : INewsCarouselService
{
    private readonly IFilteringHelper _filteringHelper;
    private readonly INewsArticleCardsService _newsArticleCardsService;
    private readonly IContentService _contentService;

    public NewsCarouselService(IFilteringHelper filteringHelper, INewsArticleCardsService newsArticleCardsService, IContentService contentService)
    {
        _filteringHelper = filteringHelper;
        _newsArticleCardsService = newsArticleCardsService;
        _contentService = contentService;
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

    public async Task<ICollection<NewsPageCardDto>> GetFilteredItemsAsync<T>(T currentPage, HashSet<Guid> featuredGuids, CardsQuery query, CancellationToken cancellationToken)
        where T : IWebPageFieldsSource
    {
        return await BuildAndExecuteQueryAsync(
            currentPage.SystemFields.ContentItemGUID.ToString(),
            currentPage.GetType().Name,
            featuredGuids,
            query,
            cancellationToken);
    }

    public async Task<ICollection<NewsPageCardDto>> GetAPIFilteredItemsAsync( CardsQuery query, CancellationToken cancellationToken)
    {
        return await BuildAndExecuteQueryAsync(
            query?.CurrentPageGuid?.ToString(),
            query?.PageType,
            new HashSet<Guid>(),
            query,
            cancellationToken);
    }
    private async Task<ICollection<NewsPageCardDto>> BuildAndExecuteQueryAsync(
        string guid,
        string pageTypeName,
        HashSet<Guid> featuredGuids,
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
            FilterParameterName.IndustryPage => x => x.WhereContains(nameof(NewsArticle.MetadataIndustries), guid),
            FilterParameterName.MicrositeMasterPage => x => x.WhereContains(nameof(NewsArticle.MicrositeMasterPages), guid),
            FilterParameterName.MicrositePage => x => x.WhereContains(nameof(NewsArticle.MicrositePages), guid),
            _ => null,
        };

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
            newsArticle => featuredGuids.Contains(newsArticle.SystemFields.WebPageItemGUID),
            cancellationToken);

        return paginationDto.Data;
    }
}
