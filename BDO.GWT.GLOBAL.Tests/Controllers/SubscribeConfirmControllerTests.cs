using BDO.GWT.Core;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Web.Core.DTOs.Pages;
using BDO.GWT.Web.Core.LiveSite.Controllers;
using BDO.GWT.Web.Core.LiveSite.Services;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Controllers;

public class SubscribeConfirmControllerTests
{
    private Mock<IContentService> _contentServiceMock = null!;
    private Mock<ISubscribeConfirmPageService> _subscribeConfirmPageServiceMock = null!;
    private SubscribeConfirmController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _contentServiceMock = new Mock<IContentService>();
        _subscribeConfirmPageServiceMock = new Mock<ISubscribeConfirmPageService>();

        _controller = new SubscribeConfirmController(
            _contentServiceMock.Object,
            _subscribeConfirmPageServiceMock.Object);
    }

    [TearDown]
    public void TearDown() => _controller.Dispose();

    private void SetupCurrentPage(SubscribeConfirm? page)
    {
        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SubscribeConfirm>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);
    }

    private static SubscribeConfirmQuery BuildQuery() => new()
    {
        SubscriptionHash = "abc123",
        DateTime = "2024-01-01",
        ContactId = 1,
        RecipientListId = 2,
    };

    // ??? Index ????????????????????????????????????????????????????????????????

    [Test]
    public async Task Index_ReturnsViewResult()
    {
        var page = new Mock<SubscribeConfirm>().Object;
        SetupCurrentPage(page);
        var query = BuildQuery();
        _subscribeConfirmPageServiceMock
            .Setup(s => s.GetSubscribeConfirmDtoAsync(page, query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SubscribeConfirmDto());

        var result = await _controller.Index(query, CancellationToken.None);

        Assert.That(result, Is.InstanceOf<ViewResult>());
    }

    [Test]
    public async Task Index_SetsSubscribeConfirmDto_AsViewModel()
    {
        var page = new Mock<SubscribeConfirm>().Object;
        var dto = new SubscribeConfirmDto { Message = "Thank you!" };
        SetupCurrentPage(page);
        var query = BuildQuery();
        _subscribeConfirmPageServiceMock
            .Setup(s => s.GetSubscribeConfirmDtoAsync(page, query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = (ViewResult)await _controller.Index(query, CancellationToken.None);

        Assert.That(result.Model, Is.SameAs(dto));
    }

    [Test]
    public async Task Index_PassesRetrievedPage_ToSubscribeConfirmPageService()
    {
        var page = new Mock<SubscribeConfirm>().Object;
        SetupCurrentPage(page);
        var query = BuildQuery();
        _subscribeConfirmPageServiceMock
            .Setup(s => s.GetSubscribeConfirmDtoAsync(page, query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SubscribeConfirmDto());

        await _controller.Index(query, CancellationToken.None);

        _subscribeConfirmPageServiceMock.Verify(
            s => s.GetSubscribeConfirmDtoAsync(page, query, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Index_PassesCancellationToken_ToServices()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;
        var page = new Mock<SubscribeConfirm>().Object;
        var query = BuildQuery();

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SubscribeConfirm>(
                It.IsAny<RetrieveCurrentPageParameters>(), token))
            .ReturnsAsync(page);
        _subscribeConfirmPageServiceMock
            .Setup(s => s.GetSubscribeConfirmDtoAsync(page, query, token))
            .ReturnsAsync(new SubscribeConfirmDto());

        await _controller.Index(query, token);

        _contentServiceMock.Verify(
            x => x.RetrieveCurrentPageAsync<SubscribeConfirm>(
                It.IsAny<RetrieveCurrentPageParameters>(), token),
            Times.Once);
        _subscribeConfirmPageServiceMock.Verify(
            s => s.GetSubscribeConfirmDtoAsync(page, query, token),
            Times.Once);
    }

    [Test]
    public async Task Index_WhenCurrentPageIsNull_StillReturnsViewResult()
    {
        SetupCurrentPage(null);
        var query = BuildQuery();
        _subscribeConfirmPageServiceMock
            .Setup(s => s.GetSubscribeConfirmDtoAsync(null, query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SubscribeConfirmDto());

        var result = await _controller.Index(query, CancellationToken.None);

        Assert.That(result, Is.InstanceOf<ViewResult>());
        _subscribeConfirmPageServiceMock.Verify(
            s => s.GetSubscribeConfirmDtoAsync(null, query, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Index_RetrievesCurrentPage_WithLinkedItemsMaxLevel4()
    {
        var page = new Mock<SubscribeConfirm>().Object;
        SetupCurrentPage(page);
        var query = BuildQuery();
        _subscribeConfirmPageServiceMock
            .Setup(s => s.GetSubscribeConfirmDtoAsync(page, query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SubscribeConfirmDto());

        await _controller.Index(query, CancellationToken.None);

        _contentServiceMock.Verify(
            x => x.RetrieveCurrentPageAsync<SubscribeConfirm>(
                It.Is<RetrieveCurrentPageParameters>(p => p.LinkedItemsMaxLevel == 4),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public void Index_PropagatesException_FromContentService()
    {
        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SubscribeConfirm>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("content error"));

        Assert.ThrowsAsync<InvalidOperationException>(
            () => _controller.Index(BuildQuery(), CancellationToken.None));
    }

    [Test]
    public void Index_PropagatesException_FromSubscribeConfirmPageService()
    {
        var page = new Mock<SubscribeConfirm>().Object;
        SetupCurrentPage(page);
        var query = BuildQuery();
        _subscribeConfirmPageServiceMock
            .Setup(s => s.GetSubscribeConfirmDtoAsync(page, query, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("service error"));

        Assert.ThrowsAsync<InvalidOperationException>(
            () => _controller.Index(query, CancellationToken.None));
    }
}
