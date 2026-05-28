using BDO;
using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Core.DTOs.Cards;
using BDO.GWT.Web.Core.Services;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Core.Services;

public interface ISectionEventsService
{
    Task<SectionEventsDTO> GetSectionEventsDtoAsync(SectionEvents page, CancellationToken cancellationToken);

    Task<PaginationDto<EventsWebinarsCardDto>> GetFilteredItemsAsync(
        CardsQuery cardsQuery,
        CancellationToken cancellationToken);
}

public class SectionEventsService : ISectionEventsService
{
    private readonly IFilteringHelper _filteringHelper;
    private readonly IHeroService _heroService;
    private readonly IEventWebinarCardsService _eventWebinarCardsService;
    private readonly IContentRetriever _contentRetriever;

    public SectionEventsService(
        IHeroService heroService,
        IEventWebinarCardsService eventWebinarCardsService,
        IFilteringHelper filteringHelper,
        IContentRetriever contentRetriever)
    {
        _heroService = heroService ?? throw new ArgumentNullException(nameof(heroService));
        _eventWebinarCardsService = eventWebinarCardsService ?? throw new ArgumentNullException(nameof(eventWebinarCardsService));
        _filteringHelper = filteringHelper ?? throw new ArgumentNullException(nameof(filteringHelper));
        _contentRetriever = contentRetriever ?? throw new ArgumentNullException(nameof(contentRetriever));
    }

    public async Task<SectionEventsDTO> GetSectionEventsDtoAsync(SectionEvents page, CancellationToken cancellationToken)
    {
        
        var bgImageUrl = page.HeroBackgroundImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url;
        var mobileImageUrl = page.HeroBackgroundImage_Mobile?.FirstOrDefault()?.ContentAttachmentAsset?.Url;
        var filters = string.IsNullOrWhiteSpace(page.EventsWebinarsListFilters)
        ? new HashSet<string>()
         : page.EventsWebinarsListFilters
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
        };
        var cardsGridDto = new CardsGridDto
        {
            Title = page.EventsWebinarsListTitle,
            Description = page.EventsWebinarsListDescription,
            Filters = page.EventsWebinarsListFilteringEnabled ? await _eventWebinarCardsService.CreateFiltersAsync(filters, null, cancellationToken) : null,
            FilteringTooltipEnabled = await _filteringHelper.IsFilteringTooltipEnabledAsync(page.EventsWebinarsListFilteringEnabled, cancellationToken),
            PageSize = 20,
            AdvancedSearchPrelinkText = page.EventsWebinarsListAdvancedSearchDescription,
            AdvancedSearchLinkText = page.EventsWebinarsListAdvancedSearchLinkText,
            AdvancedSearchLink = (page.EventsWebinarsListAdvancedSearchLinkUrl?.FirstOrDefault() as MediaLink)?.LegacyMediaLinkURL,
        };
       

        var dto = new SectionEventsDTO
        {
            Hero = await _heroService.GetHeroDtoAsync(heroData, cancellationToken),
            CardsGrid = cardsGridDto,
        };

        return dto;
    }

    public async Task<PaginationDto<EventsWebinarsCardDto>> GetFilteredItemsAsync(
        CardsQuery cardsQuery,
        CancellationToken cancellationToken)
    {
        var eventsSection = await RetrieveSectionEventsPageAsync(cancellationToken);

        var featuredPageIds = eventsSection?.EventsWebinarsListFeaturedPages
            ?.Select(p => p.SystemFields.ContentItemID)
            .Where(id => id != 0)
            .ToHashSet() ?? [];

        cardsQuery.Filters ??= new Dictionary<string, string>();
        cardsQuery.Filters[FilterParameterName.ShowAllPastEvents] =
            (eventsSection?.ShowPastEvents ?? false).ToString();

        var paginationDto = await _eventWebinarCardsService.GetFilteredItemsAsync(
            cardsQuery,
            decorateQuery: null,
            isFeatured: eventOrWebinar =>
                featuredPageIds.Contains(eventOrWebinar.SystemFields.ContentItemID),
            cancellationToken);

        return paginationDto;
    }

    private async Task<SectionEvents?> RetrieveSectionEventsPageAsync(CancellationToken cancellationToken)
    {
        var results = await _contentRetriever.RetrievePages<SectionEvents>(
            new RetrievePagesParameters { LinkedItemsMaxLevel = 1 },
            query => query.TopN(1),
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken);

        return results?.FirstOrDefault();
    }
}