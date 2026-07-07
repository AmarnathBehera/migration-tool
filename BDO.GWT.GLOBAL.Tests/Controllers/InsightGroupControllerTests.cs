using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.LiveSite.Controllers;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Controllers;

public class InsightGroupControllerTests
{
    private Mock<IInsightGroupCardListingPageService> _serviceMock = null!;
    private InsightGroupController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _serviceMock = new Mock<IInsightGroupCardListingPageService>();
        _controller = new InsightGroupController(_serviceMock.Object);
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
    public async Task Index_CallsServiceExactlyOnce()
    {
        var page = new InsightGroup();

        _serviceMock
            .Setup(x => x.GetInsightPageDtoAsync(page, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InsightCardListingDto { Title = "x", Hero = new HeroDto(), CardsGrid = new CardsGridDto { FilteringTooltipEnabled = false } });

        await _controller.Index(page, CancellationToken.None);

        _serviceMock.Verify(x => x.GetInsightPageDtoAsync(page, It.IsAny<CancellationToken>()), Times.Once);
    }
}
