using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Controllers.API;
using BDO.GWT.Web.Core.LiveSite.Controllers.API;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Tests.Controllers.API;

[TestFixture]
public class EditorsPickControllerTests
{
    private Mock<IEditorsPickService> _serviceMock = null!;
    private EditorsPickController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _serviceMock = new Mock<IEditorsPickService>();

        _serviceMock
            .Setup(x => x.GetAPIFilteredItemsAsync(
                It.IsAny<EditorsPickCardsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<EditorsPickCardDto>());

        _controller = new EditorsPickController(_serviceMock.Object);
        SetupHttpContext(new Dictionary<string, StringValues>());
    }

    // -------------------------------------------------------------------------
    // Response shape
    // -------------------------------------------------------------------------

    [Test]
    public async Task Items_ReturnsOkResult()
    {
        var query = new EditorsPickCardsQuery();

        var result = await _controller.Items(query, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task Items_ReturnsItemsFromService()
    {
        var expected = new[]
        {
            new EditorsPickCardDto { Title = "Card 1" },
            new EditorsPickCardDto { Title = "Card 2" }
        };

        _serviceMock
            .Setup(x => x.GetAPIFilteredItemsAsync(
                It.IsAny<EditorsPickCardsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var query = new EditorsPickCardsQuery();
        var result = await _controller.Items(query, CancellationToken.None);

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.EqualTo(expected));
    }

    [Test]
    public async Task Items_ServiceReturnsEmpty_ReturnsOkWithEmptyCollection()
    {
        _serviceMock
            .Setup(x => x.GetAPIFilteredItemsAsync(
                It.IsAny<EditorsPickCardsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<EditorsPickCardDto>());

        var query = new EditorsPickCardsQuery();
        var result = await _controller.Items(query, CancellationToken.None);

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var items = okResult!.Value as IEnumerable<EditorsPickCardDto>;
        Assert.That(items, Is.Empty);
    }

    // -------------------------------------------------------------------------
    // Filter extraction from query string
    // -------------------------------------------------------------------------

    [Test]
    public async Task Items_ExtractsFilterParamsFromQueryString()
    {
        SetupHttpContext(new Dictionary<string, StringValues>
        {
            { FilterParameterName.ContentType, "Article" }
        });

        var query = new EditorsPickCardsQuery();
        EditorsPickCardsQuery? capturedQuery = null;

        _serviceMock
            .Setup(x => x.GetAPIFilteredItemsAsync(
                It.IsAny<EditorsPickCardsQuery>(),
                It.IsAny<CancellationToken>()))
            .Callback<EditorsPickCardsQuery, CancellationToken>(
                (q, _) => capturedQuery = q)
            .ReturnsAsync(Array.Empty<EditorsPickCardDto>());

        await _controller.Items(query, CancellationToken.None);

        Assert.That(capturedQuery, Is.Not.Null);
        Assert.That(capturedQuery!.Filters, Contains.Key(FilterParameterName.ContentType));
        Assert.That(capturedQuery.Filters[FilterParameterName.ContentType], Is.EqualTo("Article"));
    }

    [Test]
    public async Task Items_MultipleFilterParams_AllExtracted()
    {
        SetupHttpContext(new Dictionary<string, StringValues>
        {
            { FilterParameterName.ContentType, "Article" },
            { FilterParameterName.BusinessLine, "Tax" }
        });

        var query = new EditorsPickCardsQuery();
        EditorsPickCardsQuery? capturedQuery = null;

        _serviceMock
            .Setup(x => x.GetAPIFilteredItemsAsync(
                It.IsAny<EditorsPickCardsQuery>(),
                It.IsAny<CancellationToken>()))
            .Callback<EditorsPickCardsQuery, CancellationToken>(
                (q, _) => capturedQuery = q)
            .ReturnsAsync(Array.Empty<EditorsPickCardDto>());

        await _controller.Items(query, CancellationToken.None);

        Assert.That(capturedQuery!.Filters, Contains.Key(FilterParameterName.ContentType));
        Assert.That(capturedQuery.Filters, Contains.Key(FilterParameterName.BusinessLine));
        Assert.That(capturedQuery.Filters[FilterParameterName.ContentType], Is.EqualTo("Article"));
        Assert.That(capturedQuery.Filters[FilterParameterName.BusinessLine], Is.EqualTo("Tax"));
    }

    [Test]
    public async Task Items_UnknownQueryParams_NotIncludedInFilters()
    {
        var guid = Guid.NewGuid();
        SetupHttpContext(new Dictionary<string, StringValues>
        {
            { "unknownParam", "someValue" },
            { "selectedGuids", guid.ToString() },
            { "featuredGuids", guid.ToString() },
            { "currentPageGuid", guid.ToString() }
        });

        var query = new EditorsPickCardsQuery();
        EditorsPickCardsQuery? capturedQuery = null;

        _serviceMock
            .Setup(x => x.GetAPIFilteredItemsAsync(
                It.IsAny<EditorsPickCardsQuery>(),
                It.IsAny<CancellationToken>()))
            .Callback<EditorsPickCardsQuery, CancellationToken>(
                (q, _) => capturedQuery = q)
            .ReturnsAsync(Array.Empty<EditorsPickCardDto>());

        await _controller.Items(query, CancellationToken.None);

        Assert.That(capturedQuery!.Filters, Does.Not.ContainKey("unknownParam"));
        Assert.That(capturedQuery.Filters, Does.Not.ContainKey("selectedGuids"));
        Assert.That(capturedQuery.Filters, Does.Not.ContainKey("featuredGuids"));
        Assert.That(capturedQuery.Filters, Does.Not.ContainKey("currentPageGuid"));
    }

    [Test]
    public async Task Items_EmptyQueryString_FiltersIsEmpty()
    {
        SetupHttpContext(new Dictionary<string, StringValues>());

        var query = new EditorsPickCardsQuery();
        EditorsPickCardsQuery? capturedQuery = null;

        _serviceMock
            .Setup(x => x.GetAPIFilteredItemsAsync(
                It.IsAny<EditorsPickCardsQuery>(),
                It.IsAny<CancellationToken>()))
            .Callback<EditorsPickCardsQuery, CancellationToken>(
                (q, _) => capturedQuery = q)
            .ReturnsAsync(Array.Empty<EditorsPickCardDto>());

        await _controller.Items(query, CancellationToken.None);

        Assert.That(capturedQuery!.Filters, Is.Empty);
    }

    // -------------------------------------------------------------------------
    // Query passthrough — SelectedGuids and FeaturedGuids
    // -------------------------------------------------------------------------

    [Test]
    public async Task Items_SelectedGuidsPassedThroughToService()
    {
        var guid1 = Guid.NewGuid();
        var guid2 = Guid.NewGuid();

        var query = new EditorsPickCardsQuery
        {
            SelectedGuids = new List<Guid> { guid1, guid2 }
        };

        EditorsPickCardsQuery? capturedQuery = null;
        _serviceMock
            .Setup(x => x.GetAPIFilteredItemsAsync(
                It.IsAny<EditorsPickCardsQuery>(),
                It.IsAny<CancellationToken>()))
            .Callback<EditorsPickCardsQuery, CancellationToken>(
                (q, _) => capturedQuery = q)
            .ReturnsAsync(Array.Empty<EditorsPickCardDto>());

        await _controller.Items(query, CancellationToken.None);

        Assert.That(capturedQuery!.SelectedGuids, Contains.Item(guid1));
        Assert.That(capturedQuery.SelectedGuids, Contains.Item(guid2));
        Assert.That(capturedQuery.SelectedGuids!.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task Items_FeaturedGuidsPassedThroughToService()
    {
        var featuredGuid = Guid.NewGuid();

        var query = new EditorsPickCardsQuery
        {
            FeaturedGuids = new List<Guid> { featuredGuid }
        };

        EditorsPickCardsQuery? capturedQuery = null;
        _serviceMock
            .Setup(x => x.GetAPIFilteredItemsAsync(
                It.IsAny<EditorsPickCardsQuery>(),
                It.IsAny<CancellationToken>()))
            .Callback<EditorsPickCardsQuery, CancellationToken>(
                (q, _) => capturedQuery = q)
            .ReturnsAsync(Array.Empty<EditorsPickCardDto>());

        await _controller.Items(query, CancellationToken.None);

        Assert.That(capturedQuery!.FeaturedGuids, Contains.Item(featuredGuid));
    }

    [Test]
    public async Task Items_NullSelectedGuids_DoesNotThrow()
    {
        var query = new EditorsPickCardsQuery
        {
            SelectedGuids = null,
            FeaturedGuids = null
        };

        Assert.DoesNotThrowAsync(() => _controller.Items(query, CancellationToken.None));
    }

    [Test]
    public async Task Items_EmptySelectedAndFeaturedGuids_ServiceCalledWithEmptyLists()
    {
        var query = new EditorsPickCardsQuery
        {
            SelectedGuids = new List<Guid>(),
            FeaturedGuids = new List<Guid>()
        };

        EditorsPickCardsQuery? capturedQuery = null;
        _serviceMock
            .Setup(x => x.GetAPIFilteredItemsAsync(
                It.IsAny<EditorsPickCardsQuery>(),
                It.IsAny<CancellationToken>()))
            .Callback<EditorsPickCardsQuery, CancellationToken>(
                (q, _) => capturedQuery = q)
            .ReturnsAsync(Array.Empty<EditorsPickCardDto>());

        await _controller.Items(query, CancellationToken.None);

        Assert.That(capturedQuery!.SelectedGuids, Is.Empty);
        Assert.That(capturedQuery.FeaturedGuids, Is.Empty);
    }

    // -------------------------------------------------------------------------
    // Full query passthrough
    // -------------------------------------------------------------------------

    [Test]
    public async Task Items_AllQueryFieldsPassedThroughToService()
    {
        var guid = Guid.NewGuid();
        var selectedGuid = Guid.NewGuid();
        var featuredGuid = Guid.NewGuid();

        var query = new EditorsPickCardsQuery
        {
            CurrentPageGuid = guid,
            PageType = nameof(Insight),
            SelectedGuids = new List<Guid> { selectedGuid },
            FeaturedGuids = new List<Guid> { featuredGuid },
            CurrentPage = 2,
            PageSize = 5
        };

        EditorsPickCardsQuery? capturedQuery = null;
        _serviceMock
            .Setup(x => x.GetAPIFilteredItemsAsync(
                It.IsAny<EditorsPickCardsQuery>(),
                It.IsAny<CancellationToken>()))
            .Callback<EditorsPickCardsQuery, CancellationToken>(
                (q, _) => capturedQuery = q)
            .ReturnsAsync(Array.Empty<EditorsPickCardDto>());

        await _controller.Items(query, CancellationToken.None);

        Assert.That(capturedQuery, Is.Not.Null);
        Assert.That(capturedQuery!.CurrentPageGuid, Is.EqualTo(guid));
        Assert.That(capturedQuery.PageType, Is.EqualTo(nameof(Insight)));
        Assert.That(capturedQuery.SelectedGuids, Contains.Item(selectedGuid));
        Assert.That(capturedQuery.FeaturedGuids, Contains.Item(featuredGuid));
        Assert.That(capturedQuery.CurrentPage, Is.EqualTo(2));
        Assert.That(capturedQuery.PageSize, Is.EqualTo(5));
    }

    // -------------------------------------------------------------------------
    // Cancellation token
    // -------------------------------------------------------------------------

    [Test]
    public async Task Items_PassesCancellationTokenToService()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        CancellationToken capturedToken = default;
        _serviceMock
            .Setup(x => x.GetAPIFilteredItemsAsync(
                It.IsAny<EditorsPickCardsQuery>(),
                It.IsAny<CancellationToken>()))
            .Callback<EditorsPickCardsQuery, CancellationToken>(
                (_, ct) => capturedToken = ct)
            .ReturnsAsync(Array.Empty<EditorsPickCardDto>());

        var query = new EditorsPickCardsQuery();
        await _controller.Items(query, token);

        Assert.That(capturedToken, Is.EqualTo(token));
    }

    [Test]
    public async Task Items_CallsServiceExactlyOnce()
    {
        var query = new EditorsPickCardsQuery();

        await _controller.Items(query, CancellationToken.None);

        _serviceMock.Verify(x =>
            x.GetAPIFilteredItemsAsync(
                It.IsAny<EditorsPickCardsQuery>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // -------------------------------------------------------------------------
    // Filter overwrites pre-existing query.Filters
    // -------------------------------------------------------------------------

    [Test]
    public async Task Items_FilterExtractedFromRequest_OverwritesQueryFilters()
    {
        SetupHttpContext(new Dictionary<string, StringValues>
        {
            { FilterParameterName.ContentType, "Article" }
        });

        var query = new EditorsPickCardsQuery
        {
            Filters = new Dictionary<string, string>
            {
                { FilterParameterName.ContentType, "OldValue" }
            }
        };

        EditorsPickCardsQuery? capturedQuery = null;
        _serviceMock
            .Setup(x => x.GetAPIFilteredItemsAsync(
                It.IsAny<EditorsPickCardsQuery>(),
                It.IsAny<CancellationToken>()))
            .Callback<EditorsPickCardsQuery, CancellationToken>(
                (q, _) => capturedQuery = q)
            .ReturnsAsync(Array.Empty<EditorsPickCardDto>());

        await _controller.Items(query, CancellationToken.None);

        Assert.That(capturedQuery!.Filters[FilterParameterName.ContentType], Is.EqualTo("Article"));
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private void SetupHttpContext(Dictionary<string, StringValues> queryParams)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Query = new QueryCollection(queryParams);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }
}