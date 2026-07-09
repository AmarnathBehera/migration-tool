using BDO.GWT.Core.Data;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Web.Services;
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
        CardsQuery query,
        CancellationToken cancellationToken);
}

public class EditorsPickService : IEditorsPickService
{
    private readonly IFilteringHelper _filteringHelper;
    private readonly IEditorsPickCardService _editorsPickCardService;
    private readonly IReactCachingService _reactCachingService;

    public EditorsPickService(
        IFilteringHelper filteringHelper,
        IEditorsPickCardService editorsPickCardService,
        IReactCachingService reactCachingService)
    {
        _filteringHelper = filteringHelper;
        _editorsPickCardService = editorsPickCardService;
        _reactCachingService = reactCachingService;
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

        var stateGuid = Guid.NewGuid();
        var state = new EditorsPickWidgetState(featuredGuids, selectedGuids);
        _reactCachingService.StoreObject(stateGuid.ToString(), state);

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
            StateGUID = stateGuid
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

        selectedPageGuids.UnionWith(featuredPageGuids);
        var paginationDto = await _editorsPickCardService.GetFilteredItemsAsync(
            query,
            selectedPageGuids,
            page => featuredPageGuids.Contains(page.SystemFields.ContentItemGUID),
            cancellationToken);

        return paginationDto.Data;
    }

    public async Task<ICollection<EditorsPickCardDto>> GetAPIFilteredItemsAsync(
        CardsQuery query,
        CancellationToken cancellationToken)
    {
        query ??= new CardsQuery();

        var featuredGuids = new HashSet<ContentItemReference>();
        var selectedGuids = new HashSet<ContentItemReference>();

        if (query.StateGUID.HasValue &&
            _reactCachingService.TryGetCachedObject<EditorsPickWidgetState>(query.StateGUID.Value.ToString(), out var state) &&
            state is not null)
        {
            featuredGuids = state.FeaturedGuids;
            selectedGuids = state.SelectedGuids;
        }

        return await GetFilteredItemsAsync(featuredGuids, selectedGuids, query, cancellationToken);
    }
}