//using BDO.GWT.Core.Constants;
//using BDO.GWT.Core.Data;
//using BDO.GWT.Core.Data.Queries;
//using BDO.GWT.Core.DTOs;
//using BDO.GWT.Core.Services;
//using BDO.GWT.Core.Utilities;
//using BDO.GWT.Domain.Core.Entities;
//using BDO.GWT.Domain.Core.Entities.BDOMetadata;
//using BDO.GWT.Web.Core.DTOs.Cards;
//using BDO.GWT.Web.Core.Helpers;
//using BDO.GWT.Xperience.Core.Services;
//using CMS.ContentEngine;
//using CMS.DataEngine;
//using CMS.Websites;
//using CMS.Websites.Routing;
//using Kentico.Content.Web.Mvc.Routing;

//namespace BDO.GWT.Web.Core.Services;

//public interface IEventWebinarCardsService
//{
//    Task<EventsWebinarsCardDto[]> GetEventWebinarCardDtosAsync(
//        ICollection<IEventOrWebinar> eventsAndWebinars,
//        Func<IEventOrWebinar, bool> isFeatured,
//        CancellationToken cancellationToken);

//    Task<PaginationDto<EventsWebinarsCardDto>> GetFilteredItemsAsync(
//        CardsQuery cardsQuery,
//        Action<ContentItemQueryBuilder>? decorateQuery,
//        Func<IEventOrWebinar, bool> isFeatured,
//        bool showPastPages,
//        bool filterOnEventToDate,
//        CancellationToken cancellationToken);
//}

//public class EventWebinarCardsService : IEventWebinarCardsService, IPageCardsService<IEventOrWebinar, EventsWebinarsCardDto>
//{
//    private readonly ICardsService _cardsService;
//    private readonly IContentQueryExecutor _contentQueryExecutor;
//    private readonly IWebPageQueryResultMapper _webPageQueryResultMapper;
//    private readonly IWebsiteChannelContext _websiteChannelContext;
//    private readonly IPreferredLanguageRetriever _preferredLanguageRetriever;
//    private readonly IWebPageUrlRetriever _webPageUrlRetriever;
//    private readonly ITimeZoneService _timeZoneService;

//    public EventWebinarCardsService(
//        ICardsService cardsService,
//        IContentQueryExecutor contentQueryExecutor,
//        IWebPageQueryResultMapper webPageQueryResultMapper,
//        IWebsiteChannelContext websiteChannelContext,
//        IPreferredLanguageRetriever preferredLanguageRetriever,
//        IWebPageUrlRetriever webPageUrlRetriever,
//        IInfoProvider<BusinessIssueInfo> businessIssueProvider,
//        ITimeZoneService timeZoneService)
//    {
//        _cardsService = cardsService;
//        _contentQueryExecutor = contentQueryExecutor;
//        _webPageQueryResultMapper = webPageQueryResultMapper;
//        _websiteChannelContext = websiteChannelContext;
//        _preferredLanguageRetriever = preferredLanguageRetriever;
//        _webPageUrlRetriever = webPageUrlRetriever;
//        _eventTypeProvider = eventTypeProvider;
//        _businessIssueProvider = businessIssueProvider;
//        _timeZoneService = timeZoneService;
//    }

//    public async Task<FilterItemDto<OptionDto>[]> CreateFiltersAsync(
//        HashSet<string> filters, string? treePath, CancellationToken cancellationToken)
//    {
//        return await _cardsService.CreateFiltersAsync(filters, treePath, cancellationToken);
//    }

//    public Task<PaginationDto<EventsWebinarsCardDto>> GetFilteredItemsAsync(
//        CardsQuery cardsQuery,
//        Action<ContentItemQueryBuilder>? decorateQuery,
//        Func<IEventOrWebinar, bool> isFeatured,
//        CancellationToken cancellationToken)
//        => GetFilteredItemsAsync(cardsQuery, decorateQuery, isFeatured, false, false, cancellationToken);

//    public async Task<PaginationDto<EventsWebinarsCardDto>> GetFilteredItemsAsync(
//        CardsQuery cardsQuery,
//        Action<ContentItemQueryBuilder>? decorateQuery,
//        Func<IEventOrWebinar, bool> isFeatured,
//        bool showPastPages,
//        bool filterOnEventToDate,
//        CancellationToken cancellationToken)
//    {
//        var language = _preferredLanguageRetriever.Get();
//        var channel = _websiteChannelContext.WebsiteChannelName;

