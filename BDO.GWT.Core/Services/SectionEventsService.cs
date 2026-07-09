using BDO;
using BDO.GWT.Common.Exceptions;
using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.DTOs.ReusableFieldsDTOs;
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
        ICultureContext cultureContext,
        IContentRetriever contentRetriever)
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
        var selectedPageGuid = page.EventsWebinarsListAdvancedSearchLinkUrl?.FirstOrDefault()?.WebPageGuid;
        var selectedPages = selectedPageGuid.HasValue && selectedPageGuid.Value != Guid.Empty
            ? await _contentRetriever.RetrieveAllPagesByGuids<IWebPageFieldsSource>(new[] { selectedPageGuid.Value }, cancellationToken: cancellationToken)
            : Enumerable.Empty<IWebPageFieldsSource>();

        var filters = ParseFilters(page.EventsWebinarsListFilters);

        var heroData = new HeroData
        {
            Layout = page.HeroLayout ?? string.Empty,
            Title = page.HeroTitle ?? string.Empty,
            Description = page.HeroDescription ?? string.Empty,
            BackgroundImage = ParseLink(page.HeroBackgroundImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url),
            BackgroundImageAltText = page.HeroBackgroundImageAltText ?? string.Empty,
            MobileBackgroundImage = ParseLink(page.HeroBackgroundImage_Mobile?.FirstOrDefault()?.ContentAttachmentAsset?.Url),
            MobileBackgroundImageAltText = page.HeroBackgroundImage_MobileAltText ?? string.Empty,
            CallToActionButtonType = page.HeroCallToActionButtonType ?? string.Empty,
            CallToActionButtonText = page.HeroCallToActionButtonText ?? string.Empty,
            CallToActionButtonUrl = ParseLink(page.HeroCallToActionButtonUrl?.FirstOrDefault()?.SystemFields?.WebPageUrlPath),
        };

        var cardsGridDto = new CardsGridDto
        {
            Title = page.EventsWebinarsListTitle,
            Description = page.EventsWebinarsListDescription,
            Filters = page.EventsWebinarsListFilteringEnabled
                ? await _eventWebinarCardsService.CreateFiltersAsync(filters, null, cancellationToken)
                : null,
            FilteringTooltipEnabled = await _filteringHelper.IsFilteringTooltipEnabledAsync(page.EventsWebinarsListFilteringEnabled, cancellationToken),
            PageSize = 20,
            AdvancedSearchPrelinkText = page.EventsWebinarsListAdvancedSearchDescription,
            AdvancedSearchLinkText = page.EventsWebinarsListAdvancedSearchLinkText,
            AdvancedSearchLink = selectedPages.FirstOrDefault()?.GetUrl()?.RelativePath ?? string.Empty,
        };

        var sys = page.SystemFields;

        return new SectionEventsDto
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
                ContentItemCommonDataLastPublishedWhen = sys?.ContentItemCommonDataLastPublishedWhen ?? default,
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
                WebPageItemParentID = sys?.WebPageItemParentID ?? 0,
            },
            MetaData = new MetaDataDto
            {
                MetaTitle = page.MetadataTitle,
                MetaDescription = page.MetadataDescription,
                MetadataTeaserImage = page.MetadataTeaserImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                MetaTeaserImageAltText = page.MetadataTeaserImageAltText,
                MetaCanonical = page.MetadataCanonical,
            },
            SearchAndSEO = new SearchAndSEODto
            {
                MetadataNofollow = page.MetadataNofollow,
                MetadataNoindex = page.MetadataNoindex,
                IncludeInSitemap = page.IncludeInSitemap,
            },
            OGMetaData = new OGMetaDataDto
            {
                MetadataOGTitle = page.MetadataOGTitle,
                MetadataOGDescription = page.MetadataOGDescription,
                MetadataOGImage = page.MetadataOGImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                MetadataOGTwitterImage = page.MetadataOGTwitterImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                MetadataOGTwitterImageSizeCheckbox = page.MetadataOGTwitterImageSizeCheckbox,
                Url = page.MetadataCanonical,
            },
            Hero = await _heroService.GetHeroDtoAsync(heroData, cancellationToken),
            CardsGrid = cardsGridDto,
        };
    }

    public async Task<PaginationDto<EventsWebinarsCardDto>> GetFilteredItemsAsync(CardsQuery query, CancellationToken cancellationToken)
    {
        var result = await RetrieveSectionEventsPageAsync(cancellationToken);
        var featuredPageIds = result.FeaturedPageIds;
        var showPastEvents = result.ShowPastEvents;

        query.Filters ??= new Dictionary<string, string>();
        query.Filters[FilterParameterName.ShowAllPastEvents] = showPastEvents.ToString();

        return await _eventWebinarCardsService.GetFilteredItemsAsync(
            query,
            null,
            eventOrWebinar => featuredPageIds.Contains(eventOrWebinar.SystemFields.WebPageItemID),
            showPastEvents,
            false,
            cancellationToken);
    }

    private async Task<(HashSet<int> FeaturedPageIds, bool ShowPastEvents)> RetrieveSectionEventsPageAsync(CancellationToken cancellationToken)
    {
        var eventsSection = (await _contentService.GetPagesAsync<SectionEvents>(
            new RetrievePagesParameters
            {
                LinkedItemsMaxLevel = 2,
                LanguageName = _cultureContext.CurrentLanguageTag,
                UseLanguageFallbacks = false,
            },
            query => query.OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder)),
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken)).FirstOrDefault();

        if (eventsSection == null)
        {
            throw new MissingSectionPageException($"No pages have been found of type '{nameof(SectionEvents)}'.");
        }

        var featuredPageIds = eventsSection.EventsWebinarsListFeaturedPages
            .Select(x => x.SystemFields.WebPageItemID)
            .ToHashSet();

        return (featuredPageIds, eventsSection.ShowPastEvents);
    }

    private static HashSet<string> ParseFilters(string? rawFilters)
    {
        if (string.IsNullOrWhiteSpace(rawFilters))
        {
            return [];
        }

        return rawFilters
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .ToHashSet();
    }

    private static Link? ParseLink(string? url)
        => string.IsNullOrWhiteSpace(url) ? null : Link.Parse(url);
}