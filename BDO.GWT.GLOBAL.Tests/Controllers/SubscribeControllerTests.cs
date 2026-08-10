using BDO.GWT.Core;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.LiveSite.Controllers;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Controllers;

public class SubscribeControllerTests
{
    private Mock<ISubscribePageService> _subscribePageServiceMock = null!;
    private Mock<IContentService> _contentServiceMock = null!;
    private SubscribeController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _subscribePageServiceMock = new Mock<ISubscribePageService>();
        _contentServiceMock = new Mock<IContentService>();

        _controller = new SubscribeController(
            _subscribePageServiceMock.Object,
            _contentServiceMock.Object);
    }

    [TearDown]
    public void TearDown() => _controller.Dispose();

    private void SetupCurrentPage(Subscribe? page)
    {
        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<Subscribe>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);
    }

    // ??? Index ????????????????????????????????????????????????????????????????

    [Test]
    public async Task Index_ReturnsViewResult()
    {
        var page = new Mock<Subscribe>().Object;
        SetupCurrentPage(page);
        _subscribePageServiceMock
            .Setup(s => s.GetSubscribeDtoAsync(page, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SubscribeDto());

        var result = await _controller.Index(CancellationToken.None);

        Assert.That(result, Is.InstanceOf<ViewResult>());
    }

    [Test]
    public async Task Index_PassesRetrievedPage_ToSubscribePageService()
    {
        var page = new Mock<Subscribe>().Object;
        SetupCurrentPage(page);
        _subscribePageServiceMock
            .Setup(s => s.GetSubscribeDtoAsync(page, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SubscribeDto());

        await _controller.Index(CancellationToken.None);

        _subscribePageServiceMock.Verify(
            s => s.GetSubscribeDtoAsync(page, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Index_SetsSubscribeDto_AsViewModel()
    {
        var page = new Mock<Subscribe>().Object;
        var dto = new SubscribeDto { FormType = "form" };
        SetupCurrentPage(page);
        _subscribePageServiceMock
            .Setup(s => s.GetSubscribeDtoAsync(page, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = (ViewResult)await _controller.Index(CancellationToken.None);

        Assert.That(result.Model, Is.SameAs(dto));
    }

    [Test]
    public async Task Index_PassesCancellationToken_ToContentService()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;
        var page = new Mock<Subscribe>().Object;

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<Subscribe>(
                It.IsAny<RetrieveCurrentPageParameters>(), token))
            .ReturnsAsync(page);
        _subscribePageServiceMock
            .Setup(s => s.GetSubscribeDtoAsync(page, token))
            .ReturnsAsync(new SubscribeDto());

        await _controller.Index(token);

        _contentServiceMock.Verify(
            x => x.RetrieveCurrentPageAsync<Subscribe>(
                It.IsAny<RetrieveCurrentPageParameters>(), token),
            Times.Once);
    }

    [Test]
    public async Task Index_PassesCancellationToken_ToSubscribePageService()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;
        var page = new Mock<Subscribe>().Object;

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<Subscribe>(
                It.IsAny<RetrieveCurrentPageParameters>(), token))
            .ReturnsAsync(page);
        _subscribePageServiceMock
            .Setup(s => s.GetSubscribeDtoAsync(page, token))
            .ReturnsAsync(new SubscribeDto());

        await _controller.Index(token);

        _subscribePageServiceMock.Verify(
            s => s.GetSubscribeDtoAsync(page, token),
            Times.Once);
    }

    [Test]
    public void Index_PropagatesException_FromContentService()
    {
        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<Subscribe>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("content service error"));

        Assert.ThrowsAsync<InvalidOperationException>(
            () => _controller.Index(CancellationToken.None));
    }

    [Test]
    public void Index_PropagatesException_FromSubscribePageService()
    {
        var page = new Mock<Subscribe>().Object;
        SetupCurrentPage(page);
        _subscribePageServiceMock
            .Setup(s => s.GetSubscribeDtoAsync(page, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("service error"));

        Assert.ThrowsAsync<InvalidOperationException>(
            () => _controller.Index(CancellationToken.None));
    }

    [Test]
    public async Task Index_RetrievesCurrentPage_WithLinkedItemsMaxLevel4()
    {
        var page = new Mock<Subscribe>().Object;
        SetupCurrentPage(page);
        _subscribePageServiceMock
            .Setup(s => s.GetSubscribeDtoAsync(page, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SubscribeDto());

        await _controller.Index(CancellationToken.None);

        _contentServiceMock.Verify(
            x => x.RetrieveCurrentPageAsync<Subscribe>(
                It.Is<RetrieveCurrentPageParameters>(p => p.LinkedItemsMaxLevel == 4),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ??? Null page passthrough ????????????????????????????????????????????????

    [Test]
    public async Task Index_WhenCurrentPageIsNull_StillCallsSubscribePageService()
    {
        SetupCurrentPage(null);
        _subscribePageServiceMock
            .Setup(s => s.GetSubscribeDtoAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SubscribeDto());

        var result = await _controller.Index(CancellationToken.None);

        Assert.That(result, Is.InstanceOf<ViewResult>());
        _subscribePageServiceMock.Verify(
            s => s.GetSubscribeDtoAsync(null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ??? Constructor null guards ???????????????????????????????????????????????

    [Test]
    public void Constructor_ThrowsArgumentNullException_WhenSubscribePageServiceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new SubscribeController(null!, _contentServiceMock.Object));
    }

    [Test]
    public void Constructor_ThrowsArgumentNullException_WhenContentServiceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new SubscribeController(_subscribePageServiceMock.Object, null!));
    }
}
