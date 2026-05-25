//using BDO.GWT.Core;
//using BDO.GWT.Core.DTOs.SchemaMarkups;
//using BDO.GWT.Core.Services;
//using BDO.GWT.Core.Utilities;
//using BDO.GWT.Core.ValueObjects;
//using BDO.GWT.Web.Core.DTOs;
//using BDO.GWT.Web.Core.DTOs.Pages;
//using BDO.GWT.Web.Core.DTOs.SchemaMarkups;
//using BDO.GWT.Web.Core.LiveSite.DTOs.Pages;
//using BDO.GWT.Xperience.Core.Services;
//using CMS.Automation.Internal;
//using CMS.Core;
//using CMS.DataEngine;
//using CMS.Helpers;
//using CMS.Websites.Routing;
//using Ical.Net.CalendarComponents;
//using Ical.Net.DataTypes;
//using Kentico.Content.Web.Mvc;
//using Microsoft.AspNetCore.Authentication.OAuth.Claims;
//using Microsoft.Extensions.DependencyInjection;
//using System.Data.SqlTypes;
//using System.Globalization;
//using System.Security.Claims;
//using System.Text;
//using System.Text.RegularExpressions;

//namespace BDO.GWT.Web.Core.LiveSite.Services;

//public interface IEventWebinarPageService
//{
//    Task<EventWebinarDto> GetEventPageDtoAsync(Event page, CancellationToken cancellationToken);
//    Task<EventWebinarDto> GetWebinarPageDtoAsync(Webinar page, CancellationToken cancellationToken);
//    Task<EventSchemaMarkupDto> GetEventSchemaMarkupDto(Event page, CancellationToken cancellationToken);
//    Task<EventSchemaMarkupDto> GetWebinarSchemaMarkupDto(Webinar page, CancellationToken cancellationToken);
//    Task<string?> GenerateOutlookEventCardContent(Guid pageGuid, string? source, CancellationToken cancellationToken);
//    Task<string?> GenerateOutlookWebinarCardContent(Guid pageGuid, string? source, CancellationToken cancellationToken);
//}

//public class EventWebinarPageService
//{
//    private readonly IContentService _contentRetriever;
//    private readonly IPageUrlService _pageUrlService;
//    private readonly ISettingsService _settingsService;
//    private readonly IWebsiteChannelContext _websiteChannelContext;
//    private readonly ISiteContext _siteContext;
//    private readonly IServiceScopeFactory _scopeFactory;

//    public EventWebinarPageService(
//        IContentService contentRetriever,
//        IPageUrlService pageUrlService,
//        ISettingsService settingsService,
//        IWebsiteChannelContext websiteChannelContext,
//        IServiceScopeFactory scopeFactory)
//    {
//        _contentRetriever = contentRetriever;
//        _pageUrlService = pageUrlService;
//        _settingsService = settingsService;
//        _websiteChannelContext = websiteChannelContext;
//        _scopeFactory = scopeFactory;
//    }

//    public async Task<string?> GenerateOutlookEventCardContent(Guid pageGuid, string? source, CancellationToken cancellationToken)
//    {
//        var pages = await _contentRetriever.GetPagesAsync<Event>(
//            new RetrievePagesParameters { LinkedItemsMaxLevel = 5 },
//            query => query.Where(w =>
//                w.WhereEquals("ContentItemChannelID", _websiteChannelContext.WebsiteChannelID)),
//            RetrievalCacheSettings.CacheDisabled,
//            cancellationToken);

//        var page = pages?.FirstOrDefault();
//        return await GenerateICalendarAsync(page, page?.EventLocationCity, page?.EventLocationAddress1, cancellationToken);
//    }

