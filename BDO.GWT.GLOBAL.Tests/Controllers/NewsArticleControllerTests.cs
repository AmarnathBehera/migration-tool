using BDO.GWT.Core;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Controllers;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Controllers;

public class NewsArticleControllerTests
{
    private readonly Mock<IContentService> _contentServiceMock;
    private readonly Mock<INewsArticleService> _newsArticleServiceMock;
    private readonly NewsArticleController _controller;

    public NewsArticleControllerTests()
    {
        _contentServiceMock = new Mock<IContentService>();
        _newsArticleServiceMock = new Mock<INewsArticleService>();
        _controller = new NewsArticleController(_contentServiceMock.Object, _newsArticleServiceMock.Object);
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
            .Setup(x => x.RetrieveCurrentPageAsync<NewsArticle>(
                It.IsAny<RetrieveCurrentPageParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((NewsArticle?)null);

        var result = await _controller.Index(CancellationToken.None);

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Index_WhenCurrentPageExists_ReturnsViewWithSectionNewsPageDto()
    {
        var newsArticle = new NewsArticle();
        var expectedDto = new SectionNewsPageDto();

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<NewsArticle>(
                It.IsAny<RetrieveCurrentPageParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newsArticle);

        _newsArticleServiceMock
            .Setup(x => x.GetSectionNewsPageDtoAsync(newsArticle, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var result = await _controller.Index(CancellationToken.None);

        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = (ViewResult)result;
        Assert.Multiple(() =>
        {
            Assert.That(viewResult.ViewName, Is.EqualTo("~/Views/NewsArticle/Index.cshtml"));
            Assert.That(viewResult.Model, Is.EqualTo(expectedDto));
        });
    }

    [Test]
    public async Task Index_RetrievesCurrentPageWithLinkedItemsMaxLevel4()
    {
        var newsArticle = new NewsArticle();

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<NewsArticle>(
                It.Is<RetrieveCurrentPageParameters>(p => p.LinkedItemsMaxLevel == 4),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(newsArticle);

        _newsArticleServiceMock
            .Setup(x => x.GetSectionNewsPageDtoAsync(newsArticle, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SectionNewsPageDto());

        await _controller.Index(CancellationToken.None);

        _contentServiceMock.Verify(x => x.RetrieveCurrentPageAsync<NewsArticle>(
            It.Is<RetrieveCurrentPageParameters>(p => p.LinkedItemsMaxLevel == 4),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}