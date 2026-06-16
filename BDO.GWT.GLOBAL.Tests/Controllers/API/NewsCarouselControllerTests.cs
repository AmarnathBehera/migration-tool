using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.LiveSite.Filters;
using BDO.GWT.Core.LiveSite.Services;
using BDO.GWT.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Controllers.API;

public class NewsCarouselControllerTests
{
    private Mock<INewsCarouselService> _newsCarouselServiceMock = null!;
    private NewsCarouselController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _newsCarouselServiceMock = new Mock<INewsCarouselService>();
        _controller = new NewsCarouselController(_newsCarouselServiceMock.Object);

        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext,
            RouteData = new RouteData()
        };
    }

    [Test]
    public async Task Items_ReturnsOkResult_WithItems()
    {
        // Arrange
        var query = new CardsQuery();
        var expectedItems = new List<NewsPageCardDto>
        {
            new() { Title = "Article A" },
            new() { Title = "Article B" }
        };

        _controller.ControllerContext.RouteData.Values[CultureInjectorAttribute.RouteParameter] = "en-US";

        _newsCarouselServiceMock
            .Setup(x => x.GetAPIFilteredItemsAsync(
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedItems);

        // Act
        var result = await _controller.Items(query, CancellationToken.None);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result!;
        Assert.That(okResult.Value, Is.EqualTo(expectedItems));
    }

    [Test]
    public async Task Items_ReturnsOkResult_WithEmptyCollection()
    {
        // Arrange
        var query = new CardsQuery();
        var expectedItems = new List<NewsPageCardDto>();

        _newsCarouselServiceMock
            .Setup(x => x.GetAPIFilteredItemsAsync(
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedItems);

        // Act
        var result = await _controller.Items(query, CancellationToken.None);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result!;
        var items = okResult.Value as IEnumerable<NewsPageCardDto>;
        Assert.That(items, Is.Not.Null);
        Assert.That(items, Is.Empty);
    }

    [Test]
    public async Task Items_ReturnsOkResult_WithMultipleCards()
    {
        // Arrange
        var query = new CardsQuery();
        var expectedItems = new List<NewsPageCardDto>
        {
            new() { Title = "Article A" },
            new() { Title = "Article B" },
            new() { Title = "Article C" }
        };

        _newsCarouselServiceMock
            .Setup(x => x.GetAPIFilteredItemsAsync(
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedItems);

        // Act
        var result = await _controller.Items(query, CancellationToken.None);

        // Assert
        var okResult = result.Result as OkObjectResult;
        var items = (okResult!.Value as IEnumerable<NewsPageCardDto>)!.ToList();
        Assert.Multiple(() =>
        {
            Assert.That(items, Has.Count.EqualTo(3));
            Assert.That(items[0].Title, Is.EqualTo("Article A"));
            Assert.That(items[1].Title, Is.EqualTo("Article B"));
            Assert.That(items[2].Title, Is.EqualTo("Article C"));
        });
    }

    [Test]
    public async Task Items_PassesCardsQueryToService()
    {
        // Arrange
        var query = new CardsQuery
        {
            CurrentPage = 2,
            PageSize = 10,
            CurrentPageGuid = Guid.NewGuid(),
            PageType = "NewsArticle"
        };

        CardsQuery? capturedQuery = null;
        _newsCarouselServiceMock
            .Setup(x => x.GetAPIFilteredItemsAsync(
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()))
            .Callback<CardsQuery, CancellationToken>((q, _) => capturedQuery = q)
            .ReturnsAsync(new List<NewsPageCardDto>());

        // Act
        await _controller.Items(query, CancellationToken.None);

        // Assert
        Assert.That(capturedQuery, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(capturedQuery!.CurrentPage, Is.EqualTo(2));
            Assert.That(capturedQuery.PageSize, Is.EqualTo(10));
            Assert.That(capturedQuery.CurrentPageGuid, Is.EqualTo(query.CurrentPageGuid));
            Assert.That(capturedQuery.PageType, Is.EqualTo("NewsArticle"));
        });
    }

    [Test]
    public async Task Items_PassesCancellationTokenToService()
    {
        // Arrange
        var query = new CardsQuery();
        var cts = new CancellationTokenSource();
        var token = cts.Token;

        CancellationToken capturedToken = default;
        _newsCarouselServiceMock
            .Setup(x => x.GetAPIFilteredItemsAsync(
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()))
            .Callback<CardsQuery, CancellationToken>((_, t) => capturedToken = t)
            .ReturnsAsync(new List<NewsPageCardDto>());

        // Act
        await _controller.Items(query, token);

        // Assert
        Assert.That(capturedToken, Is.EqualTo(token));
        _newsCarouselServiceMock.Verify(
            x => x.GetAPIFilteredItemsAsync(
                It.IsAny<CardsQuery>(),
                token),
            Times.Once);
    }

    [Test]
    public async Task Items_ExtractsKnownFilterQueryParameters_IntoCardsQueryFilters()
    {
        // Arrange
        var query = new CardsQuery();

        var queryCollection = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            [FilterParameterName.BusinessLine] = "consulting",
            [FilterParameterName.BusinessIssue] = "advisory"
        });

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Query = queryCollection;

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext,
            RouteData = new RouteData()
        };

        CardsQuery? capturedQuery = null;
        _newsCarouselServiceMock
            .Setup(x => x.GetAPIFilteredItemsAsync(
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()))
            .Callback<CardsQuery, CancellationToken>((q, _) => capturedQuery = q)
            .ReturnsAsync(new List<NewsPageCardDto>());

        // Act
        await _controller.Items(query, CancellationToken.None);

        // Assert
        Assert.That(capturedQuery, Is.Not.Null);
        Assert.That(capturedQuery!.Filters, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(capturedQuery.Filters.ContainsKey(FilterParameterName.BusinessLine), Is.True);
            Assert.That(capturedQuery.Filters[FilterParameterName.BusinessLine], Is.EqualTo("consulting"));
            Assert.That(capturedQuery.Filters.ContainsKey(FilterParameterName.BusinessIssue), Is.True);
            Assert.That(capturedQuery.Filters[FilterParameterName.BusinessIssue], Is.EqualTo("advisory"));
        });
    }

    [Test]
    public async Task Items_ExtractsOnlyKnownFilterParameters_IgnoresUnknownOnes()
    {
        // Arrange
        var query = new CardsQuery();

        var queryCollection = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            [FilterParameterName.BusinessIssue] = "advisory",
            ["unknownFilter"] = "ignored"
        });

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Query = queryCollection;

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext,
            RouteData = new RouteData()
        };

        CardsQuery? capturedQuery = null;
        _newsCarouselServiceMock
            .Setup(x => x.GetAPIFilteredItemsAsync(
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()))
            .Callback<CardsQuery, CancellationToken>((q, _) => capturedQuery = q)
            .ReturnsAsync(new List<NewsPageCardDto>());

        // Act
        await _controller.Items(query, CancellationToken.None);

        // Assert
        Assert.That(capturedQuery, Is.Not.Null);
        Assert.That(capturedQuery!.Filters, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(capturedQuery.Filters.ContainsKey(FilterParameterName.BusinessIssue), Is.True);
            Assert.That(capturedQuery.Filters[FilterParameterName.BusinessIssue], Is.EqualTo("advisory"));
            Assert.That(capturedQuery.Filters.ContainsKey("unknownFilter"), Is.False);
        });
    }

    [Test]
    public async Task Items_NoQueryParameters_FiltersIsEmpty()
    {
        // Arrange
        var query = new CardsQuery();

        CardsQuery? capturedQuery = null;
        _newsCarouselServiceMock
            .Setup(x => x.GetAPIFilteredItemsAsync(
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()))
            .Callback<CardsQuery, CancellationToken>((q, _) => capturedQuery = q)
            .ReturnsAsync(new List<NewsPageCardDto>());

        // Act
        await _controller.Items(query, CancellationToken.None);

        // Assert
        Assert.That(capturedQuery, Is.Not.Null);
        Assert.That(capturedQuery!.Filters, Is.Not.Null);
        Assert.That(capturedQuery.Filters, Is.Empty);
    }

    [Test]
    public async Task Items_CallsServiceExactlyOnce()
    {
        // Arrange
        var query = new CardsQuery();

        _newsCarouselServiceMock
            .Setup(x => x.GetAPIFilteredItemsAsync(
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<NewsPageCardDto>());

        // Act
        await _controller.Items(query, CancellationToken.None);

        // Assert
        _newsCarouselServiceMock.Verify(
            x => x.GetAPIFilteredItemsAsync(
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Items_PreservesOriginalQueryProperties_AfterFilterExtraction()
    {
        // Arrange
        var pageGuid = Guid.NewGuid();
        var query = new CardsQuery
        {
            CurrentPageGuid = pageGuid,
            PageType = "NewsArticle",
            CurrentPage = 1,
            PageSize = 6,
            Order = "asc"
        };

        var queryCollection = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            [FilterParameterName.BusinessLine] = "consulting"
        });

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Query = queryCollection;

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext,
            RouteData = new RouteData()
        };

        CardsQuery? capturedQuery = null;
        _newsCarouselServiceMock
            .Setup(x => x.GetAPIFilteredItemsAsync(
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()))
            .Callback<CardsQuery, CancellationToken>((q, _) => capturedQuery = q)
            .ReturnsAsync(new List<NewsPageCardDto>());

        // Act
        await _controller.Items(query, CancellationToken.None);

        // Assert
        Assert.That(capturedQuery, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(capturedQuery!.CurrentPageGuid, Is.EqualTo(pageGuid));
            Assert.That(capturedQuery.PageType, Is.EqualTo("NewsArticle"));
            Assert.That(capturedQuery.CurrentPage, Is.EqualTo(1));
            Assert.That(capturedQuery.PageSize, Is.EqualTo(6));
            Assert.That(capturedQuery.Order, Is.EqualTo("asc"));
            Assert.That(capturedQuery.Filters.ContainsKey(FilterParameterName.BusinessLine), Is.True);
        });
    }
}
