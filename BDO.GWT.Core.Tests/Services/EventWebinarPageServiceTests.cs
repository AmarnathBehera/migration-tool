using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.DTOs.SchemaMarkups;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using BDO.GWT.Core;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.LiveSite.DTOs.Pages;
using BDO.GWT.Web.Core.Services;
using CMS.DataEngine;
using CMS.Websites.Routing;
using CMS.Websites;
using Kentico.Content.Web.Mvc.Routing;
using Kentico.Content.Web.Mvc;
using Moq;
using System.Reflection;

namespace BDO.GWT.Core.Tests.Services
{
    public class EventWebinarPageServiceTests
    {
        private Mock<IContentService> _contentServiceMock = null!;
        private Mock<IWebPageUrlRetriever> _pageUrlRetrieverMock = null!;
        private Mock<IInfoProvider<GlobalsettingskeyInfo>> _globalSettingsKeyInfoProviderMock = null!;
        private Mock<IInfoProvider<WebChannelSnippetInfo>> _webChannelSnippetInfoProviderMock = null!;
        private Mock<IPreferredLanguageRetriever> _preferredLanguageRetrieverMock = null!;
        private Mock<ISocialMediaButtonsService> _socialMediaButtonsServiceMock = null!;
        private EventWebinarPageService _service = null!;

        [SetUp]
        public void Setup()
        {
            _contentServiceMock = new Mock<IContentService>();
            _pageUrlRetrieverMock = new Mock<IWebPageUrlRetriever>();
            _globalSettingsKeyInfoProviderMock = new Mock<IInfoProvider<GlobalsettingskeyInfo>>();
            _webChannelSnippetInfoProviderMock = new Mock<IInfoProvider<WebChannelSnippetInfo>>();
            _preferredLanguageRetrieverMock = new Mock<IPreferredLanguageRetriever>();
            _socialMediaButtonsServiceMock = new Mock<ISocialMediaButtonsService>();

            // EventnWebinarSetting() calls _eventInfoProvider.Get().WhereEquals(...).TopN(1).FirstOrDefault()
            // ObjectQuery<T> is a concrete Kentico class that cannot be Moq-mocked as an interface.
            // Returning null from Get() causes a NullReferenceException inside the chain, so we must
            // make EventnWebinarSetting() never reach the provider.
            // The only safe way is to NOT call methods that invoke EventnWebinarSetting()
            // (GetEventPageDtoAsync, GetWebinarPageDtoAsync, GenerateOutlook*) are kept as
            // reflection / interface-shape tests only.
            // Schema markup methods (GetEventSchemaMarkupDto, GetWebinarSchemaMarkupDto) call
            // GetUrl().AbsoluteUrl which requires a live Kentico routing context; those are also
            // kept as shape tests only.

            _socialMediaButtonsServiceMock
                .Setup(x => x.GetSocialMediaButtonsDto(
                    It.IsAny<IWebPageFieldsSource>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SocialMediaButtonsDto());

            _contentServiceMock
                .Setup(x => x.GetCurrentSiteLegalNameAsync())
                .Returns("BDO");

            _contentServiceMock
                .Setup(x => x.GetCurrentSiteLogoImageUrlAsync())
                .Returns("/logo.png");

            _service = new EventWebinarPageService(
                _contentServiceMock.Object,
                _pageUrlRetrieverMock.Object,
                _globalSettingsKeyInfoProviderMock.Object,
                _webChannelSnippetInfoProviderMock.Object,
                _preferredLanguageRetrieverMock.Object,
                _socialMediaButtonsServiceMock.Object);
        }

        #region Helpers

        /// <summary>
        /// Builds a minimal <see cref="Event"/> with <see cref="WebPageFields"/> set so that
        /// the <c>GetUrl()</c> extension reads <c>WebPageUrlPath</c> without needing to be mocked.
        /// </summary>
        private static Event BuildEvent(Action<Event>? configure = null)
        {
            var page = new Event
            {
                SystemFields = new WebPageFields
                {
                    WebPageItemGUID = Guid.NewGuid(),
                    WebPageItemID = 10,
                    WebPageUrlPath = "/events/test-event",
                    WebPageItemWebsiteChannelId = 1,
                },
                MetadataTitle = "Test Event",
                MetadataDescription = "Event description",
                EventDateFrom = DateTime.UtcNow.AddDays(1),
                EventDateTo = DateTime.UtcNow.AddDays(2),
                EventType = new List<EventType>(),
                AllowRegistration = true,
                DisableAddToCalendar = false,
                DisablePrintAndShare = false,
                ShowEventTitle = true,
                CpeEvent = false,
                RegistrationType = "internal",
            };
            configure?.Invoke(page);
            return page;
        }

        /// <summary>
        /// Builds a minimal <see cref="Webinar"/> with <see cref="WebPageFields"/> set so that
        /// the <c>GetUrl()</c> extension reads <c>WebPageUrlPath</c> without needing to be mocked.
        /// </summary>
        private static Webinar BuildWebinar(Action<Webinar>? configure = null)
        {
            var page = new Webinar
            {
                SystemFields = new WebPageFields
                {
                    WebPageItemGUID = Guid.NewGuid(),
                    WebPageItemID = 20,
                    WebPageUrlPath = "/webinars/test-webinar",
                    WebPageItemWebsiteChannelId = 1,
                },
                MetadataTitle = "Test Webinar",
                MetadataDescription = "Webinar description",
                EventDateFrom = DateTime.UtcNow.AddDays(1),
                EventDateTo = DateTime.UtcNow.AddDays(2),
                EventType = new List<EventType>(),
                AllowRegistration = true,
                DisableAddToCalendar = false,
                DisablePrintAndShare = false,
                ShowWebinarTitle = true,
                CpeEvent = false,
                RegistrationType = "internal",
            };
            configure?.Invoke(page);
            return page;
        }

        #endregion

        #region Interface shape

        [Test]
        public void Service_ImplementsInterface()
        {
            Assert.That(
                typeof(IEventWebinarPageService).IsAssignableFrom(typeof(EventWebinarPageService)),
                Is.True);
        }

        [Test]
        public void Interface_DeclaresExpectedMembers()
        {
            var t = typeof(IEventWebinarPageService);
            Assert.Multiple(() =>
            {
                Assert.That(t.GetMethod(nameof(IEventWebinarPageService.GetEventPageDtoAsync)), Is.Not.Null);
                Assert.That(t.GetMethod(nameof(IEventWebinarPageService.GetWebinarPageDtoAsync)), Is.Not.Null);
                Assert.That(t.GetMethod(nameof(IEventWebinarPageService.GetEventSchemaMarkupDto)), Is.Not.Null);
                Assert.That(t.GetMethod(nameof(IEventWebinarPageService.GetWebinarSchemaMarkupDto)), Is.Not.Null);
                Assert.That(t.GetMethod(nameof(IEventWebinarPageService.GenerateOutlookEventCardContent)), Is.Not.Null);
                Assert.That(t.GetMethod(nameof(IEventWebinarPageService.GenerateOutlookWebinarCardContent)), Is.Not.Null);
            });
        }

        [Test]
        public void Service_Constructor_AcceptsExpectedParameters()
        {
            var ctor = typeof(EventWebinarPageService)
                .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault();

            Assert.That(ctor, Is.Not.Null);
            var parameters = ctor!.GetParameters().Select(p => p.ParameterType).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(parameters, Has.Member(typeof(IContentService)));
                Assert.That(parameters, Has.Member(typeof(IWebPageUrlRetriever)));
                Assert.That(parameters, Has.Member(typeof(IInfoProvider<GlobalsettingskeyInfo>)));
                Assert.That(parameters, Has.Member(typeof(IInfoProvider<WebChannelSnippetInfo>)));
                Assert.That(parameters, Has.Member(typeof(IPreferredLanguageRetriever)));
                Assert.That(parameters, Has.Member(typeof(ISocialMediaButtonsService)));
            });
        }

