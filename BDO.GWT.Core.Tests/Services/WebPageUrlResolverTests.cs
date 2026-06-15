using BDO;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Moq;

namespace BDO.GWT.Core.Tests.Services;

public class WebPageUrlResolverTests
{
    private Mock<IWebPageUrlRetriever> _pageUrlRetrieverMock = null!;
    private WebPageUrlResolver _resolver = null!;

    [SetUp]
    public void Setup()
    {
        _pageUrlRetrieverMock = new Mock<IWebPageUrlRetriever>();
        _resolver = new WebPageUrlResolver(_pageUrlRetrieverMock.Object);
    }

    // -----------------------------------------------------------------
    // ResolvePageUrlAsync
    // -----------------------------------------------------------------

    [Test]
    public async Task ResolvePageUrlAsync_WhenRetrieverReturnsUrl_ReturnsRelativePath()
    {
        var webPageUrl = new WebPageUrl("~/en/microsite-page", "https://example.com/en/microsite-page");

        _pageUrlRetrieverMock
            .Setup(r => r.Retrieve(42, "en", It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(webPageUrl);

        var result = await _resolver.ResolvePageUrlAsync(42, "en", CancellationToken.None);

        Assert.That(result, Is.EqualTo("/en/microsite-page"));
    }

    [Test]
    public async Task ResolvePageUrlAsync_StripsTildeFromRelativePath()
    {
        var webPageUrl = new WebPageUrl("~/about-us", "https://example.com/about-us");

        _pageUrlRetrieverMock
            .Setup(r => r.Retrieve(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(webPageUrl);

        var result = await _resolver.ResolvePageUrlAsync(1, "en", CancellationToken.None);

        Assert.That(result, Does.Not.StartWith("~"));
        Assert.That(result, Is.EqualTo("/about-us"));
    }

    [Test]
    public async Task ResolvePageUrlAsync_WhenRetrieverReturnsNull_ReturnsFallbackHash()
    {
        _pageUrlRetrieverMock
            .Setup(r => r.Retrieve(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WebPageUrl?)null);

        var result = await _resolver.ResolvePageUrlAsync(99, "en", CancellationToken.None);

        Assert.That(result, Is.EqualTo("#"));
    }

    [Test]
    public async Task ResolvePageUrlAsync_WhenRelativePathIsNull_ReturnsFallbackHash()
    {
        var webPageUrl = new WebPageUrl(null!, "https://example.com/page");

        _pageUrlRetrieverMock
            .Setup(r => r.Retrieve(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(webPageUrl);

        var result = await _resolver.ResolvePageUrlAsync(1, "en", CancellationToken.None);

        Assert.That(result, Is.EqualTo("#"));
    }

    [Test]
    public async Task ResolvePageUrlAsync_PassesCorrectWebPageItemId_ToRetriever()
    {
        _pageUrlRetrieverMock
            .Setup(r => r.Retrieve(123, It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WebPageUrl("~/page", "https://example.com/page"))
            .Verifiable();

        await _resolver.ResolvePageUrlAsync(123, "en", CancellationToken.None);

        _pageUrlRetrieverMock.Verify(
            r => r.Retrieve(123, It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task ResolvePageUrlAsync_PassesCorrectLanguage_ToRetriever()
    {
        _pageUrlRetrieverMock
            .Setup(r => r.Retrieve(It.IsAny<int>(), "fr", It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WebPageUrl("~/fr/page", "https://example.com/fr/page"))
            .Verifiable();

        await _resolver.ResolvePageUrlAsync(1, "fr", CancellationToken.None);

        _pageUrlRetrieverMock.Verify(
            r => r.Retrieve(It.IsAny<int>(), "fr", It.IsAny<bool>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task ResolvePageUrlAsync_PassesCancellationToken_ToRetriever()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _pageUrlRetrieverMock
            .Setup(r => r.Retrieve(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), token))
            .ReturnsAsync(new WebPageUrl("~/page", "https://example.com/page"))
            .Verifiable();

        await _resolver.ResolvePageUrlAsync(1, "en", token);

        _pageUrlRetrieverMock.Verify(
            r => r.Retrieve(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), token),
            Times.Once);
    }

    [Test]
    public async Task ResolvePageUrlAsync_WhenPathHasNoTilde_ReturnsPathUnchanged()
    {
        var webPageUrl = new WebPageUrl("/services/advisory", "https://example.com/services/advisory");

        _pageUrlRetrieverMock
            .Setup(r => r.Retrieve(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(webPageUrl);

        var result = await _resolver.ResolvePageUrlAsync(5, "en", CancellationToken.None);

        Assert.That(result, Is.EqualTo("/services/advisory"));
    }

    [Test]
    public async Task ResolvePageUrlAsync_WhenPathHasMultipleTildes_RemovesAllTildes()
    {
        var webPageUrl = new WebPageUrl("~/~nested/path", "https://example.com/~nested/path");

        _pageUrlRetrieverMock
            .Setup(r => r.Retrieve(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(webPageUrl);

        var result = await _resolver.ResolvePageUrlAsync(1, "en", CancellationToken.None);

        Assert.That(result, Does.Not.Contain("~"));
    }
    [Test]
    public void GetRelativePath_WhenPageIsNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _resolver.GetRelativePath(null!));
    }

    [Test]
    public void GetAbsoluteUrl_WhenPageIsNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _resolver.GetAbsoluteUrl(null!));
    }
}
