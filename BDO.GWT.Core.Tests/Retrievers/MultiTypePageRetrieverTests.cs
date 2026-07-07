using BDO.GWT.Xperience.Core.Retrievers;
using CMS.ContentEngine;
using CMS.Helpers;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc.Routing;
using Moq;

namespace BDO.GWT.Core.Tests.Retrievers;

[TestFixture]
public class MultiTypePageRetrieverTests
{
    private Mock<IContentQueryExecutorAdapter> _executorMock = null!;
    private Mock<IWebsiteChannelContext> _channelContextMock = null!;
    private Mock<IProgressiveCache> _cacheMock = null!;
    private Mock<IPreferredLanguageRetriever> _languageRetrieverMock = null!;
    private MultiTypePageRetriever _retriever = null!;

    private sealed class TestPage : IWebPageFieldsSource
    {
        public WebPageFields SystemFields { get; set; } = new WebPageFields
        {
            WebPageItemID = 1,
            WebPageItemGUID = Guid.NewGuid(),
            WebPageItemName = "test-page",
            WebPageUrlPath = "/test"
        };
    }

    private sealed class TestAbstractItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    [SetUp]
    public void Setup()
    {
        _executorMock = new Mock<IContentQueryExecutorAdapter>();
        _channelContextMock = new Mock<IWebsiteChannelContext>();
        _cacheMock = new Mock<IProgressiveCache>();
        _languageRetrieverMock = new Mock<IPreferredLanguageRetriever>();

        _channelContextMock.SetupGet(x => x.WebsiteChannelName).Returns("TestChannel");
        _channelContextMock.SetupGet(x => x.IsPreview).Returns(false);
        _languageRetrieverMock.Setup(x => x.Get()).Returns("en");

        SetupCacheToPassThrough();

        _retriever = new MultiTypePageRetriever(
            _executorMock.Object,
            _channelContextMock.Object,
            _cacheMock.Object,
            _languageRetrieverMock.Object);
    }

    private void SetupCacheToPassThrough()
    {
        _cacheMock
            .Setup(c => c.LoadAsync(
                It.IsAny<Func<CacheSettings, Task<TestPage[]>>>(),
                It.IsAny<CacheSettings>()))
            .Returns<Func<CacheSettings, Task<TestPage[]>>, CacheSettings>(
                (loader, settings) => loader(settings));

        _cacheMock
            .Setup(c => c.LoadAsync(
                It.IsAny<Func<CacheSettings, Task<TestPage?>>>(),
                It.IsAny<CacheSettings>()))
            .Returns<Func<CacheSettings, Task<TestPage?>>, CacheSettings>(
                (loader, settings) => loader(settings));

        _cacheMock
            .Setup(c => c.LoadAsync(
                It.IsAny<Func<CacheSettings, Task<TestAbstractItem[]>>>(),
                It.IsAny<CacheSettings>()))
            .Returns<Func<CacheSettings, Task<TestAbstractItem[]>>, CacheSettings>(
                (loader, settings) => loader(settings));

        _cacheMock
            .Setup(c => c.LoadAsync(
                It.IsAny<Func<CacheSettings, Task<TestAbstractItem?>>>(),
                It.IsAny<CacheSettings>()))
            .Returns<Func<CacheSettings, Task<TestAbstractItem?>>, CacheSettings>(
                (loader, settings) => loader(settings));

        _cacheMock
            .Setup(c => c.LoadAsync(
                It.IsAny<Func<CacheSettings, Task<int>>>(),
                It.IsAny<CacheSettings>()))
            .Returns<Func<CacheSettings, Task<int>>, CacheSettings>(
                (loader, settings) => loader(settings));
    }

    // ── RetrievePagesAsync<TContentItem> ────────────────────────────────────

