//using BDO.GWT.Core;
//using BDO.GWT.Core.Constants;
//using BDO.GWT.Core.Data;
//using BDO.GWT.Core.Data.Queries;
//using BDO.GWT.Core.DTOs;
//using BDO.GWT.Core.Services;
//using BDO.GWT.Core.Utilities;
//using BDO.GWT.Domain.Core.Entities;
//using BDO.GWT.Web.Core.DTOs.Cards;
//using BDO.GWT.Web.Core.Helpers;
//using BDO.GWT.Xperience.Core.Retrievers;
//using BDO.GWT.Xperience.Core.Services;
//using CMS.ContentEngine;
//using CMS.EventLog;
//using System.Linq.Expressions;

//namespace BDO.GWT.Web.Core.Services;

//public interface IEventWebinarCardsService
//{
//    Task<EventsWebinarsCardDto[]> GetEventWebinarCardDtosAsync(ICollection<IEventOrWebinar> eventsAndWebinars, Func<IEventOrWebinar, bool> isFeatured, CancellationToken cancellationToken);

//    Task<PaginationDto<EventsWebinarsCardDto>> GetFilteredItemsAsync(CardsQuery cardsQuery, Action<ContentTypeQueryParameters>? decorateQuery, Func<IEventOrWebinar, bool> isFeatured, bool showPastPages, bool filterOnEventToDate, CancellationToken cancellationToken);
//}

//public class EventWebinarCardsService : IEventWebinarCardsService, IPageCardsService<IEventOrWebinar, EventsWebinarsCardDto>
//{
//    private readonly ICardsService _cardsService;
//    private readonly IChildPageHelper _childPageHelper;
//    private readonly ICultureContext _cultureContext;
//    private readonly IMultiTypePageRetriever _multiTypePageRetriever;
//    private readonly IPageUrlService _pageUrlService;
//    private readonly IContentService _contentService;
//    private readonly ITimeZoneService _timeZoneService;

//    public EventWebinarCardsService(ICardsService cardsService, IChildPageHelper childPageHelper, ICultureContext cultureContext, IMultiTypePageRetriever multiTypePageRetriever, IPageUrlService pageUrlService, IContentService _contentService, ITimeZoneService timeZoneService)
//    {
//        _cardsService = cardsService;
//        _childPageHelper = childPageHelper;
//        _cultureContext = cultureContext;
//        _multiTypePageRetriever = multiTypePageRetriever;
//        _pageUrlService = pageUrlService;
//        _contentService = _contentService;
//        _timeZoneService = timeZoneService;
//    }

//    public void AddFilterQueryDecorator<TPageFilter>(Action<IMultiTypePageQuery<Page>> decorateFilterQuery)
//        where TPageFilter : Page
//    {
//        _cardsService.AddFilterQueryDecorator<TPageFilter>(decorateFilterQuery);
//    }

//    public async Task<FilterItemDto<OptionDto>[]> CreateFiltersAsync(HashSet<string> filters, string? nodeAliasPath, CancellationToken cancellationToken)
//    {
//        return await _cardsService.CreateFiltersAsync(filters, nodeAliasPath, cancellationToken);
//    }

//    public async Task<PaginationDto<EventsWebinarsCardDto>> GetFilteredItemsAsync(CardsQuery cardsQuery, Action<IPageQuery<IEventOrWebinar>>? decorateQuery, Func<IEventOrWebinar, bool> isFeatured, CancellationToken cancellationToken)
//    {
//        return await GetFilteredItemsAsync(cardsQuery, decorateQuery, isFeatured, false, false, cancellationToken);
//    }

//    public async Task<PaginationDto<EventsWebinarsCardDto>> GetFilteredItemsAsync(CardsQuery cardsQuery, Action<IPageQuery<IEventOrWebinar>>? decorateQuery, Func<IEventOrWebinar, bool> isFeatured, bool showPastPages, bool filterOnEventToDate, CancellationToken cancellationToken)
//    {
//        var filterWhereExpressions = await GetFilterParameterWhereExpressionsAsync(cardsQuery.Filters);

//        var eventsAndWebinars = (await _multiTypePageRetriever.RetrievePagesAsync<IEventOrWebinar>(query =>
//        {
//            ApplyCommonFilter(query);
//            foreach (var whereExpression in filterWhereExpressions)
//            {
//                query.Where(whereExpression);
//            }

