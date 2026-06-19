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
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Core.Tests.Services;

[TestFixture]
public class SitemapServiceTests
{
    private Mock<IContentQueryExecutor> _contentQueryExecutorMock;
    private Mock<IWebPageUrlRetriever> _webPageUrlRetrieverMock;
    private Mock<IWebsiteChannelContext> _websiteChannelContextMock;
    private Mock<IInfoProvider<ContentLanguageInfo>> _languageInfoProviderMock;
    private Mock<ILogger<SitemapService>> _loggerMock;
    private SitemapService _sut;

    [SetUp]
    public void SetUp()
    {
        _contentQueryExecutorMock = new Mock<IContentQueryExecutor>();
        _webPageUrlRetrieverMock = new Mock<IWebPageUrlRetriever>();
        _websiteChannelContextMock = new Mock<IWebsiteChannelContext>();
        _languageInfoProviderMock = new Mock<IInfoProvider<ContentLanguageInfo>>();
        _loggerMock = new Mock<ILogger<SitemapService>>();

        _websiteChannelContextMock.SetupGet(x => x.WebsiteChannelName).Returns("TestChannel");

        _sut = new SitemapService(
            _contentQueryExecutorMock.Object,
            _webPageUrlRetrieverMock.Object,
            _websiteChannelContextMock.Object,
            _languageInfoProviderMock.Object,
            _loggerMock.Object);
    }

    // -------------------------------------------------------------------------
    // Helper
    // -------------------------------------------------------------------------

    private void SetupLanguageProvider(IEnumerable<ContentLanguageInfo> languages)
    {
        var mockQuery = new Mock<ObjectQuery<ContentLanguageInfo>>();
        mockQuery
            .Setup(x => x.GetEnumerableTypedResultAsync(
                default, default, It.IsAny<CancellationToken>()))
            .ReturnsAsync(languages);
        _languageInfoProviderMock.Setup(x => x.Get()).Returns(mockQuery.Object);
    }

    // -------------------------------------------------------------------------
    // Tests
    // -------------------------------------------------------------------------

    [Test]
    public async Task GetSitemapDtoAsync_WhenNoLanguages_ReturnsEmptySitemap()
    {
        SetupLanguageProvider(Array.Empty<ContentLanguageInfo>());

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
        SetupLanguageProvider(Array.Empty<ContentLanguageInfo>());

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
    public async Task GetSitemapDtoAsync_WhenNoLanguages_DoesNotCallContentQueryExecutor()
    {
        SetupLanguageProvider(Array.Empty<ContentLanguageInfo>());

        await _sut.GetSitemapDtoAsync(CancellationToken.None);

        _contentQueryExecutorMock.Verify(
            x => x.GetResult(
                It.IsAny<ContentItemQueryBuilder>(),
                It.IsAny<Func<IContentQueryDataContainer, object>>(),
                It.IsAny<ContentQueryExecutionOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GetSitemapDtoAsync_WhenNoLanguages_DoesNotCallUrlRetriever()
    {
        SetupLanguageProvider(Array.Empty<ContentLanguageInfo>());

        await _sut.GetSitemapDtoAsync(CancellationToken.None);

        _webPageUrlRetrieverMock.Verify(
            x => x.Retrieve(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GetSitemapDtoAsync_WhenNoLanguages_InvokesLanguageProviderOnce()
    {
        SetupLanguageProvider(Array.Empty<ContentLanguageInfo>());

        await _sut.GetSitemapDtoAsync(CancellationToken.None);

        _languageInfoProviderMock.Verify(x => x.Get(), Times.Once);
    }

    [Test]
    public async Task GetSitemapDtoAsync_ReturnsSitemapDto()
    {
        SetupLanguageProvider(Array.Empty<ContentLanguageInfo>());

        var result = await _sut.GetSitemapDtoAsync(CancellationToken.None);

        Assert.That(result, Is.InstanceOf<SitemapDto>());
    }

    [Test]
    public async Task GetSitemapDtoAsync_UrlsCollectionIsNotNull()
    {
        SetupLanguageProvider(Array.Empty<ContentLanguageInfo>());

        var result = await _sut.GetSitemapDtoAsync(CancellationToken.None);

        Assert.That(result.Urls, Is.Not.Null);
    }

    [Test]
    public async Task GetSitemapDtoAsync_UrlsIsAssignableToIEnumerable()
    {
        SetupLanguageProvider(Array.Empty<ContentLanguageInfo>());

        var result = await _sut.GetSitemapDtoAsync(CancellationToken.None);

        Assert.That(result.Urls, Is.AssignableTo<IEnumerable<SitemapUrlDto>>());
    }

    [Test]
    public void SitemapService_ImplementsISitemapService()
    {
        Assert.That(_sut, Is.InstanceOf<ISitemapService>());
    }

    [Test]
    public async Task GetSitemapDtoAsync_WhenNoLanguages_DoesNotLogError()
    {
        SetupLanguageProvider(Array.Empty<ContentLanguageInfo>());

        await _sut.GetSitemapDtoAsync(CancellationToken.None);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}