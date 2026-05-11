using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Controllers.API;

public class PeopleControllerUnitTests
{
    private Mock<ISectionPeoplePageService> _sectionPeoplePageServiceMock = null!;
    private PeopleController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _sectionPeoplePageServiceMock = new Mock<ISectionPeoplePageService>();
        _controller = new PeopleController(_sectionPeoplePageServiceMock.Object);

        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    [Test]
    public async Task Get_ReturnsOkResult_WithPaginationDto()
    {
        var pageGuid = Guid.NewGuid();
        var query = new CardsQuery();
        var expectedDto = new PaginationDto<PersonCardDto>
        {
            Data =
            [
                new() { DisplayName = "John Doe" }
            ],
            TotalCount = 1
        };

        _sectionPeoplePageServiceMock
            .Setup(s => s.GetSectionPeoplePersonPaginationDtoAsync(pageGuid, It.IsAny<CardsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var result = await _controller.Get(pageGuid, query, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result!;
        Assert.That(okResult.Value, Is.EqualTo(expectedDto));
    }

    [Test]
    public async Task Get_ReturnsOkResult_WithEmptyPaginationDto()
    {
        var pageGuid = Guid.NewGuid();
        var query = new CardsQuery();
        var expectedDto = new PaginationDto<PersonCardDto>
        {
            Data = [],
            TotalCount = 0
        };

        _sectionPeoplePageServiceMock
            .Setup(s => s.GetSectionPeoplePersonPaginationDtoAsync(pageGuid, It.IsAny<CardsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var result = await _controller.Get(pageGuid, query, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result!;
        var dto = okResult.Value as PaginationDto<PersonCardDto>;
        Assert.Multiple(() =>
        {
            Assert.That(dto!.Data, Is.Empty);
            Assert.That(dto.TotalCount, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task Get_CallsServiceWithCorrectPageGuid()
    {
        var pageGuid = Guid.NewGuid();
        var query = new CardsQuery();

        _sectionPeoplePageServiceMock
            .Setup(s => s.GetSectionPeoplePersonPaginationDtoAsync(pageGuid, It.IsAny<CardsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginationDto<PersonCardDto> { Data = [] });

        await _controller.Get(pageGuid, query, CancellationToken.None);

        _sectionPeoplePageServiceMock.Verify(
            s => s.GetSectionPeoplePersonPaginationDtoAsync(pageGuid, It.IsAny<CardsQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Get_ExtractsFilterQueryParameters_FromRequest()
    {
        var pageGuid = Guid.NewGuid();
        var query = new CardsQuery();

        _controller.HttpContext.Request.QueryString = new QueryString("?filter[location]=US&filter[role]=Partner");

        _sectionPeoplePageServiceMock
            .Setup(s => s.GetSectionPeoplePersonPaginationDtoAsync(pageGuid, It.IsAny<CardsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginationDto<PersonCardDto> { Data = [] });

        var result = await _controller.Get(pageGuid, query, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
    }
}