        [Test]
        public void Service_CanBeInstantiated_WithMockedDependencies()
        {
            Assert.That(_service, Is.Not.Null);
            Assert.That(_service, Is.InstanceOf<IEventWebinarPageService>());
        }

        #endregion

        #region Event content-type shape

        [Test]
        public void Event_HasExpectedProperties()
        {
            var type = typeof(Event);
            Assert.Multiple(() =>
            {
                Assert.That(type.GetProperty(nameof(Event.SystemFields)), Is.Not.Null);
                Assert.That(type.GetProperty(nameof(Event.MetadataTitle)), Is.Not.Null);
                Assert.That(type.GetProperty(nameof(Event.MetadataDescription)), Is.Not.Null);
                Assert.That(type.GetProperty(nameof(Event.EventDateFrom)), Is.Not.Null);
                Assert.That(type.GetProperty(nameof(Event.EventDateTo)), Is.Not.Null);
                Assert.That(type.GetProperty(nameof(Event.AllowRegistration)), Is.Not.Null);
                Assert.That(type.GetProperty(nameof(Event.RegistrationType)), Is.Not.Null);
                Assert.That(type.GetProperty(nameof(Event.DisableAddToCalendar)), Is.Not.Null);
                Assert.That(type.GetProperty(nameof(Event.DisablePrintAndShare)), Is.Not.Null);
                Assert.That(type.GetProperty(nameof(Event.ShowEventTitle)), Is.Not.Null);
                Assert.That(type.GetProperty(nameof(Event.CpeEvent)), Is.Not.Null);
            });
        }

        [Test]
        public void Event_ImplementsIWebPageFieldsSource()
        {
            Assert.That(
                typeof(IWebPageFieldsSource).IsAssignableFrom(typeof(Event)),
                Is.True);
        }

        [Test]
        public void BuildEvent_Helper_PopulatesSystemFields()
        {
            var page = BuildEvent();

            Assert.Multiple(() =>
            {
                Assert.That(page.SystemFields, Is.Not.Null);
                Assert.That(page.SystemFields.WebPageItemGUID, Is.Not.EqualTo(Guid.Empty));
                Assert.That(page.SystemFields.WebPageUrlPath, Is.EqualTo("/events/test-event"));
            });
        }

        [Test]
        public void BuildEvent_Helper_ConfigureCallback_IsApplied()
        {
            var page = BuildEvent(configure: e =>
            {
                e.MetadataTitle = "Custom Event";
                e.CpeEvent = true;
            });

            Assert.Multiple(() =>
            {
                Assert.That(page.MetadataTitle, Is.EqualTo("Custom Event"));
                Assert.That(page.CpeEvent, Is.True);
            });
        }

        #endregion

        #region Webinar content-type shape

