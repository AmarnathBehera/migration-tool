using BDO.GWT.Common.Core.Helpers;
using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs.SchemaMarkups;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.Utilities;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.DTOs.Pages;
using BDO.GWT.Web.Core.DTOs.SchemaMarkups;
using BDO.GWT.Web.Core.LiveSite.DTOs.Pages;
using BDO.GWT.Xperience.Core.Services;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Websites;
using CMS.Websites.Routing;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Kentico.Content.Web.Mvc;
using Kentico.Content.Web.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace BDO.GWT.Web.Core.Services;

public interface IEventWebinarPageService
{
    Task<EventWebinarDto> GetEventPageDtoAsync(Event page, CancellationToken cancellationToken);
    Task<EventWebinarDto> GetWebinarPageDtoAsync(Webinar page, CancellationToken cancellationToken);
    Task<EventSchemaMarkupDto> GetEventSchemaMarkupDto(Event page, CancellationToken cancellationToken);
    Task<EventSchemaMarkupDto> GetWebinarSchemaMarkupDto(Webinar page, CancellationToken cancellationToken);
    Task<string?> GenerateOutlookEventCardContent(Guid pageGuid, string? source, CancellationToken cancellationToken);
    Task<string?> GenerateOutlookWebinarCardContent(Guid pageGuid, string? source, CancellationToken cancellationToken);
}

public class EventWebinarPageService : IEventWebinarPageService
{
    private readonly IWebsiteChannelContext _websiteChannelContext;
    private readonly IInfoProvider<EventsWebinarsInfo> _eventInfoProvider;
    private readonly IInfoProvider<GlobalEventsnWebinarsInfo> _globalEventInfoProvider;
    private readonly IPreferredLanguageRetriever _preferredLanguageRetriever;
    private readonly IHeroService _heroService;
    private readonly IContentService _contentRetriever;
    private readonly IPageUrlService _pageUrlService;
    private readonly ISettingsService _settingsService;
    private readonly ISiteContext _siteContext;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IWebPageUrlRetriever _pageUrlRetriever;
    private readonly ISocialMediaButtonsService _socialMediaButtonsService;

    public EventWebinarPageService(
        IContentService contentRetriever,
        IPageUrlService pageUrlService,
        ISettingsService settingsService,
        IWebsiteChannelContext websiteChannelContext,
        IServiceScopeFactory scopeFactory,
        IWebPageUrlRetriever pageUrlRetriever,
        IInfoProvider<EventsWebinarsInfo> eventInfoProvider,
        IInfoProvider<GlobalEventsnWebinarsInfo> globalEventInfoProvider,
        IPreferredLanguageRetriever preferredLanguageRetriever,
        ISocialMediaButtonsService socialMediaButtonsService,
        IHeroService heroService)
    {
        _contentRetriever = contentRetriever;
        _pageUrlService = pageUrlService;
        _settingsService = settingsService;
        _websiteChannelContext = websiteChannelContext;
        _scopeFactory = scopeFactory;
        _pageUrlRetriever = pageUrlRetriever;
        _eventInfoProvider = eventInfoProvider;
        _globalEventInfoProvider = globalEventInfoProvider;
        _preferredLanguageRetriever = preferredLanguageRetriever;
        _socialMediaButtonsService = socialMediaButtonsService;
        _heroService = heroService;
    }

    public async Task<string?> GenerateOutlookEventCardContent(Guid pageGuid, string? source, CancellationToken cancellationToken)
    {
        var pages = await _contentRetriever.GetPagesAsync<Event>(
            new RetrievePagesParameters { LinkedItemsMaxLevel = 5 },
            query => query.Where(w =>
                w.WhereEquals("ContentItemChannelID", _websiteChannelContext.WebsiteChannelID)),
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken);

        var page = pages?.FirstOrDefault();
        return await GenerateICalendarAsync(page, page?.EventLocationCity, page?.EventLocationAddress1, cancellationToken);
    }

    public async Task<string?> GenerateOutlookWebinarCardContent(Guid pageGuid, string? source, CancellationToken cancellationToken)
    {
        var pages = await _contentRetriever.GetPagesAsync<Webinar>(
            new RetrievePagesParameters { LinkedItemsMaxLevel = 5 },
            query => query.Where(w =>
                w.WhereEquals("ContentItemChannelID", _websiteChannelContext.WebsiteChannelID)),
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken);

        var page = pages?.FirstOrDefault();
        return await GenerateICalendarAsync(page, page?.EventLocationCity, page?.EventLocationAddress1, cancellationToken);
    }

