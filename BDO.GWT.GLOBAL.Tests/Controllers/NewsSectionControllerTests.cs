using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Controllers;
using BDO.GWT.Web.Core.DTOs;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Controllers;

public class NewsSectionControllerTests
{
    private readonly Mock<IContentService> _contentServiceMock;
    private readonly Mock<INewsSectionPageService> _newsSectionPageServiceMock;
    private readonly NewsSectionController _controller;

    public NewsSectionControllerTests()
    {
        _contentServiceMock = new Mock<IContentService>();
        _newsSectionPageServiceMock = new Mock<INewsSectionPageService>();
        _controller = new NewsSectionController(_contentServiceMock.Object, _newsSectionPageServiceMock.Object);
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
            .Setup(x => x.RetrieveCurrentPageAsync<SectionNews>(
                It.IsAny<RetrieveCurrentPageParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SectionNews?)null);

        var result = await _controller.Index(CancellationToken.None);

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Index_WhenCurrentPageExists_ReturnsViewWithNewsSectionDto()
    {
        var sectionNews = new SectionNews();
        var expectedDto = new NewsSectionDto { Title = "Test", Hero = new HeroDto(), CardsGrid = new CardsGridDto { FilteringTooltipEnabled = false } };

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SectionNews>(
                It.IsAny<RetrieveCurrentPageParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sectionNews);

        _newsSectionPageServiceMock
            .Setup(x => x.GetNewsSectionDtoAsync(sectionNews, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var result = await _controller.Index(CancellationToken.None);

        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = (ViewResult)result;
        Assert.That(viewResult.Model, Is.EqualTo(expectedDto));
    }

    [Test]
    public async Task Index_RetrievesCurrentPageWithLinkedItemsMaxLevel4()
    {
        var sectionNews = new SectionNews();

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SectionNews>(
                It.Is<RetrieveCurrentPageParameters>(p => p.LinkedItemsMaxLevel == 4),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sectionNews);

        _newsSectionPageServiceMock
            .Setup(x => x.GetNewsSectionDtoAsync(sectionNews, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NewsSectionDto { Title = "Test", Hero = new HeroDto(), CardsGrid = new CardsGridDto { FilteringTooltipEnabled = false } });

        await _controller.Index(CancellationToken.None);

        _contentServiceMock.Verify(x => x.RetrieveCurrentPageAsync<SectionNews>(
            It.Is<RetrieveCurrentPageParameters>(p => p.LinkedItemsMaxLevel == 4),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