        [Test]
        public void Webinar_HasExpectedProperties()
        {
            var type = typeof(Webinar);
            Assert.Multiple(() =>
            {
                Assert.That(type.GetProperty(nameof(Webinar.SystemFields)), Is.Not.Null);
                Assert.That(type.GetProperty(nameof(Webinar.MetadataTitle)), Is.Not.Null);
                Assert.That(type.GetProperty(nameof(Webinar.MetadataDescription)), Is.Not.Null);
                Assert.That(type.GetProperty(nameof(Webinar.EventDateFrom)), Is.Not.Null);
                Assert.That(type.GetProperty(nameof(Webinar.EventDateTo)), Is.Not.Null);
                Assert.That(type.GetProperty(nameof(Webinar.AllowRegistration)), Is.Not.Null);
                Assert.That(type.GetProperty(nameof(Webinar.RegistrationType)), Is.Not.Null);
                Assert.That(type.GetProperty(nameof(Webinar.DisableAddToCalendar)), Is.Not.Null);
                Assert.That(type.GetProperty(nameof(Webinar.ShowWebinarTitle)), Is.Not.Null);
                Assert.That(type.GetProperty(nameof(Webinar.CpeEvent)), Is.Not.Null);
            });
        }

        [Test]
        public void Webinar_ImplementsIWebPageFieldsSource()
        {
            Assert.That(
                typeof(IWebPageFieldsSource).IsAssignableFrom(typeof(Webinar)),
                Is.True);
        }

        [Test]
        public void BuildWebinar_Helper_PopulatesSystemFields()
        {
            var page = BuildWebinar();

            Assert.Multiple(() =>
            {
                Assert.That(page.SystemFields, Is.Not.Null);
                Assert.That(page.SystemFields.WebPageItemGUID, Is.Not.EqualTo(Guid.Empty));
                Assert.That(page.SystemFields.WebPageUrlPath, Is.EqualTo("/webinars/test-webinar"));
            });
        }

        [Test]
        public void BuildWebinar_Helper_ConfigureCallback_IsApplied()
        {
            var page = BuildWebinar(configure: w =>
            {
                w.MetadataTitle = "Custom Webinar";
                w.CpeEvent = true;
            });

            Assert.Multiple(() =>
            {
                Assert.That(page.MetadataTitle, Is.EqualTo("Custom Webinar"));
                Assert.That(page.CpeEvent, Is.True);
            });
        }

        #endregion

        #region GenerateOutlookEventCardContent â€“ null path (no CMS context required)

        [Test]
        public async Task GenerateOutlookEventCardContent_ReturnsNull_WhenNoPageFound()
        {
            // GetPagesAsync returns empty â†’ page is null â†’ iCal method returns null early
            _contentServiceMock
                .Setup(x => x.GetPagesAsync<Event>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<Event>());

            var result = await _service.GenerateOutlookEventCardContent(
                Guid.NewGuid(), null, CancellationToken.None);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GenerateOutlookEventCardContent_ReturnsNull_WhenProviderReturnsNull()
        {
            _contentServiceMock
                .Setup(x => x.GetPagesAsync<Event>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<Event>?)null!);

            var result = await _service.GenerateOutlookEventCardContent(
                Guid.NewGuid(), null, CancellationToken.None);

            Assert.That(result, Is.Null);
        }

        #endregion

        #region GenerateOutlookWebinarCardContent â€“ null path (no CMS context required)

        [Test]
        public async Task GenerateOutlookWebinarCardContent_ReturnsNull_WhenNoPageFound()
        {
            _contentServiceMock
                .Setup(x => x.GetPagesAsync<Webinar>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<Webinar>());

            var result = await _service.GenerateOutlookWebinarCardContent(
                Guid.NewGuid(), null, CancellationToken.None);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GenerateOutlookWebinarCardContent_ReturnsNull_WhenProviderReturnsNull()
        {
            _contentServiceMock
                .Setup(x => x.GetPagesAsync<Webinar>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<Webinar>?)null!);

            var result = await _service.GenerateOutlookWebinarCardContent(
                Guid.NewGuid(), null, CancellationToken.None);

            Assert.That(result, Is.Null);
        }

        #endregion

        #region EventWebinarDto â€“ DTO property coverage

        [Test]
        public void EventWebinarDto_DefaultValues_AreCorrect()
        {
            var dto = new EventWebinarDto();

            Assert.Multiple(() =>
            {
                Assert.That(dto.Title, Is.Null);
                Assert.That(dto.Description, Is.Null);
                Assert.That(dto.CpeEvent, Is.Null);
                Assert.That(dto.Presenters, Is.Null);
            });
        }

        [Test]
        public void EventWebinarDto_SetProperties_RetainsValues()
        {
            var dto = new EventWebinarDto
            {
                Title = "My Event",
                Description = "A description",
                CpeEvent = true,
                CpeCount = "2",
                CpeCreditNotes = "Notes",
                CpeLevel = "Advanced",
                CpeDescription = "CPE desc",
                DisablePrintAndShare = true,
            };

            Assert.Multiple(() =>
            {
                Assert.That(dto.Title, Is.EqualTo("My Event"));
                Assert.That(dto.Description, Is.EqualTo("A description"));
                Assert.That(dto.CpeEvent, Is.True);
                Assert.That(dto.CpeCount, Is.EqualTo("2"));
                Assert.That(dto.CpeCreditNotes, Is.EqualTo("Notes"));
                Assert.That(dto.CpeLevel, Is.EqualTo("Advanced"));
                Assert.That(dto.CpeDescription, Is.EqualTo("CPE desc"));
                Assert.That(dto.DisablePrintAndShare, Is.True);
            });
        }