//        // Resolve filter parameters into in-memory predicates and query decorators
//        var (queryDecorators, inMemoryFilters) =
//            await BuildFiltersAsync(cardsQuery.Filters, channel, language, cancellationToken);

//        // --- Build the multi-type query (Event + Webinar content types) ---
//        // XbyK has no single multi-type retriever; query each type and union results.
//        var eventsAndWebinars = new List<IEventOrWebinar>();

//        foreach (var contentTypeName in new[] { Event.CONTENT_TYPE_NAME, Webinar.CONTENT_TYPE_NAME })
//        {
//            var builder = new ContentItemQueryBuilder()
//                .ForContentType(contentTypeName, config =>
//                {
//                    config
//                        .ForWebsite(channel)
//                        .InLanguage(language)
//                        // Exclude private and search-excluded items
//                        .Where(where => where
//                            .Where(w => w
//                                .WhereEquals(nameof(IEventOrWebinar.EventIsPrivate), false))
//                            .Where(w => w
//                                .WhereNull(nameof(IEventOrWebinar.IsSearchExcluded))
//                                .Or()
//                                .WhereEquals(nameof(IEventOrWebinar.IsSearchExcluded), false)));

//                    // Apply filter-derived query decorators (date range, etc.)
//                    foreach (var decorator in queryDecorators)
//                    {
//                        decorator(config);
//                    }

//                    decorateQuery?.Invoke(new ContentItemQueryBuilder().ForContentType(contentTypeName, c => config));
//                });

//            var results = await _contentQueryExecutor.GetWebPageResult(
//                builder,
//                container => _webPageQueryResultMapper.Map<IEventOrWebinar>(container),
//                cancellationToken: cancellationToken);

//            eventsAndWebinars.AddRange(results);
//        }

//        // Apply in-memory filters (those that can't be pushed to the query layer)
//        var filtered = eventsAndWebinars
//            .Where(item => inMemoryFilters.All(f => f(item)))
//            .ToArray();

//        // --- Ordering: featured first, then future before past, then by date ---
//        var currentDateTime = DateTime.UtcNow;

//        IEventOrWebinar[] ordered =
//        [
//            .. filtered
//                .OrderByDescending(isFeatured)
//                .ThenBy(x => x.EventDateTo <= currentDateTime)
//                .ThenBy(x => x.EventDateTo <= currentDateTime
//                    ? -x.EventDateTo.Ticks
//                    : x.EventDateTo.Ticks)
//        ];

//        var paginated = PaginationHelper.PaginatePages(ordered, cardsQuery).ToList();
//        var dtos = await GetEventWebinarCardDtosAsync(paginated, isFeatured, cancellationToken);

//        return new PaginationDto<EventsWebinarsCardDto>
//        {
//            Data = dtos,
//            TotalCount = ordered.Length,
//        };
//    }

//    // -----------------------------------------------------------------
//    // Filter building
//    // XbyK note: expression trees over IQueryable are not supported in
//    // ContentItemQueryBuilder. Push what you can into .Where() clauses;
//    // everything else falls through to in-memory LINQ predicates.
//    // -----------------------------------------------------------------
//    private async Task<(
//        List<Action<ContentTypeQueryParameters>> QueryDecorators,
//        List<Func<IEventOrWebinar, bool>> InMemoryFilters)>
//        BuildFiltersAsync(
//            IDictionary<string, string> filterValues,
//            string channel,
//            string language,
//            CancellationToken cancellationToken)
//    {
//        var queryDecorators = new List<Action<ContentTypeQueryParameters>>();
//        var inMemoryFilters = new List<Func<IEventOrWebinar, bool>>();

//        // --- Date filtering ---
//        if (filterValues.TryGetValue(FilterParameterName.ShowAllPastEvents, out var showAll)
//            && showAll != bool.TrueString)
//        {
//            var now = DateTime.Now;

