using BDO;
using BDO.GWT.Common.Exceptions;
using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Core.Services;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
namespace BDO.GWT.Core.Services;

public interface ISectionEventsService
{
    Task<SectionEventsDto> GetEventsSectionDtoAsync(SectionEvents page, CancellationToken cancellationToken);

    Task<PaginationDto<EventsWebinarsCardDto>> GetFilteredItemsAsync(CardsQuery query, CancellationToken cancellationToken);

}

public class SectionEventsService : ISectionEventsService
{
    private readonly IFilteringHelper _filteringHelper;
    private readonly IHeroService _heroService;
    private readonly IEventWebinarCardsService _eventWebinarCardsService;
    private readonly IContentService _contentService;
    private readonly ICultureContext _cultureContext;
    private readonly IContentRetriever _contentRetriever;


    public SectionEventsService(
        IHeroService heroService,
        IEventWebinarCardsService eventWebinarCardsService,
        IFilteringHelper filteringHelper,
        IContentService contentService,
        ICultureContext cultureContext, IContentRetriever contentRetriever)
    {
        _heroService = heroService;
        _eventWebinarCardsService = eventWebinarCardsService;
        _filteringHelper = filteringHelper;
        _contentService = contentService;
        _cultureContext = cultureContext;
        _contentRetriever = contentRetriever;
    }

    public async Task<SectionEventsDto> GetEventsSectionDtoAsync(SectionEvents page, CancellationToken cancellationToken)
    {

        var bgImageUrl = page.HeroBackgroundImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url;
        var mobileImageUrl = page.HeroBackgroundImage_Mobile?.FirstOrDefault()?.ContentAttachmentAsset?.Url;
        var selectedpageid = page.EventsWebinarsListAdvancedSearchLinkUrl?.FirstOrDefault()?.WebPageGuid;
        var selectedPage = selectedpageid.HasValue && selectedpageid.Value != Guid.Empty ? await _contentRetriever.RetrieveAllPagesByGuids<IWebPageFieldsSource>(new[] { selectedpageid.Value }, cancellationToken: cancellationToken)
        : Enumerable.Empty<IWebPageFieldsSource>();
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
            AdvancedSearchLink = selectedPage.FirstOrDefault()?.GetUrl()?.RelativePath ?? string.Empty,

        };

           var sys = page.SystemFields;

        var dto = new SectionEventsDto
        {
            Title = page.EventsWebinarsListTitle,
            Description = page.EventsWebinarsListDescription,
          

            BaseContent = new BaseContent
            {
                ContentItemID = sys?.ContentItemID ?? 0,
                ContentItemGUID = sys?.ContentItemGUID ?? Guid.Empty,
                ContentItemName = sys?.ContentItemName,
                ContentItemContentTypeID = sys?.ContentItemContentTypeID ?? 0,
                ContentItemIsSecured = sys?.ContentItemIsSecured ?? false,
                ContentItemCommonDataContentLanguageID = sys?.ContentItemCommonDataContentLanguageID ?? 0,
                ContentItemCommonDataFirstPublishedWhen = sys?.ContentItemCommonDataFirstPublishedWhen ?? default,
                ContentItemCommonDataLastPublishedWhen = sys?.ContentItemCommonDataLastPublishedWhen ?? default
            },

            BaseWebPage = new BaseWebPage
            {
                WebPageItemGUID = sys?.WebPageItemGUID ?? Guid.Empty,
                WebPageItemID = sys?.WebPageItemID ?? 0,
                WebPageItemName = sys?.WebPageItemName,
                WebPageItemWebsiteChannelID = sys?.WebPageItemWebsiteChannelId ?? 0,
                WebPageUrlPath = sys?.WebPageUrlPath,
                WebPageItemOrder = sys?.WebPageItemOrder ?? 0,
                WebPageItemTreePath = sys?.WebPageItemTreePath,
                WebPageUrlPathContentLanguageID = sys?.WebPageUrlPathContentLanguageID ?? 0,
                WebPageItemParentID = sys?.WebPageItemParentID ?? 0
            },
            Hero = await _heroService.GetHeroDtoAsync(heroData, cancellationToken),
            CardsGrid = cardsGridDto,
        };

        return dto;
    }
    public async Task<PaginationDto<EventsWebinarsCardDto>> GetFilteredItemsAsync(CardsQuery query, CancellationToken cancellationToken)
    {
        var result = await RetrieveSectionEventsPageAsync(cancellationToken);
        var featuredPageIds = result.FeaturedPageIds;
        var showPastEvents = result.ShowPastEvents;

        query.Filters.Add(FilterParameterName.ShowAllPastEvents, showPastEvents.ToString());

        var paginationDto = await _eventWebinarCardsService.GetFilteredItemsAsync(
           query,
           null,
           eventOrWebinar => featuredPageIds.Contains(eventOrWebinar.SystemFields.WebPageItemID),
           showPastEvents,
           false,
           cancellationToken
           );
        return paginationDto;
    }


    private async Task<(HashSet<int> FeaturedPageIds, bool ShowPastEvents)> RetrieveSectionEventsPageAsync(CancellationToken cancellationToken)
    {
        var eventsSection = (await _contentService.GetPagesAsync<SectionEvents>(
           new RetrievePagesParameters
           {
               LinkedItemsMaxLevel = 2,
               LanguageName = _cultureContext.CurrentLanguageTag,
               UseLanguageFallbacks = false
           },
           query => query
               .OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder)),
           RetrievalCacheSettings.CacheDisabled,
           cancellationToken
         )).FirstOrDefault();

        if (eventsSection == null)
        {
            throw new MissingSectionPageException($"No pages have been found of type '{nameof(SectionEvents)}'.");
        }
        var featuredPageIds = eventsSection.EventsWebinarsListFeaturedPages
            .Select(x => x.SystemFields.WebPageItemID)
            .ToHashSet();

        return (featuredPageIds, eventsSection.ShowPastEvents);
    }
}