    [Test]
    public async Task RetrievePagesAsync_SingleType_ReturnsPages()
    {
        var pages = new[] { new TestPage(), new TestPage() };

        _executorMock
            .Setup(e => e.GetMappedWebPageResultAsync<TestPage>(
                It.IsAny<ContentItemQueryBuilder>(),
                It.IsAny<ContentQueryExecutionOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pages);

        var result = await _retriever.RetrievePagesAsync<TestPage>("BDO.TestPage");

        Assert.That(result.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task RetrievePagesAsync_SingleType_WhenNoResults_ReturnsEmpty()
    {
        _executorMock
            .Setup(e => e.GetMappedWebPageResultAsync<TestPage>(
                It.IsAny<ContentItemQueryBuilder>(),
                It.IsAny<ContentQueryExecutionOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<TestPage>());

        var result = await _retriever.RetrievePagesAsync<TestPage>("BDO.TestPage");

        Assert.That(result, Is.Empty);
    }

    //[Test]
    //public async Task RetrievePagesAsync_SingleType_InvokesDecorateQuery()
    //{
    //    bool decorateCalled = false;

    //    _executorMock
    //        .Setup(e => e.GetMappedWebPageResultAsync<TestPage>(
    //            It.IsAny<ContentItemQueryBuilder>(),
    //            It.IsAny<ContentQueryExecutionOptions>(),
    //            It.IsAny<CancellationToken>()))
    //        .ReturnsAsync(Array.Empty<TestPage>());

    //    await _retriever.RetrievePagesAsync<TestPage>(
    //        "BDO.TestPage",
    //        _ => { decorateCalled = true; });

    //    Assert.That(decorateCalled, Is.True);
    //}

    [Test]
    public async Task RetrievePagesAsync_SingleType_UsesCancellationToken()
    {
        using var cts = new CancellationTokenSource();

        _executorMock
            .Setup(e => e.GetMappedWebPageResultAsync<TestPage>(
                It.IsAny<ContentItemQueryBuilder>(),
                It.IsAny<ContentQueryExecutionOptions>(),
                cts.Token))
            .ReturnsAsync(Array.Empty<TestPage>());

        await _retriever.RetrievePagesAsync<TestPage>("BDO.TestPage", cancellationToken: cts.Token);

        _executorMock.Verify(
            e => e.GetMappedWebPageResultAsync<TestPage>(
                It.IsAny<ContentItemQueryBuilder>(),
                It.IsAny<ContentQueryExecutionOptions>(),
                cts.Token),
            Times.Once);
    }

    [Test]
    public async Task RetrievePagesAsync_SingleType_UsesPreviewFromContext()
    {
        _channelContextMock.SetupGet(x => x.IsPreview).Returns(true);

        ContentQueryExecutionOptions? capturedOptions = null;

        _executorMock
            .Setup(e => e.GetMappedWebPageResultAsync<TestPage>(
                It.IsAny<ContentItemQueryBuilder>(),
                It.IsAny<ContentQueryExecutionOptions>(),
                It.IsAny<CancellationToken>()))
            .Callback<ContentItemQueryBuilder, ContentQueryExecutionOptions, CancellationToken>(
                (_, opts, _) => capturedOptions = opts)
            .ReturnsAsync(Array.Empty<TestPage>());

        await _retriever.RetrievePagesAsync<TestPage>("BDO.TestPage");

        Assert.That(capturedOptions?.ForPreview, Is.True);
    }

    // ── RetrievePagesAsync<TAbstractItem> ───────────────────────────────────

    [Test]
    public async Task RetrievePagesAsync_MultiType_ReturnsMappedItems()
    {
        var items = new[] { new TestAbstractItem { Id = 1 }, new TestAbstractItem { Id = 2 } };

        _executorMock
            .Setup(e => e.GetWebPageResultAsync(
                It.IsAny<ContentItemQueryBuilder>(),
                It.IsAny<Func<IWebPageContentQueryDataContainer, TestAbstractItem>>(),
                It.IsAny<ContentQueryExecutionOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(items);

        var result = await _retriever.RetrievePagesAsync<TestAbstractItem>(
            ["BDO.TypeA", "BDO.TypeB"],
            _ => new TestAbstractItem());

        Assert.That(result.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task RetrievePagesAsync_MultiType_WhenNoResults_ReturnsEmpty()
    {
        _executorMock
            .Setup(e => e.GetWebPageResultAsync(
                It.IsAny<ContentItemQueryBuilder>(),
                It.IsAny<Func<IWebPageContentQueryDataContainer, TestAbstractItem>>(),
                It.IsAny<ContentQueryExecutionOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<TestAbstractItem>());

        var result = await _retriever.RetrievePagesAsync<TestAbstractItem>(
            ["BDO.TypeA"],
            _ => new TestAbstractItem());

        Assert.That(result, Is.Empty);
    }



    [Test]
    public async Task RetrievePagesAsync_MultiType_UsesPreviewFromContext()
    {
        _channelContextMock.SetupGet(x => x.IsPreview).Returns(true);

        ContentQueryExecutionOptions? capturedOptions = null;

        _executorMock
            .Setup(e => e.GetWebPageResultAsync(
                It.IsAny<ContentItemQueryBuilder>(),
                It.IsAny<Func<IWebPageContentQueryDataContainer, TestAbstractItem>>(),
                It.IsAny<ContentQueryExecutionOptions>(),
                It.IsAny<CancellationToken>()))
            .Callback<ContentItemQueryBuilder, Func<IWebPageContentQueryDataContainer, TestAbstractItem>, ContentQueryExecutionOptions, CancellationToken>(
                (_, _, opts, _) => capturedOptions = opts)
            .ReturnsAsync(Array.Empty<TestAbstractItem>());

        await _retriever.RetrievePagesAsync<TestAbstractItem>(
            ["BDO.TypeA"],
            _ => new TestAbstractItem());

        Assert.That(capturedOptions?.ForPreview, Is.True);
    }

    // ── RetrievePageAsync<TContentItem> ─────────────────────────────────────

    [Test]
    public async Task RetrievePageAsync_SingleType_ReturnsFirstPage()
    {
        var pages = new[] { new TestPage(), new TestPage() };

        _executorMock
            .Setup(e => e.GetMappedWebPageResultAsync<TestPage>(
                It.IsAny<ContentItemQueryBuilder>(),
                It.IsAny<ContentQueryExecutionOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pages);

        var result = await _retriever.RetrievePageAsync<TestPage>("BDO.TestPage");

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo(pages[0]));
    }

    [Test]
    public async Task RetrievePageAsync_SingleType_WhenNoResults_ReturnsNull()
    {
        _executorMock
            .Setup(e => e.GetMappedWebPageResultAsync<TestPage>(
                It.IsAny<ContentItemQueryBuilder>(),
                It.IsAny<ContentQueryExecutionOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<TestPage>());

        var result = await _retriever.RetrievePageAsync<TestPage>("BDO.TestPage");

        Assert.That(result, Is.Null);
    }



    // ── RetrievePageAsync<TAbstractItem> ────────────────────────────────────

    [Test]
    public async Task RetrievePageAsync_MultiType_ReturnsFirstMappedItem()
    {
        var expected = new TestAbstractItem { Id = 42, Name = "First" };
        var items = new[] { expected, new TestAbstractItem { Id = 99 } };

        _executorMock
            .Setup(e => e.GetWebPageResultAsync(
                It.IsAny<ContentItemQueryBuilder>(),
                It.IsAny<Func<IWebPageContentQueryDataContainer, TestAbstractItem>>(),
                It.IsAny<ContentQueryExecutionOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(items);

        var result = await _retriever.RetrievePageAsync<TestAbstractItem>(
            ["BDO.TypeA", "BDO.TypeB"],
            _ => new TestAbstractItem());

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public async Task RetrievePageAsync_MultiType_WhenNoResults_ReturnsNull()
    {
        _executorMock
            .Setup(e => e.GetWebPageResultAsync(
                It.IsAny<ContentItemQueryBuilder>(),
                It.IsAny<Func<IWebPageContentQueryDataContainer, TestAbstractItem>>(),
                It.IsAny<ContentQueryExecutionOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<TestAbstractItem>());

        var result = await _retriever.RetrievePageAsync<TestAbstractItem>(
            ["BDO.TypeA"],
            _ => new TestAbstractItem());

        Assert.That(result, Is.Null);
    }



    // ── RetrievePageCountAsync (single type) ────────────────────────────────

    [Test]
    public async Task RetrievePageCountAsync_SingleType_ReturnsCorrectCount()
    {
        var ids = new[] { 1, 2, 3 };

        _executorMock
            .Setup(e => e.GetWebPageResultAsync(
                It.IsAny<ContentItemQueryBuilder>(),
                It.IsAny<Func<IWebPageContentQueryDataContainer, int>>(),
                It.IsAny<ContentQueryExecutionOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ids);

        var result = await _retriever.RetrievePageCountAsync("BDO.TestPage");

        Assert.That(result, Is.EqualTo(3));
    }

    [Test]
    public async Task RetrievePageCountAsync_SingleType_WhenNoPages_ReturnsZero()
    {
        _executorMock
            .Setup(e => e.GetWebPageResultAsync(
                It.IsAny<ContentItemQueryBuilder>(),
                It.IsAny<Func<IWebPageContentQueryDataContainer, int>>(),
                It.IsAny<ContentQueryExecutionOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<int>());

        var result = await _retriever.RetrievePageCountAsync("BDO.TestPage");

        Assert.That(result, Is.EqualTo(0));
    }



    // ── RetrievePageCountAsync (multi type) ─────────────────────────────────

    [Test]
    public async Task RetrievePageCountAsync_MultiType_ReturnsCorrectCount()
    {
        var ids = new[] { 10, 20 };

        _executorMock
            .Setup(e => e.GetWebPageResultAsync(
                It.IsAny<ContentItemQueryBuilder>(),
                It.IsAny<Func<IWebPageContentQueryDataContainer, int>>(),
                It.IsAny<ContentQueryExecutionOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ids);

        var result = await _retriever.RetrievePageCountAsync(["BDO.TypeA", "BDO.TypeB"]);

        Assert.That(result, Is.EqualTo(2));
    }

    [Test]
    public async Task RetrievePageCountAsync_MultiType_WhenNoPages_ReturnsZero()
    {
        _executorMock
            .Setup(e => e.GetWebPageResultAsync(
                It.IsAny<ContentItemQueryBuilder>(),
                It.IsAny<Func<IWebPageContentQueryDataContainer, int>>(),
                It.IsAny<ContentQueryExecutionOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<int>());

        var result = await _retriever.RetrievePageCountAsync(["BDO.TypeA"]);

        Assert.That(result, Is.EqualTo(0));
    }



    // ── Language & Channel ───────────────────────────────────────────────────

    [Test]
    public async Task RetrievePagesAsync_SingleType_UsesPreferredLanguage()
    {
        _languageRetrieverMock.Setup(x => x.Get()).Returns("de");

        _executorMock
            .Setup(e => e.GetMappedWebPageResultAsync<TestPage>(
                It.IsAny<ContentItemQueryBuilder>(),
                It.IsAny<ContentQueryExecutionOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<TestPage>());

        await _retriever.RetrievePagesAsync<TestPage>("BDO.TestPage");

        _languageRetrieverMock.Verify(x => x.Get(), Times.AtLeastOnce);
    }

    [Test]
    public async Task RetrievePagesAsync_MultiType_UsesPreferredLanguage()
    {
        _languageRetrieverMock.Setup(x => x.Get()).Returns("fr");

        _executorMock
            .Setup(e => e.GetWebPageResultAsync(
                It.IsAny<ContentItemQueryBuilder>(),
                It.IsAny<Func<IWebPageContentQueryDataContainer, TestAbstractItem>>(),
                It.IsAny<ContentQueryExecutionOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<TestAbstractItem>());

        await _retriever.RetrievePagesAsync<TestAbstractItem>(
            ["BDO.TypeA"],
            _ => new TestAbstractItem());

        _languageRetrieverMock.Verify(x => x.Get(), Times.AtLeastOnce);
    }

    // ── Cache interaction ────────────────────────────────────────────────────

    [Test]
    public async Task RetrievePagesAsync_SingleType_InvokesCache()
    {
        _executorMock
            .Setup(e => e.GetMappedWebPageResultAsync<TestPage>(
                It.IsAny<ContentItemQueryBuilder>(),
                It.IsAny<ContentQueryExecutionOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<TestPage>());

        await _retriever.RetrievePagesAsync<TestPage>("BDO.TestPage");

        _cacheMock.Verify(
            c => c.LoadAsync(
                It.IsAny<Func<CacheSettings, Task<TestPage[]>>>(),
                It.IsAny<CacheSettings>()),
            Times.Once);
    }

    [Test]
    public async Task RetrievePageAsync_SingleType_InvokesCache()
    {
        _executorMock
            .Setup(e => e.GetMappedWebPageResultAsync<TestPage>(
                It.IsAny<ContentItemQueryBuilder>(),
                It.IsAny<ContentQueryExecutionOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<TestPage>());

        await _retriever.RetrievePageAsync<TestPage>("BDO.TestPage");

        _cacheMock.Verify(
            c => c.LoadAsync(
                It.IsAny<Func<CacheSettings, Task<TestPage?>>>(),
                It.IsAny<CacheSettings>()),
            Times.Once);
    }

    [Test]
    public async Task RetrievePageCountAsync_SingleType_InvokesCache()
    {
        _executorMock
            .Setup(e => e.GetWebPageResultAsync(
                It.IsAny<ContentItemQueryBuilder>(),
                It.IsAny<Func<IWebPageContentQueryDataContainer, int>>(),
                It.IsAny<ContentQueryExecutionOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<int>());

        await _retriever.RetrievePageCountAsync("BDO.TestPage");

        _cacheMock.Verify(
            c => c.LoadAsync(
                It.IsAny<Func<CacheSettings, Task<int>>>(),
                It.IsAny<CacheSettings>()),
            Times.Once);
    }

    [Test]
    public async Task RetrievePageCountAsync_MultiType_InvokesCache()
    {
        _executorMock
            .Setup(e => e.GetWebPageResultAsync(
                It.IsAny<ContentItemQueryBuilder>(),
                It.IsAny<Func<IWebPageContentQueryDataContainer, int>>(),
                It.IsAny<ContentQueryExecutionOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<int>());

        await _retriever.RetrievePageCountAsync(["BDO.TypeA", "BDO.TypeB"]);

        _cacheMock.Verify(
            c => c.LoadAsync(
                It.IsAny<Func<CacheSettings, Task<int>>>(),
                It.IsAny<CacheSettings>()),
            Times.Once);
    }
}