    public async Task<EventWebinarDto> GetEventPageDtoAsync(Event page, CancellationToken cancellationToken)
        => await GetInternalDto(
            page,
            "Event",
            page.EventLocationAddress1,
            page.EventLocationCity,
            page.EventLocationCountry,
            page.EventType.ToString(),
            cancellationToken);

    public async Task<EventWebinarDto> GetWebinarPageDtoAsync(Webinar page, CancellationToken cancellationToken)
        => await GetInternalDto(
            page,
            "Webinar",
            page.EventLocationAddress1,
            page.EventLocationCity,
            page.EventLocationCountry,
            page.EventType.ToString(),
            cancellationToken);

    public async Task<EventSchemaMarkupDto> GetEventSchemaMarkupDto(Event page, CancellationToken cancellationToken)
    {
        var basicData = await GetEventBaseSchemaMarkupDto(page, cancellationToken);

        if (basicData?.Location?.Address != null)
        {
            basicData.Location.Address.StreetAddress = page.EventLocationAddress1;
            basicData.Location.Address.AddressLocality = page.EventLocationCity;
            basicData.Location.Address.AddressCountry = page.EventLocationCountry;
        }

        return basicData;
    }

    public async Task<EventSchemaMarkupDto> GetWebinarSchemaMarkupDto(Webinar page, CancellationToken cancellationToken)
    {
        var basicData = await GetEventBaseSchemaMarkupDto(page, cancellationToken);

        if (basicData?.Location?.Address != null)
        {
            basicData.Location.Address.StreetAddress = page.EventLocationAddress1;
            basicData.Location.Address.AddressLocality = page.EventLocationCity;
            basicData.Location.Address.AddressCountry = page.EventLocationCountry;
        }

        return basicData;
    }

    private Task<EventSchemaMarkupDto> GetEventBaseSchemaMarkupDto(IEventBase page, CancellationToken cancellationToken)
    {
        var pageUrl = ((IWebPageFieldsSource)page).GetUrl().AbsoluteUrl;
        var imageUrl = page.MetadataTeaserImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url;

        var dto = new EventSchemaMarkupDto
        {
            Description = GetEventPostDescription(page),
            EndDate = page.EventDateTo,
            Image = imageUrl,
            Location = new PlaceSchemaMarkupDto
            {
                Name = page.EventLocationTitle,
                Address = new PostalAddressSchemaMarkupDto
                {
                    ExtendedAddress = page.EventLocationAddress2,
                    AddressRegion = page.EventLocationState,
                    PostalCode = page.EventLocationZip,
                }
            },
            Name = page.MetadataTitle,
            Offers = new OfferSchemaMarkupDto
            {
                Url = pageUrl,
            },
            StartDate = page.EventDateFrom,
            Organizer = GetEventOrganizerSchemaMarkup(pageUrl),
        };

        return Task.FromResult(dto);
    }

    private static string? GetEventPostDescription(IEventBase? page)
    {
        if (page is null) return null;

        var desc = page.MetadataDescription;
        if (string.IsNullOrWhiteSpace(desc)) desc = page.EventPostDescription;
        if (string.IsNullOrWhiteSpace(desc)) desc = page.CpeDescription;
        if (string.IsNullOrWhiteSpace(desc)) desc = page.CustomBodyText;

        return desc;
    }

    private OrganizationSchemaMarkupDto GetEventOrganizerSchemaMarkup(string? pageAbsoluteUrl)
    {
        string? organizationUrl = null;
        if (!string.IsNullOrWhiteSpace(pageAbsoluteUrl) &&
            Uri.TryCreate(pageAbsoluteUrl, UriKind.Absolute, out var uri))
        {
            organizationUrl = $"{uri.Scheme}://{uri.Authority}";
        }

        return new OrganizationSchemaMarkupDto
        {
            Name = _contentRetriever.GetCurrentSiteLegalNameAsync() ?? string.Empty,
            Logo = _contentRetriever.GetCurrentSiteLogoImageUrlAsync() ?? string.Empty,
            Url = organizationUrl,
        };
    }