        #endregion

        #region EventWebinarDetailsRegistrationCardDto â€“ DTO property coverage

        [Test]
        public void RegistrationCardDto_SetProperties_RetainsValues()
        {
            var from = new DateTime(2026, 9, 1, 9, 0, 0, DateTimeKind.Utc);
            var to = new DateTime(2026, 9, 1, 17, 0, 0, DateTimeKind.Utc);

            var dto = new EventWebinarDetailsRegistrationCardDto
            {
                Type = "In Person",
                DateFrom = from,
                DateTo = to,
                Country = "Canada",
                City = "Toronto",
                State = "ON",
                PostalCode = "M5H 2N2",
                AddressLine1 = "123 Main St",
                AddressLine2 = "Suite 100",
                DisableRegistrationButton = false,
                DisableAddToCalendar = false,
                ZipPositionAfter = true,
                HideEventLocationZipComma = false,
                DateTextOverride = "Fall 2026",
                TimeTextOverride = "All Day",
                ContactName = "Alice",
                ContactEmail = "alice@bdo.com",
                ContactPhone = "+1-416-555-0100",
            };

            Assert.Multiple(() =>
            {
                Assert.That(dto.Type, Is.EqualTo("In Person"));
                Assert.That(dto.DateFrom, Is.EqualTo(from));
                Assert.That(dto.DateTo, Is.EqualTo(to));
                Assert.That(dto.Country, Is.EqualTo("Canada"));
                Assert.That(dto.City, Is.EqualTo("Toronto"));
                Assert.That(dto.State, Is.EqualTo("ON"));
                Assert.That(dto.PostalCode, Is.EqualTo("M5H 2N2"));
                Assert.That(dto.AddressLine1, Is.EqualTo("123 Main St"));
                Assert.That(dto.AddressLine2, Is.EqualTo("Suite 100"));
                Assert.That(dto.DisableRegistrationButton, Is.False);
                Assert.That(dto.DisableAddToCalendar, Is.False);
                Assert.That(dto.ZipPositionAfter, Is.True);
                Assert.That(dto.HideEventLocationZipComma, Is.False);
                Assert.That(dto.DateTextOverride, Is.EqualTo("Fall 2026"));
                Assert.That(dto.TimeTextOverride, Is.EqualTo("All Day"));
                Assert.That(dto.ContactName, Is.EqualTo("Alice"));
                Assert.That(dto.ContactEmail, Is.EqualTo("alice@bdo.com"));
                Assert.That(dto.ContactPhone, Is.EqualTo("+1-416-555-0100"));
            });
        }

        #endregion

        #region PresenterDto â€“ DTO property coverage

        [Test]
        public void PresenterDto_SetProperties_RetainsValues()
        {
            var dto = new PresenterDto
            {
                Name = "Jane Doe",
                Title = "Partner",
                Link = "https://example.com/jane",
            };

            Assert.Multiple(() =>
            {
                Assert.That(dto.Name, Is.EqualTo("Jane Doe"));
                Assert.That(dto.Title, Is.EqualTo("Partner"));
                Assert.That(dto.Link, Is.EqualTo("https://example.com/jane"));
            });
        }

        #endregion

        #region EventnWebinarSetting via full iCal generation (mocked empty settings)

        /// <summary>
        /// Mocks the snippet + global settings providers to return empty result sets so that
        /// <c>EventnWebinarSetting()</c> resolves to its default values ("0", false) without
        /// requiring the Kentico IoC container. This lets the full iCal generation path run.
        /// </summary>
        private void SetupEmptySettingsProviders()
        {
            _webChannelSnippetInfoProviderMock
                .Setup(p => p.Get())
                .Returns(BuildEmptyQuery<WebChannelSnippetInfo>().Object);

            _globalSettingsKeyInfoProviderMock
                .Setup(p => p.Get())
                .Returns(BuildEmptyQuery<GlobalsettingskeyInfo>().Object);
        }

        private static Mock<ObjectQuery<T>> BuildEmptyQuery<T>()
            where T : BaseInfo
        {
            var queryMock = new Mock<ObjectQuery<T>>();
            queryMock.Setup(q => q.TopN(It.IsAny<int>())).Returns(queryMock.Object);
            queryMock.As<IEnumerable<T>>()
                .Setup(x => x.GetEnumerator())
                .Returns(() => Enumerable.Empty<T>().GetEnumerator());
            return queryMock;
        }

