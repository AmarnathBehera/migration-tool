using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.LiveSite.Data;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.Data;
using CMS.ContentEngine;
using CMS.Websites;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Core.LiveSite.Services;

public interface IInsightCarouselService
{
    Task<InsightCarouselDto> GetInsightCarouselAsync<T>(T currentPage, InsightCarouselData data, CancellationToken cancellationToken)
        where T : IWebPageFieldsSource;

    Task<ICollection<ContentPageCardDto>> GetFilteredItemsAsync<T>(T currentPage, HashSet<ContentItemReference> featuredGuids, CardsQuery query, CancellationToken cancellationToken)
        where T : IWebPageFieldsSource;

    Task<ICollection<ContentPageCardDto>> GetAPIFilteredItemsAsync(CardsQuery query, CancellationToken cancellationToken);
}

public class InsightCarouselService : IInsightCarouselService
{
    private readonly IFilteringHelper _filteringHelper;
    private readonly IInsightCardService _insightCardService;
    private readonly IContentService _contentService;

    public InsightCarouselService(IFilteringHelper filteringHelper, IInsightCardService insightCardService, IContentService contentService)
    {
        _filteringHelper = filteringHelper;
        _insightCardService = insightCardService;
        _contentService = contentService;
    }

    public async Task<InsightCarouselDto> GetInsightCarouselAsync<T>(T currentPage, InsightCarouselData data, CancellationToken cancellationToken)
        where T : IWebPageFieldsSource
    {
        var dto = new InsightCarouselDto
        {
            Title = data.Title,
            Description = data.Description,
            Filters = data.FilteringEnabled ? await _insightCardService.CreateFiltersAsync(data.Filters, null, cancellationToken) : null,
            FilteringTooltipEnabled = await _filteringHelper.IsFilteringTooltipEnabledAsync(data.FilteringEnabled, cancellationToken),
            Items = await GetFilteredItemsAsync(currentPage, data.FeaturedInsightNodeGuids.ToHashSet(), new CardsQuery(), cancellationToken),
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

    public async Task<ICollection<ContentPageCardDto>> GetFilteredItemsAsync<T>(T currentPage, HashSet<ContentItemReference> featuredGuids, CardsQuery query, CancellationToken cancellationToken)
        where T : IWebPageFieldsSource
    {
        return await BuildAndExecuteQueryAsync(
            currentPage.SystemFields.ContentItemGUID.ToString(),
            currentPage.GetType().Name,
            featuredGuids,
            query,
            cancellationToken);
    }


    public async Task<ICollection<ContentPageCardDto>> GetAPIFilteredItemsAsync(CardsQuery query, CancellationToken cancellationToken)
    {
        return await BuildAndExecuteQueryAsync(
            query.CurrentPageGuid.ToString() ?? string.Empty,
            query.PageType ?? string.Empty,
            new HashSet<ContentItemReference>(),
            query,
            cancellationToken);
    }
    private async Task<ICollection<ContentPageCardDto>> BuildAndExecuteQueryAsync(
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
            FilterParameterName.ContentType,
            FilterParameterName.BusinessLine,
            FilterParameterName.ServiceArea,
            FilterParameterName.Service,
            FilterParameterName.IndustryCategory,
            FilterParameterName.SpecialtyCategory,
            FilterParameterName.MicrositeMasterPage,
            FilterParameterName.OfficeLocation
        };

        bool isOneOfTheseSeven = allowedPageTypes.Contains(pageTypeName);

        if (filter is null && isOneOfTheseSeven)
            return null!;

        if (filter is not null)
            whereExpressions.Add(filter);

        query.CurrentPage ??= 1;
        query.PageSize ??= filter is null ? 10 : 20;

        var insight = (await _contentService.RetrievePagesAsync<Insight>(cancellationToken))?
            .OfType<Insight>()
            .ToList();

        Action<RetrievePagesQueryParameters>? decorateQuery = null;
        if (whereExpressions.Any())
        {
            decorateQuery = rp =>
            {
                rp.Where(where =>
                {
                    foreach (var expr in whereExpressions)
                    {
                        expr(where);
                    }
                });
            };
        }

        var parentPath = _contentService.GetCurrentLanguage();

        var paginationDto = await _insightCardService.GetFilteredItemsAsync(
            query,
            parentPath,
            _contentService.GetCurrentLanguage(),
            decorateQuery,
            item => featuredGuids.Any(r => r.Identifier == item.SystemFields.ContentItemGUID),
            cancellationToken);

        return paginationDto.Data;
    }
}