//    public async Task<string?> GenerateOutlookWebinarCardContent(Guid pageGuid, string? source, CancellationToken cancellationToken)
//    {
//        var pages = await _contentRetriever.GetPagesAsync<Webinar>(
//                   new RetrievePagesParameters { LinkedItemsMaxLevel = 5 },
//                   query => query.Where(w =>
//                       w.WhereEquals("ContentItemChannelID", _websiteChannelContext.WebsiteChannelID)),
//                   RetrievalCacheSettings.CacheDisabled,
//                   cancellationToken);
//        var page = pages?.FirstOrDefault();
//        return await GenerateICalendarAsync(page, page?.EventLocationCity, page?.EventLocationAddress1, cancellationToken);
//    }

//    public async Task<EventWebinarDto> GetEventPageDtoAsync(Event page, CancellationToken cancellationToken)
//        => await GetInternalDto(page, "Event", page.EventLocationAddress1, page.EventLocationCity, page.EventLocationCountry, cancellationToken);

//    public async Task<EventWebinarDto> GetWebinarPageDtoAsync(Webinar page, CancellationToken cancellationToken)
//        => await GetInternalDto(page, "Webinar", page.EventLocationAddress1, page.EventLocationCity, page.EventLocationCountry, cancellationToken);

//    public async Task<EventSchemaMarkupDto> GetEventSchemaMarkupDto(Event page, CancellationToken cancellationToken)
//    {
//        // ✅ IEventBase accepted — no inheritance required
//        var basicData = await GetEventBaseSchemaMarkupDto(page, cancellationToken);

//        if (basicData?.Location?.Address != null)
//        {
//            basicData.Location.Address.StreetAddress = page.EventLocationAddress1;
//            basicData.Location.Address.AddressLocality = page.EventLocationCity;
//            basicData.Location.Address.AddressCountry = page.EventLocationCountry;
//        }

//        return basicData;
//    }

//    public async Task<EventSchemaMarkupDto> GetWebinarSchemaMarkupDto(Webinar page, CancellationToken cancellationToken)
//    {
//        var basicData = await GetEventBaseSchemaMarkupDto(page, cancellationToken);

//        if (basicData?.Location?.Address != null)
//        {
//            basicData.Location.Address.StreetAddress = page.EventLocationAddress1;
//            basicData.Location.Address.AddressLocality = page.EventLocationCity;
//            basicData.Location.Address.AddressCountry = page.EventLocationCountry;
//        }

//        return basicData;
//    }

//    // ✅ Replaces: private async Task<...> GetEventBaseSchemaMarkupDto(EventBase page, ...)
//    private async Task<EventSchemaMarkupDto> GetEventBaseSchemaMarkupDto(IEventBase page, CancellationToken cancellationToken)
//    {
//        var site = await _siteRetriever.RetrieveEntityAsync(_siteContext.CurrentSiteID, cancellationToken)
//            ?? throw new NotFoundException($"Site with ID {_siteContext.CurrentSiteID} not found.");

//        var imageUrl = page.MetadataTeaserImage?.FirstOrDefault()?.ToAbsolute(site.PresentationURL);

//        // ✅ In XbyK use SystemFields.ContentItemGUID / WebPageItemID instead of NodeID
//        var pageUrl = await _pageUrlService.GetPageAbsoluteUrlAsync(
//            site.PresentationURL,
//            page.SystemFields.WebPageItemID,
//            page.SystemFields.ContentItemLanguageName,
//            cancellationToken);

//        return new EventSchemaMarkupDto
//        {
//            Description = GetEventPostDescription(page),
//            EndDate = page.EventDateTo,
//            Image = imageUrl?.ToHtmlCompatibleString(),
//            Location = new PlaceSchemaMarkupDto
//            {
//                Name = page.EventLocationTitle,
//                Address = new PostalAddressSchemaMarkupDto
//                {
//                    ExtendedAddress = page.EventLocationAddress2,
//                    AddressRegion = page.EventLocationState,
//                    PostalCode = page.EventLocationZip,
//                }
//            },
//            Name = page.MetadataTitle,
//            Offers = new OfferSchemaMarkupDto
//            {
//                Url = pageUrl?.ToHtmlCompatibleString()
//            },
//            StartDate = page.EventDateFrom,
//            Organizer = await GetEventOrganizerSchemaMarkup(site, cancellationToken)
//        };
//    }

