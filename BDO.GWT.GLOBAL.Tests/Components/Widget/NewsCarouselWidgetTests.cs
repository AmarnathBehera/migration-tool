using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.LiveSite.Data;
using BDO.GWT.Core.LiveSite.Services;
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

public class NewsCarouselWidgetTests
{
    private Mock<INewsCarouselService> _newsCarouselServiceMock = null!;
    private Mock<IContentRetriever> _contentRetrieverMock = null!;
    private NewsCarouselWidget _widget = null!;
    private DefaultHttpContext _httpContext = null!;

    [SetUp]
    public void SetUp()
    {
        _newsCarouselServiceMock = new Mock<INewsCarouselService>();
        _contentRetrieverMock = new Mock<IContentRetriever>();

        _widget = new NewsCarouselWidget(
            _newsCarouselServiceMock.Object,
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
        Guid pageGuid,
        string contentTypeName = "BDO.NewsArticle")
    {
        var model = new ComponentViewModel<NewsCarouselWidgetProperties>();

        typeof(ComponentViewModel<NewsCarouselWidgetProperties>)
            .GetProperty("Properties")
            ?.SetValue(model, properties);

        var page = new RoutedWebPage
        {
            WebPageItemGUID = pageGuid,
            ContentTypeName = contentTypeName
        };

        typeof(ComponentViewModel<NewsCarouselWidgetProperties>)
            .GetProperty("Page")
            ?.SetValue(model, page);

        return model;
    }

    private static NewsCarouselDto CreateDefaultDto() =>
        new() { FilteringTooltipEnabled = false, Items = [], TopMargin = "0px", BottomMargin = "0px" };

    private void SetupNewsCarouselServiceMock(Action<Guid, string, NewsCarouselData, CancellationToken>? callback = null, NewsCarouselDto? dto = null)
    {
        var setup = _newsCarouselServiceMock
            .Setup(x => x.GetNewsCarouselAsync<NewsArticle>(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<NewsCarouselData>(),
                It.IsAny<CancellationToken>()));

        if (callback is not null)
        {
            setup.Callback(callback);
        }

        setup.ReturnsAsync(dto ?? CreateDefaultDto());
    }

    [Test]
    public async Task InvokeAsync_WithAllProperties_MapsDataToServiceCorrectly()
    {
        var pageGuid = Guid.NewGuid();
        var featuredRef = new ContentItemReference { Identifier = Guid.NewGuid() };
        var filters = new List<string> { "filter1", "filter2" };

        NewsCarouselData? capturedData = null;
        SetupNewsCarouselServiceMock((_, __, d, ___) => capturedData = d);

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
        NewsCarouselData? capturedData = null;
        SetupNewsCarouselServiceMock((_, __, d, ___) => capturedData = d);

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
        NewsCarouselData? capturedData = null;
        SetupNewsCarouselServiceMock((_, __, d, ___) => capturedData = d);

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
        NewsCarouselData? capturedData = null;
        SetupNewsCarouselServiceMock((_, __, d, ___) => capturedData = d);

        var properties = new NewsCarouselWidgetProperties
        {
            BackgroundImageAltText = "Background image description"
        };

        await _widget.InvokeAsync(CreateModel(properties, Guid.NewGuid()));

        Assert.That(capturedData, Is.Not.Null);
        Assert.That(capturedData!.BackgroundImageAltText, Is.EqualTo("Background image description"));
    }

    [Test]
    public async Task InvokeAsync_CallsGetNewsCarouselAsync_WithCorrectPageGuid()
    {
        var pageGuid = Guid.NewGuid();

        Guid capturedGuid = Guid.Empty;
        SetupNewsCarouselServiceMock((guid, __, ___, ____) => capturedGuid = guid);

        await _widget.InvokeAsync(CreateModel(new NewsCarouselWidgetProperties(), pageGuid));

        Assert.That(capturedGuid, Is.EqualTo(pageGuid));
    }

    [Test]
    public async Task InvokeAsync_PassesCancellationTokenToService()
    {
        var cts = new CancellationTokenSource();
        var token = cts.Token;
        _httpContext.RequestAborted = token;

        CancellationToken capturedToken = default;
        SetupNewsCarouselServiceMock((_, __, ___, t) => capturedToken = t);

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

        SetupNewsCarouselServiceMock(dto: expectedDto);

        var result = await _widget.InvokeAsync(CreateModel(new NewsCarouselWidgetProperties(), Guid.NewGuid()));

        var viewResult = result as ViewViewComponentResult;
        var dto = viewResult?.ViewData?.Model as NewsCarouselDto;

        Assert.That(dto, Is.Not.Null);
        Assert.That(dto, Is.SameAs(expectedDto));
    }

    [Test]
    public async Task InvokeAsync_WhenPageIsNull_ReturnsDefaultDto()
    {
        var model = new ComponentViewModel<NewsCarouselWidgetProperties>();

        typeof(ComponentViewModel<NewsCarouselWidgetProperties>)
            .GetProperty("Properties")
            ?.SetValue(model, new NewsCarouselWidgetProperties());

        var result = await _widget.InvokeAsync(model);

        var viewResult = result as ViewViewComponentResult;

        var dto = viewResult?.ViewData?.Model as NewsCarouselDto;

        Assert.That(dto, Is.Not.Null);

        Assert.That(dto!.FilteringTooltipEnabled, Is.False);

        _newsCarouselServiceMock.Verify(
            x => x.GetNewsCarouselAsync<NewsArticle>(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<NewsCarouselData>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task InvokeAsync_WhenFeaturedNewsIsNull_UsesEmptyCollection()
    {
        NewsCarouselData? capturedData = null;

        SetupNewsCarouselServiceMock((_, __, d, ___) => capturedData = d);

        var properties = new NewsCarouselWidgetProperties
        {
            FeaturedNews = null!
        };

        await _widget.InvokeAsync(
            CreateModel(properties, Guid.NewGuid()));

        Assert.That(
            capturedData!.NodeGuidsOfFeaturedNews,
            Is.Empty);
    }

    [Test]
    public async Task InvokeAsync_MapsFeaturedNewsCorrectly()
    {
        NewsCarouselData? capturedData = null;

        SetupNewsCarouselServiceMock((_, __, d, ___) => capturedData = d);

        var featured = new List<ContentItemReference>{
            new ContentItemReference
           {
                 Identifier = Guid.NewGuid()
           }};

        var properties = new NewsCarouselWidgetProperties
        {
            FeaturedNews = featured
        };

        await _widget.InvokeAsync(
            CreateModel(properties, Guid.NewGuid()));

        Assert.That(
            capturedData!.NodeGuidsOfFeaturedNews,
            Is.EqualTo(featured));
    }
}
