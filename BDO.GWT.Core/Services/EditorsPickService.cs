using BDO.GWT.Core.Data;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Helpers;
using CMS.ContentEngine;
using CMS.Websites;

namespace BDO.GWT.Core.Services;

public interface IEditorsPickService
{
    Task<EditorsPickDto> GetEditorsPickAsync<T>(
        T currentPage,
        EditorsPickData data,
        CancellationToken cancellationToken)
        where T : IWebPageFieldsSource;

    Task<ICollection<EditorsPickCardDto>> GetFilteredItemsAsync(
        HashSet<ContentItemReference> featuredGuids,
        HashSet<ContentItemReference> selectedGuids,
        CardsQuery query,
        CancellationToken cancellationToken);

    Task<ICollection<EditorsPickCardDto>> GetAPIFilteredItemsAsync(
        EditorsPickCardsQuery query,
        CancellationToken cancellationToken);
}

public class EditorsPickService : IEditorsPickService
{
    private readonly IFilteringHelper _filteringHelper;
    private readonly IEditorsPickCardService _editorsPickCardService;

    public EditorsPickService(
        IFilteringHelper filteringHelper,
        IEditorsPickCardService editorsPickCardService)
    {
        _filteringHelper = filteringHelper;
        _editorsPickCardService = editorsPickCardService;
    }

    public async Task<EditorsPickDto> GetEditorsPickAsync<T>(
        T currentPage,
        EditorsPickData data,
        CancellationToken cancellationToken)
        where T : IWebPageFieldsSource
    {
        ArgumentNullException.ThrowIfNull(currentPage);
        ArgumentNullException.ThrowIfNull(data);

        var featuredGuids = (data.FeaturedPageGuids ?? Enumerable.Empty<ContentItemReference>()).ToHashSet();
        var selectedGuids = (data.EditorsPickGuids ?? Enumerable.Empty<ContentItemReference>()).ToHashSet();

        var filters = data.FilteringEnabled
            ? await _editorsPickCardService.CreateFiltersAsync(
                data.Filters ?? new HashSet<string>(),
                null,
                cancellationToken)
            : null;

        var items = await GetFilteredItemsAsync(
            featuredGuids,
            selectedGuids,
            new CardsQuery(),
            cancellationToken);

        return new EditorsPickDto
        {
            Title = data.Title,
            Description = data.Description,
            Filters = filters,
            FilteringTooltipEnabled = await _filteringHelper.IsFilteringTooltipEnabledAsync(
                data.FilteringEnabled,
                cancellationToken),
            Items = items,
            BackgroundImageURL = data.BackgroundImageUrl,
            BackgroundImageAltText = data.BackgroundImageAltText,
            SeeAllLink = data is { SeeAllLinkText: not null, SeeAllLinkUrl: not null }
                ? new LinkItemDto
                {
                    Title = data.SeeAllLinkText,
                    URL = data.SeeAllLinkUrl,
                    Target = data.SeeAllLinkURLTarget ?? "_self"
                }
                : null,
            TopMargin = data.TopMargin ?? string.Empty,
            BottomMargin = data.BottomMargin ?? string.Empty,
            WidgetID = data.WidgetID,
            CurrentPageGuid = currentPage.SystemFields.ContentItemGUID,
            PageType = currentPage.GetType().Name,

            SelectedGuids = selectedGuids
                .Select(x => x.Identifier)
                .ToList(),
            FeaturedGuids = featuredGuids
                .Select(x => x.Identifier)
                .ToList(),
        };
    }

    public async Task<ICollection<EditorsPickCardDto>> GetFilteredItemsAsync(
        HashSet<ContentItemReference> featuredGuids,
        HashSet<ContentItemReference> selectedGuids,
        CardsQuery query,
        CancellationToken cancellationToken)
    {
        query ??= new CardsQuery();
        featuredGuids ??= new HashSet<ContentItemReference>();
        selectedGuids ??= new HashSet<ContentItemReference>();

        var selectedPageGuids = selectedGuids
            .Select(x => x.Identifier)
            .ToHashSet();

        var featuredPageGuids = featuredGuids
            .Select(x => x.Identifier)
            .ToHashSet();

        // Include featured in the retrieval set
        selectedPageGuids.UnionWith(featuredPageGuids);

        var paginationDto = await _editorsPickCardService.GetFilteredItemsAsync(
            query,
            selectedPageGuids,
            page => featuredPageGuids.Contains(page.SystemFields.ContentItemGUID),
            cancellationToken);

        return paginationDto.Data;
    }

    public async Task<ICollection<EditorsPickCardDto>> GetAPIFilteredItemsAsync(
        EditorsPickCardsQuery query,
        CancellationToken cancellationToken)
    {
        var selectedGuids = query.SelectedGuids?.ToHashSet() ?? new HashSet<Guid>();
        var featuredGuids = query.FeaturedGuids?.ToHashSet() ?? new HashSet<Guid>();

        // Include featured in retrieval set — same as SSR path
        var allGuids = selectedGuids.ToHashSet();
        allGuids.UnionWith(featuredGuids);

        var cardsQuery = new CardsQuery
        {
            Filters = query.Filters,
            CurrentPage = query.CurrentPage ?? 1,
            PageSize = query.PageSize ?? 10
        };

        var paginationDto = await _editorsPickCardService.GetFilteredItemsAsync(
            cardsQuery,
            allGuids,
            page => featuredGuids.Contains(page.SystemFields.ContentItemGUID),
            cancellationToken);

        return paginationDto.Data;
    }
}