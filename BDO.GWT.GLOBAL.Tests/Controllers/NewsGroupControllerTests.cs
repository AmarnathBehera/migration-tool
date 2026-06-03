using BDO.GWT.Core;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Controllers;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Controllers;

public class NewsGroupControllerTests
{
    private readonly Mock<IContentService> _contentServiceMock;
    private readonly Mock<INewsGroupPageService> _newsGroupPageServiceMock;
    private readonly NewsGroupController _controller;

    public NewsGroupControllerTests()
    {
        _contentServiceMock = new Mock<IContentService>();
        _newsGroupPageServiceMock = new Mock<INewsGroupPageService>();
        _controller = new NewsGroupController(_contentServiceMock.Object, _newsGroupPageServiceMock.Object);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _controller.Dispose();
    }

    [Test]
    public async Task Index_WhenCurrentPageIsNull_ReturnsNotFound()
    {
        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<NewsGroup>(
                It.IsAny<RetrieveCurrentPageParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((NewsGroup?)null);

        var result = await _controller.Index(CancellationToken.None);

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Index_WhenCurrentPageExists_ReturnsViewWithNewsGroupDto()
    {
        var newsGroup = new NewsGroup();
        var expectedDto = new NewsGroupDto { Title = "Test", Links = [] };

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<NewsGroup>(
                It.IsAny<RetrieveCurrentPageParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newsGroup);

        _newsGroupPageServiceMock
            .Setup(x => x.GetNewsGroupDtoAsync(newsGroup, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var result = await _controller.Index(CancellationToken.None);

        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = (ViewResult)result;
        Assert.That(viewResult.Model, Is.EqualTo(expectedDto));
    }

    [Test]
    public async Task Index_RetrievesCurrentPageWithLinkedItemsMaxLevel4()
    {
        var newsGroup = new NewsGroup();

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<NewsGroup>(
                It.Is<RetrieveCurrentPageParameters>(p => p.LinkedItemsMaxLevel == 4),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(newsGroup);

        _newsGroupPageServiceMock
            .Setup(x => x.GetNewsGroupDtoAsync(newsGroup, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NewsGroupDto { Title = "Test", Links = [] });

        await _controller.Index(CancellationToken.None);

        _contentServiceMock.Verify(x => x.RetrieveCurrentPageAsync<NewsGroup>(
            It.Is<RetrieveCurrentPageParameters>(p => p.LinkedItemsMaxLevel == 4),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
