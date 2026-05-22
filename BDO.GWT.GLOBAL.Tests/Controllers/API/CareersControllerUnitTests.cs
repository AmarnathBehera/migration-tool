using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.GLOBAL.Tests.Utilities;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using GWTXbyKWeb.Controllers.API;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Tests.Controllers.API;

public class CareersControllerUnitTests
{
    private readonly Mock<ICareersPageService> _mockService = new();

    private CareersController _controller = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        var httpContext = new DefaultHttpContext();

        var routeData = new RouteData();

        var actionDescriptor = new ControllerActionDescriptor();

        var actionContext = new ActionContext(
            httpContext,
            routeData,
            actionDescriptor);

        _controller = new CareersController(_mockService.Object)
        {
            ControllerContext = new ControllerContext(actionContext)
        };
    }

    [TearDown]
    public void TearDown()
    {
        _mockService.Reset();
    }

    [Test]
    public async Task Get_ServiceIsCalled_ReturnsOkWithPaginationDto()
    {
        // Arrange
        var pageGuid = Guid.NewGuid();

        var query = new CardsQuery();

        var paginationDto = new PaginationDto<CareerCardDto>
        {
            Data = new[]
            {
                new CareerCardDto(),
                new CareerCardDto()
            },
            TotalCount = 2
        };

        _mockService
            .Setup(x => x.GetCareerPaginationResultAsync(
                It.IsAny<Guid>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(paginationDto);

        // Act
        var result = await _controller.Get(
            pageGuid,
            query,
            CancellationToken.None);

        // Assert
        AssertUtility.IsInstanceOf<OkObjectResult>(
            result.Result,
            out var okResult);

        Assert.That(
            okResult.Value,
            Is.EqualTo(paginationDto));

        _mockService.Verify(x => x.GetCareerPaginationResultAsync(
                It.Is<Guid>(g => g == pageGuid),
                It.Is<CardsQuery>(q => q == query),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Get_EmptyResult_ReturnsOkWithEmptyPaginationDto()
    {
        // Arrange
        var pageGuid = Guid.NewGuid();

        var query = new CardsQuery();

        var emptyDto = new PaginationDto<CareerCardDto>
        {
            Data = Array.Empty<CareerCardDto>(),
            TotalCount = 0
        };

        _mockService
            .Setup(x => x.GetCareerPaginationResultAsync(
                It.IsAny<Guid>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyDto);

        // Act
        var result = await _controller.Get(
            pageGuid,
            query,
            CancellationToken.None);

        // Assert
        AssertUtility.IsInstanceOf<OkObjectResult>(
            result.Result,
            out var okResult);

        Assert.That(
            okResult.Value,
            Is.EqualTo(emptyDto));

        _mockService.Verify(x => x.GetCareerPaginationResultAsync(
                It.Is<Guid>(g => g == pageGuid),
                It.Is<CardsQuery>(q => q == query),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}