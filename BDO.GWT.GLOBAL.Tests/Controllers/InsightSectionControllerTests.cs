using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.LiveSite.Controllers;
using BDO.GWT.Web.Core.Services;
using CMS.ContentEngine;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Controllers;

public class InsightSectionControllerTests
{
    private Mock<IInsightSectionCardListingPageService> _serviceMock = null!;
    private Mock<IContentRetriever> _contentRetrieverMock = null!;
    private InsightSectionController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _serviceMock = new Mock<IInsightSectionCardListingPageService>();
        _contentRetrieverMock = new Mock<IContentRetriever>();
        _controller = new InsightSectionController(_serviceMock.Object, _contentRetrieverMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _controller.Dispose();
    }

    [Test]
    public async Task Index_ReturnsInsightListingView_WithDtoFromService()
    {
        var page = new SectionInsightsPage();
        var expectedDto = new InsightCardListingDto
        {
            Title = "Section listing",
            Hero = new HeroDto(),
            CardsGrid = new CardsGridDto { FilteringTooltipEnabled = false }
        };

        _contentRetrieverMock
            .Setup(x => x.RetrieveCurrentPage<SectionInsightsPage>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<Action<RetrieveCurrentPageQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, SectionInsightsPage, Task<SectionInsightsPage>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        _serviceMock
            .Setup(x => x.GetContentSectionDtoAsync(page, It.IsAny<CancellationToken>()))
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
    public async Task Index_ForwardsCancellationTokenToService()
    {
        var page = new SectionInsightsPage();
        var token = new CancellationTokenSource().Token;

        _contentRetrieverMock
            .Setup(x => x.RetrieveCurrentPage<SectionInsightsPage>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<Action<RetrieveCurrentPageQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, SectionInsightsPage, Task<SectionInsightsPage>>>(),
                token))
            .ReturnsAsync(page);

        _serviceMock
            .Setup(x => x.GetContentSectionDtoAsync(page, token))
            .ReturnsAsync(new InsightCardListingDto { Title = "x", Hero = new HeroDto(), CardsGrid = new CardsGridDto { FilteringTooltipEnabled = false } });

        await _controller.Index(token);

        _serviceMock.Verify(x => x.GetContentSectionDtoAsync(page, token), Times.Once);
    }
}
