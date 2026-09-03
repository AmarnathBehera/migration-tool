using System.Reflection;
using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.LiveSite.DTOs.Pages;
using BDO.GWT.Web.Core.Services;
using CMS.DataEngine;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Kentico.Content.Web.Mvc.Routing;
using Moq;

namespace BDO.GWT.Core.Tests.Services
{
    public class EventWebinarPageServiceTests
    {
        private Mock<IContentService> _contentServiceMock = null!;
        private Mock<IWebsiteChannelContext> _websiteChannelContextMock = null!;
        private Mock<IWebPageUrlRetriever> _pageUrlRetrieverMock = null!;
        private Mock<IInfoProvider<WebChannelSettingsInfo>> _webChannelSettingsInfoProviderMock = null!;
        private Mock<IInfoProvider<GlobalsettingskeyInfo>> _globalSettingsKeyInfoProviderMock = null!;
        private Mock<IInfoProvider<WebChannelSnippetInfo>> _webChannelSnippetInfoProviderMock = null!;
        private Mock<IPreferredLanguageRetriever> _preferredLanguageRetrieverMock = null!;
        private Mock<ISocialMediaButtonsService> _socialMediaButtonsServiceMock = null!;
        private EventWebinarPageService _service = null!;

        [SetUp]
        public void Setup()
        {
            _contentServiceMock = new Mock<IContentService>();
            _websiteChannelContextMock = new Mock<IWebsiteChannelContext>();
            _pageUrlRetrieverMock = new Mock<IWebPageUrlRetriever>();
            _webChannelSettingsInfoProviderMock = new Mock<IInfoProvider<WebChannelSettingsInfo>>();
            _globalSettingsKeyInfoProviderMock = new Mock<IInfoProvider<GlobalsettingskeyInfo>>();
            _webChannelSnippetInfoProviderMock = new Mock<IInfoProvider<WebChannelSnippetInfo>>();
            _preferredLanguageRetrieverMock = new Mock<IPreferredLanguageRetriever>();
            _socialMediaButtonsServiceMock = new Mock<ISocialMediaButtonsService>();

            _websiteChannelContextMock
                .Setup(x => x.WebsiteChannelID)
                .Returns(1);

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
                _websiteChannelContextMock.Object,
                _pageUrlRetrieverMock.Object,
                _webChannelSettingsInfoProviderMock.Object,
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
                Assert.That(parameters, Has.Member(typeof(IWebsiteChannelContext)));
                Assert.That(parameters, Has.Member(typeof(IWebPageUrlRetriever)));
                Assert.That(parameters, Has.Member(typeof(IInfoProvider<WebChannelSettingsInfo>)));
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

        #region GenerateOutlookEventCardContent – null path (no CMS context required)

        [Test]
        public async Task GenerateOutlookEventCardContent_ReturnsNull_WhenNoPageFound()
        {
            // GetPagesAsync returns empty → page is null → iCal method returns null early
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

        #region GenerateOutlookWebinarCardContent – null path (no CMS context required)

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

        #region EventWebinarDto – DTO property coverage

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

        #region EventWebinarDetailsRegistrationCardDto – DTO property coverage

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

        #region PresenterDto – DTO property coverage

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

        [Test]
        public void PresenterDto_Link_IsNullable()
        {
            var dto = new PresenterDto { Name = "John", Title = "Manager" };
            Assert.That(dto.Link, Is.Null);
        }

        #endregion
    }
}