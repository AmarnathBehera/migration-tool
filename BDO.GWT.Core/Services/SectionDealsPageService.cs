using BDO.GWT.Common.Exceptions;
using BDO.GWT.Core;
using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Helper;
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
           query =>
           {
               query.Where(w =>
                   w.WhereEquals("ContentItemChannelID", _websiteChannelContext.WebsiteChannelID));
           },
           RetrievalCacheSettings.CacheDisabled,
           cancellationToken);

        var page = results?.FirstOrDefault();

        var sectionDealsUpdatedFilters = new HashSet<string>();
        var sectionDealsFilters = string.IsNullOrWhiteSpace(page.SectionDealsFilters)
            ? new HashSet<string>()
               : page.SectionDealsFilters
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .ToHashSet();

        if (LanguageTag.TryParse(_cultureContext.CurrentLanguageTag, out var currentLanguageTag)
            && currentLanguageTag.Language == LanguageTag.GB.Language)
        {
            foreach (var filter in sectionDealsFilters)
            {
                switch (filter)
                {
                    case FilterParameterName.DealType:
                        sectionDealsUpdatedFilters.Add(FilterParameterName.GlobalDealType);
                        break;
                    case FilterParameterName.Industry:
                        sectionDealsUpdatedFilters.Add(FilterParameterName.GlobalIndustry);
                        break;
                    case FilterParameterName.SubIndustry:
                        sectionDealsUpdatedFilters.Add(FilterParameterName.GlobalSubIndustry);
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
            Filters = page.SectionDealsFilteringEnabled ? await CreateDealsFiltersAsync(sectionDealsUpdatedFilters, page.SystemFields.WebPageItemID , null, cancellationToken) : new List<FilterItemDto<OptionDto>>(),
            PageSize = 12,
            ApiEndpoint = $"Deals/{page.SystemFields.ContentItemGUID}" +
            $"/Get",
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
        var filterItems = new List<FilterItemDto<OptionDto>>();

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
        var rootPath = "BDO_Deal_Cards";
        var level = 1;

        if (filterParameterData.AdminDisplayName.ToLower() == "subindustry")
        {
            level = 5;
        }

        var entityType = filterParameterData.EntityType;
        if (entityType.Name == "DealCardCommon")
        {
          var items = await _contentRetriever.GetPagesAsync<DealCardCommon>(new RetrievePagesParameters { LinkedItemsMaxLevel = level,
              PathMatch = PathMatch.Section(rootPath)
          },
            query => query
                .OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder)),
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken);
          var options = items
          .Select(x =>
          {
              var type = x.GetType();

              var metadataTitle =
                  type.GetProperty("MetadataTitle")?.GetValue(x) as string;

              var metadataOgTitle =
                  type.GetProperty("MetadataOGTitle")?.GetValue(x) as string;

              var systemFields =
                  type.GetProperty("SystemFields")?.GetValue(x);

              var contentItemName =
                  systemFields?.GetType()
                      .GetProperty("ContentItemName")
                      ?.GetValue(systemFields) as string;

              var name = metadataTitle ?? metadataOgTitle ?? contentItemName;

              return new OptionDto
              {
                  Value = name,
                  Label = name
              };
          })
          .Where(x => !string.IsNullOrWhiteSpace(x.Value))
          .OrderBy(x => x.Label)
          .ToArray();


            return options;
        }
        else
        {
            return Array.Empty<OptionDto>();
        }
    }

    private async Task<OptionDto[]> BuildOptionsForTagPageFilterAsync(FilterParameterData filterParameterData, CancellationToken cancellationToken)
    {
        var entityType = filterParameterData.EntityType;

        var items = await _contentRetriever.GetItemsByTypesAsync(
            new[] { entityType },
            _websiteChannelContext.WebsiteChannelID,
            cancellationToken);

        var options = items
                    .Select(x =>
                    {
                        var type = x.GetType();

                        var tagNameValue = type.GetProperty("Name")?.GetValue(x);

                        string name = tagNameValue is string s ? s : string.Empty;


                        return new OptionDto
                        {
                            Value = name,
                            Label = name


                        };
                    })
                          .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                          .OrderBy(x => x.Label)
                          .ToArray();

        return options;
    }

    private async Task<OptionDto[]> BuildOptionsForFieldFilterAsync(FilterParameterData filterParameterData, CancellationToken cancellationToken)
    {
        var entityType = filterParameterData.EntityType;
        var options = Array.Empty<OptionDto>();

        var items = await _contentRetriever.GetItemsByTypesAsync(
            new[] { entityType },
            _websiteChannelContext.WebsiteChannelID,
            cancellationToken);

        if (items is null || !items.Any())
        {
            return options;
        }

        switch (filterParameterData.EntityType)
        {
            case { } type when type == typeof(BlogPost):
                options = items
                    .Select(x => x.GetType().GetProperty(nameof(BlogPost.MetadataKeywordsBlog))?.GetValue(x) as string)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .SelectMany(x => KeywordsHelper.ParseKeywords(x!))
                    .Distinct()
                    .Select(x => FilterDtoUtility.CreateOptionDto(x, x))
                    .ToArray();
                break;
        }

        return options;
    }

}
