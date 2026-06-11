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
    Task<NewsCarouselDto> GetNewsCarouselAsync<T>(T currentPage,NewsCarouselData data, CancellationToken cancellationToken)where T : IWebPageFieldsSource;

    Task<ICollection<NewsPageCardDto>> GetFilteredItemsAsync<T>(T currentPage, HashSet<Guid> featuredNodeGuids, CancellationToken cancellationToken) where T : IWebPageFieldsSource;
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

    public async Task<NewsCarouselDto> GetNewsCarouselAsync<T>(T currentPage, NewsCarouselData data, CancellationToken cancellationToken) where T : IWebPageFieldsSource
    {
        var dto = new NewsCarouselDto
        {
            Title = data.Title,
            Description = data.Description,
            Filters = data.FilteringEnabled ? await _newsArticleCardsService.CreateFiltersAsync(data.Filters, null, cancellationToken) : null,
            FilteringTooltipEnabled = await _filteringHelper.IsFilteringTooltipEnabledAsync(data.FilteringEnabled, cancellationToken),
            Items = await GetFilteredItemsAsync<T>(currentPage, data.NodeGuidsOfFeaturedNews.ToHashSet(), cancellationToken),
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
        };

        return dto;
    }

    public async Task<ICollection<NewsPageCardDto>> GetFilteredItemsAsync<T>(T currentPage, HashSet<Guid> featuredNodeGuids, CancellationToken cancellationToken) where T : IWebPageFieldsSource
    {
        var whereExpressions = new List<Action<WhereParameters>>();
        var guid = currentPage.SystemFields.ContentItemGUID.ToString();

        var name = currentPage.GetType().Name;
        Action<WhereParameters>? filter = name switch
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
        {
            whereExpressions.Add(filter);
        }

        CardsQuery cardsQuery = new CardsQuery();
        cardsQuery.CurrentPage ??= 1;
        cardsQuery.PageSize ??= filter is null ? 10 : 20;

        var sectionNews = (await _contentService.RetrievePagesAsync<SectionNews>(cancellationToken))?
            .OfType<SectionNews>()
            .ToList();

        var paginationDto = await _newsArticleCardsService.GetFilteredItemsAsync(
            sectionNews ?? new List<SectionNews>(),
            _contentService.GetCurrentLanguage(),
            cardsQuery,
            whereExpressions,
            newsArticle => featuredNodeGuids.Contains(newsArticle.SystemFields.WebPageItemGUID),
            cancellationToken);

        return paginationDto.Data;
    }
}
