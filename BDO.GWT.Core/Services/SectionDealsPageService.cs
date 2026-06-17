using BDO.GWT.Common.Exceptions;
using BDO.GWT.Core;
using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Helper;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.Utilities;
using BDO.GWT.Core.ValueObjects;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Web.Core.LiveSite.Services;

public interface ISectionDealsPageService
{
    Task<SectionDealsDto> GetSectionDealsDtoAsync(CancellationToken cancellationToken);

    Task<PaginationDto<DealCardDto>> GetSectionDealsDealPaginationDtoAsync(Guid pageGuid, string language, CardsQuery cardsQuery, CancellationToken cancellationToken);
}

public class SectionDealsPageService : ISectionDealsPageService
{
    private readonly ICultureContext _cultureContext;
    private readonly IContentService _contentRetriever;
    private readonly IWebsiteChannelContext _websiteChannelContext;
    private readonly IDealCardsService _dealCardsService;
    private int currentDealPageId;

    public SectionDealsPageService(ICultureContext cultureContext, IContentService contentRetriever, IWebsiteChannelContext websiteChannelContext, IDealCardsService dealCardsService)
    {
        _cultureContext = cultureContext;
        _contentRetriever = contentRetriever;
        _websiteChannelContext = websiteChannelContext;
        _dealCardsService = dealCardsService;
    }

    public async Task<SectionDealsDto> GetSectionDealsDtoAsync(CancellationToken cancellationToken)
    {
        var results = await _contentRetriever.GetPagesAsync<SectionDeals>(
           new RetrievePagesParameters
           {
               LinkedItemsMaxLevel = 5
           },
           query => query
               .Where(w => w.WhereEquals("ContentItemChannelID", _websiteChannelContext.WebsiteChannelID))
               .TopN(1),
           RetrievalCacheSettings.CacheDisabled,
           cancellationToken);

        var page = results?.FirstOrDefault();
        if (page is null)
        {
            return new SectionDealsDto
            {
                Filters = new List<FilterItemDto<OptionDto>>(),
                PageSize = 12,
                ApiEndpoint = string.Empty
            };
        }

        var sectionDealsFilters = string.IsNullOrWhiteSpace(page.SectionDealsFilters)
            ? new HashSet<string>()
            : page.SectionDealsFilters
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .ToHashSet();

        HashSet<string> sectionDealsUpdatedFilters;
        if (LanguageTag.TryParse(_cultureContext.CurrentLanguageTag, out var currentLanguageTag)
            && currentLanguageTag.Language == LanguageTag.GB.Language)
        {
            sectionDealsUpdatedFilters = new HashSet<string>(sectionDealsFilters.Count);
            foreach (var filter in sectionDealsFilters)
            {
                switch (filter)
                {
                    case FilterParameterName.DealType:
                        sectionDealsUpdatedFilters.Add(FilterParameterName.DealType);
                        break;
                    case FilterParameterName.Industry:
                        sectionDealsUpdatedFilters.Add(FilterParameterName.Industry);
                        break;
                    case FilterParameterName.SubIndustry:
                        sectionDealsUpdatedFilters.Add(FilterParameterName.SubIndustry);
                        break;
                    case FilterParameterName.Year:
                        sectionDealsUpdatedFilters.Add(FilterParameterName.Year);
                        break;
                    default:
                        sectionDealsUpdatedFilters.Add(filter);
                        break;
                }
            }
        }
        else
        {
            sectionDealsUpdatedFilters = sectionDealsFilters;
        }

        var dto = new SectionDealsDto
        {
            Filters = page.SectionDealsFilteringEnabled
                ? await CreateDealsFiltersAsync(sectionDealsUpdatedFilters, page.SystemFields.WebPageItemID, null, cancellationToken)
                : new List<FilterItemDto<OptionDto>>(),
            PageSize = 12,
            ApiEndpoint = $"Deals/{page.SystemFields.ContentItemGUID}/Get"
        };

        return dto;
    }