        [Test]
        public async Task GenerateOutlookEventCardContent_ReturnsICalString_WhenPageFound()
        {
            SetupEmptySettingsProviders();

            var page = BuildEvent(e =>
            {
                e.EventLocationTitle = "HQ";
                e.EventLocationCity = "Toronto";
                e.EventLocationAddress1 = "123 Main St";
                e.CustomBodyText = "<p>Join us</p>";
            });

            _contentServiceMock
                .Setup(x => x.GetPagesAsync<Event>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { page });

            var result = await _service.GenerateOutlookEventCardContent(
                page.SystemFields.ContentItemGUID, null, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Does.Contain("BEGIN:VCALENDAR"));
                Assert.That(result, Does.Contain("BEGIN:VEVENT"));
                Assert.That(result, Does.Contain("SUMMARY:Test Event"));
            });
        }

        [Test]
        public async Task GenerateOutlookWebinarCardContent_ReturnsICalString_WhenPageFound()
        {
            SetupEmptySettingsProviders();

            var page = BuildWebinar(w =>
            {
                w.EventLocationTitle = "Online";
                w.CustomBodyText = "<p>Webinar body</p>";
            });

            _contentServiceMock
                .Setup(x => x.GetPagesAsync<Webinar>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { page });

            var result = await _service.GenerateOutlookWebinarCardContent(
                page.SystemFields.ContentItemGUID, null, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Does.Contain("BEGIN:VCALENDAR"));
                Assert.That(result, Does.Contain("SUMMARY:Test Webinar"));
            });
        }

        [Test]
        public async Task GenerateOutlookEventCardContent_ProducesICal_WithoutCustomBodyText()
        {
            SetupEmptySettingsProviders();

            var page = BuildEvent(e => e.CustomBodyText = null!);

            _contentServiceMock
                .Setup(x => x.GetPagesAsync<Event>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { page });

            var result = await _service.GenerateOutlookEventCardContent(
                page.SystemFields.ContentItemGUID, null, CancellationToken.None);

            Assert.That(result, Does.Contain("BEGIN:VEVENT"));
        }

        [Test]
        public void GenerateOutlookEventCardContent_ThrowsOperationCanceled_WhenTokenCancelled()
        {
            SetupEmptySettingsProviders();

            var page = BuildEvent();

            _contentServiceMock
                .Setup(x => x.GetPagesAsync<Event>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { page });

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            Assert.ThrowsAsync<OperationCanceledException>(
                () => _service.GenerateOutlookEventCardContent(page.SystemFields.ContentItemGUID, null, cts.Token));
        }

        #endregion
    }
}


namespace BDO.GWT.Core.Tests.Services
{
    public class EventWebinarPageServiceCoverageTests
    {
        private Mock<IContentService> _contentServiceMock = null!;
        private Mock<IWebPageUrlRetriever> _pageUrlRetrieverMock = null!;
        private Mock<IInfoProvider<GlobalsettingskeyInfo>> _globalSettingsMock = null!;
        private Mock<IInfoProvider<WebChannelSnippetInfo>> _snippetMock = null!;
        private Mock<IPreferredLanguageRetriever> _preferredLanguageRetrieverMock = null!;
        private Mock<ISocialMediaButtonsService> _socialMediaButtonsServiceMock = null!;
        private EventWebinarPageService _service = null!;

        [SetUp]
        public void Setup()
        {
            _contentServiceMock = new Mock<IContentService>();
            _pageUrlRetrieverMock = new Mock<IWebPageUrlRetriever>();
            _globalSettingsMock = new Mock<IInfoProvider<GlobalsettingskeyInfo>>();
            _snippetMock = new Mock<IInfoProvider<WebChannelSnippetInfo>>();
            _preferredLanguageRetrieverMock = new Mock<IPreferredLanguageRetriever>();
            _socialMediaButtonsServiceMock = new Mock<ISocialMediaButtonsService>();

            _contentServiceMock.Setup(x => x.GetCurrentSiteLegalNameAsync()).Returns("BDO");
            _contentServiceMock.Setup(x => x.GetCurrentSiteLogoImageUrlAsync()).Returns("/logo.png");

            _snippetMock.Setup(p => p.Get()).Returns(BuildEmptyQuery<WebChannelSnippetInfo>().Object);
            _globalSettingsMock.Setup(p => p.Get()).Returns(BuildEmptyQuery<GlobalsettingskeyInfo>().Object);

            _service = new EventWebinarPageService(
                _contentServiceMock.Object,
                _pageUrlRetrieverMock.Object,
                _globalSettingsMock.Object,
                _snippetMock.Object,
                _preferredLanguageRetrieverMock.Object,
                _socialMediaButtonsServiceMock.Object);
        }

        #region Helpers

        private static Mock<ObjectQuery<T>> BuildQuery<T>(params T[] items) where T : BaseInfo
        {
            var queryMock = new Mock<ObjectQuery<T>>();
            queryMock.Setup(q => q.TopN(It.IsAny<int>())).Returns(queryMock.Object);
            queryMock.As<IEnumerable<T>>()
                .Setup(x => x.GetEnumerator())
                .Returns(() => items.AsEnumerable().GetEnumerator());
            return queryMock;
        }

        private static Mock<ObjectQuery<T>> BuildEmptyQuery<T>() where T : BaseInfo => BuildQuery<T>();

