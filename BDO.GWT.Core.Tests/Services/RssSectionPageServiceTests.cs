namespace BDO.GWT.Core.Tests.Services;

using BDO.GWT.Core.Services;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.DTOs;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

[TestFixture]
public class RssSectionPageServiceTests
{
    private Mock<IContentService> _contentServiceMock;
    private Mock<ICultureContext> _cultureContextMock;
    private Mock<IWebsiteChannelContext> _websiteChannelContextMock;
    private Mock<IWebPageUrlRetriever> _webPageUrlRetrieverMock;
    private RssSectionPageService _sut;

    [SetUp]
    public void SetUp()
    {
        _contentServiceMock = new Mock<IContentService>();
        _cultureContextMock = new Mock<ICultureContext>();
        _websiteChannelContextMock = new Mock<IWebsiteChannelContext>();
        _webPageUrlRetrieverMock = new Mock<IWebPageUrlRetriever>();

        _cultureContextMock.Setup(c => c.CurrentLanguageTag).Returns("en-gb");
        _websiteChannelContextMock.Setup(c => c.WebsiteChannelName).Returns("TestChannel");

        _sut = new RssSectionPageService(
            _contentServiceMock.Object,
            _cultureContextMock.Object,
            _websiteChannelContextMock.Object,
            _webPageUrlRetrieverMock.Object);
    }

    // -------------------------------------------------------------------------
    // Builders
    // -------------------------------------------------------------------------

    private static SectionRss BuildSectionRss(
        string treePath = "/en-gb/insights",
        string title = "RSS Section",
        string description = "Section Desc") =>
        new()
        {
            SystemFields = new WebPageFields
            {
                WebPageItemID = 1,
                WebPageItemTreePath = treePath,
            },
            MetadataTitle = title,
            MetadataDescription = description,
        };

    private static Rss BuildRssFeed(
        int webPageItemId,
        string feedTitle = "Feed Title",
        bool isPublic = true) =>
        new()
        {
            SystemFields = new WebPageFields
            {
                WebPageItemID = webPageItemId,
            },
            RssFeedTitle = feedTitle,
            RssPublic = isPublic,
        };

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private void SetupCurrentPage(SectionRss? page)
    {
        _contentServiceMock
            .Setup(s => s.RetrieveCurrentPageAsync<SectionRss>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);
    }

    private void SetupRssFeeds(IEnumerable<Rss> feeds)
    {
        _contentServiceMock
            .Setup(s => s.GetPagesAsync<Rss>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(feeds);
    }

    private void SetupUrlRetriever(int webPageItemId, string relativePath)
    {
        _webPageUrlRetrieverMock
            .Setup(r => r.Retrieve(
                webPageItemId,
                "en-gb",
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WebPageUrl(relativePath, $"https://example.com{relativePath.TrimStart('~')}"));
    }

    private void SetupUrlRetrieverThrows(int webPageItemId)
    {
        _webPageUrlRetrieverMock
            .Setup(r => r.Retrieve(
                webPageItemId,
                "en-gb",
                false,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("URL not found"));
    }

    // -------------------------------------------------------------------------
    // Tests
    // -------------------------------------------------------------------------

    [Test]
    public async Task GetRssSectionDtoAsync_WhenCurrentPageIsNull_ReturnsEmptyDto()
    {
        SetupCurrentPage(null);

        var result = await _sut.GetRssSectionDtoAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Title, Is.Null.Or.Empty);
            Assert.That(result.Links, Is.Null.Or.Empty);
        });
    }

    [Test]
    public async Task GetRssSectionDtoAsync_WhenNoRssFeedsExist_ReturnsEmptyLinks()
    {
        SetupCurrentPage(BuildSectionRss());
        SetupRssFeeds(Array.Empty<Rss>());

        var result = await _sut.GetRssSectionDtoAsync(CancellationToken.None);

        Assert.That(result.Links, Is.Empty);
    }

    [Test]
    public async Task GetRssSectionDtoAsync_MapsCurrentPageTitleToDto()
    {
        SetupCurrentPage(BuildSectionRss(title: "My RSS Section"));
        SetupRssFeeds(Array.Empty<Rss>());

        var result = await _sut.GetRssSectionDtoAsync(CancellationToken.None);

        Assert.That(result.Title, Is.EqualTo("My RSS Section"));
    }

    [Test]
    public async Task GetRssSectionDtoAsync_MapsCurrentPageDescriptionToDto()
    {
        SetupCurrentPage(BuildSectionRss(description: "My Description"));
        SetupRssFeeds(Array.Empty<Rss>());

        var result = await _sut.GetRssSectionDtoAsync(CancellationToken.None);

        Assert.That(result.Description, Is.EqualTo("My Description"));
    }

