using BDO.GWT.Common.Exceptions;
using BDO.GWT.Core;
using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Services;
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
    private readonly ICardsService _cardsService;
    private readonly IDealCardsService _dealCardsService;

    public SectionDealsPageService(ICultureContext cultureContext, IContentService contentRetriever, IWebsiteChannelContext websiteChannelContext, ICardsService cardsService, IDealCardsService dealCardsService)
    {
        _cultureContext = cultureContext;
        _contentRetriever = contentRetriever;
        _websiteChannelContext = websiteChannelContext;
        _cardsService = cardsService;
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
            Filters = page.SectionDealsFilteringEnabled ? await _cardsService.CreateFiltersAsync(sectionDealsUpdatedFilters, null, cancellationToken) : new List<FilterItemDto<OptionDto>>(),
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
}