        private static Event BuildEvent(Action<Event>? configure = null)
        {
            var page = new Event
            {
                SystemFields = new WebPageFields
                {
                    WebPageItemGUID = Guid.NewGuid(),
                    WebPageItemID = 10,
                    WebPageUrlPath = "/events/test-event",
                    WebPageItemWebsiteChannelId = 1,
                },
                MetadataTitle = "Test Event",
                MetadataDescription = "Event description",
                EventDateFrom = new DateTime(2030, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                EventDateTo = new DateTime(2030, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                EventType = new List<EventType>(),
            };
            configure?.Invoke(page);
            return page;
        }

        private static Webinar BuildWebinar(Action<Webinar>? configure = null)
        {
            var page = new Webinar
            {
                SystemFields = new WebPageFields
                {
                    WebPageItemGUID = Guid.NewGuid(),
                    WebPageItemID = 20,
                    WebPageUrlPath = "/webinars/test-webinar",
                    WebPageItemWebsiteChannelId = 1,
                },
                MetadataTitle = "Test Webinar",
                MetadataDescription = "Webinar description",
                EventDateFrom = new DateTime(2030, 2, 1, 10, 0, 0, DateTimeKind.Utc),
                EventDateTo = new DateTime(2030, 2, 1, 12, 0, 0, DateTimeKind.Utc),
                EventType = new List<EventType>(),
            };
            configure?.Invoke(page);
            return page;
        }

        private void SetupEventPages(params Event[] pages) =>
            _contentServiceMock
                .Setup(x => x.GetPagesAsync<Event>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(pages);

        private void SetupWebinarPages(params Webinar[] pages) =>
            _contentServiceMock
                .Setup(x => x.GetPagesAsync<Webinar>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(pages);

        private static MethodInfo GetPrivateStatic(string name) =>
            typeof(EventWebinarPageService).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new MissingMethodException(nameof(EventWebinarPageService), name);

        private static string BuildLocation(string? title, string? addr1, string? addr2, string? city, string? state, string? zip) =>
            (string)GetPrivateStatic("BuildLocationString").Invoke(null, [title, addr1, addr2, city, state, zip])!;

        private static string? EventPostDescription(Event? page) =>
            (string?)GetPrivateStatic("GetEventPostDescription").Invoke(null, [page]);

        private static string? WebinarPostDescription(Webinar? page) =>
            (string?)GetPrivateStatic("GetWebinarPostDescription").Invoke(null, [page]);

        private OrganizationSchemaMarkupDto OrganizerMarkup(string? url) =>
            (OrganizationSchemaMarkupDto)typeof(EventWebinarPageService)
                .GetMethod("GetEventOrganizerSchemaMarkup", BindingFlags.NonPublic | BindingFlags.Instance)!
                .Invoke(_service, [url])!;

        #endregion

        #region BuildLocationString

        [Test]
        public void BuildLocationString_AllParts_ContainsEachPart()
        {
            var result = BuildLocation("HQ", "1 Main St", "Suite 2", "Toronto", "ON", "M5V");

            Assert.Multiple(() =>
            {
                Assert.That(result, Does.StartWith("HQ"));
                Assert.That(result, Does.Contain("1 Main St"));
                Assert.That(result, Does.Contain("Suite 2"));
                Assert.That(result, Does.Contain("Toronto, ON"));
                Assert.That(result, Does.EndWith("M5V"));
            });
        }

        [Test]
        public void BuildLocationString_TitleOnly_RemovesTrailingEscapedNewline()
        {
            var result = BuildLocation("HQ", null, null, null, null, null);

            Assert.Multiple(()=>{
                Assert.That(result.Trim(), Is.EqualTo("HQ"));
                Assert.That(result, Does.Not.EndWith("\\n"));
            });
        }

        [Test]
        public void BuildLocationString_AllNull_ReturnsEmpty()
        {
            var result = BuildLocation(null, null, null, null, null, null);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void BuildLocationString_StateWithoutCity_HasNoComma()
        {
            var result = BuildLocation("HQ", null, null, null, "ON", null);

            Assert.Multiple(() =>
            {
                Assert.That(result, Does.Contain("ON"));
                Assert.That(result, Does.Not.Contain(","));
            });
        }

        [Test]
        public void BuildLocationString_CityWithoutState_HasNoComma()
        {
            var result = BuildLocation("HQ", "1 Main St", null, "Toronto", null, null);

            Assert.Multiple(() =>
            {
                Assert.That(result, Does.Contain("Toronto"));
                Assert.That(result, Does.Not.Contain(","));
            });
        }

        [Test]
        public void BuildLocationString_DoesNotContainDoubleSpaces()
        {
            var result = BuildLocation("HQ", null, null, "Toronto", null, "M5V");

            Assert.That(result, Does.Not.Contain("  "));
        }

        #endregion

        #region Post description fallback

        [Test]
        public void GetEventPostDescription_NullPage_ReturnsNull()
        {
            Assert.That(EventPostDescription(null), Is.Null);
        }

        [Test]
        public void GetEventPostDescription_PrefersMetadataDescription()
        {
            var page = BuildEvent(e =>
            {
                e.MetadataDescription = "meta";
                e.EventPostDescription = "post";
            });

            Assert.That(EventPostDescription(page), Is.EqualTo("meta"));
        }

        [Test]
        public void GetEventPostDescription_FallsBackToEventPostDescription()
        {
            var page = BuildEvent(e =>
            {
                e.MetadataDescription = " ";
                e.EventPostDescription = "post";
            });

            Assert.That(EventPostDescription(page), Is.EqualTo("post"));
        }

        [Test]
        public void GetEventPostDescription_FallsBackToCpeDescription()
        {
            var page = BuildEvent(e =>
            {
                e.MetadataDescription = null!;
                e.EventPostDescription = "";
                e.CpeDescription = "cpe";
            });

            Assert.That(EventPostDescription(page), Is.EqualTo("cpe"));
        }

        [Test]
        public void GetEventPostDescription_FallsBackToCustomBodyText()
        {
            var page = BuildEvent(e =>
            {
                e.MetadataDescription = null!;
                e.EventPostDescription = null!;
                e.CpeDescription = null!;
                e.CustomBodyText = "body";
            });

            Assert.That(EventPostDescription(page), Is.EqualTo("body"));
        }

        [Test]
        public void GetWebinarPostDescription_NullPage_ReturnsNull()
        {
            Assert.That(WebinarPostDescription(null), Is.Null);
        }

        [Test]
        public void GetWebinarPostDescription_PrefersMetadataDescription()
        {
            Assert.That(WebinarPostDescription(BuildWebinar(w => w.MetadataDescription = "meta")), Is.EqualTo("meta"));
        }

        [Test]
        public void GetWebinarPostDescription_FallsBackThroughChain()
        {
            var post = BuildWebinar(w => { w.MetadataDescription = ""; w.EventPostDescription = "post"; });
            var cpe = BuildWebinar(w => { w.MetadataDescription = ""; w.EventPostDescription = ""; w.CpeDescription = "cpe"; });
            var body = BuildWebinar(w => { w.MetadataDescription = ""; w.EventPostDescription = ""; w.CpeDescription = ""; w.CustomBodyText = "body"; });

            Assert.Multiple(() =>
            {
                Assert.That(WebinarPostDescription(post), Is.EqualTo("post"));
                Assert.That(WebinarPostDescription(cpe), Is.EqualTo("cpe"));
                Assert.That(WebinarPostDescription(body), Is.EqualTo("body"));
            });
        }

        [Test]
        public void GetWebinarPostDescription_AllEmpty_ReturnsLastCandidate()
        {
            var page = BuildWebinar(w =>
            {
                w.MetadataDescription = "";
                w.EventPostDescription = "";
                w.CpeDescription = "";
                w.CustomBodyText = null!;
            });

            Assert.That(WebinarPostDescription(page), Is.Null);
        }

        #endregion

        #region Organizer schema markup

        [Test]
        public void GetEventOrganizerSchemaMarkup_ValidAbsoluteUrl_ReturnsSchemeAndAuthority()
        {
            var result = OrganizerMarkup("https://www.bdo.com:8443/events/x?y=1");

            Assert.Multiple(() =>
            {
                Assert.That(result.Url, Is.EqualTo("https://www.bdo.com:8443"));
                Assert.That(result.Name, Is.EqualTo("BDO"));
                Assert.That(result.Logo, Is.EqualTo("/logo.png"));
            });
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]

        public void GetEventOrganizerSchemaMarkup_InvalidOrMissingUrl_ReturnsNullUrl(string? url)
        {
            var result = OrganizerMarkup(url);

            Assert.That(result.Url, Is.Null);
        }

        [Test]
        public void GetEventOrganizerSchemaMarkup_NullSiteNameAndLogo_DefaultToEmpty()
        {
            _contentServiceMock.Setup(x => x.GetCurrentSiteLegalNameAsync()).Returns((string)null!);
            _contentServiceMock.Setup(x => x.GetCurrentSiteLogoImageUrlAsync()).Returns((string)null!);

            var result = OrganizerMarkup("https://bdo.com");

            Assert.Multiple(() =>
            {
                Assert.That(result.Name, Is.Empty);
                Assert.That(result.Logo, Is.Empty);
            });
        }

        #endregion

        #region iCal generation

        [Test]
        public async Task GenerateOutlookEventCardContent_IncludesLocation_WhenLocationPartsPresent()
        {
            var page = BuildEvent(e =>
            {
                e.EventLocationTitle = "HQ";
                e.EventLocationCity = "Toronto";
                e.EventLocationState = "ON";
            });
            SetupEventPages(page);

            var result = await _service.GenerateOutlookEventCardContent(Guid.NewGuid(), null, CancellationToken.None);

            Assert.That(result, Does.Contain("LOCATION:"));
            Assert.That(result, Does.Contain("Toronto"));
        }

        [Test]
        public async Task GenerateOutlookEventCardContent_OmitsLocation_WhenAllLocationPartsEmpty()
        {
            SetupEventPages(BuildEvent());

            var result = await _service.GenerateOutlookEventCardContent(Guid.NewGuid(), null, CancellationToken.None);

            Assert.That(result, Does.Not.Contain("LOCATION:"));
        }

        [Test]
        public async Task GenerateOutlookEventCardContent_AddsHtmlAltDescription_WhenCustomBodyPresent()
        {
            SetupEventPages(BuildEvent(e => e.CustomBodyText = "<p>Hello</p>"));

            var result = await _service.GenerateOutlookEventCardContent(Guid.NewGuid(), null, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Does.Contain("X-ALT-DESC"));
                Assert.That(result, Does.Contain("DESCRIPTION:"));
            });
        }

        [Test]
        public async Task GenerateOutlookEventCardContent_NoAlarm_WhenSettingsDefault()
        {
            SetupEventPages(BuildEvent());

            var result = await _service.GenerateOutlookEventCardContent(Guid.NewGuid(), null, CancellationToken.None);

            Assert.That(result, Does.Not.Contain("BEGIN:VALARM"));
        }

        [Test]
        public async Task GenerateOutlookEventCardContent_HasProductIdAndVersion()
        {
            SetupEventPages(BuildEvent());

            var result = await _service.GenerateOutlookEventCardContent(Guid.NewGuid(), null, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Does.Contain("PRODID:-//BDO.Calendar//EN"));
                Assert.That(result, Does.Contain("VERSION:2.0"));
                Assert.That(result, Does.Contain("END:VCALENDAR"));
            });
        }

        [Test]
        public async Task GenerateOutlookEventCardContent_UsesUtcStartAndEnd()
        {
            SetupEventPages(BuildEvent());

            var result = await _service.GenerateOutlookEventCardContent(Guid.NewGuid(), null, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Does.Contain("20300101T100000"));
                Assert.That(result, Does.Contain("20300101T120000"));
            });
        }

