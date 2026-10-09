using BDO.GWT.Common.Exceptions;
using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.Utilities;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Core.Services;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Core.Services;

public interface ISectionPeoplePageService
{
    Task<SectionPeopleDto> GetSectionPeoplePageDtoAsync(SectionPeoplePage page, CancellationToken cancellationToken);

    Task<PaginationDto<PersonCardDto>> GetSectionPeoplePersonPaginationDtoAsync(Guid pageGuid, string language, CardsQuery cardsQuery, CancellationToken cancellationToken);
}

public class SectionPeoplePageService : ISectionPeoplePageService
{
    #region Filtering constants

    private const string OrderFilterName = "Order";
    private const string LastNameAtoZOptionValue = "asc";
    private const string LastNameZtoAOptionValue = "desc";
    private const string NodeOrderOptionValue = "nodeorder";

    #endregion

    private readonly IHeroService _heroService;
    private readonly IContentService _contentRetriever;
    private readonly IPeopleCardsService _peopleCardsService;
    private readonly IFilteringHelper _filteringHelper;
    private readonly IWebsiteChannelContext _websiteChannelContext;

    public SectionPeoplePageService(
        IContentService pageRetriever,
        IHeroService heroService,
        IPeopleCardsService peopleCardsService,
        IFilteringHelper filteringHelper,
        IWebsiteChannelContext websiteChannelContext
        )
    {
        _contentRetriever = pageRetriever;
        _heroService = heroService;
        _peopleCardsService = peopleCardsService;
        _filteringHelper = filteringHelper;
        _websiteChannelContext = websiteChannelContext;
    }

    public async Task<SectionPeopleDto> GetSectionPeoplePageDtoAsync(SectionPeoplePage page, CancellationToken cancellationToken)
    {
        var bgImageUrl = page.HeroBackgroundImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url;
        var mobileImageUrl = page.HeroBackgroundImage_Mobile?.FirstOrDefault()?.ContentAttachmentAsset?.Url;
        var ctaUrlPath = page.HeroCallToActionButtonUrl?.FirstOrDefault()?.SystemFields?.WebPageUrlPath;
        var filters = string.IsNullOrWhiteSpace(page.PeopleListFilters)
    ? new HashSet<string>()
    : page.PeopleListFilters
        .Split(';', StringSplitOptions.RemoveEmptyEntries)
        .Select(x => x.Trim())
        .ToHashSet();

        var heroData = new HeroData
        {
            Layout = page.HeroLayout,
            Title = page.HeroTitle,
            Description = page.HeroDescription,
            BackgroundImage = bgImageUrl != null ? Link.Parse(bgImageUrl) : null,
            BackgroundImageAltText = page.HeroBackgroundImageAltText,
            MobileBackgroundImage = mobileImageUrl != null ? Link.Parse(mobileImageUrl) : null,
            MobileBackgroundImageAltText = page.HeroBackgroundImage_MobileAltText,
            CallToActionButtonType = page.HeroCallToActionButtonType,
            CallToActionButtonText = page.HeroCallToActionButtonText,
            CallToActionButtonUrl = ctaUrlPath != null ? Link.Parse(ctaUrlPath) : null,
        };
        var dto = new SectionPeopleDto
        {
            Hero = await _heroService.GetHeroDtoAsync(heroData, cancellationToken),
            Filters = page.PeopleListFilteringEnabled ? await _peopleCardsService.CreateFiltersAsync(filters, null, cancellationToken) : null,
            FilteringTooltipEnabled = await _filteringHelper.IsFilteringTooltipEnabledAsync(page.PeopleListFilteringEnabled, cancellationToken),
            SortBy = page.PeopleListOrderingEnabled ? CreateOrderingDto(page.PeopleListDefaultOrder) : null,
            PageSize = 20,
            ApiEndpoint = $"People/{page.SystemFields.ContentItemGUID}/Get",
        };

        return dto;
    }

    public async Task<PaginationDto<PersonCardDto>> GetSectionPeoplePersonPaginationDtoAsync(Guid pageGuid, string language, CardsQuery cardsQuery, CancellationToken cancellationToken)
    {
        var isPreview = _websiteChannelContext.IsPreview;

        var sectionPeople = (await _contentRetriever.GetPagesAsync<SectionPeoplePage>(
            new RetrievePagesParameters
            {
                LanguageName = language,
                UseLanguageFallbacks = isPreview,
                IsForPreview = isPreview,
            },
            query => query
                .OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder))
                .Where(w => w
                    .WhereEquals("ContentItemGUID", pageGuid)
                    .Or()
                    .WhereEquals(nameof(IWebPageContentQueryDataContainer.WebPageItemGUID), pageGuid)),
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken
        )).FirstOrDefault();
        if (sectionPeople is null)
        {
            throw new MissingSectionPageException($"'{nameof(SectionPeoplePage)} page with GUID '{pageGuid}' not found.");
        }

        var paginationDto = await _peopleCardsService.GetFilteredItemsAsync(sectionPeople,language,cardsQuery, null, _ => false, cancellationToken);

        return paginationDto;
    }

    private static FilterItemDto<OptionDto> CreateOrderingDto(string defaultOrder)
    {
        var orderOptions = new[]
        {
            FilterDtoUtility.CreateOptionDto(ResourceStringKey.LiveSite.People.SortLastNameAtoZ, LastNameAtoZOptionValue),
            FilterDtoUtility.CreateOptionDto(ResourceStringKey.LiveSite.People.SortLastNameZtoA, LastNameZtoAOptionValue),
            FilterDtoUtility.CreateOptionDto(ResourceStringKey.LiveSite.People.SortNodeOrder, NodeOrderOptionValue),
        };

        var orderingDto = FilterDtoUtility.CreateFilterItemDto("select", OrderFilterName, OrderFilterName, defaultOrder, orderOptions);

        return orderingDto;
    }
}
