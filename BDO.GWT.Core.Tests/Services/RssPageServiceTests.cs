namespace BDO.GWT.Core.Tests.Services;

using BDO.GWT.Core.Services;
using CMS.ContentEngine;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

[TestFixture]
public class RssPageServiceTests
{
    private Mock<IContentQueryExecutor> _executorMock;
    private Mock<IWebPageUrlRetriever> _webPageUrlRetrieverMock;
    private Mock<IWebsiteChannelContext> _websiteChannelContextMock;
    private Mock<IContentService> _contentServiceMock;
    private Mock<ICultureContext> _cultureContextMock;
    private Mock<ILogger<RssPageService>> _loggerMock;
    private RssPageService _sut;

    [SetUp]
    public void SetUp()
    {
        _executorMock = new Mock<IContentQueryExecutor>();
        _webPageUrlRetrieverMock = new Mock<IWebPageUrlRetriever>();
        _websiteChannelContextMock = new Mock<IWebsiteChannelContext>();
        _contentServiceMock = new Mock<IContentService>();
        _cultureContextMock = new Mock<ICultureContext>();
        _loggerMock = new Mock<ILogger<RssPageService>>();

        _cultureContextMock.Setup(c => c.CurrentLanguageTag).Returns("en-gb");
        _websiteChannelContextMock.Setup(c => c.WebsiteChannelName).Returns("TestChannel");
        _websiteChannelContextMock.Setup(c => c.IsPreview).Returns(false);

        _sut = new RssPageService(
            _executorMock.Object,
            _webPageUrlRetrieverMock.Object,
            _websiteChannelContextMock.Object,
            _cultureContextMock.Object,
            _loggerMock.Object,
            _contentServiceMock.Object);
    }

    // -------------------------------------------------------------------------
    // Builders
    // -------------------------------------------------------------------------

    private static Rss BuildRssPage(string? ancestorTreePath = "/en-gb/insights") =>
        new()
        {
            SystemFields = new WebPageFields
            {
                WebPageItemID = 1,
                WebPageItemName = "RssPage",
            },
            RssFeedTitle = "Test Feed",
            RssFeedDescription = "Test Description",
            RssCategory = "News",
            RssTopN = 5,
            RssPath = ancestorTreePath is null ? null : new List<Rss>
            {
                new()
                {
                    SystemFields = new WebPageFields
                    {
                        WebPageItemTreePath = ancestorTreePath
                    }
                }
            }
        };

    private static Mock<IWebPageContentQueryDataContainer> BuildContainer(
        int webPageItemId,
        string title = "Item Title",
        string description = "Item Desc",
        DateTime? lastPublished = null)
    {
        var container = new Mock<IWebPageContentQueryDataContainer>();
        container.Setup(c => c.WebPageItemID).Returns(webPageItemId);
        container.Setup(c => c.GetValue<string>("MetadataTitle")).Returns(title);
        container.Setup(c => c.GetValue<string>("MetadataDescription")).Returns(description);
        container.Setup(c => c.GetValue<DateTime?>("ContentItemCommonDataLastPublishedWhen"))
            .Returns(lastPublished ?? new DateTime(2024, 1, 1));
        container.Setup(c => c.GetValue<string>("MetadataTeaserImageUrl")).Returns((string?)null);
        return container;
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private void SetupCurrentPage(Rss? page)
    {
        _contentServiceMock
            .Setup(s => s.RetrieveCurrentPageAsync<Rss>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);
    }

    private void SetupUrlRetriever(int webPageItemId, string absoluteUrl)
    {
        _webPageUrlRetrieverMock
            .Setup(r => r.Retrieve(
                webPageItemId,
                "en-gb",
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WebPageUrl("~/test-path", absoluteUrl));
    }

    // GetWebPageResult extension calls the async GetResult overload internally
    private void SetupExecutorWithItems(IEnumerable<IWebPageContentQueryDataContainer> containers)
    {
        _executorMock
            .Setup(e => e.GetResult(
                It.IsAny<ContentItemQueryBuilder>(),
                It.IsAny<Func<IContentQueryDataContainer, Task<RssItemData>>>(),
                It.IsAny<ContentQueryExecutionOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(async (
                ContentItemQueryBuilder _,
                Func<IContentQueryDataContainer, Task<RssItemData>> selector,
                ContentQueryExecutionOptions __,
                CancellationToken ___) =>
            {
                var results = new List<RssItemData>();
                foreach (var container in containers)
                    results.Add(await selector(container));
                return (IEnumerable<RssItemData>)results;
            });
    }

    private void SetupExecutorThrows()
    {
        _executorMock
            .Setup(e => e.GetResult(
                It.IsAny<ContentItemQueryBuilder>(),
                It.IsAny<Func<IContentQueryDataContainer, Task<RssItemData>>>(),
                It.IsAny<ContentQueryExecutionOptions>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));
    }

    // -------------------------------------------------------------------------
    // Tests
    // -------------------------------------------------------------------------

    [Test]
    public async Task GetRssFeedXmlAsync_WhenCurrentPageIsNull_ReturnsEmptyString()
    {
        SetupCurrentPage(null);

        var result = await _sut.GetRssFeedXmlAsync(CancellationToken.None);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetRssFeedXmlAsync_WhenRssPathIsNull_ReturnsXmlWithEmptyItems()
    {
        var page = BuildRssPage(ancestorTreePath: null);
        SetupCurrentPage(page);
        SetupUrlRetriever(1, "https://example.com/rss");

        var result = await _sut.GetRssFeedXmlAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Empty);
            Assert.That(result, Does.Contain("<title>Test Feed</title>"));
        });
    }

    

    [Test]
    public async Task GetRssFeedXmlAsync_WhenUrlRetrievalFailsForItem_SkipsItem()
    {
        var page = BuildRssPage();
        SetupCurrentPage(page);
        SetupUrlRetriever(1, "https://example.com/rss");

        var container = BuildContainer(webPageItemId: 99, title: "Should Be Skipped");
        SetupExecutorWithItems(new[] { container.Object });

        _webPageUrlRetrieverMock
            .Setup(r => r.Retrieve(99, "en-gb", false, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("URL not found"));

        var result = await _sut.GetRssFeedXmlAsync(CancellationToken.None);

        Assert.That(result, Does.Not.Contain("Should Be Skipped"));
    }

    [Test]
    public async Task GetRssFeedXmlAsync_WhenExecutorThrows_ReturnsXmlWithNoItems()
    {
        var page = BuildRssPage();
        SetupCurrentPage(page);
        SetupUrlRetriever(1, "https://example.com/rss");
        SetupExecutorThrows();

        var result = await _sut.GetRssFeedXmlAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Empty);
            Assert.That(result, Does.Contain("<title>Test Feed</title>"));
        });
    }

    [Test]
    public async Task GetRssFeedXmlAsync_OutputIsValidXml()
    {
        var page = BuildRssPage();
        SetupCurrentPage(page);
        SetupUrlRetriever(1, "https://example.com/rss");
        SetupExecutorWithItems(Array.Empty<IWebPageContentQueryDataContainer>());

        var result = await _sut.GetRssFeedXmlAsync(CancellationToken.None);

        Assert.DoesNotThrow(() =>
        {
            var doc = System.Xml.Linq.XDocument.Parse(result);
            Assert.That(doc.Root, Is.Not.Null);
        });
    }
}