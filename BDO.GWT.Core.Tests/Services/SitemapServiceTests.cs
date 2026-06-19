using BDO.GWT.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Core.Tests.Services;

[TestFixture]
public class SitemapServiceTests
{
    private Mock<IWebPageUrlRetriever> _webPageUrlRetrieverMock;
    private Mock<IWebsiteChannelContext> _websiteChannelContextMock;
    private Mock<ILanguageProvider> _languageProviderMock;
    private Mock<IPageNodeProvider> _pageNodeProviderMock;
    private Mock<ILogger<SitemapService>> _loggerMock;
    private SitemapService _sut;

    [SetUp]
    public void SetUp()
    {
        _webPageUrlRetrieverMock = new Mock<IWebPageUrlRetriever>();
        _websiteChannelContextMock = new Mock<IWebsiteChannelContext>();
        _languageProviderMock = new Mock<ILanguageProvider>();
        _pageNodeProviderMock = new Mock<IPageNodeProvider>();
        _loggerMock = new Mock<ILogger<SitemapService>>();

        _websiteChannelContextMock
            .SetupGet(x => x.WebsiteChannelName)
            .Returns("TestChannel");

        _sut = new SitemapService(
            _webPageUrlRetrieverMock.Object,
            _websiteChannelContextMock.Object,
            _languageProviderMock.Object,
            _pageNodeProviderMock.Object,
            _loggerMock.Object);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private void SetupLanguages(params string[] languageNames)
    {
        _languageProviderMock
            .Setup(p => p.GetAllLanguageNamesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(languageNames.ToList());
    }

    private void SetupNoPages(string languageName)
    {
        _pageNodeProviderMock
            .Setup(p => p.GetPageNodesAsync(
                It.IsAny<string>(),
                languageName,
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<XmlSitePageNode>());
    }

    private void SetupPageNodesForAllContentTypes(
    string languageName,
    params XmlSitePageNode[] nodes)
    {
        var callCount = 0;
        _pageNodeProviderMock
            .Setup(p => p.GetPageNodesAsync(
                It.IsAny<string>(),
                languageName,
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                return callCount == 1
                    ? (IEnumerable<XmlSitePageNode>)nodes
                    : Array.Empty<XmlSitePageNode>();
            });
    }

    private void SetupUrlRetriever(int webPageItemId, string languageName, string absoluteUrl)
    {
        _webPageUrlRetrieverMock
            .Setup(r => r.Retrieve(
                webPageItemId,
                languageName,
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WebPageUrl("~/path", absoluteUrl));
    }

    private void SetupUrlRetrieverThrows(int webPageItemId, string languageName)
    {
        _webPageUrlRetrieverMock
            .Setup(r => r.Retrieve(
                webPageItemId,
                languageName,
                false,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("URL not found"));
    }

    private static XmlSitePageNode BuildNode(
        int webPageItemId,
        string languageName,
        DateTime? lastModified = null) =>
        new()
        {
            WebPageItemID = webPageItemId,
            LanguageName = languageName,
            LastModified = lastModified,
        };

    // -------------------------------------------------------------------------
    // No languages
    // -------------------------------------------------------------------------

    [Test]
    public async Task GetSitemapDtoAsync_WhenNoLanguages_ReturnsEmptySitemap()
    {
        SetupLanguages();

        var result = await _sut.GetSitemapDtoAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Urls, Is.Empty);
        });
    }

    [Test]
    public async Task GetSitemapDtoAsync_WhenNoLanguages_LogsWarning()
    {
        SetupLanguages();

        await _sut.GetSitemapDtoAsync(CancellationToken.None);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) =>
                    v.ToString()!.Contains("No languages found") &&
                    v.ToString()!.Contains("Returning empty sitemap")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public async Task GetSitemapDtoAsync_WhenNoLanguages_DoesNotCallPageNodeProvider()
    {
        SetupLanguages();

        await _sut.GetSitemapDtoAsync(CancellationToken.None);

        _pageNodeProviderMock.Verify(
            p => p.GetPageNodesAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GetSitemapDtoAsync_WhenNoLanguages_DoesNotCallUrlRetriever()
    {
        SetupLanguages();

        await _sut.GetSitemapDtoAsync(CancellationToken.None);

        _webPageUrlRetrieverMock.Verify(
            r => r.Retrieve(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GetSitemapDtoAsync_WhenNoLanguages_ReturnsSitemapDtoType()
    {
        SetupLanguages();

        var result = await _sut.GetSitemapDtoAsync(CancellationToken.None);

        Assert.That(result, Is.InstanceOf<SitemapDto>());
    }

    // -------------------------------------------------------------------------
    // Languages exist, no pages
    // -------------------------------------------------------------------------

    [Test]
    public async Task GetSitemapDtoAsync_WhenLanguagesExistButNoPages_ReturnsEmptySitemap()
    {
        SetupLanguages("en-gb");
        SetupNoPages("en-gb");

        var result = await _sut.GetSitemapDtoAsync(CancellationToken.None);

        Assert.That(result.Urls, Is.Empty);
    }

    [Test]
    public async Task GetSitemapDtoAsync_WhenLanguagesExist_CallsPageNodeProviderWithCorrectLanguage()
    {
        SetupLanguages("en-gb");
        SetupNoPages("en-gb");

        await _sut.GetSitemapDtoAsync(CancellationToken.None);

        _pageNodeProviderMock.Verify(
            p => p.GetPageNodesAsync(
                It.IsAny<string>(),
                "en-gb",
                "TestChannel",
                It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    [Test]
    public async Task GetSitemapDtoAsync_WhenLanguagesExist_DoesNotLogNoLanguagesWarning()
    {
        SetupLanguages("en-gb");
        SetupNoPages("en-gb");

        await _sut.GetSitemapDtoAsync(CancellationToken.None);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("No languages found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Test]
    public async Task GetSitemapDtoAsync_WhenMultipleLanguages_CallsPageNodeProviderForEachLanguage()
    {
        SetupLanguages("en-gb", "fr");

        _pageNodeProviderMock
            .Setup(p => p.GetPageNodesAsync(
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<XmlSitePageNode>());

        await _sut.GetSitemapDtoAsync(CancellationToken.None);

        _pageNodeProviderMock.Verify(
            p => p.GetPageNodesAsync(
                It.IsAny<string>(), "en-gb",
                It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);

        _pageNodeProviderMock.Verify(
            p => p.GetPageNodesAsync(
                It.IsAny<string>(), "fr",
                It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    // -------------------------------------------------------------------------
    // Pages with URLs
    // -------------------------------------------------------------------------

    [Test]
    public async Task GetSitemapDtoAsync_WhenPageHasUrl_IncludesPageInSitemap()
    {
        SetupLanguages("en-gb");
        SetupPageNodesForAllContentTypes("en-gb", BuildNode(1, "en-gb"));
        SetupUrlRetriever(1, "en-gb", "https://example.com/page");

        var result = await _sut.GetSitemapDtoAsync(CancellationToken.None);

        Assert.That(result.Urls, Is.Not.Empty);
    }

    [Test]
    public async Task GetSitemapDtoAsync_WhenPageHasUrl_SetsCorrectLocationUri()
    {
        SetupLanguages("en-gb");
        SetupPageNodesForAllContentTypes("en-gb", BuildNode(1, "en-gb"));
        SetupUrlRetriever(1, "en-gb", "https://example.com/my-page");

        var result = await _sut.GetSitemapDtoAsync(CancellationToken.None);

        Assert.That(result.Urls.First().LocationUri,
            Is.EqualTo("https://example.com/my-page"));
    }

    [Test]
    public async Task GetSitemapDtoAsync_WhenPageHasLastModified_SetsLastModificationDate()
    {
        var lastModified = new DateTime(2024, 3, 15);
        SetupLanguages("en-gb");
        SetupPageNodesForAllContentTypes("en-gb", BuildNode(1, "en-gb", lastModified));
        SetupUrlRetriever(1, "en-gb", "https://example.com/page");

        var result = await _sut.GetSitemapDtoAsync(CancellationToken.None);

        Assert.That(result.Urls.First().LastModificationDate, Is.EqualTo(lastModified));
    }

    [Test]
    public async Task GetSitemapDtoAsync_WhenPageHasNoLastModified_LastModificationDateIsNull()
    {
        SetupLanguages("en-gb");
        SetupPageNodesForAllContentTypes("en-gb", BuildNode(1, "en-gb", lastModified: null));
        SetupUrlRetriever(1, "en-gb", "https://example.com/page");

        var result = await _sut.GetSitemapDtoAsync(CancellationToken.None);

        Assert.That(result.Urls.First().LastModificationDate, Is.Null);
    }

    [Test]
    public async Task GetSitemapDtoAsync_WhenPageExistsInSingleLanguage_AlternateUrlsIsNull()
    {
        SetupLanguages("en-gb");
        SetupPageNodesForAllContentTypes("en-gb", BuildNode(1, "en-gb"));
        SetupUrlRetriever(1, "en-gb", "https://example.com/page");

        var result = await _sut.GetSitemapDtoAsync(CancellationToken.None);

        Assert.That(result.Urls.First().AlternateUrls, Is.Null);
    }

    [Test]
    public async Task GetSitemapDtoAsync_WhenMultiplePagesExist_ReturnsAllPages()
    {
        SetupLanguages("en-gb");
        SetupPageNodesForAllContentTypes("en-gb",
            BuildNode(1, "en-gb"),
            BuildNode(2, "en-gb"));
        SetupUrlRetriever(1, "en-gb", "https://example.com/page-1");
        SetupUrlRetriever(2, "en-gb", "https://example.com/page-2");

        var result = await _sut.GetSitemapDtoAsync(CancellationToken.None);

        Assert.That(result.Urls, Has.Exactly(2).Items);
    }

    // -------------------------------------------------------------------------
    // URL retrieval failures
    // -------------------------------------------------------------------------

    [Test]
    public async Task GetSitemapDtoAsync_WhenUrlRetrievalFails_SkipsPage()
    {
        SetupLanguages("en-gb");
        SetupPageNodesForAllContentTypes("en-gb", BuildNode(1, "en-gb"));
        SetupUrlRetrieverThrows(1, "en-gb");

        var result = await _sut.GetSitemapDtoAsync(CancellationToken.None);

        Assert.That(result.Urls, Is.Empty);
    }

    [Test]
    public async Task GetSitemapDtoAsync_WhenUrlRetrievalFails_LogsWarning()
    {
        SetupLanguages("en-gb");
        SetupPageNodesForAllContentTypes("en-gb", BuildNode(1, "en-gb"));
        SetupUrlRetrieverThrows(1, "en-gb");

        await _sut.GetSitemapDtoAsync(CancellationToken.None);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) =>
                    v.ToString()!.Contains("Failed to retrieve URL")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public async Task GetSitemapDtoAsync_WhenOneUrlSucceedsAndOneFails_ReturnsOnlySuccessfulPage()
    {
        SetupLanguages("en-gb");
        SetupPageNodesForAllContentTypes("en-gb",
            BuildNode(1, "en-gb"),
            BuildNode(2, "en-gb"));
        SetupUrlRetriever(1, "en-gb", "https://example.com/page-1");
        SetupUrlRetrieverThrows(2, "en-gb");

        var result = await _sut.GetSitemapDtoAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Urls, Has.Exactly(1).Items);
            Assert.That(result.Urls.First().LocationUri,
                Is.EqualTo("https://example.com/page-1"));
        });
    }

    [Test]
    public async Task GetSitemapDtoAsync_WhenAllUrlRetrievalsFail_ReturnsEmptySitemap()
    {
        SetupLanguages("en-gb");
        SetupPageNodesForAllContentTypes("en-gb",
            BuildNode(1, "en-gb"),
            BuildNode(2, "en-gb"));
        SetupUrlRetrieverThrows(1, "en-gb");
        SetupUrlRetrieverThrows(2, "en-gb");

        var result = await _sut.GetSitemapDtoAsync(CancellationToken.None);

        Assert.That(result.Urls, Is.Empty);
    }

    // -------------------------------------------------------------------------
    // Multi-language / alternate URLs
    // -------------------------------------------------------------------------

    [Test]
    public async Task GetSitemapDtoAsync_WhenPageExistsInMultipleLanguages_GeneratesAlternateUrls()
    {
        SetupLanguages("en-gb", "fr");

        _pageNodeProviderMock
            .Setup(p => p.GetPageNodesAsync(
                It.IsAny<string>(), "en-gb",
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { BuildNode(1, "en-gb") });

        _pageNodeProviderMock
            .Setup(p => p.GetPageNodesAsync(
                It.IsAny<string>(), "fr",
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { BuildNode(1, "fr") });

        SetupUrlRetriever(1, "en-gb", "https://example.com/en-gb/page");
        SetupUrlRetriever(1, "fr", "https://example.com/fr/page");

        var result = await _sut.GetSitemapDtoAsync(CancellationToken.None);

        var withAlternates = result.Urls
            .Where(u => u.AlternateUrls != null)
            .ToList();

        Assert.That(withAlternates, Is.Not.Empty);
    }

    [Test]
    public async Task GetSitemapDtoAsync_WhenPageExistsInMultipleLanguages_AlternateUrlsHasBothLanguages()
    {
        SetupLanguages("en-gb", "fr");

        _pageNodeProviderMock
            .Setup(p => p.GetPageNodesAsync(
                It.IsAny<string>(), "en-gb",
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { BuildNode(1, "en-gb") });

        _pageNodeProviderMock
            .Setup(p => p.GetPageNodesAsync(
                It.IsAny<string>(), "fr",
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { BuildNode(1, "fr") });

        SetupUrlRetriever(1, "en-gb", "https://example.com/en-gb/page");
        SetupUrlRetriever(1, "fr", "https://example.com/fr/page");

        var result = await _sut.GetSitemapDtoAsync(CancellationToken.None);

        var languageTags = result.Urls
            .Where(u => u.AlternateUrls != null)
            .SelectMany(u => u.AlternateUrls!)
            .Select(a => a.LanguageTag)
            .ToList();

        Assert.Multiple(() =>
        {
            Assert.That(languageTags, Does.Contain("en-gb"));
            Assert.That(languageTags, Does.Contain("fr"));
        });
    }

    [Test]
    public async Task GetSitemapDtoAsync_WhenPageExistsInMultipleLanguages_AlternateUrlsContainCorrectUrls()
    {
        SetupLanguages("en-gb", "fr");

        _pageNodeProviderMock
            .Setup(p => p.GetPageNodesAsync(
                It.IsAny<string>(), "en-gb",
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { BuildNode(1, "en-gb") });

        _pageNodeProviderMock
            .Setup(p => p.GetPageNodesAsync(
                It.IsAny<string>(), "fr",
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { BuildNode(1, "fr") });

        SetupUrlRetriever(1, "en-gb", "https://example.com/en-gb/page");
        SetupUrlRetriever(1, "fr", "https://example.com/fr/page");

        var result = await _sut.GetSitemapDtoAsync(CancellationToken.None);

        var alternateUrls = result.Urls
            .Where(u => u.AlternateUrls != null)
            .SelectMany(u => u.AlternateUrls!)
            .Select(a => a.Url)
            .ToList();

        Assert.Multiple(() =>
        {
            Assert.That(alternateUrls, Does.Contain("https://example.com/en-gb/page"));
            Assert.That(alternateUrls, Does.Contain("https://example.com/fr/page"));
        });
    }

    

    // -------------------------------------------------------------------------
    // General
    // -------------------------------------------------------------------------

    [Test]
    public void SitemapService_ImplementsISitemapService()
    {
        Assert.That(_sut, Is.InstanceOf<ISitemapService>());
    }
}