using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Services;
using BDO.GWT.GLOBAL.Widgets;
using CMS.ContentEngine;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Components.Widget;

public class KeyContactsWidgetTests
{
    private Mock<IKeyContactsService> _keyContactsServiceMock = null!;
    private KeyContactsWidget _widget = null!;
    private DefaultHttpContext _httpContext = null!;

    [SetUp]
    public void SetUp()
    {
        _keyContactsServiceMock = new Mock<IKeyContactsService>();

        _widget = new KeyContactsWidget(_keyContactsServiceMock.Object);

        _httpContext = new DefaultHttpContext();
        _httpContext.RequestAborted = CancellationToken.None;

        _widget.ViewComponentContext = new ViewComponentContext
        {
            ViewContext = new ViewContext
            {
                HttpContext = _httpContext,
                ViewData = new ViewDataDictionary(
                    new EmptyModelMetadataProvider(),
                    new ModelStateDictionary())
            }
        };
    }

    private static ComponentViewModel<KeyContactsWidgetProperties> CreateModel(
        KeyContactsWidgetProperties properties)
    {
        var model = new ComponentViewModel<KeyContactsWidgetProperties>();

        typeof(ComponentViewModel<KeyContactsWidgetProperties>)
            .GetProperty("Properties")
            ?.SetValue(model, properties);

        return model;
    }

    private static PeopleGridDto CreateDefaultDto() =>
        new() { Items = [], TopMargin = "0px", BottomMargin = "0px" };

