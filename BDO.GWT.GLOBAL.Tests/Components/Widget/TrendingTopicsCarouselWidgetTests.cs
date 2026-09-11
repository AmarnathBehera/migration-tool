using BDO.GWT.Core.Components.Widget.TrendingTopicsCarouselWidget;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.TrendingTopicsCarouselWidget;
using CMS.ContentEngine;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Moq;
using System.Reflection;

namespace BDO.GWT.GLOBAL.Tests.Components.Widget;

[TestFixture]
public class TrendingTopicsCarouselWidgetTests
{
    private Mock<ITrendingTopicsCarouselService> _serviceMock = null!;
    private TrendingTopicsCarouselWidget _sut = null!;
    private DefaultHttpContext _httpContext = null!;

    [SetUp]
    public void SetUp()
    {
        _serviceMock = new Mock<ITrendingTopicsCarouselService>();
        _sut = new TrendingTopicsCarouselWidget(_serviceMock.Object);

        _httpContext = new DefaultHttpContext();
        _httpContext.RequestAborted = CancellationToken.None;

        _sut.ViewComponentContext = new ViewComponentContext
        {
            ViewContext = new Microsoft.AspNetCore.Mvc.Rendering.ViewContext
            {
                HttpContext = _httpContext
            }
        };

        _serviceMock
            .Setup(x => x.GetTrendingTopicsCarouselAsync(It.IsAny<TrendingTopicsCarouselData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TrendingTopicsCarouselDto { Items = [] });
    }

    private static ComponentViewModel<TrendingTopicsCarouselWidgetProperties> CreateModel(TrendingTopicsCarouselWidgetProperties properties)
    {
        var model = new ComponentViewModel<TrendingTopicsCarouselWidgetProperties>();

        typeof(ComponentViewModel<TrendingTopicsCarouselWidgetProperties>)
            .GetProperty("Properties", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(model, properties);

        return model;
    }

    private static TrendingTopicsCarouselWidgetProperties CreateProperties(string pageType, IEnumerable<ContentItemReference>? pages = null)
    {
        var props = new TrendingTopicsCarouselWidgetProperties
        {
            Title = "Title",
            Description = "Description",
            TrendingTopicsPageType = pageType,
            LearnMoreButtonURLTarget = "_blank",
            TopMargin = "20px",
            BottomMargin = "40px",
            WidgetID = "widget-1"
        };

        pages ??= [];

        switch (pageType)
        {
            case "Industry":
                props.IndustryPages = pages;
                break;
            case "Service":
                props.ServicePages = pages;
                break;
            case "Microsite":
                props.MicrositePages = pages;
                break;
            case "Blog":
                props.Pages = pages;
                break;
            case "Specialty":
                props.SpecialtyPages = pages;
                break;
            case "Insight":
                props.InsightPages = pages;
                break;
        }

        return props;
    }

    private static ContentItemReference Ref(Guid guid) => new() { Identifier = guid };

    [Test]
    public async Task InvokeAsync_IndustryPageType_UsesIndustryPages()
    {
        var guid = Guid.NewGuid();
        TrendingTopicsCarouselData? captured = null;
        _serviceMock.Setup(x => x.GetTrendingTopicsCarouselAsync(It.IsAny<TrendingTopicsCarouselData>(), It.IsAny<CancellationToken>()))
            .Callback<TrendingTopicsCarouselData, CancellationToken>((d, _) => captured = d)
            .ReturnsAsync(new TrendingTopicsCarouselDto { Items = [] });

        await _sut.InvokeAsync(CreateModel(CreateProperties("Industry", [Ref(guid)])));

        Assert.That(captured!.TrendingTopicsPages.Select(x => x.Identifier), Is.EquivalentTo([guid]));
    }

    [Test]
    public async Task InvokeAsync_ServicePageType_UsesServicePages()
    {
        var guid = Guid.NewGuid();
        TrendingTopicsCarouselData? captured = null;
        _serviceMock.Setup(x => x.GetTrendingTopicsCarouselAsync(It.IsAny<TrendingTopicsCarouselData>(), It.IsAny<CancellationToken>()))
            .Callback<TrendingTopicsCarouselData, CancellationToken>((d, _) => captured = d)
            .ReturnsAsync(new TrendingTopicsCarouselDto { Items = [] });

        await _sut.InvokeAsync(CreateModel(CreateProperties("Service", [Ref(guid)])));

        Assert.That(captured!.TrendingTopicsPages.Select(x => x.Identifier), Is.EquivalentTo([guid]));
    }

    [Test]
    public async Task InvokeAsync_MicrositePageType_UsesMicrositePages()
    {
        var guid = Guid.NewGuid();
        TrendingTopicsCarouselData? captured = null;
        _serviceMock.Setup(x => x.GetTrendingTopicsCarouselAsync(It.IsAny<TrendingTopicsCarouselData>(), It.IsAny<CancellationToken>()))
            .Callback<TrendingTopicsCarouselData, CancellationToken>((d, _) => captured = d)
            .ReturnsAsync(new TrendingTopicsCarouselDto { Items = [] });

        await _sut.InvokeAsync(CreateModel(CreateProperties("Microsite", [Ref(guid)])));

        Assert.That(captured!.TrendingTopicsPages.Select(x => x.Identifier), Is.EquivalentTo([guid]));
    }

    [Test]
    public async Task InvokeAsync_BlogPageType_UsesPages()
    {
        var guid = Guid.NewGuid();
        TrendingTopicsCarouselData? captured = null;
        _serviceMock.Setup(x => x.GetTrendingTopicsCarouselAsync(It.IsAny<TrendingTopicsCarouselData>(), It.IsAny<CancellationToken>()))
            .Callback<TrendingTopicsCarouselData, CancellationToken>((d, _) => captured = d)
            .ReturnsAsync(new TrendingTopicsCarouselDto { Items = [] });

        await _sut.InvokeAsync(CreateModel(CreateProperties("Blog", [Ref(guid)])));

        Assert.That(captured!.TrendingTopicsPages.Select(x => x.Identifier), Is.EquivalentTo([guid]));
    }

    [Test]
    public async Task InvokeAsync_SpecialtyPageType_UsesSpecialtyPages()
    {
        var guid = Guid.NewGuid();
        TrendingTopicsCarouselData? captured = null;
        _serviceMock.Setup(x => x.GetTrendingTopicsCarouselAsync(It.IsAny<TrendingTopicsCarouselData>(), It.IsAny<CancellationToken>()))
            .Callback<TrendingTopicsCarouselData, CancellationToken>((d, _) => captured = d)
            .ReturnsAsync(new TrendingTopicsCarouselDto { Items = [] });

        await _sut.InvokeAsync(CreateModel(CreateProperties("Specialty", [Ref(guid)])));

        Assert.That(captured!.TrendingTopicsPages.Select(x => x.Identifier), Is.EquivalentTo([guid]));
    }

    [Test]
    public async Task InvokeAsync_InsightPageType_UsesInsightPages()
    {
        var guid = Guid.NewGuid();
        TrendingTopicsCarouselData? captured = null;
        _serviceMock.Setup(x => x.GetTrendingTopicsCarouselAsync(It.IsAny<TrendingTopicsCarouselData>(), It.IsAny<CancellationToken>()))
            .Callback<TrendingTopicsCarouselData, CancellationToken>((d, _) => captured = d)
            .ReturnsAsync(new TrendingTopicsCarouselDto { Items = [] });

        await _sut.InvokeAsync(CreateModel(CreateProperties("Insight", [Ref(guid)])));

        Assert.That(captured!.TrendingTopicsPages.Select(x => x.Identifier), Is.EquivalentTo([guid]));
    }

    [Test]
    public async Task InvokeAsync_InvalidPageType_UsesNoPages()
    {
        TrendingTopicsCarouselData? captured = null;
        _serviceMock.Setup(x => x.GetTrendingTopicsCarouselAsync(It.IsAny<TrendingTopicsCarouselData>(), It.IsAny<CancellationToken>()))
            .Callback<TrendingTopicsCarouselData, CancellationToken>((d, _) => captured = d)
            .ReturnsAsync(new TrendingTopicsCarouselDto { Items = [] });

        await _sut.InvokeAsync(CreateModel(CreateProperties("Unknown", [Ref(Guid.NewGuid())])));

        Assert.That(captured!.TrendingTopicsPages, Is.Empty);
    }

    [Test]
    public async Task InvokeAsync_FiltersOutEmptyGuids()
    {
        var validGuid = Guid.NewGuid();
        TrendingTopicsCarouselData? captured = null;
        _serviceMock.Setup(x => x.GetTrendingTopicsCarouselAsync(It.IsAny<TrendingTopicsCarouselData>(), It.IsAny<CancellationToken>()))
            .Callback<TrendingTopicsCarouselData, CancellationToken>((d, _) => captured = d)
            .ReturnsAsync(new TrendingTopicsCarouselDto { Items = [] });

        await _sut.InvokeAsync(CreateModel(CreateProperties("Blog", [Ref(Guid.Empty), Ref(validGuid)])));

        Assert.That(captured!.TrendingTopicsPages.Select(x => x.Identifier), Is.EquivalentTo([validGuid]));
    }

    [Test]
    public async Task InvokeAsync_PassesWidgetMetadataToService()
    {
        TrendingTopicsCarouselData? captured = null;
        _serviceMock.Setup(x => x.GetTrendingTopicsCarouselAsync(It.IsAny<TrendingTopicsCarouselData>(), It.IsAny<CancellationToken>()))
            .Callback<TrendingTopicsCarouselData, CancellationToken>((d, _) => captured = d)
            .ReturnsAsync(new TrendingTopicsCarouselDto { Items = [] });

        await _sut.InvokeAsync(CreateModel(CreateProperties("Blog")));

        Assert.Multiple(() =>
        {
            Assert.That(captured!.Title, Is.EqualTo("Title"));
            Assert.That(captured.Description, Is.EqualTo("Description"));
            Assert.That(captured.LearnMoreButtonURLTarget, Is.EqualTo("_blank"));
            Assert.That(captured.TopMargin, Is.EqualTo("20px"));
            Assert.That(captured.BottomMargin, Is.EqualTo("40px"));
            Assert.That(captured.WidgetID, Is.EqualTo("widget-1"));
        });
    }

    [Test]
    public async Task InvokeAsync_ReturnsViewResult()
    {
        var result = await _sut.InvokeAsync(CreateModel(CreateProperties("Blog")));

        Assert.That(result, Is.InstanceOf<ViewViewComponentResult>());
    }

    [Test]
    public async Task InvokeAsync_PassesRequestAbortedToken()
    {
        CancellationToken captured = default;
        _serviceMock.Setup(x => x.GetTrendingTopicsCarouselAsync(It.IsAny<TrendingTopicsCarouselData>(), It.IsAny<CancellationToken>()))
            .Callback<TrendingTopicsCarouselData, CancellationToken>((_, token) => captured = token)
            .ReturnsAsync(new TrendingTopicsCarouselDto { Items = [] });

        await _sut.InvokeAsync(CreateModel(CreateProperties("Blog")));

        Assert.That(captured, Is.EqualTo(_httpContext.RequestAborted));
    }

    [Test]
    public async Task InvokeAsync_CallsServiceExactlyOnce()
    {
        await _sut.InvokeAsync(CreateModel(CreateProperties("Blog")));

        _serviceMock.Verify(x => x.GetTrendingTopicsCarouselAsync(It.IsAny<TrendingTopicsCarouselData>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
