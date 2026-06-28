using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Services;
using BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.EditorsPickWidget;
using CMS.ContentEngine;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Components.Widget;

public class EditorsPickWidgetTests
{
    private Mock<IEditorsPickService> _serviceMock = null!;
    private Mock<IContentRetriever> _contentRetrieverMock = null!;
    private Mock<IContentService> _contentServiceMock = null!;
    private BDO.GWT.Core.Components.Widgets.EditorsPickWidget.EditorsPickWidget _widget = null!;

    private static readonly string ExpectedViewPath =
        "~/Components/Widget/EditorsPickWidget/_EditorsPickWidget.cshtml";

    [SetUp]
    public void SetUp()
    {
        _serviceMock = new Mock<IEditorsPickService>();
        _contentRetrieverMock = new Mock<IContentRetriever>();
        _contentServiceMock = new Mock<IContentService>();

        _widget = new BDO.GWT.Core.Components.Widgets.EditorsPickWidget.EditorsPickWidget(
            _serviceMock.Object,
            _contentRetrieverMock.Object,
            _contentServiceMock.Object);

        _widget.ViewComponentContext = new ViewComponentContext
        {
            ViewContext = new ViewContext
            {
                HttpContext = new DefaultHttpContext(),
                ViewData = new ViewDataDictionary(
                    new EmptyModelMetadataProvider(),
                    new ModelStateDictionary())
            }
        };
    }

    private static ComponentViewModel<EditorsPickWidgetProperties> CreateModel(
        EditorsPickWidgetProperties properties)
    {
        var model = new ComponentViewModel<EditorsPickWidgetProperties>();
        typeof(ComponentViewModel<EditorsPickWidgetProperties>)
            .GetProperty("Properties")
            ?.SetValue(model, properties);
        return model;
    }

    private static EditorsPickDto CreateDto() => new()
    {
        FilteringTooltipEnabled = false,
        Items = Enumerable.Empty<EditorsPickCardDto>(),
        TopMargin = string.Empty,
        BottomMargin = string.Empty
    };

    // ── InvokeAsync: null page guard ─────────────────────────────────────────

