using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Widgets.SocialMediaButtonsWidget;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Moq;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Tests.Components.Widget;

[TestFixture]
public class SocialMediaButtonsWidgetTests
{
    private Mock<ISocialMediaButtonsService> _socialMediaButtonsServiceMock;
    private Mock<IContentService> _contentServiceMock;
    private Mock<IWebPageUrlRetriever> _webPageUrlRetrieverMock;
    private Mock<ICultureContext> _cultureContextMock;
    private SocialMediaButtonsWidget _sut;

    [SetUp]
    public void SetUp()
    {
        _socialMediaButtonsServiceMock = new Mock<ISocialMediaButtonsService>();
        _contentServiceMock = new Mock<IContentService>();
        _webPageUrlRetrieverMock = new Mock<IWebPageUrlRetriever>();
        _cultureContextMock = new Mock<ICultureContext>();

        _cultureContextMock
            .Setup(x => x.CurrentLanguageTag)
            .Returns("en-US");

        _sut = new SocialMediaButtonsWidget(
            _socialMediaButtonsServiceMock.Object,
            _contentServiceMock.Object,
            _webPageUrlRetrieverMock.Object,
            _cultureContextMock.Object);

        _sut.ViewComponentContext = new ViewComponentContext
        {
            ViewContext = new ViewContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    // -------------------------------------------------------------------------
    // Builders
    // -------------------------------------------------------------------------

    private static ComponentViewModel<SocialMediaButtonsWidgetProperties> BuildModel(
        string topMargin = "20px",
        string bottomMargin = "40px",
        string widgetId = "widget-1")
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
                TopMargin = topMargin,
                BottomMargin = bottomMargin,
                WidgetID = widgetId
            });
    }

    private static SocialMediaButtonsDto BuildDto(
        string? email = null,
        string? facebook = null,
        string? twitter = null,
        string? linkedIn = null,
        bool showCopyLink = false) =>
        new()
        {
            EmailButtonUrl = email,
            FacebookButtonUrl = facebook,
            TwitterButtonUrl = twitter,
            LinkedInUrl = linkedIn,
            ShowCopyLinkButton = showCopyLink,
        };

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private void SetupCurrentPage(IWebPageFieldsSource? page)
    {
        _contentServiceMock
            .Setup(s => s.RetrieveCurrentPageAsync<IWebPageFieldsSource>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);
    }

    private void SetupUrlRetriever(int webPageItemId, string absoluteUrl)
    {
        _webPageUrlRetrieverMock
            .Setup(r => r.Retrieve(
                It.IsAny<IWebPageFieldsSource>(),
                "en-US",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WebPageUrl("~/test", absoluteUrl));
    }

    private void SetupService(SocialMediaButtonsDto dto)
    {
        _socialMediaButtonsServiceMock
            .Setup(s => s.GetSocialMediaButtonsDtoNB(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);
    }

    private static SocialMediaButtonsWidgetDto ExtractDto(
        Microsoft.AspNetCore.Mvc.IViewComponentResult result)
    {
        var viewResult = (ViewViewComponentResult)result;
        return (SocialMediaButtonsWidgetDto)viewResult.ViewData!.Model!;
    }

    // -------------------------------------------------------------------------
    // Tests — null current page
    // -------------------------------------------------------------------------

    [Test]
    public async Task InvokeAsync_WhenCurrentPageIsNull_ReturnsViewResult()
    {
        SetupCurrentPage(null);

        var result = await _sut.InvokeAsync(BuildModel());

        Assert.That(result, Is.InstanceOf<ViewViewComponentResult>());
    }

    [Test]
    public async Task InvokeAsync_WhenCurrentPageIsNull_DoesNotCallService()
    {
        SetupCurrentPage(null);

        await _sut.InvokeAsync(BuildModel());

        _socialMediaButtonsServiceMock.Verify(
            s => s.GetSocialMediaButtonsDtoNB(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task InvokeAsync_WhenCurrentPageIsNull_DoesNotCallUrlRetriever()
    {
        SetupCurrentPage(null);

        await _sut.InvokeAsync(BuildModel());

        _webPageUrlRetrieverMock.Verify(
            r => r.Retrieve(
                It.IsAny<IWebPageFieldsSource>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task InvokeAsync_WhenCurrentPageIsNull_ReturnsEmptyWidgetDto()
    {
        SetupCurrentPage(null);

        var result = await _sut.InvokeAsync(BuildModel());
        var dto = ExtractDto(result);

        Assert.Multiple(() =>
        {
            Assert.That(dto.EmailButtonUrl, Is.Null.Or.Empty);
            Assert.That(dto.FacebookButtonUrl, Is.Null.Or.Empty);
            Assert.That(dto.TwitterButtonUrl, Is.Null.Or.Empty);
            Assert.That(dto.LinkedInUrl, Is.Null.Or.Empty);
            Assert.That(dto.ShowCopyLinkButton, Is.False.Or.Null);
        });
    }

    // -------------------------------------------------------------------------
    // Tests — with current page
    // -------------------------------------------------------------------------

    [Test]
    public async Task InvokeAsync_WhenCurrentPageExists_ReturnsViewResult()
    {
        var page = new Mock<IWebPageFieldsSource>();
        page.Setup(p => p.SystemFields).Returns(new WebPageFields { WebPageItemID = 10 });

        SetupCurrentPage(page.Object);
        SetupUrlRetriever(10, "https://example.com/page");
        SetupService(BuildDto());

        var result = await _sut.InvokeAsync(BuildModel());

        Assert.That(result, Is.InstanceOf<ViewViewComponentResult>());
    }

    [Test]
    public async Task InvokeAsync_WhenCurrentPageExists_CallsServiceOnce()
    {
        var page = new Mock<IWebPageFieldsSource>();
        page.Setup(p => p.SystemFields).Returns(new WebPageFields { WebPageItemID = 10 });

        SetupCurrentPage(page.Object);
        SetupUrlRetriever(10, "https://example.com/page");
        SetupService(BuildDto());

        await _sut.InvokeAsync(BuildModel());

        _socialMediaButtonsServiceMock.Verify(
            s => s.GetSocialMediaButtonsDtoNB(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task InvokeAsync_WhenCurrentPageExists_PassesAbsoluteUrlToService()
    {
        var page = new Mock<IWebPageFieldsSource>();
        page.Setup(p => p.SystemFields).Returns(new WebPageFields { WebPageItemID = 10 });

        SetupCurrentPage(page.Object);
        SetupUrlRetriever(10, "https://example.com/my-page");
        SetupService(BuildDto());

        await _sut.InvokeAsync(BuildModel());

        _socialMediaButtonsServiceMock.Verify(
            s => s.GetSocialMediaButtonsDtoNB(
                "https://example.com/my-page",
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task InvokeAsync_WhenCurrentPageExists_CallsUrlRetrieverWithCurrentLanguage()
    {
        var page = new Mock<IWebPageFieldsSource>();
        page.Setup(p => p.SystemFields).Returns(new WebPageFields { WebPageItemID = 10 });

        SetupCurrentPage(page.Object);
        SetupUrlRetriever(10, "https://example.com/page");
        SetupService(BuildDto());

        await _sut.InvokeAsync(BuildModel());

        _webPageUrlRetrieverMock.Verify(
            r => r.Retrieve(
                It.IsAny<IWebPageFieldsSource>(),
                "en-US",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // -------------------------------------------------------------------------
    // Tests — DTO mapping
    // -------------------------------------------------------------------------

    [Test]
    public async Task InvokeAsync_MapsEmailUrl()
    {
        var page = new Mock<IWebPageFieldsSource>();
        page.Setup(p => p.SystemFields).Returns(new WebPageFields { WebPageItemID = 10 });

        SetupCurrentPage(page.Object);
        SetupUrlRetriever(10, "https://example.com/page");
        SetupService(BuildDto(email: "mailto:?subject=test&body=url"));

        var result = await _sut.InvokeAsync(BuildModel());
        var dto = ExtractDto(result);

        Assert.That(dto.EmailButtonUrl, Is.EqualTo("mailto:?subject=test&body=url"));
    }

    [Test]
    public async Task InvokeAsync_MapsFacebookUrl()
    {
        var page = new Mock<IWebPageFieldsSource>();
        page.Setup(p => p.SystemFields).Returns(new WebPageFields { WebPageItemID = 10 });

        SetupCurrentPage(page.Object);
        SetupUrlRetriever(10, "https://example.com/page");
        SetupService(BuildDto(facebook: "https://facebook.com/share"));

        var result = await _sut.InvokeAsync(BuildModel());
        var dto = ExtractDto(result);

        Assert.That(dto.FacebookButtonUrl, Is.EqualTo("https://facebook.com/share"));
    }

    [Test]
    public async Task InvokeAsync_MapsTwitterUrl()
    {
        var page = new Mock<IWebPageFieldsSource>();
        page.Setup(p => p.SystemFields).Returns(new WebPageFields { WebPageItemID = 10 });

        SetupCurrentPage(page.Object);
        SetupUrlRetriever(10, "https://example.com/page");
        SetupService(BuildDto(twitter: "https://twitter.com/share"));

        var result = await _sut.InvokeAsync(BuildModel());
        var dto = ExtractDto(result);

        Assert.That(dto.TwitterButtonUrl, Is.EqualTo("https://twitter.com/share"));
    }

    [Test]
    public async Task InvokeAsync_MapsLinkedInUrl()
    {
        var page = new Mock<IWebPageFieldsSource>();
        page.Setup(p => p.SystemFields).Returns(new WebPageFields { WebPageItemID = 10 });

        SetupCurrentPage(page.Object);
        SetupUrlRetriever(10, "https://example.com/page");
        SetupService(BuildDto(linkedIn: "https://linkedin.com/share"));

        var result = await _sut.InvokeAsync(BuildModel());
        var dto = ExtractDto(result);

        Assert.That(dto.LinkedInUrl, Is.EqualTo("https://linkedin.com/share"));
    }

    [Test]
    public async Task InvokeAsync_MapsCopyButtonFlag()
    {
        var page = new Mock<IWebPageFieldsSource>();
        page.Setup(p => p.SystemFields).Returns(new WebPageFields { WebPageItemID = 10 });

        SetupCurrentPage(page.Object);
        SetupUrlRetriever(10, "https://example.com/page");
        SetupService(BuildDto(showCopyLink: true));

        var result = await _sut.InvokeAsync(BuildModel());
        var dto = ExtractDto(result);

        Assert.That(dto.ShowCopyLinkButton, Is.True);
    }

    [Test]
    public async Task InvokeAsync_MapsTopMargin()
    {
        var page = new Mock<IWebPageFieldsSource>();
        page.Setup(p => p.SystemFields).Returns(new WebPageFields { WebPageItemID = 10 });

        SetupCurrentPage(page.Object);
        SetupUrlRetriever(10, "https://example.com/page");
        SetupService(BuildDto());

        var result = await _sut.InvokeAsync(BuildModel(topMargin: "60px"));
        var dto = ExtractDto(result);

        Assert.That(dto.TopMargin, Is.EqualTo("60px"));
    }

    [Test]
    public async Task InvokeAsync_MapsBottomMargin()
    {
        var page = new Mock<IWebPageFieldsSource>();
        page.Setup(p => p.SystemFields).Returns(new WebPageFields { WebPageItemID = 10 });

        SetupCurrentPage(page.Object);
        SetupUrlRetriever(10, "https://example.com/page");
        SetupService(BuildDto());

        var result = await _sut.InvokeAsync(BuildModel(bottomMargin: "80px"));
        var dto = ExtractDto(result);

        Assert.That(dto.BottomMargin, Is.EqualTo("80px"));
    }

    [Test]
    public async Task InvokeAsync_MapsWidgetId()
    {
        var page = new Mock<IWebPageFieldsSource>();
        page.Setup(p => p.SystemFields).Returns(new WebPageFields { WebPageItemID = 10 });

        SetupCurrentPage(page.Object);
        SetupUrlRetriever(10, "https://example.com/page");
        SetupService(BuildDto());

        var result = await _sut.InvokeAsync(BuildModel(widgetId: "my-widget"));
        var dto = ExtractDto(result);

        Assert.That(dto.WidgetID, Is.EqualTo("my-widget"));
    }

    [Test]
    public async Task InvokeAsync_WhenServiceReturnsNullUrls_WidgetDtoReflectsNulls()
    {
        var page = new Mock<IWebPageFieldsSource>();
        page.Setup(p => p.SystemFields).Returns(new WebPageFields { WebPageItemID = 10 });

        SetupCurrentPage(page.Object);
        SetupUrlRetriever(10, "https://example.com/page");
        SetupService(BuildDto(email: null, facebook: null, twitter: null, linkedIn: null));

        var result = await _sut.InvokeAsync(BuildModel());
        var dto = ExtractDto(result);

        Assert.Multiple(() =>
        {
            Assert.That(dto.EmailButtonUrl, Is.Null);
            Assert.That(dto.FacebookButtonUrl, Is.Null);
            Assert.That(dto.TwitterButtonUrl, Is.Null);
            Assert.That(dto.LinkedInUrl, Is.Null);
        });
    }
}