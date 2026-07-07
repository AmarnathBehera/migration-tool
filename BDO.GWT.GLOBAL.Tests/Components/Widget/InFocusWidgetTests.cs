using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using BDO.GWT.GLOBAL.Widgets;
using CMS.ContentEngine;
using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Components.Widget;

public class InFocusWidgetTests
{
    private Mock<IInFocusService> _inFocusServiceMock = null!;
    private Mock<IContentRetriever> _contentRetrieverMock = null!;
    private InFocusWidget _widget = null!;
    private DefaultHttpContext _httpContext = null!;

    [SetUp]
    public void SetUp()
    {
        _inFocusServiceMock = new Mock<IInFocusService>();
        _contentRetrieverMock = new Mock<IContentRetriever>();

        _widget = new InFocusWidget(
            _inFocusServiceMock.Object,
            _contentRetrieverMock.Object);

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

    private static ComponentViewModel<InFocusWidgetProperties> CreateModel(
        InFocusWidgetProperties properties)
    {
        var model = new ComponentViewModel<InFocusWidgetProperties>();

        typeof(ComponentViewModel<InFocusWidgetProperties>)
            .GetProperty("Properties")
            ?.SetValue(model, properties);

        return model;
    }

    private static InFocusDto CreateDefaultDto() =>
        new() { Topics = [], TopMargin = "0px", BottomMargin = "0px" };

    [Test]
    public async Task InvokeAsync_WithAllProperties_MapsDataToServiceCorrectly()
    {
        var pageRef1 = new ContentItemReference { Identifier = Guid.NewGuid() };
        var pageRef2 = new ContentItemReference { Identifier = Guid.NewGuid() };

        InFocusData? capturedData = null;
        _inFocusServiceMock
            .Setup(x => x.GetInFocusAsync(
                It.IsAny<InFocusData>(),
                It.IsAny<CancellationToken>()))
            .Callback<InFocusData, CancellationToken>((d, _) => capturedData = d)
            .ReturnsAsync(CreateDefaultDto());

        var properties = new InFocusWidgetProperties
        {
            Title = "In Focus Title",
            Subtitle = "In Focus Subtitle",
            ImageURL = Enumerable.Empty<ContentItemReference>(),
            ImageAltText = "Alt text",
            Pages = [pageRef1, pageRef2],
            TopMargin = "20px",
            BottomMargin = "40px",
            WidgetID = "in-focus-1"
        };

        await _widget.InvokeAsync(CreateModel(properties));

        Assert.That(capturedData, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(capturedData!.Title, Is.EqualTo("In Focus Title"));
            Assert.That(capturedData!.Subtitle, Is.EqualTo("In Focus Subtitle"));
            Assert.That(capturedData!.ImageURL, Is.Null);
            Assert.That(capturedData!.ImageAltText, Is.EqualTo("Alt text"));
            Assert.That(capturedData!.PageGuids, Is.EquivalentTo(new[] { pageRef1.Identifier, pageRef2.Identifier }));
            Assert.That(capturedData!.TopMargin, Is.EqualTo("20px"));
            Assert.That(capturedData!.BottomMargin, Is.EqualTo("40px"));
            Assert.That(capturedData!.WidgetID, Is.EqualTo("in-focus-1"));
        });
    }

    [Test]
    public async Task InvokeAsync_WithNoImage_SetsImageURLToNull()
    {
        InFocusData? capturedData = null;
        _inFocusServiceMock
            .Setup(x => x.GetInFocusAsync(
                It.IsAny<InFocusData>(),
                It.IsAny<CancellationToken>()))
            .Callback<InFocusData, CancellationToken>((d, _) => capturedData = d)
            .ReturnsAsync(CreateDefaultDto());

        var properties = new InFocusWidgetProperties
        {
            Title = "Title",
            ImageURL = Enumerable.Empty<ContentItemReference>()
        };

        await _widget.InvokeAsync(CreateModel(properties));

        Assert.That(capturedData, Is.Not.Null);
        Assert.That(capturedData!.ImageURL, Is.Null);
    }

    [Test]
    public async Task InvokeAsync_WithNullPages_UsesEmptyEnumerable()
    {
        InFocusData? capturedData = null;
        _inFocusServiceMock
            .Setup(x => x.GetInFocusAsync(
                It.IsAny<InFocusData>(),
                It.IsAny<CancellationToken>()))
            .Callback<InFocusData, CancellationToken>((d, _) => capturedData = d)
            .ReturnsAsync(CreateDefaultDto());

        var properties = new InFocusWidgetProperties
        {
            Pages = null!
        };

        await _widget.InvokeAsync(CreateModel(properties));

        Assert.That(capturedData, Is.Not.Null);
        Assert.That(capturedData!.PageGuids, Is.Empty);
    }