    [Test]
    public async Task InvokeAsync_WhenCurrentPageIsNull_ReturnsEmptyView()
    {
        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<IWebPageFieldsSource>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IWebPageFieldsSource?)null);

        var result = await _widget.InvokeAsync(CreateModel(new EditorsPickWidgetProperties()));

        var viewResult = result as ViewViewComponentResult;

        Assert.Multiple(() =>
        {
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult!.ViewName, Is.EqualTo(ExpectedViewPath));
        });

        _serviceMock.Verify(
            x => x.GetEditorsPickAsync(
                It.IsAny<IWebPageFieldsSource>(),
                It.IsAny<EditorsPickData>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task InvokeAsync_WhenCurrentPageIsNull_ReturnsViewWithEmptyItems()
    {
        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<IWebPageFieldsSource>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IWebPageFieldsSource?)null);

        var result = await _widget.InvokeAsync(CreateModel(new EditorsPickWidgetProperties()));

        var viewResult = result as ViewViewComponentResult;
        var model = viewResult?.ViewData?.Model as EditorsPickDto;

        Assert.Multiple(() =>
        {
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.Items, Is.Empty);
            Assert.That(model.FilteringTooltipEnabled, Is.False);
        });
    }

    // ── InvokeAsync: property mapping ────────────────────────────────────────

    [Test]
    public async Task InvokeAsync_WithAllProperties_MapsPropertiesToDataCorrectly()
    {
        var currentPageMock = new Mock<IWebPageFieldsSource>();

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<IWebPageFieldsSource>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentPageMock.Object);

        EditorsPickData? capturedData = null;
        var expectedDto = CreateDto();

        _serviceMock
            .Setup(x => x.GetEditorsPickAsync(
                currentPageMock.Object,
                It.IsAny<EditorsPickData>(),
                It.IsAny<CancellationToken>()))
            .Callback<IWebPageFieldsSource, EditorsPickData, CancellationToken>(
                (_, data, _) => capturedData = data)
            .ReturnsAsync(expectedDto);

        var properties = new EditorsPickWidgetProperties
        {
            Title = "Top Picks",
            Description = "Our best content",
            FilteringEnabled = true,
            Filters = new List<string> { "contentType", "businessLine" },
            BackgroundImageAltText = "Alt text",
            SeeAllLinkText = "See all",
            SeeAllLinkURL = "/all-picks",
            SeeAllLinkURLTarget = "_blank",
            TopMargin = "20px",
            BottomMargin = "40px",
            WidgetID = "editors-pick-widget"
        };

        var result = await _widget.InvokeAsync(CreateModel(properties));
        var viewResult = result as ViewViewComponentResult;

        Assert.Multiple(() =>
        {
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult!.ViewName, Is.EqualTo(ExpectedViewPath));
            Assert.That(viewResult.ViewData?.Model, Is.SameAs(expectedDto));
            Assert.That(capturedData, Is.Not.Null);
            Assert.That(capturedData!.Title, Is.EqualTo("Top Picks"));
            Assert.That(capturedData.Description, Is.EqualTo("Our best content"));
            Assert.That(capturedData.FilteringEnabled, Is.True);
            Assert.That(capturedData.Filters, Is.EquivalentTo(new[] { "contentType", "businessLine" }));
            Assert.That(capturedData.BackgroundImageAltText, Is.EqualTo("Alt text"));
            Assert.That(capturedData.SeeAllLinkText, Is.EqualTo("See all"));
            Assert.That(capturedData.SeeAllLinkUrl, Is.EqualTo("/all-picks"));
            Assert.That(capturedData.SeeAllLinkURLTarget, Is.EqualTo("_blank"));
            Assert.That(capturedData.TopMargin, Is.EqualTo("20px"));
            Assert.That(capturedData.BottomMargin, Is.EqualTo("40px"));
            Assert.That(capturedData.WidgetID, Is.EqualTo("editors-pick-widget"));
        });
    }

    [Test]
    public async Task InvokeAsync_WithEmptyBackgroundAltText_SetsAltTextToNull()
    {
        var currentPageMock = new Mock<IWebPageFieldsSource>();

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<IWebPageFieldsSource>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentPageMock.Object);

        EditorsPickData? capturedData = null;

        _serviceMock
            .Setup(x => x.GetEditorsPickAsync(
                It.IsAny<IWebPageFieldsSource>(),
                It.IsAny<EditorsPickData>(),
                It.IsAny<CancellationToken>()))
            .Callback<IWebPageFieldsSource, EditorsPickData, CancellationToken>(
                (_, data, _) => capturedData = data)
            .ReturnsAsync(CreateDto());

        await _widget.InvokeAsync(CreateModel(new EditorsPickWidgetProperties
        {
            BackgroundImageAltText = string.Empty
        }));

        Assert.Multiple(() =>
        {
            Assert.That(capturedData, Is.Not.Null);
            Assert.That(capturedData!.BackgroundImageAltText, Is.Null);
        });
    }

    [Test]
    public async Task InvokeAsync_WithNullFilters_SetsEmptyFilterCollection()
    {
        var currentPageMock = new Mock<IWebPageFieldsSource>();

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<IWebPageFieldsSource>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentPageMock.Object);

        EditorsPickData? capturedData = null;

        _serviceMock
            .Setup(x => x.GetEditorsPickAsync(
                It.IsAny<IWebPageFieldsSource>(),
                It.IsAny<EditorsPickData>(),
                It.IsAny<CancellationToken>()))
            .Callback<IWebPageFieldsSource, EditorsPickData, CancellationToken>(
                (_, data, _) => capturedData = data)
            .ReturnsAsync(CreateDto());

        await _widget.InvokeAsync(CreateModel(new EditorsPickWidgetProperties
        {
            Filters = null
        }));

        Assert.Multiple(() =>
        {
            Assert.That(capturedData, Is.Not.Null);
            Assert.That(capturedData!.Filters, Is.Empty);
        });
    }

    [Test]
    public async Task InvokeAsync_WithNullPages_SetsEmptyEditorsPickGuids()
    {
        var currentPageMock = new Mock<IWebPageFieldsSource>();

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<IWebPageFieldsSource>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentPageMock.Object);

        EditorsPickData? capturedData = null;

        _serviceMock
            .Setup(x => x.GetEditorsPickAsync(
                It.IsAny<IWebPageFieldsSource>(),
                It.IsAny<EditorsPickData>(),
                It.IsAny<CancellationToken>()))
            .Callback<IWebPageFieldsSource, EditorsPickData, CancellationToken>(
                (_, data, _) => capturedData = data)
            .ReturnsAsync(CreateDto());

        await _widget.InvokeAsync(CreateModel(new EditorsPickWidgetProperties
        {
            Pages = null
        }));

        Assert.Multiple(() =>
        {
            Assert.That(capturedData, Is.Not.Null);
            Assert.That(capturedData!.EditorsPickGuids, Is.Empty);
        });
    }

    [Test]
    public async Task InvokeAsync_WithNullFeaturedPages_SetsEmptyFeaturedPageGuids()
    {
        var currentPageMock = new Mock<IWebPageFieldsSource>();

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<IWebPageFieldsSource>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentPageMock.Object);

        EditorsPickData? capturedData = null;

        _serviceMock
            .Setup(x => x.GetEditorsPickAsync(
                It.IsAny<IWebPageFieldsSource>(),
                It.IsAny<EditorsPickData>(),
                It.IsAny<CancellationToken>()))
            .Callback<IWebPageFieldsSource, EditorsPickData, CancellationToken>(
                (_, data, _) => capturedData = data)
            .ReturnsAsync(CreateDto());

        await _widget.InvokeAsync(CreateModel(new EditorsPickWidgetProperties
        {
            FeaturedPages = null
        }));

        Assert.Multiple(() =>
        {
            Assert.That(capturedData, Is.Not.Null);
            Assert.That(capturedData!.FeaturedPageGuids, Is.Empty);
        });
    }

    // ── InvokeAsync: return type ─────────────────────────────────────────────

    [Test]
    public async Task InvokeAsync_ReturnsViewViewComponentResult()
    {
        var currentPageMock = new Mock<IWebPageFieldsSource>();

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<IWebPageFieldsSource>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentPageMock.Object);

        _serviceMock
            .Setup(x => x.GetEditorsPickAsync(
                It.IsAny<IWebPageFieldsSource>(),
                It.IsAny<EditorsPickData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateDto());

        var result = await _widget.InvokeAsync(CreateModel(new EditorsPickWidgetProperties
        {
            Title = "Test"
        }));

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.TypeOf<ViewViewComponentResult>());
        });
    }

    [Test]
    public async Task InvokeAsync_ReturnsDtoFromService_AsViewModel()
    {
        var currentPageMock = new Mock<IWebPageFieldsSource>();
        var expectedDto = CreateDto();

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<IWebPageFieldsSource>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentPageMock.Object);

        _serviceMock
            .Setup(x => x.GetEditorsPickAsync(
                It.IsAny<IWebPageFieldsSource>(),
                It.IsAny<EditorsPickData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var result = await _widget.InvokeAsync(CreateModel(new EditorsPickWidgetProperties()));

        var viewResult = result as ViewViewComponentResult;

        Assert.That(viewResult!.ViewData?.Model, Is.SameAs(expectedDto));
    }
}
