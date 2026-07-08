using BDO.GWT.Controllers.API;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Controllers.API;

public class InsightsControllerTests
{
    private Mock<IInsightSectionCardListingPageService> _serviceMock = null!;
    private InsightsController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _serviceMock = new Mock<IInsightSectionCardListingPageService>();
        _controller = new InsightsController(_serviceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Test]
    public void Constructor_Throws_WhenServiceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new InsightsController(null!));
    }

    [Test]
    public async Task Get_ReturnsOk_WithPaginationResult()
    {
        var pageGuid = Guid.NewGuid();
        var query = new CardsQuery();
        var expected = new PaginationDto<ContentPageCardDto>
        {
            Data = Array.Empty<ContentPageCardDto>(),
            TotalCount = 0
        };

        _serviceMock
            .Setup(x => x.GetContentPaginationResultAsync(pageGuid, It.IsAny<CardsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.Get(pageGuid, query, CancellationToken.None);

        var okResult = result.Result as OkObjectResult;
        Assert.Multiple(() =>
        {
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.SameAs(expected));
        });
    }

    [Test]
    public async Task Get_ExtractsFilters_FromRequestQuery()
    {
        var pageGuid = Guid.NewGuid();
        var query = new CardsQuery();
        var queryCollection = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { "contentType", "Article" },
            { "businessLine", "Tax" }
        });

        _controller.ControllerContext.HttpContext.Request.Query = queryCollection;

        _serviceMock
            .Setup(x => x.GetContentPaginationResultAsync(pageGuid, It.IsAny<CardsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginationDto<ContentPageCardDto> { Data = Array.Empty<ContentPageCardDto>(), TotalCount = 0 });

        await _controller.Get(pageGuid, query, CancellationToken.None);

        _serviceMock.Verify(
            x => x.GetContentPaginationResultAsync(
                pageGuid,
                It.Is<CardsQuery>(q => q.Filters.Count > 0),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