    [Test]
    public async Task GetRssSectionDtoAsync_WhenFeedsHaveUrls_ReturnsCorrectLinkCount()
    {
        SetupCurrentPage(BuildSectionRss());
        var feed1 = BuildRssFeed(webPageItemId: 10, feedTitle: "Feed 1");
        var feed2 = BuildRssFeed(webPageItemId: 20, feedTitle: "Feed 2");
        SetupRssFeeds(new[] { feed1, feed2 });
        SetupUrlRetriever(10, "~/feed-1");
        SetupUrlRetriever(20, "~/feed-2");

        var result = await _sut.GetRssSectionDtoAsync(CancellationToken.None);

        Assert.That(result.Links, Has.Length.EqualTo(2));
    }

    [Test]
    public async Task GetRssSectionDtoAsync_MapsRssFeedTitleToLinkTitle()
    {
        SetupCurrentPage(BuildSectionRss());
        var feed = BuildRssFeed(webPageItemId: 10, feedTitle: "My Feed");
        SetupRssFeeds(new[] { feed });
        SetupUrlRetriever(10, "~/my-feed");

        var result = await _sut.GetRssSectionDtoAsync(CancellationToken.None);

        Assert.That(result.Links.First().Title, Is.EqualTo("My Feed"));
    }

    [Test]
    public async Task GetRssSectionDtoAsync_SetsLinkTargetToSelf()
    {
        SetupCurrentPage(BuildSectionRss());
        var feed = BuildRssFeed(webPageItemId: 10);
        SetupRssFeeds(new[] { feed });
        SetupUrlRetriever(10, "~/my-feed");

        var result = await _sut.GetRssSectionDtoAsync(CancellationToken.None);

        Assert.That(result.Links.First().Target, Is.EqualTo("_self"));
    }

    [Test]
    public async Task GetRssSectionDtoAsync_MapsRelativeUrlToLink()
    {
        SetupCurrentPage(BuildSectionRss());
        var feed = BuildRssFeed(webPageItemId: 10);
        SetupRssFeeds(new[] { feed });
        SetupUrlRetriever(10, "~/my-feed");

        var result = await _sut.GetRssSectionDtoAsync(CancellationToken.None);

        Assert.That(result.Links.First().URL, Is.EqualTo("~/my-feed"));
    }

    [Test]
    public async Task GetRssSectionDtoAsync_WhenUrlRetrievalFailsForOneFeed_ExcludesThatFeedFromLinks()
    {
        SetupCurrentPage(BuildSectionRss());
        var feed1 = BuildRssFeed(webPageItemId: 10, feedTitle: "Good Feed");
        var feed2 = BuildRssFeed(webPageItemId: 20, feedTitle: "Bad Feed");
        SetupRssFeeds(new[] { feed1, feed2 });
        SetupUrlRetriever(10, "~/good-feed");
        SetupUrlRetrieverThrows(20);

        var result = await _sut.GetRssSectionDtoAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Links, Has.Length.EqualTo(1));
            Assert.That(result.Links.First().Title, Is.EqualTo("Good Feed"));
        });
    }

    [Test]
    public async Task GetRssSectionDtoAsync_WhenAllUrlRetrievalsFail_ReturnsEmptyLinks()
    {
        SetupCurrentPage(BuildSectionRss());
        var feed1 = BuildRssFeed(webPageItemId: 10);
        var feed2 = BuildRssFeed(webPageItemId: 20);
        SetupRssFeeds(new[] { feed1, feed2 });
        SetupUrlRetrieverThrows(10);
        SetupUrlRetrieverThrows(20);

        var result = await _sut.GetRssSectionDtoAsync(CancellationToken.None);

        Assert.That(result.Links, Is.Empty);
    }

    [Test]
    public async Task GetRssSectionDtoAsync_CallsGetPagesWithCurrentChannelName()
    {
        SetupCurrentPage(BuildSectionRss());
        SetupRssFeeds(Array.Empty<Rss>());

        await _sut.GetRssSectionDtoAsync(CancellationToken.None);

        _contentServiceMock.Verify(s => s.GetPagesAsync<Rss>(
            It.Is<RetrievePagesParameters>(p => p.ChannelName == "TestChannel"),
            It.IsAny<Action<RetrievePagesQueryParameters>>(),
            It.IsAny<RetrievalCacheSettings>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}