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
}

public class EditorsPickService : IEditorsPickService
{
    private readonly IFilteringHelper _filteringHelper;
    private readonly IEditorsPickCardService _editorsPickCardService;
    private readonly IEditorsPickStateStore _stateStore;

    public EditorsPickService(
        IFilteringHelper filteringHelper,
        IEditorsPickCardService editorsPickCardService,
        IEditorsPickStateStore stateStore)
    {
        _filteringHelper = filteringHelper;
        _editorsPickCardService = editorsPickCardService;
        _stateStore = stateStore;
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

        _stateStore.Save(stateGuid, featuredGuids, selectedGuids);

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
            StateGUID = stateGuid,
            FeaturedGuids = string.Join(",", featuredGuids.Select(x => x.Identifier)),
            SelectedGuids = string.Join(",", selectedGuids.Select(x => x.Identifier)),
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
}