    public async Task<PaginationDto<DealCardDto>> GetSectionDealsDealPaginationDtoAsync(Guid pageGuid, string language, CardsQuery cardsQuery, CancellationToken cancellationToken)
    {
        var sectionDeal = (await _contentRetriever.GetPagesAsync<SectionDeals>(
            new RetrievePagesParameters
            {
                LanguageName = language,
                UseLanguageFallbacks = false
            },
           query => query
               .OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder))
               .Where(w => w.WhereEquals("ContentItemGUID", pageGuid)),
           RetrievalCacheSettings.CacheDisabled,
           cancellationToken
       )).FirstOrDefault();

        if (sectionDeal is null)
        {
            throw new MissingSectionPageException($"'{nameof(SectionDeals)} page with GUID '{pageGuid}' not found.");
        }

        var paginationDto = await _dealCardsService.GetFilteredItemsAsync(sectionDeal, language, cardsQuery, null, _ => false, cancellationToken);

        return paginationDto;
    }

    private async Task<FilterItemDto<OptionDto>[]> CreateDealsFiltersAsync(HashSet<string> filterParameterNames, int pageId, string? nodeAliasPath, CancellationToken cancellationToken)
    {
        currentDealPageId = pageId;
        var filterItems = new List<FilterItemDto<OptionDto>>(filterParameterNames.Count);

        foreach (var filterParameterName in filterParameterNames)
        {
            var filterParameterData = FilterUtility.GetFilterParameterData(filterParameterName);

            if (filterParameterData is null)
            {
                continue;
            }

            var options = filterParameterData.FilterParameterType switch
            {
                FilterParameterType.Page => await BuildOptionsForPageFilterAsync(filterParameterData, cancellationToken),
                FilterParameterType.Tagpage => await BuildOptionsForTagPageFilterAsync(filterParameterData, cancellationToken),

                // FilterParameterType.Entity => await BuildOptionsForEntityFilterAsync(filterParameterData, cancellationToken),
                FilterParameterType.Field => await BuildOptionsForFieldFilterAsync(filterParameterData, cancellationToken),
                //FilterParameterType.Taxonomy => await BuildOptionsForTaxonomyFilterAsync(filterParameterData, cancellationToken),
                _ => throw new NotSupportedException($"Filter parameter type {filterParameterData.FilterParameterType} is not supported."),
            };

            var filterItem = FilterDtoUtility.CreateFilterItemDto(FilterItemType.Select, filterParameterName, filterParameterData.LiveSiteDisplayName, null, options);

            filterItems.Add(filterItem);
        }

        return filterItems.ToArray();
    }

    private async Task<OptionDto[]> BuildOptionsForPageFilterAsync(FilterParameterData filterParameterData, CancellationToken cancellationToken)
    {
        const string rootPath = "/";
        const int linkedItemsMaxLevel = 5;

        var entityType = filterParameterData.EntityType;

        if (entityType == typeof(DealCardDealType))
        {
            return await GetSystemFieldNameOptionsAsync<DealCardDealType>(rootPath, linkedItemsMaxLevel, cancellationToken);
        }

        if (entityType == typeof(DealCardRange))
        {
            return await GetSystemFieldNameOptionsAsync<DealCardRange>(rootPath, linkedItemsMaxLevel, cancellationToken);
        }

        if (entityType == typeof(DealCardIndustry))
        {
            return await GetSystemFieldNameOptionsAsync<DealCardIndustry>(rootPath, linkedItemsMaxLevel, cancellationToken);
        }

        if (entityType == typeof(DealCardSubIndustry))
        {
            return await GetSystemFieldNameOptionsAsync<DealCardSubIndustry>(rootPath, linkedItemsMaxLevel, cancellationToken);
        }

        // Generic fallback: any other content type wired through FilterParameterType.Page
        // (e.g. DealCardCommon, BusinessLine, Person, etc.). Uses cached PropertyInfo to avoid
        // repeated reflection lookups per item.
        var fallbackItems = await _contentRetriever.GetItemsByTypesAsync(
        new[] { entityType },
        _websiteChannelContext.WebsiteChannelID,
        cancellationToken);

        return ProjectPageItemsToOptions(entityType, fallbackItems);
    }

    private async Task<OptionDto[]> BuildOptionsForTagPageFilterAsync(FilterParameterData filterParameterData, CancellationToken cancellationToken)
    {
        var entityType = filterParameterData.EntityType;

        var items = await _contentRetriever.GetItemsByTypesAsync(
            new[] { entityType },
            _websiteChannelContext.WebsiteChannelID,
            cancellationToken);

        if (items is null || items.Count == 0)
        {
            return Array.Empty<OptionDto>();
        }

        var nameAccessor = ItemPropertyMapper.GetAccessors(entityType).GetName;
        var buffer = new List<OptionDto>(items.Count);
        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var name = nameAccessor?.Invoke(item);
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            buffer.Add(new OptionDto
            {
                Value = name,
                Label = name
            });
        }

        buffer.Sort(static (a, b) => string.CompareOrdinal(a.Label, b.Label));
        return buffer.ToArray();
    }

    private async Task<OptionDto[]> BuildOptionsForFieldFilterAsync(FilterParameterData filterParameterData, CancellationToken cancellationToken)
    {
        var entityType = filterParameterData.EntityType;

        var items = await _contentRetriever.GetItemsByTypesAsync(
            new[] { entityType },
            _websiteChannelContext.WebsiteChannelID,
            cancellationToken);

        if (items is null || items.Count == 0)
        {
            return Array.Empty<OptionDto>();
        }

        if (entityType == typeof(BlogPost))
        {
            return items
                .OfType<BlogPost>()
                .Select(x => x.MetadataKeywordsBlog)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .SelectMany(x => KeywordsHelper.ParseKeywords(x!))
                .Distinct()
                .Select(x => FilterDtoUtility.CreateOptionDto(x, x))
                .ToArray();
        }

        return Array.Empty<OptionDto>();
    }

    private async Task<OptionDto[]> GetSystemFieldNameOptionsAsync<TPage>(
        string rootPath,
        int linkedItemsMaxLevel,
        CancellationToken cancellationToken)
        where TPage : class, IWebPageFieldsSource
    {
        var items = await _contentRetriever.GetPagesAsync<TPage>(
            new RetrievePagesParameters
            {
                LinkedItemsMaxLevel = linkedItemsMaxLevel,
                PathMatch = PathMatch.Section(rootPath)
            },
            query => query.OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder)),
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken);

        return ProjectByContentItemName(items);
    }

    private static OptionDto[] ProjectByContentItemName<TPage>(IEnumerable<TPage> items)
        where TPage : IWebPageFieldsSource
    {
        var accessors = ItemPropertyMapper.GetAccessors<TPage>();
        var buffer = new List<OptionDto>();
        foreach (var item in items)
        {
            if (item is null)
            {
                continue;
            }

            var label = accessors.GetName?.Invoke(item)
                       ?? accessors.GetContentItemName?.Invoke(item);

            var value = accessors.GetCodeName?.Invoke(item)
                       ?? accessors.GetContentItemName?.Invoke(item);
            if (string.IsNullOrWhiteSpace(label))
            {
                continue;
            }

            buffer.Add(new OptionDto
            {
                Value = value,
                Label = label
            });
        }

        buffer.Sort(static (a, b) => string.CompareOrdinal(a.Label, b.Label));
        return buffer.ToArray();
    }

    private static OptionDto[] ProjectPageItemsToOptions(Type entityType, IReadOnlyList<object> items)
    {
        if (items.Count == 0)
        {
            return Array.Empty<OptionDto>();
        }

        var accessors = ItemPropertyMapper.GetAccessors(entityType);
        var buffer = new List<OptionDto>(items.Count);

        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var codeName = accessors.GetCodeName?.Invoke(item);
            if (string.IsNullOrWhiteSpace(codeName))
            {
                continue;
            }

            var label = accessors.GetName?.Invoke(item)
                       ?? accessors.GetContentItemName?.Invoke(item);

            buffer.Add(new OptionDto
            {
                Value = codeName,
                Label = label
            });
        }

        buffer.Sort(static (a, b) => string.CompareOrdinal(a.Label, b.Label));
        return buffer.ToArray();
    }
}
