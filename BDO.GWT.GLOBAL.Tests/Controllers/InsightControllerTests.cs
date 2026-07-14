using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.DTOs.SchemaMarkups;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.LiveSite.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Controllers;

public class InsightControllerTests
{
    private Mock<IInsightPageService> _insightPageServiceMock = null!;
    private InsightController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _insightPageServiceMock = new Mock<IInsightPageService>();
        _controller = new InsightController(_insightPageServiceMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _controller.Dispose();
    }

    [Test]
    public async Task Index_ReturnsExpectedView_WithInsightDtoModel()
    {
        var expectedDto = new InsightPageDto(new ContentPageDto())
        {
            SchemaMarkup = new ArticleSchemaMarkupDto(),
            Title = "Insight title"
        };

        _insightPageServiceMock
            .Setup(x => x.GetInsightPageDtoAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var result = await _controller.Index(CancellationToken.None);

        var viewResult = result as ViewResult;
        Assert.Multiple(() =>
        {
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult!.ViewName, Is.EqualTo("~/Views/InsightPage/Index.cshtml"));
            Assert.That(viewResult.Model, Is.SameAs(expectedDto));
        });
    }

    [Test]
    public async Task Index_CallsServiceOnce_WithNullPageParameter()
    {
        _insightPageServiceMock
            .Setup(x => x.GetInsightPageDtoAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InsightPageDto(new ContentPageDto()) { SchemaMarkup = new ArticleSchemaMarkupDto() });

        await _controller.Index(CancellationToken.None);

        _insightPageServiceMock.Verify(
            x => x.GetInsightPageDtoAsync(null, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
