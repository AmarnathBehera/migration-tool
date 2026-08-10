using BDO.GWT.Core;
using BDO.GWT.Web.Core.Data.Queries;
using BDO.GWT.Web.Core.DTOs.Pages;
using BDO.GWT.Web.Core.LiveSite.Controllers;
using BDO.GWT.Web.Core.LiveSite.Services;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Controllers;

public class SubscribeRemoveControllerTests
{
    private Mock<IContentService> _contentServiceMock = null!;
    private Mock<ISubscribeRemovePageService> _subscribeRemovePageServiceMock = null!;
    private SubscribeRemoveController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _contentServiceMock = new Mock<IContentService>();
        _subscribeRemovePageServiceMock = new Mock<ISubscribeRemovePageService>();

        _controller = new SubscribeRemoveController(
            _subscribeRemovePageServiceMock.Object,
            _contentServiceMock.Object);
    }

    [TearDown]
    public void TearDown() => _controller.Dispose();

    private void SetupCurrentPage(SubscribeRemove? page)
    {
        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SubscribeRemove>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);
    }

    private static SubscribeRemoveQuery BuildQuery() => new()
    {
        Email = "user@example.com",
        NewsletterGuid = Guid.NewGuid(),
        Hash = "abc123hash",
    };

    // --- Index -------------------------------------------------------------------

    [Test]
    public async Task Index_ReturnsViewResult()
    {
        var page = new Mock<SubscribeRemove>().Object;
        var query = BuildQuery();
        SetupCurrentPage(page);
        _subscribeRemovePageServiceMock
            .Setup(s => s.GetSubscribeRemoveDtoAsync(page, query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SubscribeRemoveDto());

        var result = await _controller.Index(query, CancellationToken.None);

        Assert.That(result, Is.InstanceOf<ViewResult>());
    }

    [Test]
    public async Task Index_SetsSubscribeRemoveDto_AsViewModel()
    {
        var page = new Mock<SubscribeRemove>().Object;
        var query = BuildQuery();
        var dto = new SubscribeRemoveDto { Message = "You have been unsubscribed." };
        SetupCurrentPage(page);
        _subscribeRemovePageServiceMock
            .Setup(s => s.GetSubscribeRemoveDtoAsync(page, query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = (ViewResult)await _controller.Index(query, CancellationToken.None);

        Assert.That(result.Model, Is.SameAs(dto));
    }

    [Test]
    public async Task Index_PassesRetrievedPage_ToSubscribeRemovePageService()
    {
        var page = new Mock<SubscribeRemove>().Object;
        var query = BuildQuery();
        SetupCurrentPage(page);
        _subscribeRemovePageServiceMock
            .Setup(s => s.GetSubscribeRemoveDtoAsync(page, query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SubscribeRemoveDto());

        await _controller.Index(query, CancellationToken.None);

        _subscribeRemovePageServiceMock.Verify(
            s => s.GetSubscribeRemoveDtoAsync(page, query, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Index_PassesCancellationToken_ToServices()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;
        var page = new Mock<SubscribeRemove>().Object;
        var query = BuildQuery();

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SubscribeRemove>(
                It.IsAny<RetrieveCurrentPageParameters>(), token))
            .ReturnsAsync(page);
        _subscribeRemovePageServiceMock
            .Setup(s => s.GetSubscribeRemoveDtoAsync(page, query, token))
            .ReturnsAsync(new SubscribeRemoveDto());

        await _controller.Index(query, token);

        _contentServiceMock.Verify(
            x => x.RetrieveCurrentPageAsync<SubscribeRemove>(
                It.IsAny<RetrieveCurrentPageParameters>(), token),
            Times.Once);
        _subscribeRemovePageServiceMock.Verify(
            s => s.GetSubscribeRemoveDtoAsync(page, query, token),
            Times.Once);
    }

    [Test]
    public async Task Index_RetrievesCurrentPage_WithLinkedItemsMaxLevel4()
    {
        var page = new Mock<SubscribeRemove>().Object;
        var query = BuildQuery();
        SetupCurrentPage(page);
        _subscribeRemovePageServiceMock
            .Setup(s => s.GetSubscribeRemoveDtoAsync(page, query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SubscribeRemoveDto());

        await _controller.Index(query, CancellationToken.None);

        _contentServiceMock.Verify(
            x => x.RetrieveCurrentPageAsync<SubscribeRemove>(
                It.Is<RetrieveCurrentPageParameters>(p => p.LinkedItemsMaxLevel == 4),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public void Index_PropagatesException_FromContentService()
    {
        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SubscribeRemove>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("content error"));

        Assert.ThrowsAsync<InvalidOperationException>(
            () => _controller.Index(BuildQuery(), CancellationToken.None));
    }

    [Test]
    public void Index_PropagatesException_FromSubscribeRemovePageService()
    {
        var page = new Mock<SubscribeRemove>().Object;
        var query = BuildQuery();
        SetupCurrentPage(page);
        _subscribeRemovePageServiceMock
            .Setup(s => s.GetSubscribeRemoveDtoAsync(page, query, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("service error"));

        Assert.ThrowsAsync<InvalidOperationException>(
            () => _controller.Index(query, CancellationToken.None));
    }

    // ??? Null page passthrough ????????????????????????????????????????????????

    [Test]
    public async Task Index_WhenCurrentPageIsNull_StillCallsSubscribeRemovePageService()
    {
        var query = BuildQuery();
        SetupCurrentPage(null);
        _subscribeRemovePageServiceMock
            .Setup(s => s.GetSubscribeRemoveDtoAsync(null, query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SubscribeRemoveDto());

        var result = await _controller.Index(query, CancellationToken.None);

        Assert.That(result, Is.InstanceOf<ViewResult>());
        _subscribeRemovePageServiceMock.Verify(
            s => s.GetSubscribeRemoveDtoAsync(null, query, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ??? Constructor null guards ???????????????????????????????????????????????

    [Test]
    public void Constructor_ThrowsArgumentNullException_WhenSubscribeRemovePageServiceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new SubscribeRemoveController(null!, _contentServiceMock.Object));
    }

    [Test]
    public void Constructor_ThrowsArgumentNullException_WhenContentServiceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new SubscribeRemoveController(_subscribeRemovePageServiceMock.Object, null!));
    }
}
