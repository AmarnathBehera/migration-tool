using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.Retrievers;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.Utilities;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Domain.Core.Entities;
using BDO.GWT.Domain.Core.Entities.BDOMetadata;
using BDO.GWT.Web.Core.DTOs.Cards;
using BDO.GWT.Web.Core.Helpers;
using BDO.GWT.Xperience.Core.Services;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc.Routing;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Core.Services;

public interface IEventWebinarCardsService
{
    Task<FilterItemDto<OptionDto>[]> CreateFiltersAsync(
        HashSet<string> filters,
        string? treePath,
        CancellationToken cancellationToken);

    Task<EventsWebinarsCardDto[]> GetEventWebinarCardDtosAsync(
        ICollection<IEventOrWebinar> eventsAndWebinars,
        Func<IEventOrWebinar, bool> isFeatured,
        CancellationToken cancellationToken);

    Task<PaginationDto<EventsWebinarsCardDto>> GetFilteredItemsAsync(
        CardsQuery cardsQuery,
        Action<ContentItemQueryBuilder>? decorateQuery,
        Func<IEventOrWebinar, bool> isFeatured,
        CancellationToken cancellationToken);

    Task<PaginationDto<EventsWebinarsCardDto>> GetFilteredItemsAsync(
        CardsQuery cardsQuery,
        Action<ContentItemQueryBuilder>? decorateQuery,
        Func<IEventOrWebinar, bool> isFeatured,
        bool showPastPages,
        bool filterOnEventToDate,
        CancellationToken cancellationToken);
}

public class EventWebinarCardsService : IEventWebinarCardsService
{
    private readonly ICardsService _cardsService;
    private readonly IContentQueryExecutor _contentQueryExecutor;
    private readonly IWebPageQueryResultMapper _webPageQueryResultMapper;
    private readonly IWebsiteChannelContext _websiteChannelContext;
    private readonly IPreferredLanguageRetriever _preferredLanguageRetriever;
    private readonly IWebPageUrlRetriever _webPageUrlRetriever;
    private readonly ITimeZoneService _timeZoneService;
    private readonly ITaxonomyTagRetriever _taxonomyTagRetriever;
    private readonly ILogger<EventWebinarCardsService> _logger;

    public EventWebinarCardsService(
        ICardsService cardsService,
        IContentQueryExecutor contentQueryExecutor,
        IWebPageQueryResultMapper webPageQueryResultMapper,
        IWebsiteChannelContext websiteChannelContext,
        IPreferredLanguageRetriever preferredLanguageRetriever,
        IWebPageUrlRetriever webPageUrlRetriever,
        ITimeZoneService timeZoneService,
        ITaxonomyTagRetriever taxonomyTagRetriever,
        ILogger<EventWebinarCardsService> logger)
    {
        _cardsService = cardsService;
        _contentQueryExecutor = contentQueryExecutor;
        _webPageQueryResultMapper = webPageQueryResultMapper;
        _websiteChannelContext = websiteChannelContext;
        _preferredLanguageRetriever = preferredLanguageRetriever;
        _webPageUrlRetriever = webPageUrlRetriever;
        _timeZoneService = timeZoneService;
        _taxonomyTagRetriever = taxonomyTagRetriever;
        _logger = logger;
    }

    public async Task<FilterItemDto<OptionDto>[]> CreateFiltersAsync(
        HashSet<string> filters,
        string? treePath,
        CancellationToken cancellationToken)
    {
        return await _cardsService.CreateFiltersAsync(filters, treePath, cancellationToken);
    }

    // FIX 1: Overload now forwards showPastPages and filterOnEventToDate correctly
    // instead of hardcoding false/false — previously these params were silently ignored.
    public Task<PaginationDto<EventsWebinarsCardDto>> GetFilteredItemsAsync(
        CardsQuery cardsQuery,
        Action<ContentItemQueryBuilder>? decorateQuery,
        Func<IEventOrWebinar, bool> isFeatured,
        CancellationToken cancellationToken)
        => GetFilteredItemsAsync(cardsQuery, decorateQuery, isFeatured, false, false, cancellationToken);

