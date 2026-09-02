using BDO.GWT.GLOBAL.API.Controllers;
using BDO.GWT.GLOBAL.API.DTOs;
using BDO.GWT.GLOBAL.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.API.Tests.Controllers;

[TestFixture]
public class ContentControllerTests
{
    private const int ChannelId = 42;

    private Mock<IContentCategoryService> _contentServiceMock = null!;
    private ContentController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _contentServiceMock = new Mock<IContentCategoryService>();

        var httpContext = new DefaultHttpContext();
        httpContext.Items["ChannelId"] = ChannelId;

        _controller = new ContentController(_contentServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };
    }

    [Test]
    public async Task GetContentCategories_ReturnsCategoriesInValue()
    {
        var expected = new List<ContentCategoryDto>
        {
            new()
            {
                Name = "Business Issue",
                Code = "BusinessIssue",
                SubCategories = new List<ContentSubCategoryDto>
                {
                    new() { Id = 1, Name = "Sub 1" }
                }
            }
        };

        _contentServiceMock
            .Setup(x => x.GetContentCategoriesAsync(ChannelId, "en-GB", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetContentCategories(1, "en-GB", CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.Null);
            Assert.That(result.Value, Is.SameAs(expected));
        });

        _contentServiceMock.Verify(
            x => x.GetContentCategoriesAsync(ChannelId, "en-GB", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetContentCategories_UsesChannelIdFromHttpContext()
    {
        _contentServiceMock
            .Setup(x => x.GetContentCategoriesAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ContentCategoryDto>());

        await _controller.GetContentCategories(1, null, CancellationToken.None);

        _contentServiceMock.Verify(
            x => x.GetContentCategoriesAsync(ChannelId, null!, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetContentFromCategories_ReturnsContentInValue()
    {
        var expected = new ContentDto
        {
            InsightsSearchURL = "https://example.com/search",
            Insights = new List<InsightContentDto>()
        };

        var filters = new[]
        {
            new ContentCategoryFilter { CategoryCode = "BusinessIssue", SubCategoryIds = new[] { 1, 2 } }
        };

        _contentServiceMock
            .Setup(x => x.GetContentAsync(ChannelId, "en-GB", 5, 3, filters, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetContentFromCategories(1, "en-GB", 5, 3, filters, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.Null);
            Assert.That(result.Value, Is.SameAs(expected));
        });

        _contentServiceMock.Verify(
            x => x.GetContentAsync(ChannelId, "en-GB", 5, 3, filters, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetContentFromCategories_PassesCountsAndFilters()
    {
        var filters = Array.Empty<ContentCategoryFilter>();

        _contentServiceMock
            .Setup(x => x.GetContentAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<ContentCategoryFilter[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContentDto { Insights = new List<InsightContentDto>() });

        await _controller.GetContentFromCategories(1, "fr-FR", 10, 4, filters, CancellationToken.None);

        _contentServiceMock.Verify(
            x => x.GetContentAsync(ChannelId, "fr-FR", 10, 4, filters, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetContentCategories_ForwardsCancellationTokenToService()
    {
        using var cts = new CancellationTokenSource();
        _contentServiceMock
            .Setup(x => x.GetContentCategoriesAsync(ChannelId, "en-GB", cts.Token))
            .ReturnsAsync(new List<ContentCategoryDto>());

        await _controller.GetContentCategories(1, "en-GB", cts.Token);

        _contentServiceMock.Verify(x => x.GetContentCategoriesAsync(ChannelId, "en-GB", cts.Token), Times.Once);
    }

    [Test]
    public async Task GetContentFromCategories_ForwardsCancellationTokenToService()
    {
        using var cts = new CancellationTokenSource();
        var filters = Array.Empty<ContentCategoryFilter>();
        _contentServiceMock
            .Setup(x => x.GetContentAsync(ChannelId, "en-GB", 1, 1, filters, cts.Token))
            .ReturnsAsync(new ContentDto { Insights = new List<InsightContentDto>() });

        await _controller.GetContentFromCategories(1, "en-GB", 1, 1, filters, cts.Token);

        _contentServiceMock.Verify(x => x.GetContentAsync(ChannelId, "en-GB", 1, 1, filters, cts.Token), Times.Once);
    }

    [Test]
    public async Task GetContentCategories_WhenChannelIdMissingFromHttpContext_UsesZero()
    {
        var httpContext = new DefaultHttpContext();
        var controller = new ContentController(_contentServiceMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };

        _contentServiceMock
            .Setup(x => x.GetContentCategoriesAsync(0, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ContentCategoryDto>());

        await controller.GetContentCategories(1, "en-GB", CancellationToken.None);

        _contentServiceMock.Verify(x => x.GetContentCategoriesAsync(0, "en-GB", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void GetContentCategories_WhenServiceThrows_PropagatesException()
    {
        _contentServiceMock
            .Setup(x => x.GetContentCategoriesAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _controller.GetContentCategories(1, "en-GB", CancellationToken.None));
    }

    [Test]
    public void GetContentFromCategories_WhenServiceThrows_PropagatesException()
    {
        _contentServiceMock
            .Setup(x => x.GetContentAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<ContentCategoryFilter[]>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _controller.GetContentFromCategories(1, "en-GB", 1, 1, Array.Empty<ContentCategoryFilter>(), CancellationToken.None));
    }

    [Test]
    public async Task GetContentFromCategories_WithNullCulture_ForwardsNullToService()
    {
        var filters = Array.Empty<ContentCategoryFilter>();
        _contentServiceMock
            .Setup(x => x.GetContentAsync(ChannelId, null!, 0, 0, filters, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContentDto { Insights = new List<InsightContentDto>() });

        await _controller.GetContentFromCategories(1, null!, 0, 0, filters, CancellationToken.None);

        _contentServiceMock.Verify(x => x.GetContentAsync(ChannelId, null!, 0, 0, filters, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestCase(0, 0)]
    [TestCase(5, 10)]
    [TestCase(-1, -1)]
    public async Task GetContentFromCategories_PassesCountValues(int countInsights, int countWhatsNew)
    {
        var filters = Array.Empty<ContentCategoryFilter>();
        _contentServiceMock
            .Setup(x => x.GetContentAsync(ChannelId, "en-GB", countInsights, countWhatsNew, filters, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContentDto { Insights = new List<InsightContentDto>() });

        await _controller.GetContentFromCategories(1, "en-GB", countInsights, countWhatsNew, filters, CancellationToken.None);

        _contentServiceMock.Verify(
            x => x.GetContentAsync(ChannelId, "en-GB", countInsights, countWhatsNew, filters, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
