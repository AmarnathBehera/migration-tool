using System.Reflection;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.Services;
using BDO.GWT.Xperience.Core.Services;
using CMS.ContentEngine;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc.Routing;
using Moq;

namespace BDO.GWT.Core.Tests.Services;

[TestFixture]
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

        _channelContextMock
            .Setup(x => x.WebsiteChannelName)
            .Returns("Test");

        _timeZoneServiceMock
            .Setup(x => x.ConvertToSiteDateTime(It.IsAny<DateTime>()))
            .Returns<DateTime>(dateTime => dateTime);

        _service = new EventWebinarCardsService(
            _cardsServiceMock.Object,
            _queryExecutorMock.Object,
            _typeMapperMock.Object,
            _contentServiceMock.Object,
            _channelContextMock.Object,
            _urlRetrieverMock.Object,
            _timeZoneServiceMock.Object);
    }

    #region Constructor / Interface

    [Test]
    public void Service_ImplementsInterface()
    {
        Assert.That(
            typeof(IEventWebinarCardsService)
                .IsAssignableFrom(typeof(EventWebinarCardsService)),
            Is.True);
    }

    [Test]
    public void Interface_DeclaresExpectedMembers()
    {
        var type = typeof(IEventWebinarCardsService);

        Assert.Multiple(() =>
        {
            Assert.That(
                type.GetMethod(nameof(IEventWebinarCardsService.CreateFiltersAsync)),
                Is.Not.Null);

            Assert.That(
                type.GetMethod(nameof(IEventWebinarCardsService.GetEventWebinarCardDtosAsync)),
                Is.Not.Null);

            Assert.That(
                type.GetMethod(nameof(IEventWebinarCardsService.GetFilteredItemsAsync)),
                Is.Not.Null);
        });
    }

    [Test]
    public void Service_HasExpectedConstructor()
    {
        var constructors = typeof(EventWebinarCardsService)
            .GetConstructors(BindingFlags.Public | BindingFlags.Instance);

        Assert.That(constructors, Is.Not.Empty);

        var constructor = constructors.Single();

        Assert.That(constructor.GetParameters(), Has.Length.EqualTo(7));
    }

    #endregion

    #region CreateFiltersAsync

    [Test]
    public async Task CreateFiltersAsync_ReturnsResult_FromCardsService()
    {
        var filters = new HashSet<string>
        {
            "eventType"
        };

        var expected = new[]
        {
            new FilterItemDto<OptionDto>
            {
                Name = "eventType",
                Type = "select",
                Options = Array.Empty<OptionDto>()
            }
        };

        _cardsServiceMock
            .Setup(x => x.CreateFiltersAsync(
                filters,
                "/path",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _service.CreateFiltersAsync(
            filters,
            "/path",
            CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));

        _cardsServiceMock.Verify(
            x => x.CreateFiltersAsync(
                filters,
                "/path",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public void CreateFiltersAsync_PropagatesException()
    {
        _cardsServiceMock
            .Setup(x => x.CreateFiltersAsync(
                It.IsAny<HashSet<string>>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
                await _service.CreateFiltersAsync(
                    new HashSet<string>(),
                    null,
                    CancellationToken.None));
    }

    [Test]
    public async Task CreateFiltersAsync_PassesCancellationToken()
    {
        using var cts = new CancellationTokenSource();

        var token = cts.Token;

        _cardsServiceMock
            .Setup(x => x.CreateFiltersAsync(
                It.IsAny<HashSet<string>>(),
                It.IsAny<string?>(),
                token))
            .ReturnsAsync(Array.Empty<FilterItemDto<OptionDto>>());

        await _service.CreateFiltersAsync(
            new HashSet<string>(),
            null,
            token);

        _cardsServiceMock.Verify(
            x => x.CreateFiltersAsync(
                It.IsAny<HashSet<string>>(),
                It.IsAny<string?>(),
                token),
            Times.Once);
    }

    #endregion

    #region GetEventWebinarCardDtosAsync

    [Test]
    public async Task GetEventWebinarCardDtosAsync_ReturnsEmpty_WhenInputIsEmpty()
    {
        var result = await _service.GetEventWebinarCardDtosAsync(
            Array.Empty<IEventOrWebinar>(),
            "en",
            _ => false,
            CancellationToken.None);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetEventWebinarCardDtosAsync_SkipsItem_WhenUrlRetrievalFails()
    {
        var guid = Guid.NewGuid();

        var item = CreateEventOrWebinarMock(
            guid,
            title: "Test Event");

        _urlRetrieverMock
            .Setup(x => x.Retrieve(
                item.Object,
                "en",
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("URL retrieval failed"));

        var result = await _service.GetEventWebinarCardDtosAsync(
            new[] { item.Object },
            "en",
            _ => false,
            CancellationToken.None);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetEventWebinarCardDtosAsync_MapsEventCorrectly()
    {
        var guid = Guid.NewGuid();

        var from = new DateTime(
            2026,
            1,
            1,
            10,
            0,
            0,
            DateTimeKind.Utc);

        var to = new DateTime(
            2026,
            1,
            2,
            11,
            0,
            0,
            DateTimeKind.Utc);

        var item = CreateEventOrWebinarMock(
            guid,
            title: "Event title",
            description: "<p>Description <strong>text</strong></p>",
            location: "London",
            from: from,
            to: to,
            eventTypes:
            [
                new EventType
                {
                    Name = "Webinar",
                    Calltoactiontext = "Register"
                }
            ]);

        _urlRetrieverMock
            .Setup(x => x.Retrieve(
                item.Object,
                "en",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new WebPageUrl(
                    "~/events/test",
                    "https://example.com/events/test"));

        var result = await _service.GetEventWebinarCardDtosAsync(
            new[] { item.Object },
            "en",
            _ => true,
            CancellationToken.None);

        Assert.That(result, Has.Length.EqualTo(1));

        var dto = result[0];

        Assert.Multiple(() =>
        {
            Assert.That(dto.IsFeatured, Is.True);
            Assert.That(dto.Type, Is.EqualTo("Webinar"));
            Assert.That(dto.Title, Is.EqualTo("Event title"));
            Assert.That(dto.Description, Is.EqualTo("Description text"));
            Assert.That(dto.Location, Is.EqualTo("London"));
            Assert.That(dto.DateTime, Is.EqualTo(from));
            Assert.That(dto.DateTimeTo, Is.EqualTo(to));
            Assert.That(dto.Link.Title, Is.EqualTo("Register"));
            Assert.That(dto.Link.URL, Is.EqualTo("/events/test"));
        });
    }

    [Test]
    public async Task GetEventWebinarCardDtosAsync_SetsDateTimeToNull_WhenDatesAreSameDay()
    {
        var guid = Guid.NewGuid();

        var from = new DateTime(
            2026,
            1,
            1,
            10,
            0,
            0,
            DateTimeKind.Utc);

        var to = new DateTime(
            2026,
            1,
            1,
            12,
            0,
            0,
            DateTimeKind.Utc);

        var item = CreateEventOrWebinarMock(
            guid,
            title: "Same day event",
            from: from,
            to: to);

        SetupSuccessfulUrl(item, "en");

        var result = await _service.GetEventWebinarCardDtosAsync(
            new[] { item.Object },
            "en",
            _ => false,
            CancellationToken.None);

        Assert.That(result, Has.Length.EqualTo(1));
        Assert.That(result[0].DateTimeTo, Is.Null);
    }

    [Test]
    public async Task GetEventWebinarCardDtosAsync_SetsLocationNull_WhenLocationIsEmpty()
    {
        var item = CreateEventOrWebinarMock(
            Guid.NewGuid(),
            title: "Event",
            location: string.Empty);

        SetupSuccessfulUrl(item, "en");

        var result = await _service.GetEventWebinarCardDtosAsync(
            new[] { item.Object },
            "en",
            _ => false,
            CancellationToken.None);

        Assert.That(result, Has.Length.EqualTo(1));
        Assert.That(result[0].Location, Is.Null);
    }

    [Test]
    public async Task GetEventWebinarCardDtosAsync_SetsDescriptionNull_WhenDescriptionIsEmpty()
    {
        var item = CreateEventOrWebinarMock(
            Guid.NewGuid(),
            title: "Event",
            description: string.Empty);

        SetupSuccessfulUrl(item, "en");

        var result = await _service.GetEventWebinarCardDtosAsync(
            new[] { item.Object },
            "en",
            _ => false,
            CancellationToken.None);

        Assert.That(result, Has.Length.EqualTo(1));
        Assert.That(result[0].Description, Is.Null);
    }

    [Test]
    public async Task GetEventWebinarCardDtosAsync_SetsCtaToEmpty_WhenEventTypeIsMissing()
    {
        var item = CreateEventOrWebinarMock(
            Guid.NewGuid(),
            title: "Event",
            eventTypes: Array.Empty<EventType>());

        SetupSuccessfulUrl(item, "en");

        var result = await _service.GetEventWebinarCardDtosAsync(
            new[] { item.Object },
            "en",
            _ => false,
            CancellationToken.None);

        Assert.That(result, Has.Length.EqualTo(1));

        Assert.Multiple(() =>
        {
            Assert.That(result[0].Type, Is.Null);
            Assert.That(result[0].Link.Title, Is.EqualTo(string.Empty));
        });
    }

    [Test]
    public async Task GetEventWebinarCardDtosAsync_UsesPassedLanguage()
    {
        var item = CreateEventOrWebinarMock(
            Guid.NewGuid(),
            title: "French event");

        _urlRetrieverMock
            .Setup(x => x.Retrieve(
                item.Object,
                "fr",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new WebPageUrl(
                    "~/fr/events/test",
                    "https://example.com/fr/events/test"));

        await _service.GetEventWebinarCardDtosAsync(
            new[] { item.Object },
            "fr",
            _ => false,
            CancellationToken.None);

        _urlRetrieverMock.Verify(
            x => x.Retrieve(
                item.Object,
                "fr",
                It.IsAny<CancellationToken>()),
            Times.Once);

        _contentServiceMock.Verify(
            x => x.GetCurrentLanguage(),
            Times.Never);
    }

    [Test]
    public async Task GetEventWebinarCardDtosAsync_ReturnsOnlyItemsWithValidUrls()
    {
        var validGuid = Guid.NewGuid();
        var invalidGuid = Guid.NewGuid();

        var validItem = CreateEventOrWebinarMock(
            validGuid,
            title: "Valid");

        var invalidItem = CreateEventOrWebinarMock(
            invalidGuid,
            title: "Invalid");

        SetupSuccessfulUrl(validItem, "en");

        _urlRetrieverMock
            .Setup(x => x.Retrieve(
                invalidItem.Object,
                "en",
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException());

        var result = await _service.GetEventWebinarCardDtosAsync(
            new[]
            {
                validItem.Object,
                invalidItem.Object
            },
            "en",
            _ => false,
            CancellationToken.None);

        Assert.That(result, Has.Length.EqualTo(1));
        Assert.That(result[0].Title, Is.EqualTo("Valid"));
    }

    [Test]
    public async Task GetEventWebinarCardDtosAsync_PassesCancellationToken()
    {
        using var cts = new CancellationTokenSource();

        var token = cts.Token;

        var item = CreateEventOrWebinarMock(
            Guid.NewGuid(),
            title: "Event");

        _urlRetrieverMock
            .Setup(x => x.Retrieve(
                item.Object,
                "en",
                token))
            .ReturnsAsync(
                new WebPageUrl(
                    "~/events/token",
                    "https://example.com/events/token"));

        await _service.GetEventWebinarCardDtosAsync(
            new[] { item.Object },
            "en",
            _ => false,
            token);

        _urlRetrieverMock.Verify(
            x => x.Retrieve(
                item.Object,
                "en",
                token),
            Times.Once);
    }

    #endregion

    #region Helpers

    private Mock<IEventOrWebinar> CreateEventOrWebinarMock(
        Guid guid,
        string title = "Event title",
        string description = "",
        string location = "",
        DateTime? from = null,
        DateTime? to = null,
        EventType[]? eventTypes = null)
    {
        var mock = new Mock<IEventOrWebinar>();

        var eventFrom = from ??
            new DateTime(
                2026,
                1,
                1,
                10,
                0,
                0,
                DateTimeKind.Utc);

        var eventTo = to ??
            eventFrom.AddDays(1);

        mock.SetupGet(x => x.SystemFields)
            .Returns(
                new WebPageFields
                {
                    WebPageItemGUID = guid
                });

        mock.SetupGet(x => x.MetadataTitle)
            .Returns(title);

        mock.SetupGet(x => x.MetadataDescription)
            .Returns(description);

        mock.SetupGet(x => x.EventLocationCity)
            .Returns(location);

        mock.SetupGet(x => x.EventDateFrom)
            .Returns(eventFrom);

        mock.SetupGet(x => x.EventDateTo)
            .Returns(eventTo);

        mock.SetupGet(x => x.EventType)
            .Returns(eventTypes ?? Array.Empty<EventType>());

        return mock;
    }

    private void SetupSuccessfulUrl(
        Mock<IEventOrWebinar> item,
        string language)
    {
        _urlRetrieverMock
            .Setup(x => x.Retrieve(
                item.Object,
                language,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new WebPageUrl(
                    "~/events/test",
                    "https://example.com/events/test"));
    }

    #endregion
}