//            decorateQuery?.Invoke(query.PageQuery);
//            query.Select(x => new
//            {
//                x.EventDateFrom,
//                x.EventDateTo,
//                x.EventLocationCity,
//                x.EventType,
//                x.ID,
//                x.MetadataDescription,
//                x.MetadataTitle,
//                x.NodeGUID,
//                x.NodeID,
//                x.ShowPastEvent,
//            });
//        }, cancellationToken))
//            .ToArray();

//        var currentDateTime = DateTime.UtcNow;

//        IEventOrWebinar[] orderedEventsAndWebinars =
//        [
//            .. eventsAndWebinars.OrderByDescending(isFeatured)
//                        .ThenBy(order => order.EventDateTo <= currentDateTime)
//                        .ThenBy(order => order.EventDateTo <= currentDateTime
//                          ? -order.EventDateTo.Ticks
//                          : order.EventDateTo.Ticks)
//        ];

//        var paginatedEventsWebinars = PaginationHelper.PaginatePages(orderedEventsAndWebinars, cardsQuery)
//            .ToList();

//        var dtos = await GetEventWebinarCardDtosAsync(paginatedEventsWebinars, isFeatured, cancellationToken);

//        var paginationDto = new PaginationDto<EventsWebinarsCardDto>
//        {
//            Data = dtos,
//            TotalCount = orderedEventsAndWebinars.Length,
//        };

//        return paginationDto;
//    }

//    private static void ApplyCommonFilter(IMultiTypePageQuery<IEventOrWebinar> multiTypePageQuery)
//    {
//        multiTypePageQuery
//            // TODO: Change to page.IsSearchExcluded != true when we implement IS NOT NULL check for DB fields that can be null
//            .Where(x => (x.IsSearchExcluded == false || x.IsSearchExcluded == null) && !x.EventIsPrivate);
//    }

//    private async Task<List<Expression<Func<IEventOrWebinar, bool>>>> GetFilterParameterWhereExpressionsAsync(IDictionary<string, string> filterParameterValueByName)
//    {
//        var whereExpressions = new List<Expression<Func<IEventOrWebinar, bool>>>();

//        if (filterParameterValueByName.TryGetValue(FilterParameterName.ShowAllPastEvents, out var showAllPastEvents))
//        {
//            if (showAllPastEvents != bool.TrueString)
//            {
//                if (filterParameterValueByName.TryGetValue(FilterParameterName.ShowOngoingEvents, out var showOngoingEvents) && showOngoingEvents == bool.TrueString)
//                {
//                    whereExpressions.Add(x => x.ShowPastEvent || x.EventDateTo > DateTime.Now);
//                }
//                else
//                {
//                    whereExpressions.Add(x => x.ShowPastEvent || x.EventDateFrom > DateTime.Now);
//                }
//            }
//        }
//        else
//        {
//            whereExpressions.Add(x => x.EventDateFrom > DateTime.Now);
//        }

//        foreach (var (filterParameterName, filterParameterValue) in filterParameterValueByName)
//        {
//            var filterParameterData = FilterUtility.GetFilterParameterData(filterParameterName);

//            if (filterParameterData is null)
//            {
//                continue;
//            }

//            if (filterParameterData.FilterParameterType == FilterParameterType.Page)
//            {
//                var page = await _multiTypePageRetriever.RetrievePageAsync(query => query
//                    .Types(filterParameterData.EntityType)
//                    .Where(x => x.Name == filterParameterValue)
//                );

//                if (page is null)
//                {
//                    continue;
//                }

//                var whereExpression = filterParameterName switch
//                {
//                    FilterParameterName.BusinessLine => await BuildBusinessLineWhereExpression(page.NodeAliasPath),
//                    FilterParameterName.ServiceArea => await BuildServiceAreaWhereExpression(page.NodeAliasPath),
//                    FilterParameterName.IndustryCategory => await BuildIndustryCategoryWhereExpression(page.NodeAliasPath),
//                    FilterParameterName.SpecialtyCategory => await BuildSpecialtyCategoryWhereExpression(page.NodeAliasPath),
//                    FilterParameterName.MicrositeMasterPage => await BuildMicrositeMasterPageWhereExpression(page.NodeAliasPath),
//                    FilterParameterName.OfficeLocation => x => x.MetadataOfficeLocation.Contains(page.NodeID),
//                    _ => null,
//                };

