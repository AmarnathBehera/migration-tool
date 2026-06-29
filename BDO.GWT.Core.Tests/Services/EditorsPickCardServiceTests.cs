using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Services;
using CMS.ContentEngine;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Moq;
using System.Reflection;

namespace BDO.GWT.Core.Tests.Services;

public class EditorsPickCardServiceTests
{
    private Mock<ICardsService> _cardsServiceMock = null!;
    private Mock<IContentService> _contentServiceMock = null!;
    private Mock<IWebPageUrlRetriever> _urlRetrieverMock = null!;
    private Mock<IContentRetriever> _contentRetrieverMock = null!;
    private EditorsPickCardService _service = null!;
    private IWebPageFieldsSource[] _retrievedPages = [];
    private int _retrieveCallCount;

    [SetUp]
    public void SetUp()
    {
        _cardsServiceMock = new Mock<ICardsService>();
        _contentServiceMock = new Mock<IContentService>();
        _urlRetrieverMock = new Mock<IWebPageUrlRetriever>();
        _contentRetrieverMock = new Mock<IContentRetriever>();

        _contentServiceMock
            .Setup(x => x.GetCurrentLanguage())
            .Returns("en-US");

        _service = new EditorsPickCardService(
            _cardsServiceMock.Object,
            _contentServiceMock.Object,
            _urlRetrieverMock.Object,
            _contentRetrieverMock.Object);

        typeof(EditorsPickCardService)
            .GetField("_retrieveContentByGuidsAsync", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(_service, (Func<IEnumerable<string>, IEnumerable<Guid>, RetrieveContentOfContentTypesParameters, CancellationToken, Task<IEnumerable<IWebPageFieldsSource>>>)((_, _, _, _) =>
            {
                _retrieveCallCount++;
                return Task.FromResult<IEnumerable<IWebPageFieldsSource>>(_retrievedPages);
            }));
    }

    // ── Structure ────────────────────────────────────────────────────────────

    [Test]
    public void Service_ImplementsInterface()
    {
        Assert.That(typeof(IEditorsPickCardService).IsAssignableFrom(typeof(EditorsPickCardService)), Is.True);
    }

    [Test]
    public void Interface_DeclaresExpectedMembers()
    {
        var t = typeof(IEditorsPickCardService);
        Assert.Multiple(() =>
        {
            Assert.That(t.GetMethod(nameof(IEditorsPickCardService.CreateFiltersAsync)), Is.Not.Null);
            Assert.That(t.GetMethod(nameof(IEditorsPickCardService.GetFilteredItemsAsync)), Is.Not.Null);
        });
    }

    [Test]
    public void Service_HasExpectedConstructor()
    {
        var ctor = typeof(EditorsPickCardService)
            .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault();

        Assert.That(ctor, Is.Not.Null);
        Assert.That(ctor!.GetParameters(), Has.Length.GreaterThan(0));
    }

    // ── CreateFiltersAsync ───────────────────────────────────────────────────

    [Test]
    public async Task CreateFiltersAsync_DelegatesTo_CardsService()
    {
        var filters = new HashSet<string> { "contentType" };
        var expected = new[]
        {
            new FilterItemDto<OptionDto> { Name = "contentType", Type = "select", Options = [] }
        };

        _cardsServiceMock
            .Setup(x => x.CreateFiltersAsync(filters, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _service.CreateFiltersAsync(filters, null, CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));
        _cardsServiceMock.Verify(
            x => x.CreateFiltersAsync(filters, null, It.IsAny<CancellationToken>()),
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
            .ThrowsAsync(new InvalidOperationException("service failure"));

        Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateFiltersAsync(new HashSet<string>(), null, CancellationToken.None));
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
            .ReturnsAsync([]);

        await _service.CreateFiltersAsync(new HashSet<string>(), null, token);

        _cardsServiceMock.Verify(
            x => x.CreateFiltersAsync(
                It.IsAny<HashSet<string>>(),
                It.IsAny<string?>(),
                token),
            Times.Once);
    }

    // ── GetFilteredItemsAsync ────────────────────────────────────────────────

    [Test]
    public async Task GetFilteredItemsAsync_WhenCardsQueryIsNull_UsesDefaultPaging()
    {
        var pages = Enumerable.Range(1, 12)
            .Select(i => (IWebPageFieldsSource)CreatePage($"Title {i}", publishDateUtc: DateTime.UtcNow.AddDays(-i)))
            .ToArray();

        SetupContentRetriever(pages);

        _urlRetrieverMock
            .Setup(x => x.Retrieve(It.IsAny<IWebPageFieldsSource>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Ignore URL in this test"));

        var result = await _service.GetFilteredItemsAsync(
            new CardsQuery(),
            new HashSet<Guid> { Guid.NewGuid() },
            _ => false,
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.TotalCount, Is.EqualTo(12));
            Assert.That(result.Data, Has.Length.EqualTo(10)); // default page size
        });
    }

    [Test]
    public async Task GetFilteredItemsAsync_AppliesContentTypeFilter()
    {
        var pages = new IWebPageFieldsSource[]
        {
            CreatePage("News item", contentType: "NewsArticle"),
            CreatePage("Webinar item", contentType: "Webinar")
        };

        SetupContentRetriever(pages);

        _urlRetrieverMock
            .Setup(x => x.Retrieve(It.IsAny<IWebPageFieldsSource>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Ignore URL in this test"));

        var query = new CardsQuery
        {
            CurrentPage = 1,
            PageSize = 10,
            Filters = new Dictionary<string, string>
            {
                ["contentType"] = "NewsArticle"
            }
        };

        var result = await _service.GetFilteredItemsAsync(
            query,
            new HashSet<Guid> { Guid.NewGuid() },
            _ => false,
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.TotalCount, Is.EqualTo(1));
            Assert.That(result.Data, Has.Length.EqualTo(1));
            Assert.That(result.Data[0].Type, Is.EqualTo("NewsArticle"));
            Assert.That(result.Data[0].Title, Is.EqualTo("News item"));
        });
    }

    [Test]
    public async Task GetFilteredItemsAsync_AppliesBusinessLineFilter_FromLinkedItems()
    {
        var pages = new IWebPageFieldsSource[]
        {
            CreatePage("Tax article", businessLines: ["Tax"]),
            CreatePage("Audit article", businessLines: ["Audit"])
        };

        SetupContentRetriever(pages);

        _urlRetrieverMock
            .Setup(x => x.Retrieve(It.IsAny<IWebPageFieldsSource>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Ignore URL in this test"));

        var query = new CardsQuery
        {
            CurrentPage = 1,
            PageSize = 10,
            Filters = new Dictionary<string, string>
            {
                ["businessLine"] = "Tax"
            }
        };

        var result = await _service.GetFilteredItemsAsync(
            query,
            new HashSet<Guid> { Guid.NewGuid() },
            _ => false,
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.TotalCount, Is.EqualTo(1));
            Assert.That(result.Data, Has.Length.EqualTo(1));
            Assert.That(result.Data[0].Title, Is.EqualTo("Tax article"));
        });
    }

    [Test]
    public async Task GetFilteredItemsAsync_OrdersFeaturedFirst_ThenByPublishDateDescending()
    {
        var featuredOld = CreatePage("Featured old", publishDateUtc: new DateTime(2022, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var regularNew = CreatePage("Regular new", publishDateUtc: new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var regularMid = CreatePage("Regular mid", publishDateUtc: new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        var pages = new IWebPageFieldsSource[] { regularMid, featuredOld, regularNew };

        SetupContentRetriever(pages);

        _urlRetrieverMock
            .Setup(x => x.Retrieve(It.IsAny<IWebPageFieldsSource>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Ignore URL in this test"));

        bool IsFeatured(IWebPageFieldsSource page) =>
            page is TestPage p && p.MetadataTitle == "Featured old";

        var result = await _service.GetFilteredItemsAsync(
            new CardsQuery { CurrentPage = 1, PageSize = 10 },
            new HashSet<Guid> { Guid.NewGuid() },
            IsFeatured,
            CancellationToken.None);

        var titles = new HashSet<string> { "Featured old",
            "Regular new",
            "Regular mid"};

        Assert.That(result.Data.Select(x => x.Title).ToArray(), Is.EqualTo(titles));
    }

    // ── Test helpers ──────────────────────────────────────────────────────────

    private sealed class TestLinkedItem
    {
        public string? Name { get; set; }
    }

    private sealed class TestPage : IWebPageFieldsSource
    {
        public WebPageFields SystemFields { get; set; } = new();
        public string? MetadataTitle { get; set; }
        public DateTime? DocumentPublishFrom { get; set; }
        public string? ContentType { get; set; }
        public IEnumerable<TestLinkedItem>? MetadataBusinessLines { get; set; }
    }

    private static TestPage CreatePage(
        string title,
        string? contentType = null,
        DateTime? publishDateUtc = null,
        string[]? businessLines = null)
    {
        return new TestPage
        {
            MetadataTitle = title,
            ContentType = contentType,
            DocumentPublishFrom = publishDateUtc,
            MetadataBusinessLines = businessLines?.Select(x => new TestLinkedItem { Name = x }).ToArray()
        };
    }

    private void SetupContentRetriever(IWebPageFieldsSource[] pages)
    {
        _retrievedPages = pages;
    }
}