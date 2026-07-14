using System.Reflection;
using BDO;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Retrievers;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.Services;
using BDO.GWT.Xperience.Core.Services;
using CMS.ContentEngine;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Kentico.Content.Web.Mvc.Routing;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace BDO.GWT.Core.Tests.Services
{
    public class EventWebinarCardsServiceTests
    {
        private Mock<ICardsService> _cardsServiceMock = null!;
        private Mock<IContentQueryExecutor> _queryExecutorMock = null!;
        private Mock<IContentQueryModelTypeMapper> _typeMapperMock = null!;
        private Mock<IContentService> _contentServiceMock = null!;
        private Mock<IWebsiteChannelContext> _channelContextMock = null!;
        private Mock<IWebPageUrlRetriever> _urlRetrieverMock = null!;
        private Mock<ITimeZoneService> _timeZoneServiceMock = null!;
        private EventWebinarCardsService _service = null!;

        [SetUp]
        public void Setup()
        {
            _cardsServiceMock = new Mock<ICardsService>();
            _queryExecutorMock = new Mock<IContentQueryExecutor>();
            _typeMapperMock = new Mock<IContentQueryModelTypeMapper>();
            _contentServiceMock = new Mock<IContentService>();
            _channelContextMock = new Mock<IWebsiteChannelContext>();
            _urlRetrieverMock = new Mock<IWebPageUrlRetriever>();
            _timeZoneServiceMock = new Mock<ITimeZoneService>();

            _channelContextMock.Setup(x => x.WebsiteChannelID).Returns(1);
            _channelContextMock.Setup(x => x.WebsiteChannelName).Returns("Test");
            _timeZoneServiceMock
                .Setup(x => x.ConvertToSiteDateTime(It.IsAny<DateTime>()))
                .Returns<DateTime>(d => d);

            _service = new EventWebinarCardsService(
                _cardsServiceMock.Object,
                _queryExecutorMock.Object,
                _typeMapperMock.Object,
                _contentServiceMock.Object,
                _channelContextMock.Object,
                _urlRetrieverMock.Object,
                _timeZoneServiceMock.Object
                );
        }


        [Test]
        public void Service_ImplementsInterface()
        {
            Assert.That(typeof(IEventWebinarCardsService).IsAssignableFrom(typeof(EventWebinarCardsService)), Is.True);
        }

        [Test]
        public void Interface_DeclaresExpectedMembers()
        {
            var t = typeof(IEventWebinarCardsService);
            Assert.Multiple(() =>
            {
                Assert.That(t.GetMethod(nameof(IEventWebinarCardsService.CreateFiltersAsync)), Is.Not.Null);
                Assert.That(t.GetMethod(nameof(IEventWebinarCardsService.GetEventWebinarCardDtosAsync)), Is.Not.Null);
                Assert.That(t.GetMethod(nameof(IEventWebinarCardsService.GetFilteredItemsAsync)), Is.Not.Null);
            });
        }

        [Test]
        public void Service_HasExpectedConstructor()
        {
            var ctor = typeof(EventWebinarCardsService).GetConstructors(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault();
            Assert.That(ctor, Is.Not.Null);
            Assert.That(ctor!.GetParameters(), Has.Length.GreaterThan(0));
        }

        // ── CreateFiltersAsync ──────────────────────────────────────────────

        [Test]
        public async Task CreateFiltersAsync_DelegatesTo_CardsService()
        {
            var filters = new HashSet<string> { "eventType" };
            var expected = new[]
            {
                new FilterItemDto<OptionDto> { Name = "eventType", Type = "select", Options = Array.Empty<OptionDto>() }
            };

            _cardsServiceMock
                .Setup(x => x.CreateFiltersAsync(filters, "/path", It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _service.CreateFiltersAsync(filters, "/path", CancellationToken.None);

            Assert.That(result, Is.SameAs(expected));
            _cardsServiceMock.Verify(
                x => x.CreateFiltersAsync(filters, "/path", It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public void CreateFiltersAsync_PropagatesException()
        {
            _cardsServiceMock
                .Setup(x => x.CreateFiltersAsync(It.IsAny<HashSet<string>>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("boom"));

            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.CreateFiltersAsync(new HashSet<string>(), null, CancellationToken.None));
        }

        [Test]
        public async Task CreateFiltersAsync_PassesCancellationToken()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            _cardsServiceMock
                .Setup(x => x.CreateFiltersAsync(It.IsAny<HashSet<string>>(), It.IsAny<string?>(), token))
                .ReturnsAsync(Array.Empty<FilterItemDto<OptionDto>>());

            await _service.CreateFiltersAsync(new HashSet<string>(), null, token);

            _cardsServiceMock.Verify(
                x => x.CreateFiltersAsync(It.IsAny<HashSet<string>>(), It.IsAny<string?>(), token),
                Times.Once);
        }

        // ── GetEventWebinarCardDtosAsync ────────────────────────────────────

        [Test]
        public async Task GetEventWebinarCardDtosAsync_ReturnsEmpty_WhenInputIsEmpty()
        {
            var result = await _service.GetEventWebinarCardDtosAsync(
                new List<IEventOrWebinar>(),
                _ => false,
                CancellationToken.None);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetEventWebinarCardDtosAsync_SkipsItems_WhenUrlRetrievalFails()
        {
            var item = new Mock<IEventOrWebinar>();
            item.Setup(x => x.SystemFields).Returns(new WebPageFields { WebPageItemGUID = Guid.NewGuid() });
            item.Setup(x => x.MetadataTitle).Returns("Title");

            _urlRetrieverMock
                .Setup(x => x.Retrieve(It.IsAny<IWebPageFieldsSource>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("url fail"));

            var result = await _service.GetEventWebinarCardDtosAsync(
                new List<IEventOrWebinar> { item.Object },
                _ => false,
                CancellationToken.None);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetEventWebinarCardDtosAsync_MapsCoreFields_WhenUrlIsResolved()
        {
            _contentServiceMock.Setup(x => x.GetCurrentLanguage()).Returns("en");

            var guid = Guid.NewGuid();
            var item = new Mock<IEventOrWebinar>();
            item.SetupGet(x => x.SystemFields).Returns(new WebPageFields { WebPageItemGUID = guid });
            item.SetupGet(x => x.MetadataTitle).Returns("Event title");
            item.SetupGet(x => x.MetadataDescription).Returns("<p>Description <strong>text</strong></p>");
            item.SetupGet(x => x.EventLocationCity).Returns("London");
            item.SetupGet(x => x.EventDateFrom).Returns(new DateTime(2026, 01, 01, 10, 00, 00, DateTimeKind.Utc));
            item.SetupGet(x => x.EventDateTo).Returns(new DateTime(2026, 01, 02, 11, 00, 00, DateTimeKind.Utc));
            item.SetupGet(x => x.EventType).Returns(new[] { new EventType { Name = "Webinar" } });

            _urlRetrieverMock
                .Setup(x => x.Retrieve(item.Object, "en", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new WebPageUrl("~/events/test", "https://example.com/events/test"));

            var result = await _service.GetEventWebinarCardDtosAsync(
                new List<IEventOrWebinar> { item.Object },
                e => e.SystemFields.WebPageItemGUID == guid,
                CancellationToken.None);

            Assert.That(result, Has.Length.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(result[0].IsFeatured, Is.True);
                Assert.That(result[0].Type, Is.EqualTo("Webinar"));
                Assert.That(result[0].Title, Is.EqualTo("Event title"));
                Assert.That(result[0].Description, Is.EqualTo("Description text"));
                Assert.That(result[0].Location, Is.EqualTo("London"));
                Assert.That(result[0].DateTimeTo, Is.Not.Null);
                Assert.That(result[0].Link.Title, Is.EqualTo("Webinar"));
                Assert.That(result[0].Link.URL, Is.EqualTo("/events/test"));
            });
        }

        [Test]
        public async Task GetEventWebinarCardDtosAsync_SetsNullDateToLocationAndDescription_WhenNotProvided()
        {
            _contentServiceMock.Setup(x => x.GetCurrentLanguage()).Returns("en");

            var guid = Guid.NewGuid();
            var sameDay = new DateTime(2026, 01, 01, 10, 00, 00, DateTimeKind.Utc);

            var item = new Mock<IEventOrWebinar>();
            item.SetupGet(x => x.SystemFields).Returns(new WebPageFields { WebPageItemGUID = guid });
            item.SetupGet(x => x.MetadataTitle).Returns("Event title");
            item.SetupGet(x => x.MetadataDescription).Returns(string.Empty);
            item.SetupGet(x => x.EventLocationCity).Returns(string.Empty);
            item.SetupGet(x => x.EventDateFrom).Returns(sameDay);
            item.SetupGet(x => x.EventDateTo).Returns(sameDay.AddHours(2));
            item.SetupGet(x => x.EventType).Returns(Array.Empty<EventType>());

            _urlRetrieverMock
                .Setup(x => x.Retrieve(item.Object, "en", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new WebPageUrl("~/events/test", "https://example.com/events/test"));

            var result = await _service.GetEventWebinarCardDtosAsync(
                new List<IEventOrWebinar> { item.Object },
                _ => false,
                CancellationToken.None);

            Assert.That(result, Has.Length.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(result[0].DateTimeTo, Is.Null);
                Assert.That(result[0].Location, Is.Null);
                Assert.That(result[0].Description, Is.Null);
                Assert.That(result[0].Link.Title, Is.EqualTo(string.Empty));
            });
        }

        [Test]
        public async Task GetEventWebinarCardDtosAsync_UsesCurrentLanguage_ForUrlRetrieval()
        {
            _contentServiceMock.Setup(x => x.GetCurrentLanguage()).Returns("fr");

            var guid = Guid.NewGuid();
            var item = new Mock<IEventOrWebinar>();
            item.SetupGet(x => x.SystemFields).Returns(new WebPageFields { WebPageItemGUID = guid });
            item.SetupGet(x => x.MetadataTitle).Returns("Titre");
            item.SetupGet(x => x.EventDateFrom).Returns(new DateTime(2026, 01, 01, 10, 00, 00, DateTimeKind.Utc));
            item.SetupGet(x => x.EventDateTo).Returns(new DateTime(2026, 01, 02, 10, 00, 00, DateTimeKind.Utc));
            item.SetupGet(x => x.EventType).Returns(Array.Empty<EventType>());

            _urlRetrieverMock
                .Setup(x => x.Retrieve(item.Object, "fr", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new WebPageUrl("~/events/fr", "https://example.com/events/fr"));

            await _service.GetEventWebinarCardDtosAsync(
                new List<IEventOrWebinar> { item.Object },
                _ => false,
                CancellationToken.None);

            _urlRetrieverMock.Verify(x => x.Retrieve(item.Object, "fr", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetEventWebinarCardDtosAsync_ReturnsOnlyItems_WithResolvedUrls_WhenMixedResults()
        {
            _contentServiceMock.Setup(x => x.GetCurrentLanguage()).Returns("en");

            var guidOk = Guid.NewGuid();
            var guidFail = Guid.NewGuid();

            var okItem = new Mock<IEventOrWebinar>();
            okItem.SetupGet(x => x.SystemFields).Returns(new WebPageFields { WebPageItemGUID = guidOk });
            okItem.SetupGet(x => x.MetadataTitle).Returns("OK title");
            okItem.SetupGet(x => x.EventDateFrom).Returns(new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc));
            okItem.SetupGet(x => x.EventDateTo).Returns(new DateTime(2026, 1, 2, 10, 0, 0, DateTimeKind.Utc));
            okItem.SetupGet(x => x.EventType).Returns(Array.Empty<EventType>());

            var failedItem = new Mock<IEventOrWebinar>();
            failedItem.SetupGet(x => x.SystemFields).Returns(new WebPageFields { WebPageItemGUID = guidFail });
            failedItem.SetupGet(x => x.MetadataTitle).Returns("Fail title");
            failedItem.SetupGet(x => x.EventDateFrom).Returns(new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc));
            failedItem.SetupGet(x => x.EventDateTo).Returns(new DateTime(2026, 1, 2, 10, 0, 0, DateTimeKind.Utc));
            failedItem.SetupGet(x => x.EventType).Returns(Array.Empty<EventType>());

            _urlRetrieverMock
                .Setup(x => x.Retrieve(okItem.Object, "en", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new WebPageUrl("~/events/ok", "https://example.com/events/ok"));

            _urlRetrieverMock
                .Setup(x => x.Retrieve(failedItem.Object, "en", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("no url"));

            var result = await _service.GetEventWebinarCardDtosAsync(
                new List<IEventOrWebinar> { okItem.Object, failedItem.Object },
                _ => false,
                CancellationToken.None);

            Assert.That(result, Has.Length.EqualTo(1));
            Assert.That(result[0].Title, Is.EqualTo("OK title"));
        }

        [Test]
        public async Task GetEventWebinarCardDtosAsync_PassesCancellationToken_ToUrlRetriever()
        {
            _contentServiceMock.Setup(x => x.GetCurrentLanguage()).Returns("en");

            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            var item = new Mock<IEventOrWebinar>();
            item.SetupGet(x => x.SystemFields).Returns(new WebPageFields { WebPageItemGUID = Guid.NewGuid() });
            item.SetupGet(x => x.MetadataTitle).Returns("Title");
            item.SetupGet(x => x.EventDateFrom).Returns(new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc));
            item.SetupGet(x => x.EventDateTo).Returns(new DateTime(2026, 1, 2, 10, 0, 0, DateTimeKind.Utc));
            item.SetupGet(x => x.EventType).Returns(Array.Empty<EventType>());

            _urlRetrieverMock
                .Setup(x => x.Retrieve(item.Object, "en", token))
                .ReturnsAsync(new WebPageUrl("~/events/token", "https://example.com/events/token"));

            await _service.GetEventWebinarCardDtosAsync(
                new List<IEventOrWebinar> { item.Object },
                _ => false,
                token);

            _urlRetrieverMock.Verify(x => x.Retrieve(item.Object, "en", token), Times.Once);
        }
    }
}