//                if (whereExpression is not null)
//                {
//                    whereExpressions.Add(whereExpression);
//                }
//            }
//            else if (filterParameterData.FilterParameterType == FilterParameterType.Entity)
//            {
//                switch (filterParameterName)
//                {
//                    case FilterParameterName.BusinessIssue:
//                        {
//                            var entity = await _businessIssueRetriever.RetrieveEntityAsync(query => query.Where(x => x.CodeName == filterParameterValue));

//                            if (entity is not null)
//                            {
//                                whereExpressions.Add(x => x.MetadataBusinessIssues.Contains(entity.ID));
//                            }

//                            break;
//                        }

//                    case FilterParameterName.EventType:
//                        {
//                            var entity = await _eventTypeRetriever.RetrieveEntityAsync(query => query.Where(x => x.CodeName == filterParameterValue));

//                            if (entity is not null)
//                            {
//                                whereExpressions.Add(x => x.EventType.Contains(entity.ID));
//                            }

//                            break;
//                        }
//                }
//            }
//        }

//        return whereExpressions;
//    }

//    private async Task<Expression<Func<IEventOrWebinar, bool>>?> BuildBusinessLineWhereExpression(string nodeAliasPathPrefix)
//    {
//        var nodeIdsByPageType = await _childPageHelper.GetDescendantNodeIdsByPageType([
//            typeof(BusinessLine),
//            typeof(ServiceArea),
//            typeof(Service),
//        ], nodeAliasPathPrefix);

//        var businessLineNodeIds = PageTypeUtility.GetPageTypeNodeIds<BusinessLine>(nodeIdsByPageType);
//        var serviceAreaNodeIds = PageTypeUtility.GetPageTypeNodeIds<ServiceArea>(nodeIdsByPageType);
//        var serviceNodeIds = PageTypeUtility.GetPageTypeNodeIds<Service>(nodeIdsByPageType);

//        return x =>
//            x.MetadataBusinessLines.Intersect(businessLineNodeIds).Any() ||
//            x.MetadataServiceAreas.Intersect(serviceAreaNodeIds).Any() ||
//            x.MetadataServices.Intersect(serviceNodeIds).Any();
//    }

//    private async Task<Expression<Func<IEventOrWebinar, bool>>> BuildServiceAreaWhereExpression(string nodeAliasPathPrefix)
//    {
//        var nodeIdsByPageType = await _childPageHelper.GetDescendantNodeIdsByPageType([
//            typeof(ServiceArea),
//            typeof(Service),
//        ], nodeAliasPathPrefix);

//        var serviceAreaNodeIds = PageTypeUtility.GetPageTypeNodeIds<ServiceArea>(nodeIdsByPageType);
//        var serviceNodeIds = PageTypeUtility.GetPageTypeNodeIds<Service>(nodeIdsByPageType);

//        return x =>
//            x.MetadataServiceAreas.Intersect(serviceAreaNodeIds).Any() ||
//            x.MetadataServices.Intersect(serviceNodeIds).Any();
//    }

//    private async Task<Expression<Func<IEventOrWebinar, bool>>?> BuildIndustryCategoryWhereExpression(string nodeAliasPathPrefix)
//    {
//        var nodeIdsByPageType = await _childPageHelper.GetDescendantNodeIdsByPageType([
//            typeof(IndustryCategory),
//            typeof(Industry),
//        ], nodeAliasPathPrefix);

//        var industryCategoryNodeIds = PageTypeUtility.GetPageTypeNodeIds<IndustryCategory>(nodeIdsByPageType);
//        var industryNodeIds = PageTypeUtility.GetPageTypeNodeIds<Industry>(nodeIdsByPageType);

//        return x =>
//            x.MetadataIndustryCategories.Intersect(industryCategoryNodeIds).Any() ||
//            x.MetadataIndustries.Intersect(industryNodeIds).Any();
//    }

//    private async Task<Expression<Func<IEventOrWebinar, bool>>?> BuildSpecialtyCategoryWhereExpression(string nodeAliasPathPrefix)
//    {
//        var nodeIdsByPageType = await _childPageHelper.GetDescendantNodeIdsByPageType([
//            typeof(SpecialtiesCategory),
//            typeof(SpecialtiesArea),
//            typeof(SpecialtiesPage),
//        ], nodeAliasPathPrefix);

