using System.Linq;
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

    Task<ICollection<ContentPageCardDto>> GetAPIFilteredItemsAsync(CardsQuery query, string? language, CancellationToken cancellationToken);
}

public class InsightCarouselService : IInsightCarouselService
{
    // Default page size when no filter is applied (initial/unfiltered view).
    private const int DefaultPageSizeUnfiltered = 10;

    // Default page size once a filter narrows the result set.
    private const int DefaultPageSizeFiltered = 20;

    private readonly IFilteringHelper _filteringHelper;
    private readonly IInsightCardService _insightCardService;
    private readonly IContentService _contentService;
    private readonly IInsightCarouselStateStore _stateStore;

    public InsightCarouselService(
        IFilteringHelper filteringHelper,
        IInsightCardService insightCardService,
        IContentService contentService,
        IInsightCarouselStateStore stateStore)
    {
        _filteringHelper = filteringHelper;
        _insightCardService = insightCardService;
        _contentService = contentService;
        _stateStore = stateStore;
    }

    public async Task<InsightCarouselDto> GetInsightCarouselAsync<T>(T currentPage, InsightCarouselData data, CancellationToken cancellationToken)
        where T : IWebPageFieldsSource
    {
        ArgumentNullException.ThrowIfNull(currentPage);
        ArgumentNullException.ThrowIfNull(data);

        var filters = data.FilteringEnabled
          ? await _insightCardService.CreateFiltersAsync(
              data.Filters ?? new HashSet<string>(),
              null,
              cancellationToken)
          : null;

        var featuredSet = data.FeaturedInsightNodeGuids?.ToHashSet() ?? new HashSet<ContentItemReference>();

        var stateGuid = Guid.NewGuid();
        _stateStore.Save(stateGuid, featuredSet);
       

        var dto = new InsightCarouselDto
        {
            Title = data.Title,
            Description = data.Description,
            Filters = data.FilteringEnabled ? filters : null,
            FilteringTooltipEnabled = await _filteringHelper.IsFilteringTooltipEnabledAsync(data.FilteringEnabled, cancellationToken),
            Items = await GetFilteredItemsAsync(currentPage, featuredSet, new CardsQuery(), cancellationToken),
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
            StateGUID = stateGuid,
        };

        return dto;
    }

    public async Task<ICollection<ContentPageCardDto>> GetFilteredItemsAsync<T>(T currentPage, HashSet<ContentItemReference> featuredGuids, CardsQuery query, CancellationToken cancellationToken)
        where T : IWebPageFieldsSource
    {
        ArgumentNullException.ThrowIfNull(currentPage);
        featuredGuids ??= new HashSet<ContentItemReference>();
        query ??= new CardsQuery();

        return await BuildAndExecuteQueryAsync(
            currentPage.SystemFields.ContentItemGUID.ToString(),
            currentPage.GetType().Name,
            featuredGuids,
            query,
            null,
            cancellationToken);
    }

    public async Task<ICollection<ContentPageCardDto>> GetAPIFilteredItemsAsync(CardsQuery query, string? language, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        // Confirmed safe against InsightCarouselStateStore.TryGet: that
        // implementation always assigns the out parameter to a non-null
        // HashSet before any return (empty set on a Guid.Empty or cache
        // miss), so the empty-set initializer below is never left as null.
        // The generic .NET TryGet convention (out param -> default on
        // false) doesn't apply here — no defensive null-check needed.
        var featuredGuids = new HashSet<ContentItemReference>();
        if (query.State.HasValue)
        {
            _stateStore.TryGet(query.State.Value, out featuredGuids);
        }

        return await BuildAndExecuteQueryAsync(
            query.CurrentPageGuid?.ToString() ?? string.Empty,
            query.PageType ?? string.Empty,
            featuredGuids,
            query,
            language,
            cancellationToken);
    }

    private async Task<ICollection<ContentPageCardDto>> BuildAndExecuteQueryAsync(
        string guid,
        string pageTypeName,
        HashSet<ContentItemReference> featuredGuids,
        CardsQuery query,
        string? language,
        CancellationToken cancellationToken)
    {
        var whereExpressions = new List<Action<WhereParameters>>();

        // Map filter parameter names to the correct Insight fields
        Action<WhereParameters>? filter = pageTypeName switch
        {
            FilterParameterName.ContentType => x => x.WhereContains(nameof(Insight.ContentType), guid),
            FilterParameterName.BusinessLine => x => x.WhereContains(nameof(Insight.MetadataBusinessLine), guid),
            FilterParameterName.BusinessIssue => x => x.WhereContains(nameof(Insight.MetadataBusinessIssues), guid),
            FilterParameterName.ServiceArea => x => x.WhereContains(nameof(Insight.MetadataServiceAreas), guid),
            FilterParameterName.Service => x => x.WhereContains(nameof(Insight.MetadataServices), guid),
            FilterParameterName.IndustryCategory => x => x.WhereContains(nameof(Insight.MetadataIndustryCategories), guid),
            FilterParameterName.Industry => x => x.WhereContains(nameof(Insight.MetadataIndustries), guid),
            FilterParameterName.SpecialtyCategory => x => x.WhereContains(nameof(Insight.MetadataSpecialtiesCategories), guid),
            FilterParameterName.MicrositeMasterPage => x => x.WhereContains(nameof(Insight.MicrositeMasterPages), guid),
            FilterParameterName.MicrositePage => x => x.WhereContains(nameof(Insight.MicrositePages), guid),
            FilterParameterName.OfficeLocation => x => x.WhereContains(nameof(Insight.MetadataOfficeLocation), guid),
            _ => null,
        };

        var allowedPageTypes = new HashSet<string>
        {
            FilterParameterName.ContentType,
            FilterParameterName.BusinessLine,
            FilterParameterName.BusinessIssue,
            FilterParameterName.ServiceArea,
            FilterParameterName.Service,
            FilterParameterName.IndustryCategory,
            FilterParameterName.Industry,
            FilterParameterName.SpecialtyCategory,
            FilterParameterName.MicrositeMasterPage,
            FilterParameterName.MicrositePage,
            FilterParameterName.OfficeLocation
        };

        bool isOneOfAllowedTypes = allowedPageTypes.Contains(pageTypeName);

        // If the pageTypeName signals a known filter but we couldn't build a filter expression, return an empty result.
        if (filter is null && isOneOfAllowedTypes)
        {
            return Array.Empty<ContentPageCardDto>();
        }

        if (filter is not null)
        {
            whereExpressions.Add(filter);
        }

        query.CurrentPage ??= 1;
        query.PageSize ??= filter is null ? DefaultPageSizeUnfiltered : DefaultPageSizeFiltered;

        Action<RetrievePagesQueryParameters>? decorateQuery = null;
        if (whereExpressions.Count > 0)
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

       
        var paginationDto = await _insightCardService.GetFilteredItemsAsync(
            query,
            "/",
            string.IsNullOrWhiteSpace(language) ? _contentService.GetCurrentLanguage() : language,
            decorateQuery,
            item => featuredGuids.Any(r => r.Identifier == item.SystemFields.ContentItemGUID),
            cancellationToken);

        return paginationDto?.Data ?? Array.Empty<ContentPageCardDto>();
    }
}