//    // ✅ Replaces: private static string? GetEventPostDescription(EventBase? page)
//    private static string? GetEventPostDescription(IEventBase? page)
//    {
//        if (page is null) return null;

//        var desc = page.MetadataDescription;

//        if (string.IsNullOrWhiteSpace(desc)) desc = page.EventPostDescription;
//        if (string.IsNullOrWhiteSpace(desc)) desc = page.CpeDescription;
//        if (string.IsNullOrWhiteSpace(desc)) desc = page.CustomBodyText;

//        return desc;
//    }

//    private async Task<OrganizationSchemaMarkupDto> GetEventOrganizerSchemaMarkup(Site site, CancellationToken cancellationToken)
//    {
//        return new OrganizationSchemaMarkupDto
//        {
//            Name = await _currentSiteHelper.GetCurrentSiteLegalNameAsync(cancellationToken),
//            Logo = await _currentSiteHelper.GetCurrentSiteLogoImageUrlAsync(cancellationToken),
//            Url = site.PresentationURL.ToHtmlCompatibleString(),
//        };
//    }

//    private async Task<string?> GenerateICalendarAsync(IEventBase? eventBase, string? locationCity, string? eventLocationAddress1, CancellationToken cancellationToken)
//    {
//        if (eventBase is null) return null;

//        var calendarEvent = new CalendarEvent
//        {
//            Start = new CalDateTime(eventBase.EventDateFrom.ToUniversalTime()),
//            End = new CalDateTime(eventBase.EventDateTo.ToUniversalTime()),
//        };

//        var location = BuildLocationString(
//            eventBase.EventLocationTitle, eventLocationAddress1,
//            eventBase.EventLocationAddress2, locationCity,
//            eventBase.EventLocationState, eventBase.EventLocationZip);

//        if (!string.IsNullOrWhiteSpace(location))
//            calendarEvent.Location = location;

//        calendarEvent.Summary = eventBase.MetadataTitle;

//        if (!string.IsNullOrWhiteSpace(eventBase.CustomBodyText))
//        {
//            calendarEvent.Description = HtmlHelper.ExtractTextFromHtml(eventBase.CustomBodyText);
//            var htmlDescription = $"""
//                    <html><head></head><body><span style='font-family:"Trebuchet MS";font-size:10pt'>{eventBase.CustomBodyText}</span></body></html>
//                    """;
//            calendarEvent.AddProperty("X-ALT-DESC;FMTTYPE=text/html", htmlDescription);
//        }

//        var calendarEventTriggerInMinutes = (int?)(await _settingsService
//            .GetCurrentSiteSettingAsync(SettingName.BDO.MinutesBeforeToTriggerCalendarReminder, cancellationToken))?.Value;

//        if (calendarEventTriggerInMinutes > 0)
//        {
//            calendarEvent.Alarms.Add(new Alarm
//            {
//                Trigger = new Trigger(TimeSpan.FromMinutes(-calendarEventTriggerInMinutes.Value)),
//                Action = AlarmAction.Display,
//                Description = "Reminder",
//            });
//        }

//        var calendar = new Calendar
//        {
//            Version = "2.0",
//            ProductId = "-//BDO.Calendar//EN",
//        };

//        calendar.Events.Add(calendarEvent);

//        return new ComponentSerializer().SerializeToString(calendar);
//    }

//             ✅ Replaces: private async Task<EventWebinarDto> GetInternalDto(EventBase page, ...)
//            private async Task<EventWebinarDto> GetInternalDto(IEventBase page, string type, string? eventLocationAddress1, string? eventLocationCity, string? eventLocationCountry, CancellationToken cancellationToken)
//    {
//        RelativeLink? eventRegistrationFormPageUrl = null;

