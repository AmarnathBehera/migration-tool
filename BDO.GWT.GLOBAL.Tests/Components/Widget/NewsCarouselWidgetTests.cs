using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.LiveSite.Data;
using BDO.GWT.Core.LiveSite.Services;
using BDO.GWT.GLOBAL.Widgets.NewsCarouselWidget;
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

public class NewsCarouselWidgetTests
{
    private Mock<INewsCarouselService> _newsCarouselServiceMock = null!;
    private Mock<IContentService> _contentServiceMock = null!;
    private Mock<IContentRetriever> _contentRetrieverMock = null!;
    private NewsCarouselWidget _widget = null!;
    private DefaultHttpContext _httpContext = null!;

    [SetUp]
    public void SetUp()
    {
        _newsCarouselServiceMock = new Mock<INewsCarouselService>();
        _contentServiceMock = new Mock<IContentService>();
        _contentRetrieverMock = new Mock<IContentRetriever>();

        _widget = new NewsCarouselWidget(
            _newsCarouselServiceMock.Object,
            _contentServiceMock.Object,
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

    private static ComponentViewModel<NewsCarouselWidgetProperties> CreateModel(
        NewsCarouselWidgetProperties properties,
        Guid pageGuid)
    {
        //var model = new ComponentViewModel<NewsCarouselWidgetProperties>();
        //typeof(ComponentViewModel<NewsCarouselWidgetProperties>)
        //    .GetProperty("Properties")?.SetValue(model, properties);

        //var pageMock = new Mock<IWebPageFieldsSource>();
        //pageMock.Setup(p => p.SystemFields.WebPageItemGUID).Returns(pageGuid);
        //typeof(ComponentViewModel<NewsCarouselWidgetProperties>)
        //    .GetProperty("Page")?.SetValue(model, pageMock.Object);
        var model = new ComponentViewModel<NewsCarouselWidgetProperties>();

        typeof(ComponentViewModel<NewsCarouselWidgetProperties>)
            .GetProperty("Properties")
            ?.SetValue(model, properties);

        var page = new RoutedWebPage
        {
            WebPageItemGUID = pageGuid
        };

        typeof(ComponentViewModel<NewsCarouselWidgetProperties>)
            .GetProperty("Page")
            ?.SetValue(model, page);

        return model;

        //return model;
    }

    private static NewsCarouselDto CreateDefaultDto() =>
        new() { FilteringTooltipEnabled = false, Items = [], TopMargin = "0px", BottomMargin = "0px" };

    private void SetupContentServiceMock(IWebPageFieldsSource? page = null)
    {
        _contentServiceMock
            .Setup(x => x.GetItemsByGUIDAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(new List<IWebPageFieldsSource> { page ?? new Mock<IWebPageFieldsSource>().Object });
    }

    [Test]
    public async Task InvokeAsync_WithNullModel_ReturnsViewWithFilteringTooltipDisabled()
    {
        var result = await _widget.InvokeAsync(null!);

        var viewResult = result as ViewViewComponentResult;
        var dto = viewResult?.ViewData?.Model as NewsCarouselDto;

        Assert.That(dto, Is.Not.Null);
        Assert.That(dto!.FilteringTooltipEnabled, Is.False);
    }

    [Test]
    public async Task InvokeAsync_WithNullModel_DoesNotCallService()
    {
        await _widget.InvokeAsync(null!);

        _newsCarouselServiceMock.Verify(
            x => x.GetNewsCarouselAsync(
                It.IsAny<IWebPageFieldsSource>(),
                It.IsAny<NewsCarouselData>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
    [Test]
    public async Task InvokeAsync_WithAllProperties_MapsDataToServiceCorrectly()
    {
        var pageGuid = Guid.NewGuid();
        var featuredRef = new ContentItemReference { Identifier = Guid.NewGuid() };
        var filters = new List<string> { "filter1", "filter2" };

        SetupContentServiceMock();

        NewsCarouselData? capturedData = null;
        _newsCarouselServiceMock
            .Setup(x => x.GetNewsCarouselAsync(
                It.IsAny<IWebPageFieldsSource>(),
                It.IsAny<NewsCarouselData>(),
                It.IsAny<CancellationToken>()))
            .Callback<IWebPageFieldsSource, NewsCarouselData, CancellationToken>((_, d, _) => capturedData = d)
            .ReturnsAsync(CreateDefaultDto());

        var properties = new NewsCarouselWidgetProperties
        {
            Title = "News Title",
            Description = "News Description",
            FilteringEnabled = true,
            Filters = filters,
            FeaturedNews = [featuredRef],
            BackgroundImageAltText = "Alt text",
            SeeAllLinkText = "See All",
            SeeAllLinkURL = "/news",
            SeeAllLinkURLTarget = "_blank",
            TopMargin = "20px",
            BottomMargin = "40px",
            WidgetID = "news-carousel-1"
        };

        await _widget.InvokeAsync(CreateModel(properties, pageGuid));

        Assert.That(capturedData, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(capturedData!.Title, Is.EqualTo("News Title"));
            Assert.That(capturedData!.Description, Is.EqualTo("News Description"));
            Assert.That(capturedData!.FilteringEnabled, Is.True);
            Assert.That(capturedData!.Filters, Is.EquivalentTo(filters));
            Assert.That(capturedData!.BackgroundImageAltText, Is.EqualTo("Alt text"));
            Assert.That(capturedData!.SeeAllLinkText, Is.EqualTo("See All"));
            Assert.That(capturedData!.SeeAllLinkUrl, Is.EqualTo("/news"));
            Assert.That(capturedData!.SeeAllLinkURLTarget, Is.EqualTo("_blank"));
            Assert.That(capturedData!.TopMargin, Is.EqualTo("20px"));
            Assert.That(capturedData!.BottomMargin, Is.EqualTo("40px"));
            Assert.That(capturedData!.WidgetID, Is.EqualTo("news-carousel-1"));
        });
    }

    [Test]
    public async Task InvokeAsync_WithNullFilters_UsesEmptyHashSet()
    {
        SetupContentServiceMock();

        NewsCarouselData? capturedData = null;
        _newsCarouselServiceMock
            .Setup(x => x.GetNewsCarouselAsync(
                It.IsAny<IWebPageFieldsSource>(),
                It.IsAny<NewsCarouselData>(),
                It.IsAny<CancellationToken>()))
            .Callback<IWebPageFieldsSource, NewsCarouselData, CancellationToken>((_, d, _) => capturedData = d)
            .ReturnsAsync(CreateDefaultDto());

        var properties = new NewsCarouselWidgetProperties
        {
            Filters = null!
        };

        await _widget.InvokeAsync(CreateModel(properties, Guid.NewGuid()));

        Assert.That(capturedData, Is.Not.Null);
        Assert.That(capturedData!.Filters, Is.Empty);
    }

    [Test]
    public async Task InvokeAsync_WithEmptyBackgroundImageAltText_MapsAltTextAsNull()
    {
        SetupContentServiceMock();

        NewsCarouselData? capturedData = null;
        _newsCarouselServiceMock
            .Setup(x => x.GetNewsCarouselAsync(
                It.IsAny<IWebPageFieldsSource>(),
                It.IsAny<NewsCarouselData>(),
                It.IsAny<CancellationToken>()))
            .Callback<IWebPageFieldsSource, NewsCarouselData, CancellationToken>((_, d, _) => capturedData = d)
            .ReturnsAsync(CreateDefaultDto());

        var properties = new NewsCarouselWidgetProperties
        {
            BackgroundImageAltText = string.Empty
        };

        await _widget.InvokeAsync(CreateModel(properties, Guid.NewGuid()));

        Assert.That(capturedData, Is.Not.Null);
        Assert.That(capturedData!.BackgroundImageAltText, Is.Null);
    }

    [Test]
    public async Task InvokeAsync_WithNonEmptyBackgroundImageAltText_MapsAltTextCorrectly()
    {
        SetupContentServiceMock();

        NewsCarouselData? capturedData = null;
        _newsCarouselServiceMock
            .Setup(x => x.GetNewsCarouselAsync(
                It.IsAny<IWebPageFieldsSource>(),
                It.IsAny<NewsCarouselData>(),
                It.IsAny<CancellationToken>()))
            .Callback<IWebPageFieldsSource, NewsCarouselData, CancellationToken>((_, d, _) => capturedData = d)
            .ReturnsAsync(CreateDefaultDto());

        var properties = new NewsCarouselWidgetProperties
        {
            BackgroundImageAltText = "Background image description"
        };

        await _widget.InvokeAsync(CreateModel(properties, Guid.NewGuid()));

        Assert.That(capturedData, Is.Not.Null);
        Assert.That(capturedData!.BackgroundImageAltText, Is.EqualTo("Background image description"));
    }

    [Test]
    public async Task InvokeAsync_CallsGetItemsByGUIDAsync_WithCorrectPageGuid()
    {
        var pageGuid = Guid.NewGuid();

        List<Guid>? capturedGuids = null;
        _contentServiceMock
            .Setup(x => x.GetItemsByGUIDAsync(It.IsAny<List<Guid>>()))
            .Callback<List<Guid>>(guids => capturedGuids = guids)
            .ReturnsAsync(new List<IWebPageFieldsSource> { new Mock<IWebPageFieldsSource>().Object });

        _newsCarouselServiceMock
            .Setup(x => x.GetNewsCarouselAsync(
                It.IsAny<IWebPageFieldsSource>(),
                It.IsAny<NewsCarouselData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateDefaultDto());

        await _widget.InvokeAsync(CreateModel(new NewsCarouselWidgetProperties(), pageGuid));

        Assert.That(capturedGuids, Is.Not.Null);
        Assert.That(capturedGuids, Contains.Item(pageGuid));
    }

    [Test]
    public async Task InvokeAsync_PassesCancellationTokenToService()
    {
        var cts = new CancellationTokenSource();
        var token = cts.Token;
        _httpContext.RequestAborted = token;

        SetupContentServiceMock();

        CancellationToken capturedToken = default;
        _newsCarouselServiceMock
            .Setup(x => x.GetNewsCarouselAsync(
                It.IsAny<IWebPageFieldsSource>(),
                It.IsAny<NewsCarouselData>(),
                It.IsAny<CancellationToken>()))
            .Callback<IWebPageFieldsSource, NewsCarouselData, CancellationToken>((_, _, t) => capturedToken = t)
            .ReturnsAsync(CreateDefaultDto());

        await _widget.InvokeAsync(CreateModel(new NewsCarouselWidgetProperties(), Guid.NewGuid()));

        Assert.That(capturedToken, Is.EqualTo(token));
    }

    [Test]
    public async Task InvokeAsync_ReturnsViewWithServiceResult()
    {
        var expectedDto = new NewsCarouselDto
        {
            Title = "Returned Title",
            FilteringTooltipEnabled = true,
            Items = [new NewsPageCardDto { Title = "Card 1" }],
            TopMargin = "40px",
            BottomMargin = "20px",
            WidgetID = "widget-id"
        };

        SetupContentServiceMock();

        _newsCarouselServiceMock
            .Setup(x => x.GetNewsCarouselAsync(
                It.IsAny<IWebPageFieldsSource>(),
                It.IsAny<NewsCarouselData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var result = await _widget.InvokeAsync(CreateModel(new NewsCarouselWidgetProperties(), Guid.NewGuid()));

        var viewResult = result as ViewViewComponentResult;
        var dto = viewResult?.ViewData?.Model as NewsCarouselDto;

        Assert.That(dto, Is.Not.Null);
        Assert.That(dto, Is.SameAs(expectedDto));
    }
}