//            if (filterValues.TryGetValue(FilterParameterName.ShowOngoingEvents, out var ongoing)
//                && ongoing == bool.TrueString)
//            {
//                // Show items where ShowPastEvent == true OR EventDateTo > now
//                inMemoryFilters.Add(x => x.ShowPastEvent || x.EventDateTo > now);
//            }
//            else
//            {
//                inMemoryFilters.Add(x => x.ShowPastEvent || x.EventDateFrom > now);
//            }
//        }
//        else if (!filterValues.ContainsKey(FilterParameterName.ShowAllPastEvents))
//        {
//            var now = DateTime.Now;
//            inMemoryFilters.Add(x => x.EventDateFrom > now);
//        }

//        // --- Taxonomy / reference filters ---
//        foreach (var (name, value) in filterValues)
//        {
//            var paramData = FilterUtility.GetFilterParameterData(name);
//            if (paramData is null) continue;

//            if (paramData.FilterParameterType == FilterParameterType.Page)
//            {
//                // Resolve the filter page by tree path / codename to get its WebPageItemTreePath
//                var filterPage = await GetFilterPageAsync(paramData.EntityType, value, channel, language, cancellationToken);
//                if (filterPage is null) continue;

//                Func<IEventOrWebinar, bool>? predicate = name switch
//                {
//                    FilterParameterName.BusinessLine =>
//                        await BuildBusinessLinePredicateAsync(filterPage.WebPageItemTreePath, channel, language, cancellationToken),
//                    FilterParameterName.ServiceArea =>
//                        await BuildServiceAreaPredicateAsync(filterPage.WebPageItemTreePath, channel, language, cancellationToken),
//                    FilterParameterName.IndustryCategory =>
//                        await BuildIndustryCategoryPredicateAsync(filterPage.WebPageItemTreePath, channel, language, cancellationToken),
//                    FilterParameterName.SpecialtyCategory =>
//                        await BuildSpecialtyCategoryPredicateAsync(filterPage.WebPageItemTreePath, channel, language, cancellationToken),
//                    FilterParameterName.MicrositeMasterPage =>
//                        await BuildMicrositeMasterPagePredicateAsync(filterPage.WebPageItemTreePath, channel, language, cancellationToken),
//                    // OfficeLocation: match on WebPageItemID instead of NodeID
//                    FilterParameterName.OfficeLocation =>
//                        x => x.MetadataOfficeLocation.Contains(filterPage.WebPageItemID),
//                    _ => null
//                };

//                if (predicate is not null)
//                {
//                    inMemoryFilters.Add(predicate);
//                }
//            }
//            else if (paramData.FilterParameterType == FilterParameterType.Entity)
//            {
//                switch (name)
//                {
//                    case FilterParameterName.BusinessIssue:
//                        {
//                            var entity = _businessIssueProvider
//                                .Get()
//                                .WhereEquals("CodeName", value)
//                                .FirstOrDefault();

//                            if (entity is not null)
//                            {
//                                inMemoryFilters.Add(x => x.MetadataBusinessIssues.Contains(entity.BusinessIssueID));
//                            }
//                            break;
//                        }

//                    case FilterParameterName.EventType:
//                        {
//                            var entity = _eventTypeProvider
//                                .Get()
//                                .WhereEquals("CodeName", value)
//                                .FirstOrDefault();

//                            if (entity is not null)
//                            {
//                                inMemoryFilters.Add(x => x.EventType.Contains(entity.EventTypeID));
//                            }
//                            break;
//                        }
//                }
//            }
//        }

//        return (queryDecorators, inMemoryFilters);
//    }

//    // -----------------------------------------------------------------
//    // Hierarchy helpers — replaces IChildPageHelper.GetDescendantNodeIdsByPageType
//    // XbyK: query descendants by tree path prefix per content type,
//    // collect WebPageItemIDs, then filter in memory via linked item GUIDs.
//    // -----------------------------------------------------------------

//    private async Task<IWebPageFieldsSource?> GetFilterPageAsync(
//        Type contentType, string codeName, string channel, string language, CancellationToken ct)
//    {
//        var builder = new ContentItemQueryBuilder()
//            .ForContentType(ContentTypeHelper.GetContentTypeName(contentType), config => config
//                .ForWebsite(channel)
//                .InLanguage(language)
//                .Where(w => w.WhereEquals(nameof(IWebPageFieldsSource.SystemFields.WebPageItemName), codeName))
//                .TopN(1));

