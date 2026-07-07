using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Web.Core.Controllers;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Controllers;

public class InsightCategoryControllerTests
{
    private Mock<IInsightCategoryCardListingPageService> _serviceMock = null!;
    private InsightCategoryController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _serviceMock = new Mock<IInsightCategoryCardListingPageService>();
        _controller = new InsightCategoryController(_serviceMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _controller.Dispose();
    }

    [Test]
    public async Task Index_ReturnsInsightListingView_WithServiceModel()
    {
        var page = new InsightCategory();
        var expectedDto = new InsightCardListingDto
        {
            Title = "Category listing",
            Hero = new HeroDto(),
            CardsGrid = new CardsGridDto { FilteringTooltipEnabled = false }
        };

        _serviceMock
            .Setup(x => x.GetInsightPageDtoAsync(page, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var result = await _controller.Index(page, CancellationToken.None);

        var viewResult = result as ViewResult;
        Assert.Multiple(() =>
        {
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult!.ViewName, Is.EqualTo("~/Views/InsightListing/Index.cshtml"));
            Assert.That(viewResult.Model, Is.SameAs(expectedDto));
        });
    }

    [Test]
    public async Task Index_PassesCancellationTokenToService()
    {
        var page = new InsightCategory();
        var token = new CancellationTokenSource().Token;

        _serviceMock
            .Setup(x => x.GetInsightPageDtoAsync(page, token))
            .ReturnsAsync(new InsightCardListingDto { Title = "x", Hero = new HeroDto(), CardsGrid = new CardsGridDto { FilteringTooltipEnabled = false } });

        await _controller.Index(page, token);

        _serviceMock.Verify(x => x.GetInsightPageDtoAsync(page, token), Times.Once);
    }
}
