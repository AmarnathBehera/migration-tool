using BDO.GWT.Core.Services;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Moq;

namespace BDO.GWT.Core.Tests.Services;

public class NewsGroupPageServiceTests
{
    private Mock<IContentService> _contentServiceMock = null!;
    private Mock<IWebPageUrlResolver> _urlResolverMock = null!;
    private INewsGroupPageService _service = null!;

    [SetUp]
    public void Setup()
    {
        _contentServiceMock = new Mock<IContentService>();
        _urlResolverMock = new Mock<IWebPageUrlResolver>();

        _service = new NewsGroupPageService(
            _contentServiceMock.Object,
            _urlResolverMock.Object);
    }

    private static NewsGroup CreatePage(int id) =>
        new()
        {
            NewsGroupTitle = "Group Title",
            Description = "Group Description",
            SystemFields = new WebPageFields
            {
                WebPageItemID = id
            }
        };

    private static NewsArticle CreateArticle(string title, DateTime? date) =>
        new()
        {
            MetadataTitle = title,
            SystemFields = new WebPageFields
            {
                ContentItemCommonDataLastPublishedWhen = date
            }
        };

    private void SetupContentService(IEnumerable<NewsArticle> data)
    {
        _contentServiceMock
            .Setup(x => x.GetPagesAsync<NewsArticle>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(data);
    }

    [Test]
    public async Task GetNewsGroupDtoAsync_ReturnsMappedDto()
    {
        var page = CreatePage(1);

        SetupContentService(new List<NewsArticle>
        {
            CreateArticle("A", DateTime.UtcNow.AddDays(-1)),
            CreateArticle("B", DateTime.UtcNow)
        });

        _urlResolverMock
            .Setup(x => x.GetRelativePath(It.IsAny<NewsArticle>()))
            .Returns<NewsArticle>(x => "/url/" + x.MetadataTitle);

        var result = await _service.GetNewsGroupDtoAsync(page, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Title, Is.EqualTo("Group Title"));
            Assert.That(result.Description, Is.EqualTo("Group Description"));
            Assert.That(result.Links, Has.Length.EqualTo(2));
        });
    }

    [Test]
    public async Task GetNewsGroupDtoAsync_ReturnsEmpty_WhenNoArticles()
    {
        var page = CreatePage(1);

        SetupContentService(new List<NewsArticle>());

        var result = await _service.GetNewsGroupDtoAsync(page, CancellationToken.None);

        Assert.That(result.Links, Is.Empty);
    }

    [Test]
    public async Task GetNewsGroupDtoAsync_OrdersByLatestDate_FirstItemIsNewest()
    {
        var page = CreatePage(1);

        var older = CreateArticle("Old", DateTime.UtcNow.AddDays(-5));
        var newer = CreateArticle("New", DateTime.UtcNow);

        SetupContentService(new[] { older, newer });

        _urlResolverMock
            .Setup(x => x.GetRelativePath(It.IsAny<NewsArticle>()))
            .Returns<NewsArticle>(x => x.MetadataTitle);

        var result = await _service.GetNewsGroupDtoAsync(page, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Links.First().Title, Is.EqualTo("New"));
            Assert.That(result.Links.Last().Title, Is.EqualTo("Old"));
        });
    }

    [Test]
    public async Task GetNewsGroupDtoAsync_UsesUrlResolver_ForEachItem()
    {
        var page = CreatePage(1);

        SetupContentService(new[]
        {
            CreateArticle("Test", DateTime.UtcNow)
        });

        _urlResolverMock
            .Setup(x => x.GetRelativePath(It.IsAny<NewsArticle>()))
            .Returns("/resolved-url");

        var result = await _service.GetNewsGroupDtoAsync(page, CancellationToken.None);

        _urlResolverMock.Verify(x => x.GetRelativePath(It.IsAny<NewsArticle>()), Times.Once);
        Assert.That(result.Links[0].URL, Is.EqualTo("/resolved-url"));
    }

    [Test]
    public async Task GetNewsGroupDtoAsync_MapsPublishDateCorrectly()
    {
        var page = CreatePage(1);

        var date = DateTime.UtcNow;

        SetupContentService(new[]
        {
            CreateArticle("Mapped", date)
        });

        _urlResolverMock
            .Setup(x => x.GetRelativePath(It.IsAny<NewsArticle>()))
            .Returns("/x");

        var result = await _service.GetNewsGroupDtoAsync(page, CancellationToken.None);

        Assert.That(result.Links[0].PublishFrom, Is.EqualTo(date));
    }

    [Test]
    public async Task GetNewsGroupDtoAsync_HandlesNullPublishDate()
    {
        var page = CreatePage(1);

        SetupContentService(new[]
        {
            CreateArticle("NullDate", null)
        });

        _urlResolverMock
            .Setup(x => x.GetRelativePath(It.IsAny<NewsArticle>()))
            .Returns("/x");

        var result = await _service.GetNewsGroupDtoAsync(page, CancellationToken.None);

        Assert.That(result.Links[0].PublishFrom, Is.Null);
    }

    [Test]
    public async Task GetNewsGroupDtoAsync_MapsAllFieldsCorrectly()
    {
        var page = CreatePage(1);

        var article = CreateArticle("FinalCheck", DateTime.UtcNow);

        SetupContentService(new[] { article });

        _urlResolverMock
            .Setup(x => x.GetRelativePath(It.IsAny<NewsArticle>()))
            .Returns("/final");

        var result = await _service.GetNewsGroupDtoAsync(page, CancellationToken.None);

        var link = result.Links.First();

        Assert.Multiple(() =>
        {
            Assert.That(link.Title, Is.EqualTo("FinalCheck"));
            Assert.That(link.URL, Is.EqualTo("/final"));
            Assert.That(link.PublishFrom, Is.Not.Null);
        });
    }

    [Test]
    public async Task GetNewsGroupDtoAsync_PassesCancellationToken_ToContentService()
    {
        var page = CreatePage(1);
        using var cts = new CancellationTokenSource();
        var expectedToken = cts.Token;

        SetupContentService(new List<NewsArticle>());

        await _service.GetNewsGroupDtoAsync(page, expectedToken);

        _contentServiceMock.Verify(
            x => x.GetPagesAsync<NewsArticle>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                expectedToken),
            Times.Once);
    }
}