//        var directionsUrl = page.ShowEventLocation
//            ? GoogleMapsHelper.GenerateGoogleMapsLink(page.EventLocationLongitude, page.EventLocationLatitude)
//            : GoogleMapsHelper.GenerateGoogleMapsLink(page.EventLocationTitle, eventLocationAddress1, page.EventLocationAddress2, eventLocationCity, page.EventLocationState, page.EventLocationZip);

//        // ✅ In XbyK use SystemFields.WebPageItemID instead of NodeID
//        var formPage = await _eventRegistrationFormPageRetriever.RetrievePageAsync(
//            query => query.Where(x => x.WhereEquals("WebPageItemParentID", page.SystemFields.WebPageItemID)),
//            cancellationToken);

//        if (formPage is not null)
//        {
//            eventRegistrationFormPageUrl = await _pageUrlService.GetPageRelativeUrlAsync(
//                formPage.SystemFields.WebPageItemID,
//                formPage.SystemFields.ContentItemLanguageName,
//                cancellationToken);
//        }

//        var siteUrl = (await _siteRetriever.RetrieveEntityAsync(_siteContext.CurrentSiteID, cancellationToken))?.PresentationURL.ToString();

//        var registrationButtonUrl = !string.IsNullOrEmpty(eventRegistrationFormPageUrl?.Path)
//            ? $"{siteUrl}/{eventRegistrationFormPageUrl.Path}"
//            : null;

//        if (page.RegistrationType == "external")
//            registrationButtonUrl = page.RegistrationFormLink?.ToHtmlCompatibleString();

//        var displayLocationTitleSetting = await _settingsService.GetCurrentSiteSettingAsync(
//            SettingName.BDO.DisplayLocationTitleInEventWebinarSidebar, cancellationToken);

//        var displayLocationTitle = (bool?)displayLocationTitleSetting?.Value ?? false;

//        return new EventWebinarDto
//        {
//            Title = page.ShowInWidget ? page.MetadataTitle : null,
//            Description = HtmlUtility.RemoveTildeFromUrlTypeHtmlAttributes(page.MetadataDescription),
//            RegistrationCard = new EventWebinarDetailsRegistrationCardDto
//            {
//                // ✅ EventType is now IEnumerable<TagReference> — resolve name from tag
//                Type = page.EventType?.Count() == 1
//                    ? page.EventType.First().TagName
//                    : null,
//                DateFrom = page.EventDateFrom,
//                DateTo = page.EventDateTo,
//                Country = eventLocationCountry,
//                City = eventLocationCity,
//                State = page.EventLocationState,
//                PostalCode = page.EventLocationZip,
//                LocationTitle = displayLocationTitle ? page.EventLocationTitle : null,
//                AddressLine1 = eventLocationAddress1,
//                AddressLine2 = page.EventLocationAddress2,
//                GetDirectionsUrl = directionsUrl,
//                RegistrationButtonUrl = page.EventDateTo > DateTime.Now ? registrationButtonUrl : null,
//                DisableRegistrationButton = !page.AllowRegistration,
//                RecordingUrl = page.EventVideo,
//                DisableAddToCalendar = page.DisableAddToCalendar,
//                // ✅ SystemFields.ContentItemGUID replaces page.PageGUID
//                AddToCalendarUrl = !page.DisableAddToCalendar
//                    ? $"{siteUrl}/OutlookEvent/{type}/{page.SystemFields.ContentItemGUID}"
//                    : null,
//                ContactEmail = page.EventContactEmail,
//                ContactName = page.EventContactName,
//                ContactPhone = page.EventContactPhone,
//                ZipPositionAfter = page.EventLocationZipPosition == "after",
//                HideEventLocationZipComma = page.HideEventLocationZipComma,
//                DateTextOverride = page.EventDateOverride,
//                TimeTextOverride = page.EventTimeOverride,
//            },
//            CpeEvent = page.CpeEvent,
//            CpeCount = page.CpeEvent ? page.CpeCount : null,
//            CpeCreditNotes = page.CpeEvent ? page.CpeCreditNotes : null,
//            CpeLevel = page.CpeEvent ? page.ProgramLevel : null,
//            CpeDescription = page.CpeEvent ? HtmlUtility.RemoveTildeFromUrlTypeHtmlAttributes(page.CpeDescription) : null,
//            Presenters = (await _presenterPageRetriever.RetrievePagesAsync(
//                query => query.Where(x => x.WhereEquals("WebPageItemParentID", page.SystemFields.WebPageItemID)
//                    .And().WhereTrue("IsPublished")),
//                cancellationToken))
//                .Select(p => new PresenterDto
//                {
//                    Title = p.Title,
//                    Name = p.Name,
//                    Link = p.Link,
//                }).ToList(),
//            DisablePrintAndShare = page.DisablePrintAndShare,
//            SocialMediaButtons = await _socialMediaButtonsService.GetSocialMediaButtonsDto(page, cancellationToken),
//        };
//    }