//        var results = await _contentQueryExecutor.GetWebPageResult(
//            builder,
//            container => _webPageQueryResultMapper.Map<IWebPageFieldsSource>(container),
//            cancellationToken: ct);

//        return results.FirstOrDefault();
//    }

//    private async Task<HashSet<int>> GetDescendantItemIDsAsync(
//        string contentTypeName, string treePathPrefix, string channel, string language, CancellationToken ct)
//    {
//        var builder = new ContentItemQueryBuilder()
//            .ForContentType(contentTypeName, config => config
//                .ForWebsite(channel)
//                .InLanguage(language)
//                .Where(w => w.WhereStartsWith(
//                    nameof(IWebPageContentQueryDataContainer.WebPageItemTreePath),
//                    treePathPrefix)));

//        var results = await _contentQueryExecutor.GetWebPageResult(
//            builder,
//            container => container.WebPageItemID,
//            cancellationToken: ct);

//        return results.ToHashSet();
//    }

//    private async Task<Func<IEventOrWebinar, bool>> BuildBusinessLinePredicateAsync(
//        string treePathPrefix, string channel, string language, CancellationToken ct)
//    {
//        var businessLineIDs = await GetDescendantItemIDsAsync(BusinessLine.CONTENT_TYPE_NAME, treePathPrefix, channel, language, ct);
//        var serviceAreaIDs = await GetDescendantItemIDsAsync(ServiceArea.CONTENT_TYPE_NAME, treePathPrefix, channel, language, ct);
//        var serviceIDs = await GetDescendantItemIDsAsync(Service.CONTENT_TYPE_NAME, treePathPrefix, channel, language, ct);

//        return x =>
//            x.MetadataBusinessLines.Intersect(businessLineIDs).Any() ||
//            x.MetadataServiceAreas.Intersect(serviceAreaIDs).Any() ||
//            x.MetadataServices.Intersect(serviceIDs).Any();
//    }

//    private async Task<Func<IEventOrWebinar, bool>> BuildServiceAreaPredicateAsync(
//        string treePathPrefix, string channel, string language, CancellationToken ct)
//    {
//        var serviceAreaIDs = await GetDescendantItemIDsAsync(ServiceArea.CONTENT_TYPE_NAME, treePathPrefix, channel, language, ct);
//        var serviceIDs = await GetDescendantItemIDsAsync(Service.CONTENT_TYPE_NAME, treePathPrefix, channel, language, ct);

//        return x =>
//            x.MetadataServiceAreas.Intersect(serviceAreaIDs).Any() ||
//            x.MetadataServices.Intersect(serviceIDs).Any();
//    }

//    private async Task<Func<IEventOrWebinar, bool>> BuildIndustryCategoryPredicateAsync(
//        string treePathPrefix, string channel, string language, CancellationToken ct)
//    {
//        var industryCategoryIDs = await GetDescendantItemIDsAsync(IndustryCategory.CONTENT_TYPE_NAME, treePathPrefix, channel, language, ct);
//        var industryIDs = await GetDescendantItemIDsAsync(Industry.CONTENT_TYPE_NAME, treePathPrefix, channel, language, ct);

//        return x =>
//            x.MetadataIndustryCategories.Intersect(industryCategoryIDs).Any() ||
//            x.MetadataIndustries.Intersect(industryIDs).Any();
//    }

//    private async Task<Func<IEventOrWebinar, bool>> BuildSpecialtyCategoryPredicateAsync(
//        string treePathPrefix, string channel, string language, CancellationToken ct)
//    {
//        var specialtyCategoryIDs = await GetDescendantItemIDsAsync(SpecialtiesCategory.CONTENT_TYPE_NAME, treePathPrefix, channel, language, ct);
//        var specialtyAreaIDs = await GetDescendantItemIDsAsync(SpecialtiesArea.CONTENT_TYPE_NAME, treePathPrefix, channel, language, ct);
//        var specialtyIDs = await GetDescendantItemIDsAsync(SpecialtiesPage.CONTENT_TYPE_NAME, treePathPrefix, channel, language, ct);

