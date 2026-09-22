using System.Linq;
using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.LiveSite.Data;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.Data;
using BDO.GWT.Web.Core.LiveSite.Data;
using CMS.ContentEngine;
using CMS.Websites;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Core.Services;

public interface IRelatedContentCarouselService
{
    Task<RelatedContentCarouselDto> GetRelatedContentCarouselDtoAsync(Guid currentPageGuid, string pageTypeName, RelatedContentCarouselData data, CancellationToken cancellationToken);

    Task<ICollection<RelatedContentCardDto>> GetFilteredItemsAsync(Guid currentPageGuid, string pageTypeName, HashSet<ContentItemReference> featuredGuids, RelatedContentCardsQuery query, CancellationToken cancellationToken);

    Task<ICollection<RelatedContentCardDto>> GetAPIFilteredItemsAsync(RelatedContentCardsQuery query, CancellationToken cancellationToken);
}

public class RelatedContentCarouselService : IRelatedContentCarouselService
{
    private const int DefaultPageSizeUnfiltered = 10;
    private const int DefaultPageSizeFiltered = 20;
    private readonly IFilteringHelper _filteringHelper;
    private readonly IRelatedContentCardService _relatedContentCardService;
    private readonly IContentService _contentService;

    public RelatedContentCarouselService(IFilteringHelper filteringHelper, IRelatedContentCardService relatedContentCardService, IContentService contentService)
    {
        _filteringHelper = filteringHelper;
        _relatedContentCardService = relatedContentCardService;
        _contentService = contentService;
    }

    public async Task<RelatedContentCarouselDto> GetRelatedContentCarouselDtoAsync(Guid currentPageGuid, string pageTypeName, RelatedContentCarouselData data, CancellationToken cancellationToken)
    {
        var filters = data.FilteringEnabled
          ? await _relatedContentCardService.CreateFiltersAsync(
              data.Filters ?? new HashSet<string>(),
              null,
              cancellationToken)
          : null;

        var featuredSet = data.FeaturedPageNodeGuids?.ToHashSet() ?? new HashSet<ContentItemReference>();

        var dto = new RelatedContentCarouselDto
        {
            Title = data.Title,
            Description = data.Description,
            Filters = data.FilteringEnabled ? filters : null,
            ApiEndpoint = "relatedContentCarousel/items",
            SelectedPageTypes = data.SelectedPageTypes,
            FilteringTooltipEnabled = await _filteringHelper.IsFilteringTooltipEnabledAsync(data.FilteringEnabled, cancellationToken),
            Items = await GetFilteredItemsAsync(currentPageGuid, pageTypeName, featuredSet, new RelatedContentCardsQuery() { SelectedPageTypes=data.SelectedPageTypes} , cancellationToken),
            BackgroundImageURL = data.BackgroundImageUrl,
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
            CurrentPageGuid = currentPageGuid,
            PageType = pageTypeName

        };

        return dto;
    }

    public async Task<ICollection<RelatedContentCardDto>> GetFilteredItemsAsync(Guid currentPageGuid, string pageTypeName, HashSet<ContentItemReference> featuredGuids, RelatedContentCardsQuery query, CancellationToken cancellationToken)
    {
        return await BuildAndExecuteQueryAsync(
            currentPageGuid.ToString(),
            pageTypeName,
            featuredGuids,
            query,
            cancellationToken);
    }


    public async Task<ICollection<RelatedContentCardDto>> GetAPIFilteredItemsAsync(RelatedContentCardsQuery query, CancellationToken cancellationToken)
    {
        // Safe null handling for CurrentPageGuid
        return await BuildAndExecuteQueryAsync(
            query.CurrentPageGuid?.ToString() ?? string.Empty,
            query.PageType ?? string.Empty,
            new HashSet<ContentItemReference>(),
            query,
            cancellationToken);
    }

    private async Task<ICollection<RelatedContentCardDto>> BuildAndExecuteQueryAsync(
        string guid,
        string pageTypeName,
        HashSet<ContentItemReference> featuredGuids,
        RelatedContentCardsQuery query,
        CancellationToken cancellationToken)
    {
        var whereExpressions = new List<Action<WhereParameters>>();

        // Map filter parameter names to the correct Insight fields
        Action<WhereParameters>? filter = pageTypeName switch
        {
            FilterParameterName.ContentType => x => x.WhereContains(nameof(Insight.ContentType), guid),
            FilterParameterName.BusinessLine => x => x.WhereContains(nameof(Insight.MetadataBusinessLine), guid),
            FilterParameterName.ServiceArea => x => x.WhereContains(nameof(Insight.MetadataServiceAreas), guid),
            FilterParameterName.IndustryCategory => x => x.WhereContains(nameof(Insight.MetadataIndustryCategories), guid),


            _ => null,
        };

        var allowedPageTypes = new HashSet<string>
        {
            FilterParameterName.ContentType,
            FilterParameterName.BusinessLine,
            FilterParameterName.ServiceArea,
            FilterParameterName.IndustryCategory,

        };

        bool isOneOfAllowedTypes = allowedPageTypes.Contains(pageTypeName);

        // If the pageTypeName signals a known filter but we couldn't build a filter expression, return an empty result.
        if (filter is null && isOneOfAllowedTypes)
            return Array.Empty<RelatedContentCardDto>();

        if (filter is not null)
            whereExpressions.Add(filter);

        query.CurrentPage ??= 1;
        query.PageSize ??= filter is null ? DefaultPageSizeUnfiltered : DefaultPageSizeFiltered;

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


        var paginationDto = await _relatedContentCardService.GetFilteredItemsAsync(
            query,
            "/",
            _contentService.GetCurrentLanguage(),
            decorateQuery,
            item => featuredGuids.Any(r => r.Identifier == item.SystemFields.ContentItemGUID),
            cancellationToken);

        return paginationDto?.Data ?? Array.Empty<RelatedContentCardDto>();
    }
}