    [Test]
    public async Task InvokeAsync_WithAllProperties_MapsDataToServiceCorrectly()
    {
        var personRef = new ContentItemReference { Identifier = Guid.NewGuid() };
        var contributorRef = new ContentItemReference { Identifier = Guid.NewGuid() };

        KeyContactsData? capturedData = null;
        _keyContactsServiceMock
            .Setup(x => x.GetKeyContactsAsync(
                It.IsAny<KeyContactsData>(),
                It.IsAny<CancellationToken>()))
            .Callback<KeyContactsData, CancellationToken>((d, _) => capturedData = d)
            .ReturnsAsync(CreateDefaultDto());

        var properties = new KeyContactsWidgetProperties
        {
            Title = "Key Contacts",
            Description = "Description text",
            People = [personRef],
            Contributors = [contributorRef],
            IsOrderOverriden = true,
            OrderCriteria = PeopleOrderCriteria.LastName,
            IsDescendingOrder = true,
            ShowOffice = true,
            SeeAllLinkText = "See All",
            SeeAllLinkURL = "/people",
            SeeAllLinkURLTarget = "_blank",
            TopMargin = "20px",
            BottomMargin = "40px",
            WidgetID = "key-contacts-1"
        };

        await _widget.InvokeAsync(CreateModel(properties));

        Assert.That(capturedData, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(capturedData!.Title, Is.EqualTo("Key Contacts"));
            Assert.That(capturedData!.Description, Is.EqualTo("Description text"));
            Assert.That(capturedData!.OrderedPeopleNodeGuids, Is.EquivalentTo(new[] { personRef }));
            Assert.That(capturedData!.OrderedContributorsNodeGuids, Is.EquivalentTo(new[] { contributorRef }));
            Assert.That(capturedData!.IsOrderOverriden, Is.True);
            Assert.That(capturedData!.OrderCriteria, Is.EqualTo(PeopleOrderCriteria.LastName));
            Assert.That(capturedData!.IsDescendingOrder, Is.True);
            Assert.That(capturedData!.ShowOffice, Is.True);
            Assert.That(capturedData!.SeeAllLinkText, Is.EqualTo("See All"));
            Assert.That(capturedData!.SeeAllLinkUrl, Is.EqualTo("/people"));
            Assert.That(capturedData!.SeeAllLinkURLTarget, Is.EqualTo("_blank"));
            Assert.That(capturedData!.TopMargin, Is.EqualTo("20px"));
            Assert.That(capturedData!.BottomMargin, Is.EqualTo("40px"));
            Assert.That(capturedData!.WidgetID, Is.EqualTo("key-contacts-1"));
        });
    }

    [Test]
    public async Task InvokeAsync_WithNullPeople_UsesEmptyEnumerable()
    {
        KeyContactsData? capturedData = null;
        _keyContactsServiceMock
            .Setup(x => x.GetKeyContactsAsync(
                It.IsAny<KeyContactsData>(),
                It.IsAny<CancellationToken>()))
            .Callback<KeyContactsData, CancellationToken>((d, _) => capturedData = d)
            .ReturnsAsync(CreateDefaultDto());

        var properties = new KeyContactsWidgetProperties
        {
            People = null!
        };

        await _widget.InvokeAsync(CreateModel(properties));

        Assert.That(capturedData, Is.Not.Null);
        Assert.That(capturedData!.OrderedPeopleNodeGuids, Is.Empty);
    }

    [Test]
    public async Task InvokeAsync_WithNullContributors_UsesEmptyEnumerable()
    {
        KeyContactsData? capturedData = null;
        _keyContactsServiceMock
            .Setup(x => x.GetKeyContactsAsync(
                It.IsAny<KeyContactsData>(),
                It.IsAny<CancellationToken>()))
            .Callback<KeyContactsData, CancellationToken>((d, _) => capturedData = d)
            .ReturnsAsync(CreateDefaultDto());

        var properties = new KeyContactsWidgetProperties
        {
            Contributors = null!
        };

        await _widget.InvokeAsync(CreateModel(properties));

        Assert.That(capturedData, Is.Not.Null);
        Assert.That(capturedData!.OrderedContributorsNodeGuids, Is.Empty);
    }

    [Test]
    public async Task InvokeAsync_WithDefaultProperties_PassesDefaultValuesToService()
    {
        KeyContactsData? capturedData = null;
        _keyContactsServiceMock
            .Setup(x => x.GetKeyContactsAsync(
                It.IsAny<KeyContactsData>(),
                It.IsAny<CancellationToken>()))
            .Callback<KeyContactsData, CancellationToken>((d, _) => capturedData = d)
            .ReturnsAsync(CreateDefaultDto());

        await _widget.InvokeAsync(CreateModel(new KeyContactsWidgetProperties()));

        Assert.That(capturedData, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(capturedData!.IsOrderOverriden, Is.False);
            Assert.That(capturedData!.IsDescendingOrder, Is.False);
            Assert.That(capturedData!.ShowOffice, Is.False);
            Assert.That(capturedData!.SeeAllLinkURLTarget, Is.EqualTo("_self"));
            Assert.That(capturedData!.TopMargin, Is.EqualTo("0px"));
            Assert.That(capturedData!.BottomMargin, Is.EqualTo("0px"));
            Assert.That(capturedData!.OrderedPeopleNodeGuids, Is.Empty);
            Assert.That(capturedData!.OrderedContributorsNodeGuids, Is.Empty);
        });
    }

    [Test]
    public async Task InvokeAsync_PassesCancellationTokenToService()
    {
        var cts = new CancellationTokenSource();
        var token = cts.Token;
        _httpContext.RequestAborted = token;

        CancellationToken capturedToken = default;
        _keyContactsServiceMock
            .Setup(x => x.GetKeyContactsAsync(
                It.IsAny<KeyContactsData>(),
                It.IsAny<CancellationToken>()))
            .Callback<KeyContactsData, CancellationToken>((_, t) => capturedToken = t)
            .ReturnsAsync(CreateDefaultDto());

        await _widget.InvokeAsync(CreateModel(new KeyContactsWidgetProperties()));

        Assert.That(capturedToken, Is.EqualTo(token));
    }

    [Test]
    public async Task InvokeAsync_ReturnsViewWithServiceResult()
    {
        var expectedDto = new PeopleGridDto
        {
            Title = "Returned Title",
            Description = "Returned Description",
            Items = [new PersonCardDto { DisplayName = "John Doe" }],
            TopMargin = "40px",
            BottomMargin = "20px",
            WidgetID = "widget-id"
        };

        _keyContactsServiceMock
            .Setup(x => x.GetKeyContactsAsync(
                It.IsAny<KeyContactsData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var result = await _widget.InvokeAsync(CreateModel(new KeyContactsWidgetProperties()));

        var viewResult = result as ViewViewComponentResult;
        var dto = viewResult?.ViewData?.Model as PeopleGridDto;

        Assert.That(dto, Is.Not.Null);
        Assert.That(dto, Is.SameAs(expectedDto));
    }

    [Test]
    public async Task InvokeAsync_ReturnsCorrectViewPath()
    {
        _keyContactsServiceMock
            .Setup(x => x.GetKeyContactsAsync(
                It.IsAny<KeyContactsData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateDefaultDto());

        var result = await _widget.InvokeAsync(CreateModel(new KeyContactsWidgetProperties()));

        var viewResult = result as ViewViewComponentResult;

        Assert.That(viewResult, Is.Not.Null);
        Assert.That(viewResult!.ViewName, Is.EqualTo("~/Components/Widget/KeyContactsWidget/_KeyContactsWidget.cshtml"));
    }

    [Test]
    public async Task InvokeAsync_CallsServiceExactlyOnce()
    {
        _keyContactsServiceMock
            .Setup(x => x.GetKeyContactsAsync(
                It.IsAny<KeyContactsData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateDefaultDto());

        await _widget.InvokeAsync(CreateModel(new KeyContactsWidgetProperties()));

        _keyContactsServiceMock.Verify(
            x => x.GetKeyContactsAsync(
                It.IsAny<KeyContactsData>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
