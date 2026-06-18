using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Widgets.SocialMediaButtonsWidget;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Moq;
using NUnit.Framework;

namespace BDO.GWT.GLOBAL.Tests.Components.Widget;

[TestFixture]
public class SocialMediaButtonsWidgetTests
{
    private Mock<ISocialMediaButtonsService> _socialMediaButtonsServiceMock = null!;
    private Mock<IWebPageDataContextRetriever> _webPageDataContextRetrieverMock = null!;
    private Mock<IWebPageUrlRetriever> _webPageUrlRetrieverMock = null!;
    private Mock<ICultureContext> _cultureContextMock = null!;

    private SocialMediaButtonsWidget _widget = null!;

    [SetUp]
    public void Setup()
    {
        _socialMediaButtonsServiceMock = new Mock<ISocialMediaButtonsService>();
        _webPageDataContextRetrieverMock = new Mock<IWebPageDataContextRetriever>();
        _webPageUrlRetrieverMock = new Mock<IWebPageUrlRetriever>();
        _cultureContextMock = new Mock<ICultureContext>();

        _cultureContextMock
            .Setup(x => x.CurrentLanguageTag)
            .Returns("en-US");

        _widget = new SocialMediaButtonsWidget(
            _socialMediaButtonsServiceMock.Object,
            _webPageDataContextRetrieverMock.Object,
            _webPageUrlRetrieverMock.Object,
            _cultureContextMock.Object);

        _widget.ViewComponentContext = new ViewComponentContext
        {
            ViewContext = new Microsoft.AspNetCore.Mvc.Rendering.ViewContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    private static ComponentViewModel<SocialMediaButtonsWidgetProperties> CreateModel()
    {
        var page = new RoutedWebPage
        {
            WebPageItemID = 10,
            LanguageName = "en-US",
            WebsiteChannelName = "TestSite",
            ContentTypeName = "Page"
        };

        return ComponentViewModel<SocialMediaButtonsWidgetProperties>.Create(
            page,
            new SocialMediaButtonsWidgetProperties
            {
                TopMargin = "20px",
                BottomMargin = "40px",
                WidgetID = "widget1"
            });
    }

    [Test]
    public async Task InvokeAsync_ReturnsViewResult()
    {
        var page = new RoutedWebPage
        {
            WebPageItemID = 10
        };

        var context = (WebPageDataContext)Activator.CreateInstance(
            typeof(WebPageDataContext),
            nonPublic: true,
            args: new object[] { page })!;

        _webPageDataContextRetrieverMock
            .Setup(x => x.TryRetrieve(out context))
            .Returns(true);

        _webPageUrlRetrieverMock
            .Setup(x => x.Retrieve(
                10,
                "en-US",
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WebPageUrl("~/test", "https://test.com"));

        _socialMediaButtonsServiceMock
            .Setup(x => x.GetSocialMediaButtonsDtoNB(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SocialMediaButtonsDto());

        var result = await _widget.InvokeAsync(CreateModel());

        Assert.That(result, Is.InstanceOf<ViewViewComponentResult>());
    }

    [Test]
    public async Task InvokeAsync_WhenNoPageContext_ReturnsEmptyDto()
    {
        WebPageDataContext context = null!;

        _webPageDataContextRetrieverMock
            .Setup(x => x.TryRetrieve(out context))
            .Returns(false);

        var result = await _widget.InvokeAsync(CreateModel());

        Assert.That(result, Is.InstanceOf<ViewViewComponentResult>());
    }

    [Test]
    public async Task InvokeAsync_CallsServiceOnce()
    {
        var page = new RoutedWebPage { WebPageItemID = 10 };

        var context = (WebPageDataContext)Activator.CreateInstance(
            typeof(WebPageDataContext),
            true,
            new object[] { page })!;

        _webPageDataContextRetrieverMock
            .Setup(x => x.TryRetrieve(out context))
            .Returns(true);

        _webPageUrlRetrieverMock
            .Setup(x => x.Retrieve(
                10,
                "en-US",
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WebPageUrl("~/test", "https://test.com"));

        _socialMediaButtonsServiceMock
            .Setup(x => x.GetSocialMediaButtonsDtoNB(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SocialMediaButtonsDto());

        await _widget.InvokeAsync(CreateModel());

        _socialMediaButtonsServiceMock.Verify(
            x => x.GetSocialMediaButtonsDtoNB(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task InvokeAsync_UsesCurrentLanguage()
    {
        var page = new RoutedWebPage { WebPageItemID = 10 };

        var context = (WebPageDataContext)Activator.CreateInstance(
            typeof(WebPageDataContext),
            true,
            new object[] { page })!;

        _webPageDataContextRetrieverMock
            .Setup(x => x.TryRetrieve(out context))
            .Returns(true);

        _webPageUrlRetrieverMock
            .Setup(x => x.Retrieve(
                10,
                "en-US",
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WebPageUrl("~/test", "https://test.com"));

        _socialMediaButtonsServiceMock
            .Setup(x => x.GetSocialMediaButtonsDtoNB(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SocialMediaButtonsDto());

        await _widget.InvokeAsync(CreateModel());

        _webPageUrlRetrieverMock.Verify(
            x => x.Retrieve(
                10,
                "en-US",
                false,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task InvokeAsync_PassesAbsoluteUrlToService()
    {
        var page = new RoutedWebPage { WebPageItemID = 10 };

        var context = (WebPageDataContext)Activator.CreateInstance(
            typeof(WebPageDataContext),
            true,
            new object[] { page })!;

        _webPageDataContextRetrieverMock
            .Setup(x => x.TryRetrieve(out context))
            .Returns(true);

        _webPageUrlRetrieverMock
            .Setup(x => x.Retrieve(
                10,
                "en-US",
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WebPageUrl("~/test", "https://abc.com"));

        _socialMediaButtonsServiceMock
            .Setup(x => x.GetSocialMediaButtonsDtoNB(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SocialMediaButtonsDto());

        await _widget.InvokeAsync(CreateModel());

        _socialMediaButtonsServiceMock.Verify(
            x => x.GetSocialMediaButtonsDtoNB(
                "https://abc.com",
                "",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task InvokeAsync_MapsEmailUrl()
    {
        var dto = new SocialMediaButtonsDto
        {
            EmailButtonUrl = "email"
        };

        var page = new RoutedWebPage { WebPageItemID = 10 };

        var context = (WebPageDataContext)Activator.CreateInstance(
            typeof(WebPageDataContext),
            true,
            new object[] { page })!;

        _webPageDataContextRetrieverMock
            .Setup(x => x.TryRetrieve(out context))
            .Returns(true);

        _webPageUrlRetrieverMock
            .Setup(x => x.Retrieve(
                It.IsAny<int>(),
                It.IsAny<string>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WebPageUrl("~/test", "https://abc.com"));

        _socialMediaButtonsServiceMock
            .Setup(x => x.GetSocialMediaButtonsDtoNB(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await _widget.InvokeAsync(CreateModel());

        var viewResult = (ViewViewComponentResult)result;
        var model = (SocialMediaButtonsWidgetDto)viewResult.ViewData!.Model!;

        Assert.That(model.EmailButtonUrl, Is.EqualTo("email"));
    }

    [Test]
    public async Task InvokeAsync_MapsFacebookUrl() { }

    [Test]
    public async Task InvokeAsync_MapsTwitterUrl() { }

    [Test]
    public async Task InvokeAsync_MapsLinkedInUrl() { }

    [Test]
    public async Task InvokeAsync_MapsCopyButtonFlag() { }

    [Test]
    public async Task InvokeAsync_MapsTopMargin() { }

    [Test]
    public async Task InvokeAsync_MapsBottomMargin() { }

    [Test]
    public async Task InvokeAsync_MapsWidgetId() { }
}