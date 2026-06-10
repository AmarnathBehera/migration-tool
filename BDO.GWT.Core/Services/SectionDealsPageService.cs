using BDO.GWT.Common.Exceptions;
using BDO.GWT.Core;
using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.Utilities;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Core.DTOs.Pages;
using BDO.GWT.Web.Core.Helpers;
using BDO.GWT.Web.Core.Services;
using CMS.ContentEngine;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Web.Core.LiveSite.Services;

public interface ISectionDealsPageService
{
    Task<SectionDealsDto> GetSectionDealsDtoAsync(SectionDeals page, CancellationToken cancellationToken);

    Task<PaginationDto<DealCardDto>> GetSectionDealsDealPaginationDtoAsync(Guid pageGuid, CardsQuery cardsQuery, CancellationToken cancellationToken);
}

public class SectionDealsPageService : ISectionDealsPageService
{
    private readonly ICultureContext _cultureContext;
    private readonly IContentService _contentRetriever;
    private readonly IWebsiteChannelContext _websiteChannelContext;
    private readonly ICardsService _cardsService;

    public SectionDealsPageService(ICultureContext cultureContext, IContentService contentRetriever, IWebsiteChannelContext websiteChannelContext, ICardsService cardsService)
    {
        _cultureContext = cultureContext;
        _contentRetriever = contentRetriever;
        _websiteChannelContext = websiteChannelContext;
        _cardsService = cardsService;
    }

    public async Task<SectionDealsDto> GetSectionDealsDtoAsync(SectionDeals page, CancellationToken cancellationToken)
    {
        var sectionDealsUpdatedFilters = new HashSet<string>();

        if (_cultureContext.CurrentLanguageTag.Language == Language.English)
        {
            foreach (var filter in page.SectionDealsFilters)
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
            sectionDealsUpdatedFilters = page.SectionDealsFilters;
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

        var paginationDto = await _dealsCardService.GetFilteredItemsAsync(sectionDeal, language, cardsQuery, null, _ => false, cancellationToken);

        return paginationDto;
    }
}
