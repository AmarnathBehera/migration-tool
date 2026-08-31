using BDO.GWT.AzureCognitiveSearch.Services;
using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Entities.Search;
using BDO.GWT.Core.Extensions;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.Utilities;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Helpers;
using CMS.Websites.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using CMS.Core;

namespace BDO.GWT.Core.Services
{
    public interface ISearchPageService
    {
        Task<SearchResultDto> GetSearchResultsAsync(SearchResultQuery query, CancellationToken cancellationToken);

        Task<SearchPageDto> GetSearchPageDtoAsync(string searchText, CancellationToken cancellationToken);
    }
    public class SearchPageService(
        ICultureContext cultureContext,
        //IContentService contentService,
        //IUrlHelperFactory urlHelperFactory,
        //IHttpContextAccessor httpContextAccessor,
        //IWebsiteChannelContext websiteChannelContext,
        //IWebPageUrlResolver urlResolver,
        ITypeMapper typeMapper,
        ISearchService searchService,
        //IDateTimeProviderSearch dateTimeProvider,
        ISearchCardHelper searchCardHelper,
        IEventLogService eventLogService

            ) : ISearchPageService
    {
        #region Page type group keys

        private const string InsightsPageGroupKey = "Insights";
        private const string PeoplePageGroupKey = "People";
        private const string IndustriesPageGroupKey = "Industries";
        private const string ServicesPageGroupKey = "Services";
        private const string BlogsPageGroupKey = "Blogs";
        private const string NewsPageGroupKey = "News";
        private const string SpecialtiesPageGroupKey = "Specialties";
        private const string EventsWebinarsPageGroupKey = "EventsWebinars";
        private const string DealsPageGroupKey = "Deals";

        #endregion
        #region Filtering constants

        private const string FilterByFilterName = "filterBy";
        private const string SortByFilterName = "sortBy";
        private const string TopResultsOptionValue = "topResults";
        private const string MostRecentOptionValue = "mostRecent";

        #endregion

        #region Dictionaries

        private static readonly Dictionary<string, string> ResourceStringByPageTypeGroupKey = new()
        {
            [InsightsPageGroupKey] = ResourceStringKey.LiveSite.SearchPage.Insights,
            [PeoplePageGroupKey] = ResourceStringKey.LiveSite.SearchPage.People,
            [IndustriesPageGroupKey] = ResourceStringKey.LiveSite.SearchPage.Industries,
            [ServicesPageGroupKey] = ResourceStringKey.LiveSite.SearchPage.Services,
            [BlogsPageGroupKey] = ResourceStringKey.LiveSite.SearchPage.Blogs,
            [NewsPageGroupKey] = ResourceStringKey.LiveSite.SearchPage.News,
            [SpecialtiesPageGroupKey] = ResourceStringKey.LiveSite.SearchPage.Specialties,
            [EventsWebinarsPageGroupKey] = ResourceStringKey.LiveSite.SearchPage.EventsWebinars,
            [DealsPageGroupKey] = ResourceStringKey.LiveSite.SearchPage.Deals,
        };

        private static readonly Dictionary<string, IEnumerable<Type>> PageTypesByPageGroupKey = new()
        {
            [InsightsPageGroupKey] = new[]
            {
            typeof(Insight),
        },
            [PeoplePageGroupKey] = new[]
            {
            typeof(Person),
        },
            [IndustriesPageGroupKey] = new[]
            {
            typeof(IndustryService), typeof(Industry), typeof(IndustryCategory),
        },
            [ServicesPageGroupKey] = new[]
            {
            typeof(BusinessLine), typeof(ServiceArea), typeof(Service), typeof(Sub_Service),
        },
            [BlogsPageGroupKey] = new[]
            {
            typeof(BlogPost),
        },
            [NewsPageGroupKey] = new[]
            {
            typeof(NewsArticle),
        },
            [SpecialtiesPageGroupKey] = new[]
            {
            typeof(SpecialtiesCategory), typeof(SpecialtiesArea), typeof(SpecialtiesPage),
        },
            [EventsWebinarsPageGroupKey] = new[]
            {
            typeof(Event), typeof(Webinar),
        },
            [DealsPageGroupKey] = new[]
            {
            typeof(Deal), typeof(RemoteDeal),
        },
        };

        private static readonly Dictionary<Type, string> PageTypeGroupKeyByPageType = PageTypesByPageGroupKey.InvertKeyAndListValueDictionary();

        #endregion

        private readonly ICultureContext _cultureContext = cultureContext;               
        private readonly ISearchService _searchService = searchService;
        private readonly ITypeMapper _typeMapper = typeMapper;
        
        private readonly ISearchCardHelper _searchCardHelper = searchCardHelper;
        public async Task<SearchResultDto> GetSearchResultsAsync(SearchResultQuery query, CancellationToken cancellationToken)
        {
            var searchText = query.SearchText ?? string.Empty;
            var currentPage = query.CurrentPage ?? 1;
            var pageSize = query.PageSize ?? 12;

            var response = await _searchService.SearchAsync<SearchDocument>(searchText, q =>
            {
                q.IncludeTotalCount();

                //q.OnlyPublishedPages(_dateTimeProvider);
                q.Where(x => x.LanguageTag == _cultureContext.CurrentLanguageTag);

                if (query.SortBy == MostRecentOptionValue)
                {
                    q.OrderBy(x => x.Descending(nameof(SearchDocument.SortDate)));
                }

                if (!string.IsNullOrEmpty(query.FilterBy) && PageTypesByPageGroupKey.TryGetValue(query.FilterBy, out var pageTypes))
                {
                    var classNames = pageTypes.Select(x => _typeMapper.GetClassNameFromPageType(x)).ToList();
                    q.Where(x => classNames.Contains(x.ClassName));
                }

                q.Skip(PaginationHelper.CalculateSkip(currentPage, pageSize));
                q.Take(pageSize);
            }, cancellationToken);

            var currentLanguageTag = _cultureContext.CurrentLanguageTag;
            // TEMP DEBUG - remove after diagnosis
            eventLogService.LogInformation("SearchPageService", "GetSearchResultsAsync",
                $"CurrentLanguageTag (query side): '{currentLanguageTag}'");

            var existingDocuments = response.Documents.ToList();

            var totalItems = (int)response.Count!;
            var totalPages = PaginationHelper.CalculateTotalPages(totalItems, pageSize);

            var cards = await _searchCardHelper.GetSearchCardDtos(existingDocuments, cancellationToken);
            var filters = await GetFiltersAsync(searchText, cancellationToken);

            var dto = new SearchResultDto
            {
                TotalPages = totalPages,
                TotalCount = totalItems,
                Data = cards,
                Filters = filters,
            };

            return dto;
        }

        public async Task<SearchPageDto> GetSearchPageDtoAsync(string searchText, CancellationToken cancellationToken)
        {
            var dto = new SearchPageDto
            {
                InitialSearchText = searchText,
                Filters = await GetFiltersAsync(searchText, cancellationToken),
                SortBy = GetSortByFilter(),
            };

            

            return dto;
        }

        private async Task<FilterItemDto<SearchFacetOptionDto>> GetFiltersAsync(string searchText, CancellationToken cancellationToken)
        {
            var searchResponse = await _searchService.SearchAsync<SearchDocument>(searchText, q =>
            {
                q.IncludeFacet(x => x.ClassName, 100);
                //q.OnlyPublishedPages(_dateTimeProvider);
                q.Where(x => x.LanguageTag == _cultureContext.CurrentLanguageTag);
                q.Take(0);
            }, cancellationToken);

            var pageGroupKeys = PageTypesByPageGroupKey.Keys;

            var facetsByClassName = searchResponse.Facets[nameof(SearchDocument.ClassName)]
                .Where(x => x is { Value: string })
                .GroupBy(x => GetPageTypeGroupKeyByClassName((string)x.Value!));

            var joinedData = pageGroupKeys.GroupJoin(
                facetsByClassName,
                pageGroupKey => pageGroupKey,
                facetGroup => facetGroup.Key,
                (pageGroupKey, facetGroup) => (PageGroupKey: pageGroupKey, FacetGroup: facetGroup.DefaultIfEmpty())
            );

            var searchFilterDtos = new SearchFacetOptionDto[pageGroupKeys.Count];
            var index = 0;

            foreach (var (pageGroupKey, facetGroup) in joinedData)
            {
                searchFilterDtos[index] = new SearchFacetOptionDto
                {
                    Label = ResourceStringByPageTypeGroupKey[pageGroupKey],
                    Value = pageGroupKey,
                    Count = SumFacetCounts(facetGroup),
                };

                index++;
            }

            var filterItemDto = FilterDtoUtility.CreateFilterItemDto(FilterItemType.Select, FilterByFilterName, ResourceStringKey.LiveSite.FilterBy, null, searchFilterDtos);

            return filterItemDto;
        }

        private static FilterItemDto<OptionDto> GetSortByFilter()
        {
            var sortByFilterDtos = new[]
            {
            FilterDtoUtility.CreateOptionDto(ResourceStringKey.LiveSite.SearchPage.TopResults, TopResultsOptionValue),
            FilterDtoUtility.CreateOptionDto(ResourceStringKey.LiveSite.SearchPage.MostRecent, MostRecentOptionValue),
        };

            var filterItemDto = FilterDtoUtility.CreateFilterItemDto(FilterItemType.Select, SortByFilterName, ResourceStringKey.LiveSite.SortBy, TopResultsOptionValue, sortByFilterDtos);

            return filterItemDto;
        }

        private string? GetPageTypeGroupKeyByClassName(string? className)
        {
            return PageTypeGroupKeyByPageType.TryGetValue(_typeMapper.GetTypeFromPageClassNameInvariant(className ?? string.Empty), out var pageTypeGroupKey) ? pageTypeGroupKey : null;
        }

        private static long SumFacetCounts(IEnumerable<IGrouping<string?, SearchFacet>?> facetGroups)
        {
            long total = 0;

            foreach (var group in facetGroups)
            {
                if (group is null)
                {
                    continue;
                }

                foreach (var facet in group)
                {
                    total += facet.Count ?? 0;
                }
            }

            return total;
        }
    }
        
}
