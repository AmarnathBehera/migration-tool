using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Web.Core.Services;
using CMS.ContentEngine;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Core.Services;

public interface IEventsWebinarsSliderService
{
    Task<EventsWebinarsSliderDto> GetEventsWebinarsSliderAsync<T>(
        T currentPage,
        EventsWebinarsSliderData data,
        CancellationToken cancellationToken)
        where T : IWebPageFieldsSource;

    Task<ICollection<EventsWebinarsCardDto>> GetFilteredItemsAsync<T>(
        T currentPage,
        HashSet<ContentItemReference> featuredGuids,
        CardsQuery query,
        CancellationToken cancellationToken)
        where T : IWebPageFieldsSource;

    Task<ICollection<EventsWebinarsCardDto>> GetAPIFilteredItemsAsync(
        CardsQuery query,
        CancellationToken cancellationToken);
}

public class EventsWebinarsSliderService : IEventsWebinarsSliderService
{
    private readonly IFilteringHelper _filteringHelper;
    private readonly IEventWebinarCardsService _eventWebinarCardsService;
    private readonly IWebsiteChannelContext _websiteChannelContext;
    private readonly IContentQueryExecutor _contentQueryExecutor;

    public EventsWebinarsSliderService(
        IEventWebinarCardsService eventWebinarCardsService,
        IFilteringHelper filteringHelper,
        IWebsiteChannelContext websiteChannelContext,
        IContentQueryExecutor contentQueryExecutor)
    {
        _eventWebinarCardsService = eventWebinarCardsService;
        _filteringHelper = filteringHelper;
        _websiteChannelContext = websiteChannelContext;
        _contentQueryExecutor = contentQueryExecutor;
    }

    public async Task<EventsWebinarsSliderDto> GetEventsWebinarsSliderAsync<T>(
        T currentPage,
        EventsWebinarsSliderData data,
        CancellationToken cancellationToken)
        where T : IWebPageFieldsSource
    {
        ArgumentNullException.ThrowIfNull(currentPage);
        ArgumentNullException.ThrowIfNull(data);

        var featuredGuids = (data.FeaturedEventAndWebinarNodeGuids
            ?? Enumerable.Empty<ContentItemReference>())
            .ToHashSet();

        var filters = data.FilteringEnabled
            ? await _eventWebinarCardsService.CreateFiltersAsync(
                data.Filters ?? new HashSet<string>(),
                null,
                cancellationToken)
            : null;

        var items = await GetFilteredItemsAsync(
            currentPage,
            featuredGuids,
            new CardsQuery(),
            cancellationToken);

        return new EventsWebinarsSliderDto
        {
            Title = data.Title,
            Description = data.Description,
            Filters = filters,
            FilteringTooltipEnabled = await _filteringHelper.IsFilteringTooltipEnabledAsync(
                data.FilteringEnabled,
                cancellationToken),
            Items = items,
            BackgroundImageURL = data.BackgroundImages,
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
            PageType = currentPage.GetType().Name
        };
    }

    public async Task<ICollection<EventsWebinarsCardDto>> GetFilteredItemsAsync<T>(
        T currentPage,
        HashSet<ContentItemReference> featuredGuids,
        CardsQuery query,
        CancellationToken cancellationToken)
        where T : IWebPageFieldsSource
    {
        ArgumentNullException.ThrowIfNull(currentPage);

        query ??= new CardsQuery();
        featuredGuids ??= new HashSet<ContentItemReference>();

        return await BuildAndExecuteQueryAsync(
            currentPage.SystemFields.ContentItemGUID,
            currentPage.GetType().Name,
            featuredGuids,
            query,
            cancellationToken);
    }

    public async Task<ICollection<EventsWebinarsCardDto>> GetAPIFilteredItemsAsync(
        CardsQuery query,
        CancellationToken cancellationToken)
    {
        query ??= new CardsQuery();

        return await BuildAndExecuteQueryAsync(
            query.CurrentPageGuid ?? Guid.Empty,
            query.PageType,
            new HashSet<ContentItemReference>(),
            query,
            cancellationToken);
    }

