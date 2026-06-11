using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Utilities;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;

namespace BDO.GWT.Core.Services
{
    public interface IDealCardsService
    {
        Task<List<FilterItemDto<OptionDto>>> CreateFiltersAsync(
            HashSet<string> filters,
            string? nodeAliasPath,
            CancellationToken cancellationToken);

        Task<PaginationDto<DealCardDto>> GetFilteredItemsAsync(
            SectionDeals page,
            string language,
            CardsQuery cardsQuery,
            Action<ContentItemQueryBuilder>? decorateQuery,
            Func<Deal, bool> isFeatured,
            CancellationToken cancellationToken);
    }

    public class DealCardsService : IDealCardsService
    {
        private readonly ICardsService _cardsService;
        private readonly IContentService _contentService;
        private readonly ISettingService _settingService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DealCardsService(
            ICardsService cardsService,
            IContentService contentService,
            ISettingService settingService,
            IUrlHelperFactory urlHelperFactory,
            IHttpContextAccessor httpContextAccessor
            )
        {
            _cardsService = cardsService;
            _contentService = contentService;
            _settingService = settingService;
            _urlHelperFactory = urlHelperFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<FilterItemDto<OptionDto>>> CreateFiltersAsync(
            HashSet<string> filters,
            string? nodeAliasPath,
            CancellationToken cancellationToken)
        {
            var result = await _cardsService.CreateFiltersAsync(filters, nodeAliasPath, cancellationToken);
            return result.ToList();
        }

        public async Task<PaginationDto<DealCardDto>> GetFilteredItemsAsync(
        SectionDeals page,
        string language,
        CardsQuery cardsQuery,
        Action<ContentItemQueryBuilder>? decorateQuery,
        Func<Deal, bool> isFeatured,
        CancellationToken cancellationToken)
        {
            var filterWhereExpressions = await GetFilterParameterWhereExpressionsAsync(cardsQuery.Filters);

            var order = cardsQuery.Order?.ToLower() ?? "asc";

            var deals = await _contentService.GetPagesAsync<Deal>(
                   new RetrievePagesParameters
                   {
                       LinkedItemsMaxLevel = 2,
                       LanguageName = language,
                       UseLanguageFallbacks = false
                   },
                query =>
                {
                    query.Where(w =>
                    {
                        w.WhereEquals("WebPageItemParentID", page.SystemFields.WebPageItemID);
                        foreach (var filter in filterWhereExpressions)
                        {
                            filter(w);
                        }
                    });
                    query.OrderBy(nameof(Deal.SystemFields.WebPageItemID));
                }, RetrievalCacheSettings.CacheDisabled,
                cancellationToken
            );
            var dealsList = deals?.OfType<Deal>().ToList() ?? new List<Deal>();

            var cardDtos = await CreatePeopleCardDtosAsync(dealsList);

            return new PaginationDto<DealCardDto>
            {
                Data = cardDtos.ToArray(),
                TotalCount = dealsList.Count
            };
        }
        private async Task<List<Action<WhereParameters>>> GetFilterParameterWhereExpressionsAsync(
         IDictionary<string, string> filterParameterValueByName)
        {
            var filters = new List<Action<WhereParameters>>();

            foreach (var (filterName, filterValue) in filterParameterValueByName)
            {
                var filterData = FilterUtility.GetFilterParameterData(filterName);

                if (filterData == null)
                    continue;

                var filter = filterData.FilterParameterType switch
                {
                    FilterParameterType.Page => await GetPageFilterAsync(filterName, filterValue),
                    _ => null,
                };

                if (filter is not null)
                {
                    filters.Add(filter);
                }
            }

            return filters;
        }

        private async Task<Action<WhereParameters>?> GetPageFilterAsync(string filterName, string filterValue)
        {
            // Year is a scalar filter (e.g. "2024"), not a content-item lookup.
            // Resolve it before the page query so the page-null short-circuit doesn't swallow it.
            if (filterName == FilterParameterName.Year)
            {
                if (!int.TryParse(filterValue, out var year))
                {
                    return null;
                }

                var startOfYear = new DateTime(year, 1, 1);
                var startOfNextYear = new DateTime(year + 1, 1, 1);

                return w => w
                    .WhereGreaterOrEquals(nameof(Deal.DateField), startOfYear)
                    .WhereLess(nameof(Deal.DateField), startOfNextYear);
            }

            var page = await _contentService.GetPageByTypeAndNameAsync(
                FilterUtility.GetFilterParameterData(filterName)!.EntityType, filterValue);

            if (page is null)
                return null;

            var guid = page.SystemFields.ContentItemGUID.ToString();

            return filterName switch
            {
                FilterParameterName.Industry => w => w.WhereContains(nameof(Deal.Industry), guid),
                FilterParameterName.SubIndustry => w => w.WhereContains(nameof(Deal.SubIndustry), guid),
                FilterParameterName.DealType => w => w.WhereContains(nameof(Deal.DealType), guid),
                _ => null,
            };
        }
        private async Task<List<DealCardDto>> CreatePeopleCardDtosAsync(List<Deal> deals)
        {
            var httpContext = _httpContextAccessor.HttpContext!;
            var actionContext = new ActionContext(httpContext, httpContext.GetRouteData() ?? new RouteData(), new ActionDescriptor());
            var urlHelper = _urlHelperFactory.GetUrlHelper(actionContext);

            var dtos = new List<DealCardDto>();
            foreach (var deal in deals)
            {
                dtos.Add(new DealCardDto
                {
                    Title = deal.MetadataTitle,
                    Description = deal.MetadataDescription,
                    Country = deal.DealCountry,
                    ImageURL = deal.MetadataTeaserImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                    ImageAltText = deal.MetadataTeaserImageAltText,
                    ReadMoreLink = deal.GetUrl().AbsoluteUrl
                });
            }

            return dtos;
        }

    }

}