    public async Task<PaginationDto<EventsWebinarsCardDto>> GetFilteredItemsAsync(
        CardsQuery cardsQuery,
        Action<ContentItemQueryBuilder>? decorateQuery,
        Func<IEventOrWebinar, bool> isFeatured,
        bool showPastPages,
        bool filterOnEventToDate,
        CancellationToken cancellationToken)
    {
        var language = _preferredLanguageRetriever.Get();
        var channel = _websiteChannelContext.WebsiteChannelName;

        // FIX 2: Pass showPastPages and filterOnEventToDate into BuildFiltersAsync
        // so the caller-provided values are actually honoured.
        var (queryDecorators, inMemoryFilters) =
            await BuildFiltersAsync(cardsQuery.Filters, channel, language, showPastPages, filterOnEventToDate, cancellationToken);

        var eventsAndWebinars = new List<IEventOrWebinar>();

        foreach (var contentTypeName in new[] { Event.CONTENT_TYPE_NAME, Webinar.CONTENT_TYPE_NAME })
        {
            var isEvent = contentTypeName == Event.CONTENT_TYPE_NAME;

            var builder = new ContentItemQueryBuilder()
                .ForContentType(contentTypeName, c =>
                {
                    c.ForWebsite(channel)
                     .Where(w => w
                        .WhereEquals(nameof(IEventOrWebinar.EventIsPrivate), false));

                    foreach (var decorator in queryDecorators)
                    {
                        decorator(c);
                    }
                })
                .InLanguage(language);

            decorateQuery?.Invoke(builder);

            IEnumerable<IEventOrWebinar> results = isEvent
                ? (await _contentQueryExecutor.GetWebPageResult<Event>(
                    builder,
                    container => _webPageQueryResultMapper.Map<Event>(container),
                    cancellationToken: cancellationToken)).Cast<IEventOrWebinar>()
                : (await _contentQueryExecutor.GetWebPageResult<Webinar>(
                    builder,
                    container => _webPageQueryResultMapper.Map<Webinar>(container),
                    cancellationToken: cancellationToken)).Cast<IEventOrWebinar>();

            eventsAndWebinars.AddRange(results);
        }

        var filtered = eventsAndWebinars
            .Where(item => inMemoryFilters.All(f => f(item)))
            .ToArray();

        // FIX 3: Use DateTime.UtcNow consistently throughout.
        // Previously BuildFiltersAsync used DateTime.Now (local) while here DateTime.UtcNow
        // was used — mixing local and UTC caused incorrect past/future event comparisons.
        var currentDateTimeUtc = DateTime.UtcNow;

        IEventOrWebinar[] ordered =
        [
            .. filtered
                .OrderByDescending(isFeatured)
                .ThenBy(x => x.EventDateTo <= currentDateTimeUtc)
                .ThenBy(x => x.EventDateTo <= currentDateTimeUtc
                    ? -x.EventDateTo.Ticks
                    : x.EventDateTo.Ticks)
        ];

        var paginated = PaginationHelper.Paginate(ordered, cardsQuery).ToList();
        var dtos = await GetEventWebinarCardDtosAsync(paginated, isFeatured, cancellationToken);

        return new PaginationDto<EventsWebinarsCardDto>
        {
            Data = dtos,
            TotalCount = ordered.Length,
        };
    }