        [Test]
        public async Task GenerateOutlookEventCardContent_UsesFirstPage_WhenMultipleReturned()
        {
            SetupEventPages(BuildEvent(e => e.MetadataTitle = "First"), BuildEvent(e => e.MetadataTitle = "Second"));

            var result = await _service.GenerateOutlookEventCardContent(Guid.NewGuid(), null, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Does.Contain("SUMMARY:First"));
                Assert.That(result, Does.Not.Contain("SUMMARY:Second"));
            });
        }

        [Test]
        public async Task GenerateOutlookWebinarCardContent_OmitsAltDescription_WhenNoCustomBody()
        {
            SetupWebinarPages(BuildWebinar(w => w.CustomBodyText = null!));

            var result = await _service.GenerateOutlookWebinarCardContent(Guid.NewGuid(), null, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Does.Contain("BEGIN:VEVENT"));
                Assert.That(result, Does.Not.Contain("X-ALT-DESC"));
            });
        }

        [Test]
        public async Task GenerateOutlookWebinarCardContent_IncludesLocation_WhenCityPresent()
        {
            SetupWebinarPages(BuildWebinar(w =>
            {
                w.EventLocationTitle = "Online";
                w.EventLocationCity = "Chicago";
            }));

            var result = await _service.GenerateOutlookWebinarCardContent(Guid.NewGuid(), null, CancellationToken.None);

            Assert.That(result, Does.Contain("Chicago"));
        }

        [Test]
        public async Task GenerateOutlookWebinarCardContent_NoAlarm_WhenSettingsDefault()
        {
            SetupWebinarPages(BuildWebinar());

            var result = await _service.GenerateOutlookWebinarCardContent(Guid.NewGuid(), null, CancellationToken.None);

            Assert.That(result, Does.Not.Contain("BEGIN:VALARM"));
        }

        [Test]
        public async Task GenerateOutlookEventCardContent_AddsAlarm_WhenGlobalSettingHasMinutesToTrigger()
        {
            _globalSettingsMock
                .Setup(p => p.Get())
                .Returns(BuildQuery<GlobalsettingskeyInfo>(new TestGlobalSettingsKeyInfo
                {
                    Minutestotriggercalendarreminder = 15,
                    Displaylocationtitleinasidebar = false,
                }).Object);
            SetupEventPages(BuildEvent());

            var result = await _service.GenerateOutlookEventCardContent(Guid.NewGuid(), null, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Does.Contain("BEGIN:VALARM"));
                Assert.That(result, Does.Contain("TRIGGER:-PT15M"));
            });
        }

        [Test]
        public async Task GenerateOutlookWebinarCardContent_UsesChannelSettingMinutes_BeforeGlobal()
        {
            _snippetMock
                .Setup(p => p.Get())
                .Returns(BuildQuery<WebChannelSnippetInfo>(new TestWebChannelSnippetInfo
                {
                    MinutestotriggercalendarreminderInheritfromglobal = false,
                    Minutestotriggercalendarreminder = 30,
                    DisplaylocationtitleinasidebarInheritfromglobal = true,
                }).Object);
            _globalSettingsMock
                .Setup(p => p.Get())
                .Returns(BuildQuery<GlobalsettingskeyInfo>(new TestGlobalSettingsKeyInfo
                {
                    Minutestotriggercalendarreminder = 5,
                    Displaylocationtitleinasidebar = false,
                }).Object);
            SetupWebinarPages(BuildWebinar());

            var result = await _service.GenerateOutlookWebinarCardContent(Guid.NewGuid(), null, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Does.Contain("BEGIN:VALARM"));
                Assert.That(result, Does.Contain("TRIGGER:-PT30M"));
                Assert.That(result, Does.Not.Contain("TRIGGER:-PT5M"));
            });
        }

        [Test]
        public void GenerateOutlookWebinarCardContent_ThrowsOperationCanceled_WhenTokenCancelled()
        {
            SetupWebinarPages(BuildWebinar());
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            Assert.CatchAsync<OperationCanceledException>(() =>
                _service.GenerateOutlookWebinarCardContent(Guid.NewGuid(), null, cts.Token));
        }

        [Test]
        public async Task GenerateOutlookWebinarCardContent_ReturnsNull_WhenEmptyResult()
        {
            SetupWebinarPages();

            var result = await _service.GenerateOutlookWebinarCardContent(Guid.NewGuid(), null, CancellationToken.None);

            Assert.That(result, Is.Null);
        }

        #endregion

        private sealed class TestGlobalSettingsKeyInfo : GlobalsettingskeyInfo
        {
            public override int Minutestotriggercalendarreminder { get; set; }
            public override bool Displaylocationtitleinasidebar { get; set; }
        }

        private sealed class TestWebChannelSnippetInfo : WebChannelSnippetInfo
        {
            public override int Minutestotriggercalendarreminder { get; set; }
            public override bool MinutestotriggercalendarreminderInheritfromglobal { get; set; }
            public override bool Displaylocationtitleinasidebar { get; set; }
            public override bool DisplaylocationtitleinasidebarInheritfromglobal { get; set; }
        }
    }
}
