using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.LiveSite.Filters;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;

namespace BDO.GWT.GLOBAL.Tests.Controllers.API;

[TestFixture]
public class BlogPostsControllerTests
{
    private Mock<IBlogsPageService> _blogsPageServiceMock = null!;
    private BlogPostsController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _blogsPageServiceMock = new Mock<IBlogsPageService>();
        _controller = new BlogPostsController(_blogsPageServiceMock.Object);

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
        var pageId = 42;
        var query = new CardsQuery();
        var expectedResult = new PaginationDto<BlogPageCardDto?>
        {
            Data = new[] { new BlogPageCardDto { Title = "Post A" } },
            TotalCount = 1
        };

        _controller.ControllerContext.RouteData.Values[CultureInjectorAttribute.RouteParameter] = "en-US";

        _blogsPageServiceMock
            .Setup(x => x.GetBlogsPaginationResultAsync(
                pageId,
                "en-US",
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Get(pageId, query, CancellationToken.None);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.Multiple(() =>
        {
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.EqualTo(expectedResult));
        });
    }

    [Test]
    public async Task Get_PassesCorrectPageIdToService()
    {
        // Arrange
        var pageId = 77;
        var query = new CardsQuery();

        _controller.ControllerContext.RouteData.Values[CultureInjectorAttribute.RouteParameter] = "en-US";

        _blogsPageServiceMock
            .Setup(x => x.GetBlogsPaginationResultAsync(
                pageId,
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginationDto<BlogPageCardDto?> { Data = Array.Empty<BlogPageCardDto?>(), TotalCount = 0 });

        // Act
        await _controller.Get(pageId, query, CancellationToken.None);

        // Assert
        _blogsPageServiceMock.Verify(
            x => x.GetBlogsPaginationResultAsync(
                77,
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Get_ExtractsLanguageFromRouteData()
    {
        // Arrange
        var pageId = 1;
        var query = new CardsQuery();

        _controller.ControllerContext.RouteData.Values[CultureInjectorAttribute.RouteParameter] = "fr-FR";

        _blogsPageServiceMock
            .Setup(x => x.GetBlogsPaginationResultAsync(
                It.IsAny<int>(),
                "fr-FR",
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginationDto<BlogPageCardDto?> { Data = Array.Empty<BlogPageCardDto?>(), TotalCount = 0 });

        // Act
        await _controller.Get(pageId, query, CancellationToken.None);

        // Assert
        _blogsPageServiceMock.Verify(
            x => x.GetBlogsPaginationResultAsync(
                It.IsAny<int>(),
                "fr-FR",
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Get_WhenLanguageRouteValueIsNull_PassesNullLanguage()
    {
        // Arrange
        var pageId = 1;
        var query = new CardsQuery();

        // No culture route value set

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        _ = _blogsPageServiceMock
            .Setup(x => x.GetBlogsPaginationResultAsync(
                It.IsAny<int>(),
                null,
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginationDto<BlogPageCardDto?> { Data = Array.Empty<BlogPageCardDto?>(), TotalCount = 0 });
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        // Act
        await _controller.Get(pageId, query, CancellationToken.None);

        // Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        _blogsPageServiceMock.Verify(
            x => x.GetBlogsPaginationResultAsync(
                It.IsAny<int>(),
                null,
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [Test]
    public async Task Get_PassesCorrectCancellationToken()
    {
        // Arrange
        var pageId = 1;
        var query = new CardsQuery();
        var cts = new CancellationTokenSource();
        var token = cts.Token;

        _controller.ControllerContext.RouteData.Values[CultureInjectorAttribute.RouteParameter] = "en-US";

        _blogsPageServiceMock
            .Setup(x => x.GetBlogsPaginationResultAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                token))
            .ReturnsAsync(new PaginationDto<BlogPageCardDto?> { Data = Array.Empty<BlogPageCardDto?>(), TotalCount = 0 });

        // Act
        await _controller.Get(pageId, query, token);

        // Assert
        _blogsPageServiceMock.Verify(
            x => x.GetBlogsPaginationResultAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                token),
            Times.Once);
    }

    [Test]
    public async Task Get_ReturnsOkResult_WithEmptyPagination()
    {
        // Arrange
        var pageId = 1;
        var query = new CardsQuery();
        var expectedResult = new PaginationDto<BlogPageCardDto?>
        {
            Data = Array.Empty<BlogPageCardDto?>(),
            TotalCount = 0
        };

        _controller.ControllerContext.RouteData.Values[CultureInjectorAttribute.RouteParameter] = "en-US";

        _blogsPageServiceMock
            .Setup(x => x.GetBlogsPaginationResultAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Get(pageId, query, CancellationToken.None);

        // Assert
        var okResult = result.Result as OkObjectResult;
        var paginationDto = okResult!.Value as PaginationDto<BlogPageCardDto?>;
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
        var pageId = 10;
        var query = new CardsQuery();
        var expectedCards = new BlogPageCardDto?[]
        {
            new BlogPageCardDto { Title = "Post A" },
            new BlogPageCardDto { Title = "Post B" },
            new BlogPageCardDto { Title = "Post C" }
        };
        var expectedResult = new PaginationDto<BlogPageCardDto?>
        {
            Data = expectedCards,
            TotalCount = 25
        };

        _controller.ControllerContext.RouteData.Values[CultureInjectorAttribute.RouteParameter] = "en-US";

        _blogsPageServiceMock
            .Setup(x => x.GetBlogsPaginationResultAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Get(pageId, query, CancellationToken.None);

        // Assert
        var okResult = result.Result as OkObjectResult;
        var paginationDto = okResult!.Value as PaginationDto<BlogPageCardDto?>;
        Assert.Multiple(() =>
        {
            Assert.That(paginationDto!.TotalCount, Is.EqualTo(25));
            Assert.That(paginationDto.Data, Has.Length.EqualTo(3));
            Assert.That(paginationDto.Data[0]!.Title, Is.EqualTo("Post A"));
            Assert.That(paginationDto.Data[1]!.Title, Is.EqualTo("Post B"));
            Assert.That(paginationDto.Data[2]!.Title, Is.EqualTo("Post C"));
        });
    }

    [Test]
    public async Task Get_PassesCardsQueryToService()
    {
        // Arrange
        var pageId = 1;
        var query = new CardsQuery { CurrentPage = 3, PageSize = 5 };

        _controller.ControllerContext.RouteData.Values[CultureInjectorAttribute.RouteParameter] = "en-US";

        CardsQuery? capturedQuery = null;
        _blogsPageServiceMock
            .Setup(x => x.GetBlogsPaginationResultAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()))
            .Callback<int, string, CardsQuery, CancellationToken>((_, _, q, _) => capturedQuery = q)
            .ReturnsAsync(new PaginationDto<BlogPageCardDto?> { Data = Array.Empty<BlogPageCardDto?>(), TotalCount = 0 });

        // Act
        await _controller.Get(pageId, query, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(capturedQuery, Is.Not.Null);
            Assert.That(capturedQuery!.CurrentPage, Is.EqualTo(3));
            Assert.That(capturedQuery.PageSize, Is.EqualTo(5));
        });
    }

    [Test]
    public async Task Get_ExtractsFilterQueryParametersFromRequest()
    {
        // Arrange
        var pageId = 1;
        var query = new CardsQuery();

        var queryCollection = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { "keywords", "test" },
            { "businessLine", "consulting" }
        });

        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString("?keywords=test&businessLine=consulting");
        httpContext.Request.Query = queryCollection;

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext,
            RouteData = new RouteData()
        };
        _controller.ControllerContext.RouteData.Values[CultureInjectorAttribute.RouteParameter] = "en-US";

        _blogsPageServiceMock
            .Setup(x => x.GetBlogsPaginationResultAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginationDto<BlogPageCardDto?> { Data = Array.Empty<BlogPageCardDto?>(), TotalCount = 0 });

        // Act
        await _controller.Get(pageId, query, CancellationToken.None);

        // Assert
        _blogsPageServiceMock.Verify(
            x => x.GetBlogsPaginationResultAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.Is<CardsQuery>(q => q.Filters != null),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion
}