    [Test]
    public async Task InvokeAsync_WithDefaultProperties_PassesDefaultValues()
    {
        InFocusData? capturedData = null;
        _inFocusServiceMock
            .Setup(x => x.GetInFocusAsync(
                It.IsAny<InFocusData>(),
                It.IsAny<CancellationToken>()))
            .Callback<InFocusData, CancellationToken>((d, _) => capturedData = d)
            .ReturnsAsync(CreateDefaultDto());

        await _widget.InvokeAsync(CreateModel(new InFocusWidgetProperties()));

        Assert.That(capturedData, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(capturedData!.TopMargin, Is.EqualTo("0px"));
            Assert.That(capturedData!.BottomMargin, Is.EqualTo("0px"));
            Assert.That(capturedData!.PageGuids, Is.Empty);
        });
    }

    [Test]
    public async Task InvokeAsync_PassesCancellationTokenToService()
    {
        var cts = new CancellationTokenSource();
        var token = cts.Token;
        _httpContext.RequestAborted = token;

        CancellationToken capturedToken = default;
        _inFocusServiceMock
            .Setup(x => x.GetInFocusAsync(
                It.IsAny<InFocusData>(),
                It.IsAny<CancellationToken>()))
            .Callback<InFocusData, CancellationToken>((_, t) => capturedToken = t)
            .ReturnsAsync(CreateDefaultDto());

        await _widget.InvokeAsync(CreateModel(new InFocusWidgetProperties()));

        Assert.That(capturedToken, Is.EqualTo(token));
    }

    [Test]
    public async Task InvokeAsync_ReturnsViewWithServiceResult()
    {
        var expectedDto = new InFocusDto
        {
            Title = "Returned Title",
            Subtitle = "Returned Subtitle",
            Topics = [new InFocusTopicDto { Title = "Topic 1", Items = [] }],
            TopMargin = "40px",
            BottomMargin = "20px",
            WidgetID = "widget-id"
        };

        _inFocusServiceMock
            .Setup(x => x.GetInFocusAsync(
                It.IsAny<InFocusData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var result = await _widget.InvokeAsync(CreateModel(new InFocusWidgetProperties()));

        var viewResult = result as ViewViewComponentResult;
        var dto = viewResult?.ViewData?.Model as InFocusDto;

        Assert.That(dto, Is.Not.Null);
        Assert.That(dto, Is.SameAs(expectedDto));
    }

    [Test]
    public async Task InvokeAsync_ReturnsCorrectViewPath()
    {
        _inFocusServiceMock
            .Setup(x => x.GetInFocusAsync(
                It.IsAny<InFocusData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateDefaultDto());

        var result = await _widget.InvokeAsync(CreateModel(new InFocusWidgetProperties()));

        var viewResult = result as ViewViewComponentResult;

        Assert.That(viewResult, Is.Not.Null);
        Assert.That(viewResult!.ViewName, Is.EqualTo("~/Components/Widget/InFocusWidget/_InFocusWidget.cshtml"));
    }

    [Test]
    public async Task InvokeAsync_CallsServiceExactlyOnce()
    {
        _inFocusServiceMock
            .Setup(x => x.GetInFocusAsync(
                It.IsAny<InFocusData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateDefaultDto());

        await _widget.InvokeAsync(CreateModel(new InFocusWidgetProperties()));

        _inFocusServiceMock.Verify(
            x => x.GetInFocusAsync(
                It.IsAny<InFocusData>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task InvokeAsync_WithNoImage_DoesNotSetImageURL()
    {
        InFocusData? capturedData = null;
        _inFocusServiceMock
            .Setup(x => x.GetInFocusAsync(
                It.IsAny<InFocusData>(),
                It.IsAny<CancellationToken>()))
            .Callback<InFocusData, CancellationToken>((d, _) => capturedData = d)
            .ReturnsAsync(CreateDefaultDto());

        var properties = new InFocusWidgetProperties
        {
            ImageURL = Enumerable.Empty<ContentItemReference>()
        };

        await _widget.InvokeAsync(CreateModel(properties));

        Assert.That(capturedData, Is.Not.Null);
        Assert.That(capturedData!.ImageURL, Is.Null);
    }
}