    private Task<string?> GenerateICalendarAsync(IEventBase? eventBase, string? locationCity, string? eventLocationAddress1, CancellationToken cancellationToken)
    {
        if (eventBase is null)
            return Task.FromResult<string?>(null);

        cancellationToken.ThrowIfCancellationRequested();

        var calendarEvent = new CalendarEvent
        {
            Start = new CalDateTime(eventBase.EventDateFrom.ToUniversalTime()),
            End = new CalDateTime(eventBase.EventDateTo.ToUniversalTime()),
        };

        var location = BuildLocationString(
            eventBase.EventLocationTitle, eventLocationAddress1,
            eventBase.EventLocationAddress2, locationCity,
            eventBase.EventLocationState, eventBase.EventLocationZip);

        if (!string.IsNullOrWhiteSpace(location))
            calendarEvent.Location = location;

        calendarEvent.Summary = eventBase.MetadataTitle;

        if (!string.IsNullOrWhiteSpace(eventBase.CustomBodyText))
        {
            calendarEvent.Description = HtmlHelper.ExtractTextFromHtml(eventBase.CustomBodyText);
            var htmlDescription = $"""
                <html><head></head><body><span style='font-family:"Trebuchet MS";font-size:10pt'>{eventBase.CustomBodyText}</span></body></html>
                """;
            calendarEvent.AddProperty("X-ALT-DESC;FMTTYPE=text/html", htmlDescription);
        }

        var (minutesToTriggerRaw, _) = EventnWebinarSetting();
        int? calendarEventTriggerInMinutes =
            int.TryParse(minutesToTriggerRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : null;

        if (calendarEventTriggerInMinutes > 0)
        {
            var calendarAlarm = new Alarm
            {
                Trigger = new Ical.Net.DataTypes.Trigger(new Ical.Net.DataTypes.Duration(0, 0, 0, -calendarEventTriggerInMinutes.Value, 0)),
                Action = AlarmAction.Display,
                Description = "Reminder",
            };
            calendarEvent.Alarms.Add(calendarAlarm);
        }

        var calendar = new Ical.Net.Calendar
        {
            Version = "2.0",
            ProductId = "-//BDO.Calendar//EN",
        };

        calendar.Events.Add(calendarEvent);

        return Task.FromResult<string?>(new ComponentSerializer().SerializeToString(calendar));
    }

    private async Task<EventWebinarDto> GetInternalDto(
        IEventBase page,
        string type,
        string? eventLocationAddress1,
        string? eventLocationCity,
        string? eventLocationCountry,
        string? eventTypeName,
        CancellationToken cancellationToken)
    {
        string? registrationButtonUrl = null;

        var directionsUrl = page.ShowEventLocation
            ? GoogleMapsHelper.GenerateGoogleMapsLink(page.EventLocationLongitude, page.EventLocationLatitude)
            : GoogleMapsHelper.GenerateGoogleMapsLink(page.EventLocationTitle, eventLocationAddress1, page.EventLocationAddress2, eventLocationCity, page.EventLocationState, page.EventLocationZip);

        var formPage = (await _contentRetriever.GetPagesAsync<EventRegistrationForm>(
            new RetrievePagesParameters { LinkedItemsMaxLevel = 1 },
            query => query
                .Where(x => x.WhereEquals("WebPageItemParentID", page.SystemFields.WebPageItemID))
                .TopN(1),
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken)).FirstOrDefault();

        if (formPage is not null)
        {
            var url = await _pageUrlRetriever.Retrieve(formPage, _preferredLanguageRetriever.Get());
            registrationButtonUrl = url?.AbsoluteUrl;
        }

        var pageAbsoluteUrl = ((IWebPageFieldsSource)page).GetUrl().AbsoluteUrl;
        string? siteUrl = null;
        if (!string.IsNullOrWhiteSpace(pageAbsoluteUrl) &&
            Uri.TryCreate(pageAbsoluteUrl, UriKind.Absolute, out var uri))
        {
            siteUrl = $"{uri.Scheme}://{uri.Authority}";
        }

        if (string.Equals(page.RegistrationType, "external", StringComparison.OrdinalIgnoreCase))
        {
            registrationButtonUrl = page.RegistrationFormLink;
        }

        var (_, displayLocationTitle) = EventnWebinarSetting();

        // ✅ Fetch presenter child pages — same pattern as K13
        var presenterPages = await _contentRetriever.GetPagesAsync<Presenter>(
            new RetrievePagesParameters { LinkedItemsMaxLevel = 1 },
            query => query
                .Where(x => x.WhereEquals("WebPageItemParentID", page.SystemFields.WebPageItemID)),
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken);

        var presenters = presenterPages?
            .Select(p => new PresenterDto
            {
                Name = p.Name ?? string.Empty,
                Title = p.Title ?? string.Empty,
                Link = p.Link,
            })
            .ToList() ?? new List<PresenterDto>();

        var sys = page.SystemFields;

        return new EventWebinarDto
        {
            Title = page.ShowInWidget ? page.MetadataTitle : null,
            Description = HtmlUtility.RemoveTildeFromUrlTypeHtmlAttributes(page.MetadataDescription),
            RegistrationCard = new EventWebinarDetailsRegistrationCardDto
            {
                Type = eventTypeName,
                DateFrom = page.EventDateFrom,
                DateTo = page.EventDateTo,
                Country = eventLocationCountry,
                City = eventLocationCity,
                State = page.EventLocationState,
                PostalCode = page.EventLocationZip,
                LocationTitle = displayLocationTitle ? page.EventLocationTitle : null,
                AddressLine1 = eventLocationAddress1,
                AddressLine2 = page.EventLocationAddress2,
                GetDirectionsUrl = directionsUrl,
                RegistrationButtonUrl = page.EventDateTo > DateTime.UtcNow ? registrationButtonUrl : null,
                DisableRegistrationButton = !page.AllowRegistration,
                RecordingUrl = page.EventVideo,
                DisableAddToCalendar = page.DisableAddToCalendar,
                AddToCalendarUrl = !page.DisableAddToCalendar && !string.IsNullOrEmpty(siteUrl)
                    ? $"{siteUrl}/OutlookEvent/{type}/{page.SystemFields.ContentItemGUID}"
                    : null,
                ContactEmail = page.EventContactEmail,
                ContactName = page.EventContactName,
                ContactPhone = page.EventContactPhone,
                ZipPositionAfter = page.EventLocationZipPosition == "after",
                HideEventLocationZipComma = page.HideEventLocationZipComma,
                DateTextOverride = page.EventDateOverride,
                TimeTextOverride = page.EventTimeOverride,
            },
            CpeEvent = page.CpeEvent,
            CpeCount = page.CpeEvent ? page.CpeCount : null,
            CpeCreditNotes = page.CpeEvent ? page.CpeCreditNotes : null,
            CpeLevel = page.CpeEvent ? page.ProgramLevel : null,
            CpeDescription = page.CpeEvent
                ? HtmlUtility.RemoveTildeFromUrlTypeHtmlAttributes(page.CpeDescription)
                : null,
            // ✅ Now populated instead of empty list
            Presenters = presenters,
            DisablePrintAndShare = page.DisablePrintAndShare,
            SocialMediaButtons = await _socialMediaButtonsService.GetEventSocialMediaButtonsDto(page, page.MetadataTitle, cancellationToken),
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
        };
    }

    private static string BuildLocationString(string? locationTitle, string? locationAddress, string? locationAddress2, string? locationCity, string? locationState, string? locationZip)
    {
        var sb = new StringBuilder();
        sb.Append(locationTitle + " \\n");
        sb.Append(!string.IsNullOrEmpty(locationAddress) ? locationAddress + " \\n" : " ");
        sb.Append(!string.IsNullOrEmpty(locationAddress2) ? locationAddress2 + " \\n" : "");
        sb.Append(!string.IsNullOrEmpty(locationCity) ? locationCity : " ");

        if (!string.IsNullOrEmpty(locationCity))
            sb.Append(!string.IsNullOrEmpty(locationState) ? ", " + locationState + " " : " ");
        else
            sb.Append(!string.IsNullOrEmpty(locationState) ? " " + locationState + " " : " ");

        sb.Append(!string.IsNullOrEmpty(locationZip) ? locationZip : "");

        var location = sb.ToString();
        location = Regex.Replace(location.Trim(), @"\\n$", "");
        location = location.Replace("  ", " ");
        return location;
    }

    private (string MinutesToTrigger, bool DisplayLocationInSidebar) EventnWebinarSetting()
    {
        int currentWebsiteID = _websiteChannelContext.WebsiteChannelID;

        var currentChannelSettings = _eventInfoProvider
            .Get()
            .WhereEquals(nameof(EventsWebinarsInfo.EventnWebinarWebChannelSettingID), currentWebsiteID)
            .TopN(1)
            .FirstOrDefault();

        bool inheritFromGlobal = currentChannelSettings == null
            || ValidationHelper.GetBoolean(currentChannelSettings.GetValue("InheritFromGlobal"), false);

        if (!inheritFromGlobal && currentChannelSettings != null)
        {
            return (
                currentChannelSettings.Minutestotriggercalendarreminder.ToString(),
                currentChannelSettings.Displaylocationtitleinasidebar
            );
        }

        var globalValue = _globalEventInfoProvider
            .Get()
            .TopN(1)
            .GetEnumerableTypedResult()
            .FirstOrDefault();

        return (
            globalValue?.Minutestotriggercalendarreminder.ToString() ?? string.Empty,
            globalValue?.Displaylocationtitleinasidebar ?? false
        );
    }
}