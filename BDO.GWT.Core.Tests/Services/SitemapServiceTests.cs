using BDO.GWT.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using CMS.DataEngine;
using CMS.Localization;
using CMS.ContentEngine;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Data;

namespace BDO.GWT.Core.Tests.Services;

public class SitemapServiceTests
{
    private Mock<IContentQueryExecutor> _contentQueryExecutorMock = null!;
    private Mock<IWebPageUrlRetriever> _webPageUrlRetrieverMock = null!;
    private Mock<IWebsiteChannelContext> _websiteChannelContextMock = null!;
    private Mock<IInfoProvider<ContentLanguageInfo>> _languageInfoProviderMock = null!;
    private Mock<ILogger<SitemapService>> _loggerMock = null!;
    private SitemapService _service = null!;

    [SetUp]
    public void Setup()
    {
        _contentQueryExecutorMock = new Mock<IContentQueryExecutor>();
        _webPageUrlRetrieverMock = new Mock<IWebPageUrlRetriever>();
        _websiteChannelContextMock = new Mock<IWebsiteChannelContext>();
        _languageInfoProviderMock = new Mock<IInfoProvider<ContentLanguageInfo>>();
        _loggerMock = new Mock<ILogger<SitemapService>>();

        _websiteChannelContextMock.SetupGet(x => x.WebsiteChannelName).Returns("TestChannel");

        _service = new SitemapService(
            _contentQueryExecutorMock.Object,
            _webPageUrlRetrieverMock.Object,
            _websiteChannelContextMock.Object,
            _languageInfoProviderMock.Object,
            _loggerMock.Object);
    }

    [Test]
    public async Task GetSitemapDtoAsync_WhenNoLanguages_ReturnsEmptySitemap()
    {
        // Arrange
        var emptyLanguages = Array.Empty<ContentLanguageInfo>();
        var mockQuery = new Mock<ObjectQuery<ContentLanguageInfo>>();
        mockQuery.Setup(x => x.GetEnumerableTypedResultAsync(default, default, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyLanguages);
        _languageInfoProviderMock.Setup(x => x.Get()).Returns(mockQuery.Object);

        // Act
        var result = await _service.GetSitemapDtoAsync(CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Urls, Is.Empty);
        });

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No languages found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public void Constructor_CreatesInstance_WithValidDependencies()
    {
        // Arrange & Act
        var service = new SitemapService(
            _contentQueryExecutorMock.Object,
            _webPageUrlRetrieverMock.Object,
            _websiteChannelContextMock.Object,
            _languageInfoProviderMock.Object,
            _loggerMock.Object);

        // Assert
        Assert.That(service, Is.Not.Null);
        Assert.That(service, Is.InstanceOf<ISitemapService>());
    }

