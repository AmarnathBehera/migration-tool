using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Controllers;
using CMS.ContentEngine;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Controllers.API;

public class EditorsPickControllerTests
{
    private Mock<IEditorsPickService> _serviceMock = null!;
    private EditorsPickController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _serviceMock = new Mock<IEditorsPickService>();
        _controller = new EditorsPickController(_serviceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Test]
    public async Task Items_ReturnsOk_WithResults()
    {
        var query = new CardsQuery();
        var expected = new List<EditorsPickCardDto> { new() { Title = "Test" } };

        _serviceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<HashSet<ContentItemReference>>(),
                It.IsAny<HashSet<ContentItemReference>>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.Items(query, "", "", CancellationToken.None);

        var okResult = result.Result as OkObjectResult;
        Assert.Multiple(() =>
        {
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.SameAs(expected));
        });
    }

    [Test]
    public async Task Items_ReturnsOk_WithEmptyCollection_WhenNoResults()
    {
        var query = new CardsQuery();

        _serviceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<HashSet<ContentItemReference>>(),
                It.IsAny<HashSet<ContentItemReference>>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EditorsPickCardDto>());

        var result = await _controller.Items(query, "", "", CancellationToken.None);

        var okResult = result.Result as OkObjectResult;
        Assert.Multiple(() =>
        {
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value as ICollection<EditorsPickCardDto>, Is.Empty);
        });
    }

    [Test]
    public async Task Items_ParsesFeaturedGuids_Correctly()
    {
        var guid1 = Guid.NewGuid();
        var guid2 = Guid.NewGuid();
        var featuredGuids = $"{guid1},{guid2}";
        var query = new CardsQuery();

        _serviceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.Is<HashSet<ContentItemReference>>(s => s.Count == 2),
                It.IsAny<HashSet<ContentItemReference>>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EditorsPickCardDto>());

        var result = await _controller.Items(query, featuredGuids, "", CancellationToken.None);

        _serviceMock.Verify(x => x.GetFilteredItemsAsync(
            It.Is<HashSet<ContentItemReference>>(s => s.Count == 2),
            It.IsAny<HashSet<ContentItemReference>>(),
            It.IsAny<CardsQuery>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Items_ParsesSelectedGuids_Correctly()
    {
        var guid1 = Guid.NewGuid();
        var selectedGuids = guid1.ToString();
        var query = new CardsQuery();

        _serviceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<HashSet<ContentItemReference>>(),
                It.Is<HashSet<ContentItemReference>>(s => s.Count == 1),
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EditorsPickCardDto>());

        var result = await _controller.Items(query, "", selectedGuids, CancellationToken.None);

        _serviceMock.Verify(x => x.GetFilteredItemsAsync(
            It.IsAny<HashSet<ContentItemReference>>(),
            It.Is<HashSet<ContentItemReference>>(s => s.Count == 1),
            It.IsAny<CardsQuery>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Items_HandlesEmptyGuidStrings_AsEmptySets()
    {
        var query = new CardsQuery();

        _serviceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.Is<HashSet<ContentItemReference>>(s => s.Count == 0),
                It.Is<HashSet<ContentItemReference>>(s => s.Count == 0),
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EditorsPickCardDto>());

        var result = await _controller.Items(query, "", "", CancellationToken.None);

        _serviceMock.Verify(x => x.GetFilteredItemsAsync(
            It.Is<HashSet<ContentItemReference>>(s => s.Count == 0),
            It.Is<HashSet<ContentItemReference>>(s => s.Count == 0),
            It.IsAny<CardsQuery>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Items_IgnoresInvalidGuids_InFeaturedGuidsString()
    {
        var validGuid = Guid.NewGuid();
        var featuredGuids = $"{validGuid},not-a-guid,also-invalid";
        var query = new CardsQuery();

        _serviceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.Is<HashSet<ContentItemReference>>(s => s.Count == 1),
                It.IsAny<HashSet<ContentItemReference>>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EditorsPickCardDto>());

        var result = await _controller.Items(query, featuredGuids, "", CancellationToken.None);

        _serviceMock.Verify(x => x.GetFilteredItemsAsync(
            It.Is<HashSet<ContentItemReference>>(s => s.Count == 1),
            It.IsAny<HashSet<ContentItemReference>>(),
            It.IsAny<CardsQuery>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Items_ExtractsFilters_FromRequestQuery()
    {
        var query = new CardsQuery();
        var queryCollection = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { "contentType", "Article" },
            { "businessLine", "Tax" }
        });

        _controller.ControllerContext.HttpContext.Request.Query = queryCollection;

        _serviceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<HashSet<ContentItemReference>>(),
                It.IsAny<HashSet<ContentItemReference>>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EditorsPickCardDto>());

        await _controller.Items(query, "", "", CancellationToken.None);

        Assert.That(query.Filters, Is.Not.Null);
    }

    [Test]
    public async Task Items_HandlesNullGuidStrings_AsEmptySets()
    {
        var query = new CardsQuery();

        _serviceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.Is<HashSet<ContentItemReference>>(s => s.Count == 0),
                It.Is<HashSet<ContentItemReference>>(s => s.Count == 0),
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EditorsPickCardDto>());

        var result = await _controller.Items(query, null!, null!, CancellationToken.None);

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
    }

    [Test]
    public async Task Items_HandlesWhitespaceGuids_AsEmptySets()
    {
        var query = new CardsQuery();

        _serviceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.Is<HashSet<ContentItemReference>>(s => s.Count == 0),
                It.Is<HashSet<ContentItemReference>>(s => s.Count == 0),
                It.IsAny<CardsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EditorsPickCardDto>());

        var result = await _controller.Items(query, "   ", "   ", CancellationToken.None);

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
    }

    [Test]
    public async Task Items_PassesCancellationToken_ToService()
    {
        var query = new CardsQuery();
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _serviceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<HashSet<ContentItemReference>>(),
                It.IsAny<HashSet<ContentItemReference>>(),
                It.IsAny<CardsQuery>(),
                token))
            .ReturnsAsync(new List<EditorsPickCardDto>());

        await _controller.Items(query, "", "", token);

        _serviceMock.Verify(x => x.GetFilteredItemsAsync(
            It.IsAny<HashSet<ContentItemReference>>(),
            It.IsAny<HashSet<ContentItemReference>>(),
            It.IsAny<CardsQuery>(),
            token), Times.Once);
    }
}
