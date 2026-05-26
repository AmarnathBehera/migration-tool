using BDO.GWT.Common.Exceptions;
using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Utilities;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Domain.Core.Entities;
using BDO.GWT.Web.Core.DTOs.Cards;
using BDO.GWT.Web.Core.Services;
using Kentico.Content.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDO.GWT.Core.Services;
public interface ISectionEventsService
{
    Task<SectionEventsDTO>GetSectionEventsDtoAsync(SectionEvents page, CancellationToken cancellationToken);

    //Task<PaginationDto<PersonCardDto>> GetSectionEventsPaginationDtoAsync(Guid pageGuid, CardsQuery cardsQuery, CancellationToken cancellationToken);
}
public class SectionEventsService : ISectionEventsService
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
    private readonly ICultureContext _cultureContext;

    public SectionEventsService(
        IContentService pageRetriever,
        IHeroService heroService,
        IPeopleCardsService peopleCardsService,
        ICultureContext cultureContext)
    {
        _contentRetriever = pageRetriever;
        _heroService = heroService;
        _peopleCardsService = peopleCardsService;
        _cultureContext = cultureContext;
    }

    public async Task<SectionEventsDTO> GetSectionEventsDtoAsync(SectionEvents page, CancellationToken cancellationToken)
    {
        var bgImageUrl = page.HeroBackgroundImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url;
        var mobileImageUrl = page.HeroBackgroundImage_Mobile?.FirstOrDefault()?.ContentAttachmentAsset?.Url;
       

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
        };
        var dto = new SectionEventsDTO
        {
            Hero = await _heroService.GetHeroDtoAsync(heroData, cancellationToken),
            FilteringTooltipEnabled = true,
            Title = page.EventsWebinarsListTitle,
            Description = page.EventsWebinarsListDescription,
           // Filters = page.EventsWebinarsListFilteringEnabled ? await _eventWebinarCardsService.CreateFiltersAsync(page.EventsWebinarsListFilters, null, cancellationToken) : null,
            //FilteringTooltipEnabled = await _filteringHelper.IsFilteringTooltipEnabledAsync(page.EventsWebinarsListFilteringEnabled, cancellationToken),
            PageSize = 20,
            AdvancedSearchPrelinkText = page.EventsWebinarsListAdvancedSearchDescription,
            AdvancedSearchLinkText = page.EventsWebinarsListAdvancedSearchLinkText,
           // AdvancedSearchLink = page.EventsWebinarsListAdvancedSearchLinkUrl?.ToHtmlCompatibleString(),

        };

        return dto;
    }


}