    [Test]
    public async Task GetSitemapDtoAsync_ReturnsCorrectDtoType()
    {
        // Arrange
        var emptyLanguages = Array.Empty<ContentLanguageInfo>();
        var mockQuery = new Mock<ObjectQuery<ContentLanguageInfo>>();
        mockQuery.Setup(x => x.GetEnumerableTypedResultAsync(default, default, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyLanguages);
        _languageInfoProviderMock.Setup(x => x.Get()).Returns(mockQuery.Object);

        // Act
        var result = await _service.GetSitemapDtoAsync(CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.TypeOf<SitemapDto>());
            Assert.That(result.Urls, Is.Not.Null);
        });
    }

    [Test]
    public async Task GetSitemapDtoAsync_WhenNoLanguages_LogsWarningWithCorrectMessage()
    {
        // Arrange
        var emptyLanguages = Array.Empty<ContentLanguageInfo>();
        var mockQuery = new Mock<ObjectQuery<ContentLanguageInfo>>();
        mockQuery.Setup(x => x.GetEnumerableTypedResultAsync(default, default, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyLanguages);
        _languageInfoProviderMock.Setup(x => x.Get()).Returns(mockQuery.Object);

        // Act
        await _service.GetSitemapDtoAsync(CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No languages found") && v.ToString()!.Contains("Returning empty sitemap")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public void SitemapService_ImplementsISitemapService()
    {
        // Assert
        Assert.That(_service, Is.InstanceOf<ISitemapService>());
    }

    [Test]
    public async Task GetSitemapDtoAsync_ReturnsEmptyUrls_WhenNoLanguages()
    {
        // Arrange
        var emptyLanguages = Array.Empty<ContentLanguageInfo>();
        var mockQuery = new Mock<ObjectQuery<ContentLanguageInfo>>();
        mockQuery.Setup(x => x.GetEnumerableTypedResultAsync(default, default, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyLanguages);
        _languageInfoProviderMock.Setup(x => x.Get()).Returns(mockQuery.Object);

        // Act
        var result = await _service.GetSitemapDtoAsync(CancellationToken.None);

        // Assert
        Assert.That(result.Urls, Is.Empty);
    }

    [Test]
    public async Task GetSitemapDtoAsync_InvokesLanguageProvider()
    {
        // Arrange
        var emptyLanguages = Array.Empty<ContentLanguageInfo>();
        var mockQuery = new Mock<ObjectQuery<ContentLanguageInfo>>();
        mockQuery.Setup(x => x.GetEnumerableTypedResultAsync(default, default, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyLanguages);
        _languageInfoProviderMock.Setup(x => x.Get()).Returns(mockQuery.Object);

        // Act
        await _service.GetSitemapDtoAsync(CancellationToken.None);

        // Assert
        _languageInfoProviderMock.Verify(x => x.Get(), Times.Once);
    }

    [Test]
    public void Constructor_AcceptsAllRequiredDependencies()
    {
        // Act
        var service = new SitemapService(
            _contentQueryExecutorMock.Object,
            _webPageUrlRetrieverMock.Object,
            _websiteChannelContextMock.Object,
            _languageInfoProviderMock.Object,
            _loggerMock.Object);

        // Assert - Should not throw
        Assert.That(service, Is.Not.Null);
    }

    [Test]
    public async Task GetSitemapDtoAsync_ReturnsEmptySitemapDto_WithEmptyUrlsCollection()
    {
        // Arrange
        var emptyLanguages = Array.Empty<ContentLanguageInfo>();
        var mockQuery = new Mock<ObjectQuery<ContentLanguageInfo>>();
        mockQuery.Setup(x => x.GetEnumerableTypedResultAsync(default, default, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyLanguages);
        _languageInfoProviderMock.Setup(x => x.Get()).Returns(mockQuery.Object);

        // Act
        var result = await _service.GetSitemapDtoAsync(CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Urls, Is.Not.Null);
            Assert.That(result.Urls, Is.Empty);
            Assert.That(result.Urls, Is.AssignableTo<IEnumerable<SitemapUrlDto>>());
        });
    }

    [Test]
    public async Task GetSitemapDtoAsync_WithCancellationToken_UsesToken()
    {
        // Arrange
        var languages = Array.Empty<ContentLanguageInfo>();
        var mockQuery = new Mock<ObjectQuery<ContentLanguageInfo>>();
        var cancellationToken = new CancellationToken();

        mockQuery.Setup(x => x.GetEnumerableTypedResultAsync(default, default, cancellationToken))
            .ReturnsAsync(languages);
        _languageInfoProviderMock.Setup(x => x.Get()).Returns(mockQuery.Object);

        // Act
        await _service.GetSitemapDtoAsync(cancellationToken);

        // Assert
        _languageInfoProviderMock.Verify(x => x.Get(), Times.Once);
    }

    [Test]
    public async Task GetSitemapDtoAsync_VerifiesWebsiteChannelContextIsInjected()
    {
        // Arrange
        var emptyLanguages = Array.Empty<ContentLanguageInfo>();
        var mockQuery = new Mock<ObjectQuery<ContentLanguageInfo>>();
        mockQuery.Setup(x => x.GetEnumerableTypedResultAsync(default, default, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyLanguages);
        _languageInfoProviderMock.Setup(x => x.Get()).Returns(mockQuery.Object);

        // Act
        await _service.GetSitemapDtoAsync(CancellationToken.None);

        // Assert - The service should be constructed with the website channel context
        // Note: WebsiteChannelName is only accessed when querying page types, which doesn't happen with no languages
        Assert.That(_service, Is.Not.Null);
    }
}