//    private static string BuildLocationString(string? locationTitle, string? locationAddress, string? locationAddress2, string? locationCity, string? locationState, string? locationZip)
//    {
//        var sb = new StringBuilder();
//        sb.Append(locationTitle + " \\n");
//        sb.Append(!string.IsNullOrEmpty(locationAddress) ? locationAddress + " \\n" : " ");
//        sb.Append(!string.IsNullOrEmpty(locationAddress2) ? locationAddress2 + " \\n" : "");
//        sb.Append(!string.IsNullOrEmpty(locationCity) ? locationCity : " ");

//        if (!string.IsNullOrEmpty(locationCity))
//            sb.Append(!string.IsNullOrEmpty(locationState) ? ", " + locationState + " " : " ");
//        else
//            sb.Append(!string.IsNullOrEmpty(locationState) ? " " + locationState + " " : " ");

//        sb.Append(!string.IsNullOrEmpty(locationZip) ? locationZip : "");

//        var location = sb.ToString();
//        location = Regex.Replace(location.Trim(), @"\\n$", "");
//        location = location.Replace("  ", " ");
//        return location;
//    }

//    private (string MinutesToTrigger, bool DisplayLocationInSidebar) EventnWebinarSetting()
//    {
//        using var scope = _scopeFactory.CreateScope();
//        var websiteChannelContext = scope.ServiceProvider.GetRequiredService<IWebsiteChannelContext>();
//        var globalEventInfoProvider = scope.ServiceProvider.GetRequiredService<IInfoProvider<GlobalEventsnWebinarsInfo>>();
//        var eventInfoProvider = scope.ServiceProvider.GetRequiredService<IInfoProvider<EventsWebinarsInfo>>();

//        int currentWebsiteID = websiteChannelContext.WebsiteChannelID;

//        var currentChannelSettings = eventInfoProvider
//            .Get()
//            .WhereEquals(nameof(EventsWebinarsInfo.EventnWebinarWebChannelSettingID), currentWebsiteID)
//            .TopN(1)
//            .FirstOrDefault();

//        bool inheritFromGlobal = currentChannelSettings == null  // if no channel settings found, go global
//            || ValidationHelper.GetBoolean(currentChannelSettings.GetValue("InheritFromGlobal"), false);

//        if (!inheritFromGlobal && currentChannelSettings != null)
//        {
//            return (
//                currentChannelSettings.Minutestotriggercalendarreminder.ToString(),
//                currentChannelSettings.Displaylocationtitleinasidebar
//            );
//        }

//        // Fall back to global settings
//        var globalValue = globalEventInfoProvider
//            .Get()
//            .TopN(1)
//            .GetEnumerableTypedResult()
//            .FirstOrDefault();

//        return (
//            globalValue?.Minutestotriggercalendarreminder.ToString() ?? string.Empty,
//            globalValue?.Displaylocationtitleinasidebar ?? false
//        );
//    }
//}