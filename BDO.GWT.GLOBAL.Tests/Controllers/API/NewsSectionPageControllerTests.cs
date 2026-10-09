using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.LiveSite.Filters;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Controllers.API;

public class NewsSectionPageControllerTests
{
    private readonly Mock<INewsSectionPageService> _newsSectionPageServiceMock;
    private readonly NewsSectionPageController _controller;

    public NewsSectionPageControllerTests()
    {
        _newsSectionPageServiceMock = new Mock<INewsSectionPageService>();
        _controller = new NewsSectionPageController(_newsSectionPageServiceMock.Object);
    }

    [Test]
    public async Task Get_ReturnsOk_WithPaginationDto()
    {
        var pageGuid = Guid.NewGuid();
        var query = new CardsQuery();
        var expected = new PaginationDto<NewsPageCardDto>
        {
            Data = [new NewsPageCardDto { Title = "Article" }],
            TotalCount = 1
        };

        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext,
            RouteData = new Microsoft.AspNetCore.Routing.RouteData()
        };
        _controller.RouteData.Values[CultureInjectorAttribute.RouteParameter] = "en-US";

        _newsSectionPageServiceMock
            .Setup(x => x.GetNewsArticlePaginationResultAsync(
                pageGuid,
                "en-US",
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.Get(pageGuid, query, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var ok = (OkObjectResult)result.Result!;
        Assert.That(ok.Value, Is.EqualTo(expected));
    }

    [Test]
    public async Task Get_PassesPageGuidLanguageAndCancellationToken_ToService()
    {
        var pageGuid = Guid.NewGuid();
        var tokenSource = new CancellationTokenSource();
        var query = new CardsQuery();
        var expected = new PaginationDto<NewsPageCardDto> { Data = [], TotalCount = 0 };

        Guid capturedGuid = Guid.Empty;
        string? capturedLanguage = null;
        CancellationToken capturedToken = default;

        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext,
            RouteData = new Microsoft.AspNetCore.Routing.RouteData()
        };
        _controller.RouteData.Values[CultureInjectorAttribute.RouteParameter] = "fr-FR";

        _newsSectionPageServiceMock
            .Setup(x => x.GetNewsArticlePaginationResultAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()))
            .Callback<Guid, string, CardsQuery, CancellationToken>((guid, language, _, token) =>
            {
                capturedGuid = guid;
                capturedLanguage = language;
                capturedToken = token;
            })
            .ReturnsAsync(expected);

        await _controller.Get(pageGuid, query, tokenSource.Token);

        Assert.Multiple(() =>
        {
            Assert.That(capturedGuid, Is.EqualTo(pageGuid));
            Assert.That(capturedLanguage, Is.EqualTo("fr-FR"));
            Assert.That(capturedToken, Is.EqualTo(tokenSource.Token));
        });
    }

    [Test]
    public async Task Get_ExtractsOnlyKnownFilterParameters_IntoCardsQueryFilters()
    {
        var pageGuid = Guid.NewGuid();
        var query = new CardsQuery();
        var expected = new PaginationDto<NewsPageCardDto> { Data = [], TotalCount = 0 };

        CardsQuery? capturedQuery = null;

        var requestQuery = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            [FilterParameterName.BusinessIssue] = "advisory",
            ["unknownFilter"] = "ignored"
        });

        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = QueryString.Create(new Dictionary<string, string?>
        {
            [FilterParameterName.BusinessIssue] = "advisory",
            ["unknownFilter"] = "ignored"
        });
        httpContext.Request.Query = requestQuery;

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext,
            RouteData = new Microsoft.AspNetCore.Routing.RouteData()
        };
        _controller.RouteData.Values[CultureInjectorAttribute.RouteParameter] = "en-US";

        _newsSectionPageServiceMock
            .Setup(x => x.GetNewsArticlePaginationResultAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()))
            .Callback<Guid, string, CardsQuery, CancellationToken>((_, _, cardsQuery, _) => capturedQuery = cardsQuery)
            .ReturnsAsync(expected);

        await _controller.Get(pageGuid, query, CancellationToken.None);

        Assert.That(capturedQuery, Is.Not.Null);
        Assert.That(capturedQuery!.Filters, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(capturedQuery.Filters.ContainsKey(FilterParameterName.BusinessIssue), Is.True);
            Assert.That(capturedQuery.Filters[FilterParameterName.BusinessIssue], Is.EqualTo("advisory"));
            Assert.That(capturedQuery.Filters.ContainsKey("unknownFilter"), Is.False);
        });
    }
}
