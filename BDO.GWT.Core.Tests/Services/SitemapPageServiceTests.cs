using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using CMS.ContentEngine;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace BDO.GWT.Core.Tests.Services;

[TestFixture]
public class SitemapPageServiceTests
{
    private Mock<IHeroService> _heroServiceMock = null!;
    private Mock<IContentQueryExecutor> _contentQueryExecutorMock = null!;
    private Mock<IWebPageUrlRetriever> _webPageUrlRetrieverMock = null!;
    private Mock<IWebsiteChannelContext> _websiteChannelContextMock = null!;
    private Mock<ICultureContext> _cultureContextMock = null!;
    private Mock<IContentService> _contentServiceMock = null!;
    private Mock<ILogger<SitemapPageService>> _loggerMock = null!;

    private SitemapPageService _service = null!;

    [SetUp]
    public void Setup()
    {
        _heroServiceMock = new Mock<IHeroService>();
        _contentQueryExecutorMock = new Mock<IContentQueryExecutor>();
        _webPageUrlRetrieverMock = new Mock<IWebPageUrlRetriever>();
        _websiteChannelContextMock = new Mock<IWebsiteChannelContext>();
        _cultureContextMock = new Mock<ICultureContext>();
        _contentServiceMock = new Mock<IContentService>();
        _loggerMock = new Mock<ILogger<SitemapPageService>>();

        _websiteChannelContextMock
            .SetupGet(x => x.WebsiteChannelName)
            .Returns("TestSite");

        _cultureContextMock
            .SetupGet(x => x.CurrentLanguageTag)
            .Returns("en-US");

        _service = new SitemapPageService(
            _heroServiceMock.Object,
            _contentQueryExecutorMock.Object,
            _webPageUrlRetrieverMock.Object,
            _websiteChannelContextMock.Object,
            _cultureContextMock.Object,
            _contentServiceMock.Object,
            _loggerMock.Object);
    }

    private static List<SitemapPageNode> CreatePages()
    {
        return new List<SitemapPageNode>
        {
            new()
            {
                WebPageItemID = 1,
                Name = "Home",
                TreePath = "/home"
            }
        };
    }

    [Test]
    public async Task GetSitemapPageDtoAsync_ReturnsMappedDto()
    {
        var sitemapPage = new Sitemap
        {
            HeroTitle = "Sitemap Hero",
            HeroDescription = "Hero Description"
        };

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<Sitemap>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sitemapPage);

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(
                It.IsAny<HeroData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto
            {
                Title = "Mapped Hero"
            });

        _contentQueryExecutorMock
            .Setup(x => x.GetResult(
                It.IsAny<ContentItemQueryBuilder>(),
                It.IsAny<Func<IContentQueryDataContainer, SitemapPageNode>>(),
                It.IsAny<ContentQueryExecutionOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreatePages());

        _webPageUrlRetrieverMock
            .Setup(x => x.Retrieve(
                1,
                It.IsAny<string>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WebPageUrl("/home", "/home"));

        var result = await _service.GetSitemapPageDtoAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Hero.Title, Is.EqualTo("Mapped Hero"));
            Assert.That(result.Items[0].Title, Is.EqualTo("Home"));
            Assert.That(result.Items[0].Link, Is.EqualTo("/home"));
        });
    }

    [Test]
    public void GetSitemapPageDtoAsync_Throws_WhenNoPagesReturned()
    {
        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<Sitemap>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Sitemap());

        _contentQueryExecutorMock
            .Setup(x => x.GetResult(
                It.IsAny<ContentItemQueryBuilder>(),
                It.IsAny<Func<IContentQueryDataContainer, SitemapPageNode>>(),
                It.IsAny<ContentQueryExecutionOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<SitemapPageNode>());

        Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.GetSitemapPageDtoAsync(CancellationToken.None));
    }

    [Test]
    public async Task GetSitemapPageDtoAsync_CallsHeroServiceOnce()
    {
        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<Sitemap>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Sitemap());

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(
                It.IsAny<HeroData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        _contentQueryExecutorMock
            .Setup(x => x.GetResult(
                It.IsAny<ContentItemQueryBuilder>(),
                It.IsAny<Func<IContentQueryDataContainer, SitemapPageNode>>(),
                It.IsAny<ContentQueryExecutionOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreatePages());

        _webPageUrlRetrieverMock
            .Setup(x => x.Retrieve(
                It.IsAny<int>(),
                It.IsAny<string>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WebPageUrl("/home", "/home"));

        await _service.GetSitemapPageDtoAsync(CancellationToken.None);

        _heroServiceMock.Verify(
            x => x.GetHeroDtoAsync(
                It.IsAny<HeroData>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetSitemapPageDtoAsync_CallsContentServiceOnce()
    {
        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<Sitemap>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Sitemap());

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(
                It.IsAny<HeroData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        _contentQueryExecutorMock
            .Setup(x => x.GetResult(
                It.IsAny<ContentItemQueryBuilder>(),
                It.IsAny<Func<IContentQueryDataContainer, SitemapPageNode>>(),
                It.IsAny<ContentQueryExecutionOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreatePages());

        _webPageUrlRetrieverMock
            .Setup(x => x.Retrieve(
                It.IsAny<int>(),
                It.IsAny<string>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WebPageUrl("/home", "/home"));

        await _service.GetSitemapPageDtoAsync(CancellationToken.None);

        _contentServiceMock.Verify(
            x => x.RetrieveCurrentPageAsync<Sitemap>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetSitemapPageDtoAsync_CallsUrlRetriever()
    {
        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<Sitemap>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Sitemap());

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(
                It.IsAny<HeroData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        _contentQueryExecutorMock
            .Setup(x => x.GetResult(
                It.IsAny<ContentItemQueryBuilder>(),
                It.IsAny<Func<IContentQueryDataContainer, SitemapPageNode>>(),
                It.IsAny<ContentQueryExecutionOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreatePages());

        _webPageUrlRetrieverMock
            .Setup(x => x.Retrieve(
                It.IsAny<int>(),
                It.IsAny<string>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WebPageUrl("/home", "/home"));

        await _service.GetSitemapPageDtoAsync(CancellationToken.None);

        _webPageUrlRetrieverMock.Verify(
            x => x.Retrieve(
                1,
                It.IsAny<string>(),
                false,
                It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    [Test]
    public async Task GetSitemapPageDtoAsync_UsesHeroReturnedByHeroService()
    {
        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<Sitemap>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Sitemap());

        var heroDto = new HeroDto
        {
            Title = "Expected Hero"
        };

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(
                It.IsAny<HeroData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(heroDto);

        _contentQueryExecutorMock
            .Setup(x => x.GetResult(
                It.IsAny<ContentItemQueryBuilder>(),
                It.IsAny<Func<IContentQueryDataContainer, SitemapPageNode>>(),
                It.IsAny<ContentQueryExecutionOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreatePages());

        _webPageUrlRetrieverMock
            .Setup(x => x.Retrieve(
                It.IsAny<int>(),
                It.IsAny<string>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WebPageUrl("/home", "/home"));

        var result = await _service.GetSitemapPageDtoAsync(CancellationToken.None);

        Assert.That(result.Hero.Title, Is.EqualTo("Expected Hero"));
    }
}