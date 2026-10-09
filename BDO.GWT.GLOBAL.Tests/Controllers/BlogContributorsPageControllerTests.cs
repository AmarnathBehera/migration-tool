using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Controllers;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Controllers;

public class BlogContributorsPageControllerTests
{
    private Mock<IBlogContributorsPageService> _blogContributorsPageServiceMock = null!;
    private Mock<IContentService> _contentServiceMock = null!;
    private BlogContributorsPageController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _blogContributorsPageServiceMock = new Mock<IBlogContributorsPageService>();
        _contentServiceMock = new Mock<IContentService>();
        _controller = new BlogContributorsPageController(
            _blogContributorsPageServiceMock.Object,
            _contentServiceMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _controller.Dispose();
    }

    [Test]
    public async Task Index_ReturnsNotFound_WhenCurrentPageIsNull()
    {
        _contentServiceMock
            .Setup(s => s.RetrieveCurrentPageAsync<BlogContributors>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((BlogContributors?)null);

        var result = await _controller.Index(CancellationToken.None);

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task Index_ReturnsViewResult_WhenPageExists()
    {
        var page = new BlogContributors();
        var expectedDto = new BlogContributorsDto();

        _contentServiceMock
            .Setup(s => s.RetrieveCurrentPageAsync<BlogContributors>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        _blogContributorsPageServiceMock
            .Setup(s => s.GetSectionPeoplePageDtoAsync(page, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var result = await _controller.Index(CancellationToken.None);

        Assert.That(result, Is.TypeOf<ViewResult>());
    }

    [Test]
    public async Task Index_ReturnsCorrectViewName()
    {
        var page = new BlogContributors();

        _contentServiceMock
            .Setup(s => s.RetrieveCurrentPageAsync<BlogContributors>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        _blogContributorsPageServiceMock
            .Setup(s => s.GetSectionPeoplePageDtoAsync(page, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlogContributorsDto());

        var result = await _controller.Index(CancellationToken.None) as ViewResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.ViewName, Is.EqualTo("~/Views/BlogContributorsPage/Index.cshtml"));
    }

    [Test]
    public async Task Index_ReturnsViewResult_WithCorrectModel()
    {
        var page = new BlogContributors();
        var expectedDto = new BlogContributorsDto
        {
            Hero = new HeroDto { Title = "Contributors" },
            PageSize = 20,
            ApiEndpoint = "BlogContributors/test/Get"
        };

        _contentServiceMock
            .Setup(s => s.RetrieveCurrentPageAsync<BlogContributors>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        _blogContributorsPageServiceMock
            .Setup(s => s.GetSectionPeoplePageDtoAsync(page, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var result = await _controller.Index(CancellationToken.None) as ViewResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Model, Is.SameAs(expectedDto));
    }

    [Test]
    public async Task Index_DoesNotCallPageService_WhenCurrentPageIsNull()
    {
        _contentServiceMock
            .Setup(s => s.RetrieveCurrentPageAsync<BlogContributors>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((BlogContributors?)null);

        await _controller.Index(CancellationToken.None);

        _blogContributorsPageServiceMock.Verify(
            s => s.GetSectionPeoplePageDtoAsync(
                It.IsAny<BlogContributors>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task Index_CallsContentService_WithLinkedItemsMaxLevel4()
    {
        var page = new BlogContributors();

        _contentServiceMock
            .Setup(s => s.RetrieveCurrentPageAsync<BlogContributors>(
                It.Is<RetrieveCurrentPageParameters>(p => p.LinkedItemsMaxLevel == 2),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        _blogContributorsPageServiceMock
            .Setup(s => s.GetSectionPeoplePageDtoAsync(page, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlogContributorsDto());

        await _controller.Index(CancellationToken.None);

        _contentServiceMock.Verify(
            s => s.RetrieveCurrentPageAsync<BlogContributors>(
                It.Is<RetrieveCurrentPageParameters>(p => p.LinkedItemsMaxLevel == 2),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Index_PassesCancellationToken_ToContentService()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _contentServiceMock
            .Setup(s => s.RetrieveCurrentPageAsync<BlogContributors>(
                It.IsAny<RetrieveCurrentPageParameters>(), token))
            .ReturnsAsync((BlogContributors?)null);

        await _controller.Index(token);

        _contentServiceMock.Verify(
            s => s.RetrieveCurrentPageAsync<BlogContributors>(
                It.IsAny<RetrieveCurrentPageParameters>(), token),
            Times.Once);
    }

    [Test]
    public async Task Index_PassesCancellationToken_ToPageService()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;
        var page = new BlogContributors();

        _contentServiceMock
            .Setup(s => s.RetrieveCurrentPageAsync<BlogContributors>(
                It.IsAny<RetrieveCurrentPageParameters>(), token))
            .ReturnsAsync(page);

        _blogContributorsPageServiceMock
            .Setup(s => s.GetSectionPeoplePageDtoAsync(page, token))
            .ReturnsAsync(new BlogContributorsDto());

        await _controller.Index(token);

        _blogContributorsPageServiceMock.Verify(
            s => s.GetSectionPeoplePageDtoAsync(page, token),
            Times.Once);
    }

    [Test]
    public void Index_WhenContentServiceThrows_ExceptionIsPropagated()
    {
        _contentServiceMock
            .Setup(s => s.RetrieveCurrentPageAsync<BlogContributors>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Content service error"));

        Assert.ThrowsAsync<Exception>(async () =>
            await _controller.Index(CancellationToken.None));
    }

    [Test]
    public void Index_WhenPageServiceThrows_ExceptionIsPropagated()
    {
        var page = new BlogContributors();

        _contentServiceMock
            .Setup(s => s.RetrieveCurrentPageAsync<BlogContributors>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        _blogContributorsPageServiceMock
            .Setup(s => s.GetSectionPeoplePageDtoAsync(page, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Page service error"));

        Assert.ThrowsAsync<Exception>(async () =>
            await _controller.Index(CancellationToken.None));
    }

    [Test]
    public void Index_WhenServiceThrowsOperationCanceled_ExceptionIsPropagated()
    {
        _contentServiceMock
            .Setup(s => s.RetrieveCurrentPageAsync<BlogContributors>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await _controller.Index(CancellationToken.None));
    }

    [Test]
    public async Task Index_CallsPageService_WithRetrievedPage()
    {
        var page = new BlogContributors();

        _contentServiceMock
            .Setup(s => s.RetrieveCurrentPageAsync<BlogContributors>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        _blogContributorsPageServiceMock
            .Setup(s => s.GetSectionPeoplePageDtoAsync(page, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlogContributorsDto());

        await _controller.Index(CancellationToken.None);

        _blogContributorsPageServiceMock.Verify(
            s => s.GetSectionPeoplePageDtoAsync(page, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}