using BDO.GWT.Core;
using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.Utilities;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Core.Entities;
using BDO.GWT.Web.Core.Helpers;
using BDO.GWT.Xperience.Core.Services;
using CMS.ContentEngine;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc.Routing;

namespace BDO.GWT.Web.Core.Services;

public interface IEventWebinarCardsService
{
    Task<FilterItemDto<OptionDto>[]> CreateFiltersAsync(
        HashSet<string> filters,
        string? treePath,
        CancellationToken cancellationToken);

    Task<EventsWebinarsCardDto[]> GetEventWebinarCardDtosAsync(
        ICollection<IEventOrWebinar> eventsAndWebinars,
        string lang,
        Func<IEventOrWebinar, bool> isFeatured,
        CancellationToken cancellationToken);


    Task<PaginationDto<EventsWebinarsCardDto>> GetFilteredItemsAsync(
        CardsQuery cardsQuery,
        string language,
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
    private readonly IContentQueryModelTypeMapper _webPageQueryResultMapper;
    private readonly IWebsiteChannelContext _websiteChannelContext;
    private readonly IWebPageUrlRetriever _webPageUrlRetriever;
    private readonly ITimeZoneService _timeZoneService;
    private readonly IContentService _contentService;

    public EventWebinarCardsService(
        ICardsService cardsService,
        IContentQueryExecutor contentQueryExecutor,
        IContentQueryModelTypeMapper webPageQueryResultMapper,
        IContentService contentService,
        IWebsiteChannelContext websiteChannelContext,
        IWebPageUrlRetriever webPageUrlRetriever,
        ITimeZoneService timeZoneService)
    {
        _cardsService = cardsService;
        _contentQueryExecutor = contentQueryExecutor;
        _webPageQueryResultMapper = webPageQueryResultMapper;
        _websiteChannelContext = websiteChannelContext;
        _webPageUrlRetriever = webPageUrlRetriever;
        _timeZoneService = timeZoneService;
        _contentService = contentService;
    }

    public async Task<FilterItemDto<OptionDto>[]> CreateFiltersAsync(
        HashSet<string> filters,
        string? treePath,
        CancellationToken cancellationToken)
    {
        return await _cardsService.CreateFiltersAsync(filters, treePath, cancellationToken);
    }

    private async Task<List<Action<WhereParameters>>> GetFilterParameterWhereExpressionsAsync(IDictionary<string, string> filterParameterValueByName)
    {
        var whereExpressions = new List<Action<WhereParameters>>();

        foreach (var (filterParameterName, filterParameterValue) in filterParameterValueByName)
        {
            var filterParameterData = FilterUtility.GetFilterParameterData(filterParameterName);

            if (filterParameterData is null)
            {
                continue;
            }

            var filter = filterParameterData.FilterParameterType switch
            {
                FilterParameterType.Page => await GetPageFilterAsync(filterParameterName, filterParameterValue),
                FilterParameterType.Tagpage => await GetTagPageFilterAsync(filterParameterName, filterParameterValue),
                _ => null,
            };

            if (filter is not null)
            {
                whereExpressions.Add(filter);
            }
        }

        return whereExpressions;
    }
    private async Task<Action<WhereParameters>?> GetPageFilterAsync(string filterName, string filterValue)
    {
        var page = await _contentService.GetPageByTypeAndNameAsync(
            FilterUtility.GetFilterParameterData(filterName)!.EntityType, filterValue);

        if (page is null)
            return null;

        var guid = page.SystemFields.ContentItemGUID.ToString();

        return filterName switch
        {
            FilterParameterName.Service => x => x.WhereContains(nameof(IEventOrWebinar.MetadataServices), guid),
            FilterParameterName.ServiceArea => x => x.WhereContains(nameof(IEventOrWebinar.MetadataServiceAreas), guid),
            FilterParameterName.IndustryCategory => x => x.WhereContains(nameof(IEventOrWebinar.MetadataIndustryCategories), guid),
            FilterParameterName.IndustryPage => x => x.WhereContains(nameof(IEventOrWebinar.MetadataIndustries), guid),
            FilterParameterName.BusinessLine => x => x.WhereContains(nameof(IEventOrWebinar.MicrositeMasterPages), guid),
            FilterParameterName.SpecialtyCategory => x => x.WhereContains(nameof(IEventOrWebinar.MetadataSpecialtiesCategories), guid),
            FilterParameterName.OfficeLocation => x => x.WhereContains(nameof(IEventOrWebinar.MetadataOfficeLocation), guid),
            FilterParameterName.ShowAllPastEvents => x => x.WhereContains(nameof(IEventOrWebinar.ShowPastEvent), guid),
            FilterParameterName.MicrositeMasterPage => x => x.WhereContains(nameof(IEventOrWebinar.MicrositeMasterPages), guid),
            FilterParameterName.MicrositePage => x => x.WhereContains(nameof(IEventOrWebinar.MicrositePages), guid),

            _ => null,
        };
    }

    private async Task<Action<WhereParameters>?> GetTagPageFilterAsync(string filterName, string filterValue)
    {

        var page = await _contentService.GetPageByTypeAndNameAsync(
            FilterUtility.GetFilterParameterData(filterName)!.EntityType, filterValue);

        if (page is null)
            return null;

        var guid = page.SystemFields.ContentItemGUID.ToString();

        return filterName switch
        {

            FilterParameterName.EventType => w => w.WhereContains(nameof(IEventOrWebinar.EventType), guid),
            FilterParameterName.BusinessIssue => x => x.WhereContains(nameof(IEventOrWebinar.MetadataBusinessIssues), guid),

            _ => null,
        };
    }
    public async Task<PaginationDto<EventsWebinarsCardDto>> GetFilteredItemsAsync(
     CardsQuery cardsQuery,
     string lang,
     Action<ContentItemQueryBuilder>? decorateQuery,
     Func<IEventOrWebinar, bool> isFeatured,
     bool showPastPages,
     bool filterOnEventToDate,
     CancellationToken cancellationToken)
    {
        var filterWhereExpressions = await GetFilterParameterWhereExpressionsAsync(cardsQuery.Filters);
        var language = lang ?? _contentService.GetCurrentLanguage();
        var channel = _websiteChannelContext.WebsiteChannelName;

        ContentItemQueryBuilder BuildQuery(string contentTypeName)
        {
            var builder = new ContentItemQueryBuilder()
                .ForContentType(contentTypeName, c =>
                {
                    c.ForWebsite(channel)
                    .WithLinkedItems(2)
                     .Where(w => w.WhereEquals(nameof(IEventOrWebinar.EventIsPrivate), false));
                     
                    foreach (var decorator in filterWhereExpressions)
                    {
                        c.Where(decorator);
                    }
                })
                .InLanguage(language);

            decorateQuery?.Invoke(builder);
            return builder;
        }


        var eventsTask = _contentQueryExecutor.GetWebPageResult<Event>(
            BuildQuery(Event.CONTENT_TYPE_NAME),
            container => _webPageQueryResultMapper.Map<Event>(container),
            cancellationToken: cancellationToken);

        var webinarsTask = _contentQueryExecutor.GetWebPageResult<Webinar>(
            BuildQuery(Webinar.CONTENT_TYPE_NAME),
            container => _webPageQueryResultMapper.Map<Webinar>(container),
            cancellationToken: cancellationToken);

        await Task.WhenAll(eventsTask, webinarsTask);

        var eventsAndWebinars = eventsTask.Result.Select(e => (IEventOrWebinar)new EventWrapper(e))
        .Concat(webinarsTask.Result.Select(w => (IEventOrWebinar)new WebinarWrapper(w)))
        .ToList();

        var currentDateTimeUtc = DateTime.UtcNow;
        var filtered = eventsAndWebinars
            .Where(x => x is not null)
            .Where(x =>
            {
                if (!showPastPages)
                {
                    return x.EventDateTo > currentDateTimeUtc;
                }
                return true;
            });

        IEventOrWebinar[] ordered =
        [
            .. filtered
            .OrderByDescending(isFeatured)
            .ThenBy(x => x.EventDateTo <= currentDateTimeUtc)
            .ThenBy(x => x.EventDateTo <= currentDateTimeUtc
                ? DateTime.MaxValue.Ticks - x.EventDateTo.Ticks 
                : x.EventDateTo.Ticks)                         
        ];

        var paginated = PaginationHelper.Paginate(ordered, cardsQuery).ToList();
        var dtos = await GetEventWebinarCardDtosAsync(paginated, language, isFeatured, cancellationToken);

        return new PaginationDto<EventsWebinarsCardDto>
        {
            Data = dtos,
            TotalCount = ordered.Length,
        };
    }



    public async Task<EventsWebinarsCardDto[]> GetEventWebinarCardDtosAsync(
        ICollection<IEventOrWebinar> eventsAndWebinars,
        string lang,
        Func<IEventOrWebinar, bool> isFeatured,
        CancellationToken cancellationToken)
    {
        var language = lang ?? _contentService.GetCurrentLanguage();

        var urlTasks = eventsAndWebinars.Select(async x =>
        {
            try
            {
                var url = await _webPageUrlRetriever.Retrieve(x, language, cancellationToken);
                var relativeUrl = ((RelativeLink)Link.Parse(url.RelativePath)).ToHtmlCompatibleString();
                return (Guid: x.SystemFields.WebPageItemGUID, RelativeUrl: (string?)relativeUrl);
            }
            catch
            {
                return (Guid: x.SystemFields.WebPageItemGUID, RelativeUrl: (string?)null);
            }
        });

        var urlResults = await Task.WhenAll(urlTasks);

        var urlByGuid = urlResults
            .Where(r => r.RelativeUrl is not null)
            .ToDictionary(r => r.Guid, r => r.RelativeUrl!);



        var dtos = eventsAndWebinars
            .Where(eventOrWebinar => urlByGuid.ContainsKey(eventOrWebinar.SystemFields.WebPageItemGUID))
            .Select(eventOrWebinar =>
            {
                
                var firstEventType = eventOrWebinar.EventType?.FirstOrDefault();
                var eventTypeName = firstEventType?.Name;

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
                        URL = urlByGuid[eventOrWebinar.SystemFields.WebPageItemGUID],
                    },
                };
            }).ToArray();

        return dtos;
    }
}