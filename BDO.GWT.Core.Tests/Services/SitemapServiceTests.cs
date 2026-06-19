using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.Services;
using Moq;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Core.Tests.Services;

[TestFixture]
public class SitemapServiceTests
{
    private readonly Mock<ISitemapService> _sitemapServiceMock;

    public SitemapServiceTests()
    {
        _sitemapServiceMock = new Mock<ISitemapService>();
    }

    [Test]
    public async Task GetSitemapDtoAsync_ReturnsDto()
    {
        var expectedDto = new SitemapDto { Urls = [] };

        _sitemapServiceMock
            .Setup(s => s.GetSitemapDtoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var result = await _sitemapServiceMock.Object.GetSitemapDtoAsync(CancellationToken.None);

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task GetSitemapDtoAsync_WhenNoPages_ReturnsEmptyUrls()
    {
        _sitemapServiceMock
            .Setup(s => s.GetSitemapDtoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SitemapDto { Urls = [] });

        var result = await _sitemapServiceMock.Object.GetSitemapDtoAsync(CancellationToken.None);

        Assert.That(result.Urls, Is.Empty);
    }

    [Test]
    public async Task GetSitemapDtoAsync_WhenPagesExist_ReturnsNonEmptyUrls()
    {
        var urls = new List<SitemapUrlDto>
        {
            new() { LocationUri = "https://example.com/page-1" },
            new() { LocationUri = "https://example.com/page-2" },
        };

        _sitemapServiceMock
            .Setup(s => s.GetSitemapDtoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SitemapDto { Urls = urls });

        var result = await _sitemapServiceMock.Object.GetSitemapDtoAsync(CancellationToken.None);

        Assert.That(result.Urls, Has.Exactly(2).Items);
    }

    [Test]
    public async Task GetSitemapDtoAsync_UrlsContainCorrectLocationUri()
    {
        var urls = new List<SitemapUrlDto>
        {
            new() { LocationUri = "https://example.com/careers" },
        };

        _sitemapServiceMock
            .Setup(s => s.GetSitemapDtoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SitemapDto { Urls = urls });

        var result = await _sitemapServiceMock.Object.GetSitemapDtoAsync(CancellationToken.None);

        Assert.That(result.Urls.First().LocationUri, Is.EqualTo("https://example.com/careers"));
    }

    [Test]
    public async Task GetSitemapDtoAsync_WhenMultiLanguage_ReturnsAlternateUrls()
    {
        var urls = new List<SitemapUrlDto>
        {
            new()
            {
                LocationUri = "https://example.com/en-gb/page",
                AlternateUrls =
                [
                    new() { Url = "https://example.com/en-gb/page", LanguageTag = "en-gb" },
                    new() { Url = "https://example.com/fr/page", LanguageTag = "fr" },
                ]
            }
        };

        _sitemapServiceMock
            .Setup(s => s.GetSitemapDtoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SitemapDto { Urls = urls });

        var result = await _sitemapServiceMock.Object.GetSitemapDtoAsync(CancellationToken.None);

        Assert.That(result.Urls.First().AlternateUrls, Has.Exactly(2).Items);
    }

    [Test]
    public async Task GetSitemapDtoAsync_WhenSingleLanguage_ReturnsNullAlternateUrls()
    {
        var urls = new List<SitemapUrlDto>
        {
            new()
            {
                LocationUri = "https://example.com/en-gb/page",
                AlternateUrls = null
            }
        };

        _sitemapServiceMock
            .Setup(s => s.GetSitemapDtoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SitemapDto { Urls = urls });

        var result = await _sitemapServiceMock.Object.GetSitemapDtoAsync(CancellationToken.None);

        Assert.That(result.Urls.First().AlternateUrls, Is.Null);
    }

    [Test]
    public async Task GetSitemapDtoAsync_UrlsContainLastModificationDate()
    {
        var lastModified = new DateTime(2024, 6, 1);
        var urls = new List<SitemapUrlDto>
        {
            new() { LocationUri = "https://example.com/page", LastModificationDate = lastModified }
        };

        _sitemapServiceMock
            .Setup(s => s.GetSitemapDtoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SitemapDto { Urls = urls });

        var result = await _sitemapServiceMock.Object.GetSitemapDtoAsync(CancellationToken.None);

        Assert.That(result.Urls.First().LastModificationDate, Is.EqualTo(lastModified));
    }

    [Test]
    public async Task GetSitemapDtoAsync_IsCalledOnce()
    {
        _sitemapServiceMock
            .Setup(s => s.GetSitemapDtoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SitemapDto { Urls = [] });

        await _sitemapServiceMock.Object.GetSitemapDtoAsync(CancellationToken.None);

        _sitemapServiceMock.Verify(
            s => s.GetSitemapDtoAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}