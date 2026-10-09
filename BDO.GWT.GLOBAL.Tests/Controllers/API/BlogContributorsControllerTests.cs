using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.LiveSite.Controllers.API;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;

namespace BDO.GWT.GLOBAL.Tests.Controllers.API;

[TestFixture]
public class BlogContributorsControllerTests
{
    private Mock<IBlogContributorsPageService> _blogContributorsPageServiceMock = null!;
    private BlogContributorsController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _blogContributorsPageServiceMock = new Mock<IBlogContributorsPageService>();
        _controller = new BlogContributorsController(_blogContributorsPageServiceMock.Object);

        // Setup HttpContext with empty query collection
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext,
            RouteData = new RouteData()
        };
    }

    #region Get

    [Test]
    public async Task Get_ReturnsOkResult_WithPaginationDto()
    {
        // Arrange
        var pageGuid = Guid.NewGuid();
        var query = new CardsQuery();
        var expectedResult = new PaginationDto<PersonCardDto>
        {
            Data = [new PersonCardDto { DisplayName = "John Doe" }],
            TotalCount = 1
        };

        _blogContributorsPageServiceMock
            .Setup(x => x.GetBlogContributorsPaginationDtoAsync(
                pageGuid,
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Get(pageGuid, query, CancellationToken.None);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.Multiple(() =>
        {
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.EqualTo(expectedResult));
        });
    }

    [Test]
    public async Task Get_PassesCorrectPageGuidToService()
    {
        // Arrange
        var pageGuid = Guid.NewGuid();
        var query = new CardsQuery();

        _blogContributorsPageServiceMock
            .Setup(x => x.GetBlogContributorsPaginationDtoAsync(
                pageGuid,
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginationDto<PersonCardDto> { Data = [], TotalCount = 0 });

        // Act
        await _controller.Get(pageGuid, query, CancellationToken.None);

        // Assert
        _blogContributorsPageServiceMock.Verify(
            x => x.GetBlogContributorsPaginationDtoAsync(
                pageGuid,
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Get_PassesCardsQueryToService()
    {
        // Arrange
        var pageGuid = Guid.NewGuid();
        var query = new CardsQuery { CurrentPage = 3, PageSize = 5 };

        CardsQuery? capturedQuery = null;
        _blogContributorsPageServiceMock
            .Setup(x => x.GetBlogContributorsPaginationDtoAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()))
            .Callback<Guid, string, CardsQuery, CancellationToken>((_, _, q, _) => capturedQuery = q)
            .ReturnsAsync(new PaginationDto<PersonCardDto> { Data = [], TotalCount = 0 });

        // Act
        await _controller.Get(pageGuid, query, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(capturedQuery, Is.Not.Null);
            Assert.That(capturedQuery!.CurrentPage, Is.EqualTo(3));
            Assert.That(capturedQuery.PageSize, Is.EqualTo(5));
        });
    }

    [Test]
    public async Task Get_PassesCorrectCancellationToken()
    {
        // Arrange
        var pageGuid = Guid.NewGuid();
        var query = new CardsQuery();
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _blogContributorsPageServiceMock
            .Setup(x => x.GetBlogContributorsPaginationDtoAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                token))
            .ReturnsAsync(new PaginationDto<PersonCardDto> { Data = [], TotalCount = 0 });

        // Act
        await _controller.Get(pageGuid, query, token);

        // Assert
        _blogContributorsPageServiceMock.Verify(
            x => x.GetBlogContributorsPaginationDtoAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                token),
            Times.Once);
    }

    [Test]
    public async Task Get_ReturnsOkResult_WithEmptyPagination()
    {
        // Arrange
        var pageGuid = Guid.NewGuid();
        var query = new CardsQuery();
        var expectedResult = new PaginationDto<PersonCardDto>
        {
            Data = [],
            TotalCount = 0
        };

        _blogContributorsPageServiceMock
            .Setup(x => x.GetBlogContributorsPaginationDtoAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Get(pageGuid, query, CancellationToken.None);

        // Assert
        var okResult = result.Result as OkObjectResult;
        var paginationDto = okResult!.Value as PaginationDto<PersonCardDto>;
        Assert.Multiple(() =>
        {
            Assert.That(okResult, Is.Not.Null);
            Assert.That(paginationDto!.TotalCount, Is.EqualTo(0));
            Assert.That(paginationDto.Data, Is.Empty);
        });
    }

    [Test]
    public async Task Get_ReturnsOkResult_WithMultipleCards()
    {
        // Arrange
        var pageGuid = Guid.NewGuid();
        var query = new CardsQuery();
        var expectedCards = new PersonCardDto[]
        {
            new() { DisplayName = "Person A" },
            new() { DisplayName = "Person B" },
            new() { DisplayName = "Person C" }
        };
        var expectedResult = new PaginationDto<PersonCardDto>
        {
            Data = expectedCards,
            TotalCount = 25
        };

        _blogContributorsPageServiceMock
            .Setup(x => x.GetBlogContributorsPaginationDtoAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Get(pageGuid, query, CancellationToken.None);

        // Assert
        var okResult = result.Result as OkObjectResult;
        var paginationDto = okResult!.Value as PaginationDto<PersonCardDto>;
        Assert.Multiple(() =>
        {
            Assert.That(paginationDto!.TotalCount, Is.EqualTo(25));
            Assert.That(paginationDto.Data, Has.Length.EqualTo(3));
            Assert.That(paginationDto.Data[0].DisplayName, Is.EqualTo("Person A"));
            Assert.That(paginationDto.Data[1].DisplayName, Is.EqualTo("Person B"));
            Assert.That(paginationDto.Data[2].DisplayName, Is.EqualTo("Person C"));
        });
    }

    [Test]
    public void Get_WhenServiceThrows_ExceptionIsPropagated()
    {
        // Arrange
        var pageGuid = Guid.NewGuid();
        var query = new CardsQuery();

        _blogContributorsPageServiceMock
            .Setup(x => x.GetBlogContributorsPaginationDtoAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Service error"));

        // Act & Assert
        Assert.ThrowsAsync<Exception>(async () =>
            await _controller.Get(pageGuid, query, CancellationToken.None));
    }

    [Test]
    public void Get_WhenServiceThrowsOperationCanceled_ExceptionIsPropagated()
    {
        // Arrange
        var pageGuid = Guid.NewGuid();
        var query = new CardsQuery();

        _blogContributorsPageServiceMock
            .Setup(x => x.GetBlogContributorsPaginationDtoAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await _controller.Get(pageGuid, query, CancellationToken.None));
    }

    [Test]
    public async Task Get_CallsServiceExactlyOnce()
    {
        // Arrange
        var pageGuid = Guid.NewGuid();
        var query = new CardsQuery();

        _blogContributorsPageServiceMock
            .Setup(x => x.GetBlogContributorsPaginationDtoAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginationDto<PersonCardDto> { Data = [], TotalCount = 0 });

        // Act
        await _controller.Get(pageGuid, query, CancellationToken.None);

        // Assert
        _blogContributorsPageServiceMock.Verify(
            x => x.GetBlogContributorsPaginationDtoAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion
}