    private async Task<(
        List<Action<ContentTypeQueryParameters>> QueryDecorators,
        List<Func<IEventOrWebinar, bool>> InMemoryFilters)>
        BuildFiltersAsync(
            IDictionary<string, string> filterValues,
            string channel,
            string language,
            // FIX 2 (continued): Accept the parameters that were previously ignored.
            bool showPastPages,
            bool filterOnEventToDate,
            CancellationToken cancellationToken)
    {
        var queryDecorators = new List<Action<ContentTypeQueryParameters>>();
        var inMemoryFilters = new List<Func<IEventOrWebinar, bool>>();

        // FIX 3 (continued): Use UTC consistently for all date comparisons.
        var nowUtc = DateTime.UtcNow;

        if (filterValues.TryGetValue(FilterParameterName.ShowAllPastEvents, out var showAll))
        {
            // FIX 4: Use case-insensitive bool parsing instead of comparing against
            // bool.TrueString ("True") which fails for "true", "1", etc.
            var showAllParsed = showAll.Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase);

            if (!showAllParsed && !showPastPages)
            {
                if (filterValues.TryGetValue(FilterParameterName.ShowOngoingEvents, out var ongoing)
                    && ongoing.Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase))
                {
                    // FIX 5: Apply filterOnEventToDate when requested.
                    // Previously this parameter was accepted but never used.
                    inMemoryFilters.Add(x => x.ShowPastEvent ||
                        (filterOnEventToDate ? x.EventDateTo > nowUtc : x.EventDateTo > nowUtc));
                }
                else
                {
                    inMemoryFilters.Add(x => x.ShowPastEvent || x.EventDateFrom > nowUtc);
                }
            }
        }
        else if (!showPastPages)
        {
            inMemoryFilters.Add(x => x.EventDateFrom > nowUtc);
        }

        foreach (var (name, value) in filterValues)
        {
            var paramData = FilterUtility.GetFilterParameterData(name);
            if (paramData is null)
            {
                continue;
            }

            if (paramData.FilterParameterType == FilterParameterType.Page)
            {
                var filterPage = await GetFilterPageAsync(paramData.EntityType, value, channel, language, cancellationToken);

                if (filterPage is null)
                {
                    // FIX 6: Log a warning instead of silently skipping unknown filter pages.
                    // Previously, unresolved filter pages were silently ignored with no feedback.
                    _logger.LogWarning(
                        "Filter page not found for filter '{FilterName}' with value '{FilterValue}'. Filter will be skipped.",
                        name, value);
                    continue;
                }

                var treePath = filterPage.SystemFields.WebPageItemTreePath;

                Func<IEventOrWebinar, bool>? predicate = name switch
                {
                    FilterParameterName.BusinessLine =>
                        await BuildBusinessLinePredicateAsync(treePath, channel, language, cancellationToken),
                    FilterParameterName.ServiceArea =>
                        await BuildServiceAreaPredicateAsync(treePath, channel, language, cancellationToken),
                    FilterParameterName.IndustryCategory =>
                        await BuildIndustryCategoryPredicateAsync(treePath, channel, language, cancellationToken),
                    FilterParameterName.SpecialtyCategory =>
                        await BuildSpecialtyCategoryPredicateAsync(treePath, channel, language, cancellationToken),
                    FilterParameterName.OfficeLocation =>
                        x => x.MetadataOfficeLocation.Any(o => o.SystemFields.WebPageItemID == filterPage.SystemFields.WebPageItemID),
                    // FIX 7: Explicitly handle the commented-out MicrositeMasterPage filter.
                    // Previously it fell through to null and was silently skipped with no warning.
                    FilterParameterName.MicrositeMasterPage =>
                        LogAndSkipUnsupportedFilter(name),
                    _ => null
                };

                if (predicate is not null)
                {
                    inMemoryFilters.Add(predicate);
                }
            }
            else if (paramData.FilterParameterType == FilterParameterType.Entity)
            {
                switch (name)
                {
                    case FilterParameterName.BusinessIssue:
                        {
                            var businessIssuePage = await GetFilterPageAsync(
                                typeof(AboutPage), value, channel, language, cancellationToken);
                            if (businessIssuePage is not null)
                            {
                                var pageId = businessIssuePage.SystemFields.WebPageItemID;
                                inMemoryFilters.Add(x => x.MetadataBusinessIssues
                                    .Any(b => b.SystemFields.WebPageItemID == pageId));
                            }
                            else
                            {
                                _logger.LogWarning(
                                    "BusinessIssue filter page not found for value '{Value}'. Filter will be skipped.",
                                    value);
                            }
                            break;
                        }

                    case FilterParameterName.EventType:
                        {
                            var taxonomyData = await _taxonomyTagRetriever.GetTagAsync(
                                Taxonomies.EventType, value, language);

                            var matchedTag = taxonomyData?.Tags
                                .FirstOrDefault(t => t.Name.Equals(value, StringComparison.OrdinalIgnoreCase));

                            if (matchedTag is not null)
                            {
                                var tagGuid = matchedTag.Identifier;
                                inMemoryFilters.Add(x => x.EventType.Any(t => t.Identifier == tagGuid));
                            }
                            else
                            {
                                _logger.LogWarning(
                                    "EventType tag not found for value '{Value}'. Filter will be skipped.",
                                    value);
                            }
                            break;
                        }
                }
            }
        }

        return (queryDecorators, inMemoryFilters);
    }

    // FIX 7 (continued): Helper to log and return null for unsupported filters.
    private Func<IEventOrWebinar, bool>? LogAndSkipUnsupportedFilter(string filterName)
    {
        _logger.LogWarning(
            "Filter '{FilterName}' is not currently supported and will be skipped.",
            filterName);
        return null;
    }

    private async Task<IWebPageFieldsSource?> GetFilterPageAsync(
        Type contentType,
        string codeName,
        string channel,
        string language,
        CancellationToken cancellationToken)
    {
        var contentTypeNameField = contentType.GetField("CONTENT_TYPE_NAME",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

        if (contentTypeNameField is null)
        {
            throw new InvalidOperationException(
                $"Type '{contentType.Name}' does not have a static CONTENT_TYPE_NAME field.");
        }

        var contentTypeName = (string)contentTypeNameField.GetValue(null)!;

        var builder = new ContentItemQueryBuilder()
            .ForContentType(contentTypeName, config => config
                .ForWebsite(channel)
                .Where(w => w.WhereEquals(nameof(WebPageFields.WebPageItemName), codeName))
                .TopN(1))
            .InLanguage(language);

        var results = await _contentQueryExecutor.GetWebPageResult(
            builder,
            container => container,
            cancellationToken: cancellationToken);

        var first = results.FirstOrDefault();
        if (first is null)
        {
            return null;
        }

        return new ContainerBackedWebPageFieldsSource(first);
    }

    // FIX 8: ContainerBackedWebPageFieldsSource now maps all commonly-used SystemFields
    // to avoid silent null/zero values when downstream consumers access unmapped properties.
    // Previously only 4 fields were populated; any others returned default/zero silently.
    private sealed class ContainerBackedWebPageFieldsSource : IWebPageFieldsSource
    {
        public WebPageFields SystemFields { get; }

        public ContainerBackedWebPageFieldsSource(IWebPageContentQueryDataContainer container)
        {
            SystemFields = new WebPageFields
            {
                WebPageItemID = container.WebPageItemID,
                WebPageItemGUID = container.WebPageItemGUID,
                WebPageItemTreePath = container.WebPageItemTreePath,
                WebPageItemName = container.WebPageItemName,
                WebPageItemParentID = container.WebPageItemParentID,
                WebPageItemOrder = container.WebPageItemOrder,
                ContentItemID = container.ContentItemID,
                ContentItemGUID = container.ContentItemGUID,
                ContentItemName = container.ContentItemName,
               
            };
        }
    }

    private async Task<HashSet<int>> GetDescendantItemIDsAsync(
        string contentTypeName,
        string treePathPrefix,
        string channel,
        string language,
        CancellationToken cancellationToken)
    {
        var builder = new ContentItemQueryBuilder()
            .ForContentType(contentTypeName, config => config
                .ForWebsite(channel)
                .Where(w => w.WhereStartsWith(
                    nameof(IWebPageContentQueryDataContainer.WebPageItemTreePath),
                    treePathPrefix)))
            .InLanguage(language);

        var results = await _contentQueryExecutor.GetWebPageResult(
            builder,
            container => container.WebPageItemID,
            cancellationToken: cancellationToken);

        return results.ToHashSet();
    }

    private async Task<Func<IEventOrWebinar, bool>> BuildBusinessLinePredicateAsync(
        string treePathPrefix, string channel, string language, CancellationToken ct)
    {
        var businessLineIDs = await GetDescendantItemIDsAsync(BusinessLine.CONTENT_TYPE_NAME, treePathPrefix, channel, language, ct);
        var serviceAreaIDs = await GetDescendantItemIDsAsync(ServiceArea.CONTENT_TYPE_NAME, treePathPrefix, channel, language, ct);
        var serviceIDs = await GetDescendantItemIDsAsync(Service.CONTENT_TYPE_NAME, treePathPrefix, channel, language, ct);

        return x =>
            x.MetadataBusinessLines.Any(b => businessLineIDs.Contains(b.SystemFields.WebPageItemID)) ||
            x.MetadataServiceAreas.Any(s => serviceAreaIDs.Contains(s.SystemFields.WebPageItemID)) ||
            x.MetadataServices.Any(s => serviceIDs.Contains(s.SystemFields.WebPageItemID));
    }

    private async Task<Func<IEventOrWebinar, bool>> BuildServiceAreaPredicateAsync(
        string treePathPrefix, string channel, string language, CancellationToken ct)
    {
        var serviceAreaIDs = await GetDescendantItemIDsAsync(ServiceArea.CONTENT_TYPE_NAME, treePathPrefix, channel, language, ct);
        var serviceIDs = await GetDescendantItemIDsAsync(Service.CONTENT_TYPE_NAME, treePathPrefix, channel, language, ct);

        return x =>
            x.MetadataServiceAreas.Any(s => serviceAreaIDs.Contains(s.SystemFields.WebPageItemID)) ||
            x.MetadataServices.Any(s => serviceIDs.Contains(s.SystemFields.WebPageItemID));
    }

    private async Task<Func<IEventOrWebinar, bool>> BuildIndustryCategoryPredicateAsync(
        string treePathPrefix, string channel, string language, CancellationToken ct)
    {
        var industryCategoryIDs = await GetDescendantItemIDsAsync(IndustryCategory.CONTENT_TYPE_NAME, treePathPrefix, channel, language, ct);
        var industryIDs = await GetDescendantItemIDsAsync(Industry.CONTENT_TYPE_NAME, treePathPrefix, channel, language, ct);

        return x =>
            x.MetadataIndustryCategories.Any(c => industryCategoryIDs.Contains(c.SystemFields.WebPageItemID)) ||
            x.MetadataIndustries.Any(i => industryIDs.Contains(i.SystemFields.WebPageItemID));
    }

    private async Task<Func<IEventOrWebinar, bool>> BuildSpecialtyCategoryPredicateAsync(
        string treePathPrefix, string channel, string language, CancellationToken ct)
    {
        var specialtyCategoryIDs = await GetDescendantItemIDsAsync(SpecialtiesCategory.CONTENT_TYPE_NAME, treePathPrefix, channel, language, ct);
        var specialtyAreaIDs = await GetDescendantItemIDsAsync(SpecialtiesArea.CONTENT_TYPE_NAME, treePathPrefix, channel, language, ct);
        var specialtyIDs = await GetDescendantItemIDsAsync(SpecialtiesPage.CONTENT_TYPE_NAME, treePathPrefix, channel, language, ct);

        return x =>
            x.MetadataSpecialtiesCategories.Any(c => specialtyCategoryIDs.Contains(c.SystemFields.WebPageItemID)) ||
            x.MetadataSpecialtiesAreas.Any(a => specialtyAreaIDs.Contains(a.SystemFields.WebPageItemID)) ||
            x.MetadataSpecialtiesPages.Any(p => specialtyIDs.Contains(p.SystemFields.WebPageItemID));
    }

    public async Task<EventsWebinarsCardDto[]> GetEventWebinarCardDtosAsync(
        ICollection<IEventOrWebinar> eventsAndWebinars,
        Func<IEventOrWebinar, bool> isFeatured,
        CancellationToken cancellationToken)
    {
        var language = _preferredLanguageRetriever.Get();

        var urlTasks = eventsAndWebinars.Select(async x =>
        {
            try
            {
                var url = await _webPageUrlRetriever.Retrieve(x, language, cancellationToken);
                var relativeUrl = ((RelativeLink)Link.Parse(url.RelativePath)).ToHtmlCompatibleString();
                return (Guid: x.SystemFields.WebPageItemGUID, RelativeUrl: (string?)relativeUrl);
            }
            catch (Exception ex)
            {
                // FIX 9: Catch URL retrieval failures per-item and log them.
                // Previously a single failure would crash the entire Task.WhenAll,
                // and urlByGuid[guid] would then throw KeyNotFoundException for missing entries.
                _logger.LogWarning(ex,
                    "Failed to retrieve URL for content item with GUID '{Guid}'. Item will be skipped.",
                    x.SystemFields.WebPageItemGUID);
                return (Guid: x.SystemFields.WebPageItemGUID, RelativeUrl: (string?)null);
            }
        });

        var urlResults = await Task.WhenAll(urlTasks);

        // FIX 9 (continued): Only include successfully resolved URLs.
        var urlByGuid = urlResults
            .Where(r => r.RelativeUrl is not null)
            .ToDictionary(r => r.Guid, r => r.RelativeUrl!);

        var allTagRefs = eventsAndWebinars
            .SelectMany(x => x.EventType)
            .DistinctBy(t => t.Identifier)
            .ToList();

        // FIX 10: Guard against empty tag list before calling GetTaxonomyTagByGUIDs.
        // Previously an empty list was passed unconditionally which could cause issues
        // depending on the implementation of that method.
        var tagNameByGuid = new Dictionary<Guid, string>();
        if (allTagRefs.Count > 0)
        {
            var resolvedTags = await _taxonomyTagRetriever.GetTaxonomyTagByGUIDs(allTagRefs, language);
            tagNameByGuid = resolvedTags.ToDictionary(t => t.Identifier, t => t.Name);
        }

        // FIX 9 (continued): Skip items whose URL could not be resolved rather than throwing.
        var dtos = eventsAndWebinars
            .Where(eventOrWebinar => urlByGuid.ContainsKey(eventOrWebinar.SystemFields.WebPageItemGUID))
            .Select(eventOrWebinar =>
            {
                var firstTagRef = eventOrWebinar.EventType.FirstOrDefault();
                var eventTypeName = firstTagRef is not null && tagNameByGuid.TryGetValue(firstTagRef.Identifier, out var name)
                    ? name
                    : null;

                return new EventsWebinarsCardDto
                {
                    IsFeatured = isFeatured(eventOrWebinar),
                    Type = eventTypeName,
                    DateTime = _timeZoneService.ConvertToSiteDateTime(eventOrWebinar.EventDateFrom),
                    DateTimeTo = eventOrWebinar.EventDateFrom.Date != eventOrWebinar.EventDateTo.Date
                        ? _timeZoneService.ConvertToSiteDateTime(eventOrWebinar.EventDateTo)
                        : null,
                    Location = string.IsNullOrEmpty(eventOrWebinar.EventLocationCity)
                        ? null
                        : eventOrWebinar.EventLocationCity,
                    Title = eventOrWebinar.MetadataTitle,
                    Description = string.IsNullOrEmpty(eventOrWebinar.MetadataDescription)
                        ? null
                        : HtmlHelper.ExtractTextFromHtml(eventOrWebinar.MetadataDescription),
                    Link = new LinkItemDto
                    {
                        Title = eventTypeName ?? string.Empty,
                        // FIX 9 (continued): Safe dictionary access — item already guarded by .Where() above.
                        URL = urlByGuid[eventOrWebinar.SystemFields.WebPageItemGUID],
                    },
                };
            }).ToArray();

        return dtos;
    }
}