//        var specialtyCategoryNodeIds = PageTypeUtility.GetPageTypeNodeIds<SpecialtiesCategory>(nodeIdsByPageType);
//        var specialtyAreaNodeIds = PageTypeUtility.GetPageTypeNodeIds<SpecialtiesArea>(nodeIdsByPageType);
//        var specialtyNodeIds = PageTypeUtility.GetPageTypeNodeIds<SpecialtiesPage>(nodeIdsByPageType);

//        return x =>
//            x.MetadataSpecialtiesCategories.Intersect(specialtyCategoryNodeIds).Any() ||
//            x.MetadataSpecialtiesAreas.Intersect(specialtyAreaNodeIds).Any() ||
//            x.MetadataSpecialtiesPages.Intersect(specialtyNodeIds).Any();
//    }

//    private async Task<Expression<Func<IEventOrWebinar, bool>>?> BuildMicrositeMasterPageWhereExpression(string nodeAliasPathPrefix)
//    {
//        var nodeIdsByPageType = await _childPageHelper.GetDescendantNodeIdsByPageType([
//            typeof(MicrositeMasterPage),
//            typeof(MicrositePage),
//        ], nodeAliasPathPrefix);

//        var micrositeMasterPageNodeIds = PageTypeUtility.GetPageTypeNodeIds<MicrositeMasterPage>(nodeIdsByPageType);
//        var micrositePageNodeIds = PageTypeUtility.GetPageTypeNodeIds<MicrositePage>(nodeIdsByPageType);

//        return x =>
//            x.MicrositeMasterPages.Intersect(micrositeMasterPageNodeIds).Any() ||
//            x.MicrositePages.Intersect(micrositePageNodeIds).Any();
//    }

//    public async Task<EventsWebinarsCardDto[]> GetEventWebinarCardDtosAsync(ICollection<IEventOrWebinar> eventsAndWebinars, Func<IEventOrWebinar, bool> isFeatured, CancellationToken cancellationToken)
//    {
//        var eventTypeIds = eventsAndWebinars
//            .Select(eventOrWebinar => ExtractEventTypeId(eventOrWebinar.EventType))
//            .OfType<int>();

//        var eventTypeById = (await _eventTypeRetriever.RetrieveEntitiesAsync(query => query
//                .Where(eventType => eventType.LanguageTag == _cultureContext.CurrentLanguageTag && eventTypeIds.Contains(eventType.ID))
//            , cancellationToken)).ToDictionary(x => x.ID, x => x);

//        var pageUrlByNodeId = await _pageUrlService.GetPageRelativeUrlByNodeIdAsync(eventsAndWebinars.Select(x => x.NodeID), _cultureContext.CurrentLanguageTag, cancellationToken);

//        var dtos = eventsAndWebinars.Select(eventOrWebinar =>
//        {
//            EventType? eventType = null;
//            var eventTypeId = ExtractEventTypeId(eventOrWebinar.EventType);
//            if (eventTypeId.HasValue)
//            {
//                eventTypeById.TryGetValue(eventTypeId.Value, out eventType);
//            }

//            var dto = new EventsWebinarsCardDto
//            {
//                IsFeatured = isFeatured(eventOrWebinar),
//                Type = eventType?.Name,
//                DateTime = _timeZoneService.ConvertToSiteDateTime(eventOrWebinar.EventDateFrom),
//                DateTimeTo = (eventOrWebinar.EventDateFrom.Date != eventOrWebinar.EventDateTo.Date ? _timeZoneService.ConvertToSiteDateTime(eventOrWebinar.EventDateTo) : null),
//                Location = string.IsNullOrEmpty(eventOrWebinar.EventLocationCity) ? null : eventOrWebinar.EventLocationCity,
//                Title = eventOrWebinar.MetadataTitle,
//                Description = string.IsNullOrEmpty(eventOrWebinar.MetadataDescription) ? null : HtmlHelper.ExtractTextFromHtml(eventOrWebinar.MetadataDescription),
//                Link = new LinkItemDto
//                {
//                    Title = eventType?.CallToActionText ?? "",
//                    URL = pageUrlByNodeId[eventOrWebinar.NodeID].ToHtmlCompatibleString(),
//                },
//            };

//            return dto;
//        }).ToArray();

//        return dtos;
//    }

//    private static int? ExtractEventTypeId(IReadOnlyCollection<int> eventTypeIds)
//    {
//        return eventTypeIds.Count == 0 ? null : eventTypeIds.First();
//    }
//}
