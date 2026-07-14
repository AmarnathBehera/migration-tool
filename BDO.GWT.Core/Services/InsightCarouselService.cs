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
        var filters = data.FilteringEnabled
          ? await _insightCardService.CreateFiltersAsync(
              data.Filters ?? new HashSet<string>(),
              null,
              cancellationToken)
          : null;

        var featuredSet = data.FeaturedInsightNodeGuids?.ToHashSet() ?? new HashSet<ContentItemReference>();

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
        // Safe null handling for CurrentPageGuid
        return await BuildAndExecuteQueryAsync(
            query.CurrentPageGuid?.ToString() ?? string.Empty,
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
            return Array.Empty<ContentPageCardDto>();

        if (filter is not null)
            whereExpressions.Add(filter);

        query.CurrentPage ??= 1;
        query.PageSize ??= filter is null ? 10 : 20;

        // Removed unused retrieval of all insights (was unused).
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
            _contentService.GetCurrentLanguage(),
            decorateQuery,
            item => featuredGuids.Any(r => r.Identifier == item.SystemFields.ContentItemGUID),
            cancellationToken);

        return paginationDto?.Data ?? Array.Empty<ContentPageCardDto>();
    }
}