//        return x =>
//            x.MetadataSpecialtiesCategories.Intersect(specialtyCategoryIDs).Any() ||
//            x.MetadataSpecialtiesAreas.Intersect(specialtyAreaIDs).Any() ||
//            x.MetadataSpecialtiesPages.Intersect(specialtyIDs).Any();
//    }

//    private async Task<Func<IEventOrWebinar, bool>> BuildMicrositeMasterPagePredicateAsync(
//        string treePathPrefix, string channel, string language, CancellationToken ct)
//    {
//        var masterPageIDs = await GetDescendantItemIDsAsync(MicrositeMasterPage.CONTENT_TYPE_NAME, treePathPrefix, channel, language, ct);
//        var micrositePageIDs = await GetDescendantItemIDsAsync(MicrositePage.CONTENT_TYPE_NAME, treePathPrefix, channel, language, ct);

//        return x =>
//            x.MicrositeMasterPages.Intersect(masterPageIDs).Any() ||
//            x.MicrositePages.Intersect(micrositePageIDs).Any();
//    }

//    // -----------------------------------------------------------------
//    // Card DTO assembly
//    // -----------------------------------------------------------------
//    public async Task<EventsWebinarsCardDto[]> GetEventWebinarCardDtosAsync(
//        ICollection<IEventOrWebinar> eventsAndWebinars,
//        Func<IEventOrWebinar, bool> isFeatured,
//        CancellationToken cancellationToken)
//    {
//        var language = _preferredLanguageRetriever.Get();

//        // Resolve EventType info objects (replaces IRetriever<EventType>)
//        var eventTypeIds = eventsAndWebinars
//            .Select(x => ExtractEventTypeId(x.EventType))
//            .OfType<int>()
//            .Distinct()
//            .ToList();

//        var eventTypeById = _eventTypeProvider
//            .Get()
//            .WhereIn("EventTypeID", eventTypeIds)
//            .WhereEquals("LanguageTag", language)
//            .ToDictionary(x => x.EventTypeID, x => x);

//        // Resolve URLs — XbyK: IWebPageUrlRetriever keyed by WebPageItemGuid
//        var urlTasks = eventsAndWebinars.Select(async x =>
//        {
//            var url = await _webPageUrlRetriever.Retrieve(x.WebPageItemGuid, language, cancellationToken);
//            return (x.WebPageItemGuid, RelativeUrl: url.RelativePath);
//        });

//        var urlResults = await Task.WhenAll(urlTasks);
//        var urlByGuid = urlResults.ToDictionary(r => r.WebPageItemGuid, r => r.RelativeUrl);

//        var dtos = eventsAndWebinars.Select(eventOrWebinar =>
//        {
//            EventTypeInfo? eventType = null;
//            var eventTypeId = ExtractEventTypeId(eventOrWebinar.EventType);
//            if (eventTypeId.HasValue)
//            {
//                eventTypeById.TryGetValue(eventTypeId.Value, out eventType);
//            }

//            return new EventsWebinarsCardDto
//            {
//                IsFeatured = isFeatured(eventOrWebinar),
//                Type = eventType?.Name,
//                DateTime = _timeZoneService.ConvertToSiteDateTime(eventOrWebinar.EventDateFrom),
//                DateTimeTo = eventOrWebinar.EventDateFrom.Date != eventOrWebinar.EventDateTo.Date
//                    ? _timeZoneService.ConvertToSiteDateTime(eventOrWebinar.EventDateTo)
//                    : null,
//                Location = string.IsNullOrEmpty(eventOrWebinar.EventLocationCity)
//                    ? null
//                    : eventOrWebinar.EventLocationCity,
//                Title = eventOrWebinar.MetadataTitle,
//                Description = string.IsNullOrEmpty(eventOrWebinar.MetadataDescription)
//                    ? null
//                    : HtmlHelper.ExtractTextFromHtml(eventOrWebinar.MetadataDescription),
//                Link = new LinkItemDto
//                {
//                    Title = eventType?.CallToActionText ?? string.Empty,
//                    URL = urlByGuid[eventOrWebinar.WebPageItemGuid],
//                },
//            };
//        }).ToArray();

//        return dtos;
//    }

//    private static int? ExtractEventTypeId(IReadOnlyCollection<int> eventTypeIds)
//        => eventTypeIds.Count == 0 ? null : eventTypeIds.First();
//}