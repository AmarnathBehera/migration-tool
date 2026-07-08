using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.LiveSite.Controllers;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using CMS.ContentEngine;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Controllers;

public class InsightGroupControllerTests
{
    private Mock<IInsightGroupCardListingPageService> _serviceMock = null!;
    private Mock<IContentRetriever> _contentRetrieverMock = null!;
    private InsightGroupController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _serviceMock = new Mock<IInsightGroupCardListingPageService>();
        _contentRetrieverMock = new Mock<IContentRetriever>();
        _controller = new InsightGroupController(_serviceMock.Object, _contentRetrieverMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _controller.Dispose();
    }

    [Test]
    public async Task Index_ReturnsInsightListingView_WithServiceResult()
    {
        var page = new InsightGroup();
        var expectedDto = new InsightCardListingDto
        {
            Title = "Group listing",
            Hero = new HeroDto(),
            CardsGrid = new CardsGridDto { FilteringTooltipEnabled = false }
        };

        _contentRetrieverMock
            .Setup(x => x.RetrieveCurrentPage<InsightGroup>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<Action<RetrieveCurrentPageQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, InsightGroup, Task<InsightGroup>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        _serviceMock
            .Setup(x => x.GetInsightPageDtoAsync(page, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var result = await _controller.Index(CancellationToken.None);

        var viewResult = result as ViewResult;
        Assert.Multiple(() =>
        {
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult!.ViewName, Is.EqualTo("~/Views/InsightListing/Index.cshtml"));
            Assert.That(viewResult.Model, Is.SameAs(expectedDto));
        });
    }

    [Test]
    public async Task Index_CallsServiceExactlyOnce()
    {
        var page = new InsightGroup();

        _contentRetrieverMock
            .Setup(x => x.RetrieveCurrentPage<InsightGroup>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<Action<RetrieveCurrentPageQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, InsightGroup, Task<InsightGroup>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        _serviceMock
            .Setup(x => x.GetInsightPageDtoAsync(page, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InsightCardListingDto { Title = "x", Hero = new HeroDto(), CardsGrid = new CardsGridDto { FilteringTooltipEnabled = false } });

        await _controller.Index(CancellationToken.None);

        _serviceMock.Verify(x => x.GetInsightPageDtoAsync(page, It.IsAny<CancellationToken>()), Times.Once);
    }
}