    private async Task<ICollection<EventsWebinarsCardDto>> BuildAndExecuteQueryAsync(
        Guid pageGuid,
        string? pageTypeName,
        HashSet<ContentItemReference> featuredGuids,
        CardsQuery query,
        CancellationToken cancellationToken)
    {
        var guidStr = pageGuid.ToString();

        Action<ContentItemQueryBuilder>? decorateQuery = pageTypeName switch
        {
            FilterParameterName.BusinessLine => b => b.Parameters(p =>
                p.Where(w => w.WhereContains(nameof(IEventOrWebinar.MetadataBusinessLines), guidStr))),

            FilterParameterName.BusinessIssue => b => b.Parameters(p =>
                p.Where(w => w.WhereContains(nameof(IEventOrWebinar.MetadataBusinessIssues), guidStr))),

            FilterParameterName.EventType => b => b.Parameters(p =>
                p.Where(w => w.WhereContains(nameof(IEventOrWebinar.EventType), guidStr))),

            FilterParameterName.ServiceArea => b => b.Parameters(p =>
                p.Where(w => w.WhereContains(nameof(IEventOrWebinar.MetadataServiceAreas), guidStr))),

            FilterParameterName.Service => b => b.Parameters(p =>
                p.Where(w => w.WhereContains(nameof(IEventOrWebinar.MetadataServices), guidStr))),

            FilterParameterName.IndustryCategory => b => b.Parameters(p =>
                p.Where(w => w.WhereContains(nameof(IEventOrWebinar.MetadataIndustryCategories), guidStr))),

            FilterParameterName.IndustryPage => b => b.Parameters(p =>
                p.Where(w => w.WhereContains(nameof(IEventOrWebinar.MetadataIndustries), guidStr))),

            FilterParameterName.MicrositeMasterPage => b => b.Parameters(p =>
                p.Where(w => w.WhereContains(nameof(IEventOrWebinar.MicrositeMasterPages), guidStr))),

            FilterParameterName.MicrositePage => b => b.Parameters(p =>
                p.Where(w => w.WhereContains(nameof(NewsArticle.MicrositePages), guidStr))),

            _ => null
        };

        query.CurrentPage ??= 1;
        query.PageSize ??= decorateQuery is null ? 10 : 20;

        var result = await RetrieveFeaturedPageIdsAsync(
            featuredGuids,
            cancellationToken);

        var featuredPageIds = result.FeaturedPageIds;

        var paginationDto = await _eventWebinarCardsService.GetFilteredItemsAsync(
            query,
            decorateQuery,
            item => featuredPageIds.Contains(item.SystemFields.WebPageItemID),
            showPastPages: false,
            filterOnEventToDate: true,
            cancellationToken);

        return paginationDto.Data;
    }

    private async Task<(HashSet<int> FeaturedPageIds, bool ShowPastEvents)> RetrieveFeaturedPageIdsAsync(
        HashSet<ContentItemReference> featuredGuids,
        CancellationToken cancellationToken)
    {
        if (featuredGuids.Count == 0)
        {
            return (new HashSet<int>(), false);
        }

        var guids = featuredGuids
            .Select(reference => reference.Identifier)
            .ToHashSet();

        var builder = new ContentItemQueryBuilder()
            .ForContentType(Event.CONTENT_TYPE_NAME, query => query
                .ForWebsite(_websiteChannelContext.WebsiteChannelName)
                .Where(where => where.WhereIn(nameof(WebPageFields.ContentItemGUID), guids)))
            .ForContentType(Webinar.CONTENT_TYPE_NAME, query => query
                .ForWebsite(_websiteChannelContext.WebsiteChannelName)
                .Where(where => where.WhereIn(nameof(WebPageFields.ContentItemGUID), guids)));

        var results = await _contentQueryExecutor.GetWebPageResult(
            builder,
            container => container.WebPageItemID,
            cancellationToken: cancellationToken);

        return (results.ToHashSet(), false);
    }
}