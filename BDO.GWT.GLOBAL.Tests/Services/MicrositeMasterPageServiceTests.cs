using BDO;
using BDO.GWT.Core;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Web.Services;
using CMS.ContentEngine;
using Kentico.Content.Web.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Services;

public class MicrositeMasterPageServiceTests
{
    private static readonly string[] ExpectedValidCss = ["/styles/valid.css"];
    private static readonly string[] ExpectedValidJs = ["/scripts/valid.js"];
    private static readonly string[] ExpectedCustomCss = ["/styles/custom.css"];
    private static readonly string[] ExpectedCustomJs = ["/scripts/custom.js"];

    private Mock<IContentService> _contentServiceMock = null!;
    private MicrositeMasterPageService _service = null!;

    [SetUp]
    public void Setup()
    {
        _contentServiceMock = new Mock<IContentService>();
        _service = new MicrositeMasterPageService(_contentServiceMock.Object);
    }

    [Test]
    public async Task GetMicrositeMasterPageDtoAsync_WhenPageIsNull_ReturnsEmptyDto()
    {
        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<MicrositeMasterPage>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((MicrositeMasterPage?)null);

        var result = await _service.GetMicrositeMasterPageDtoAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.HeadHtml, Is.Null);
            Assert.That(result.BodyTopHtml, Is.Null);
            Assert.That(result.BodyBottomHtml, Is.Null);
            Assert.That(result.CustomCssLinks, Is.Empty);
            Assert.That(result.CustomJsLinks, Is.Empty);
        });
    }

    [Test]
    public async Task GetMicrositeMasterPageDtoAsync_WhenPageExists_MapsHtmlFieldsCorrectly()
    {
        var page = new MicrositeMasterPage
        {
            HeadHTML = "<meta name='test' />",
            BodyTopHTML = "<div id='top'></div>",
            BodyBottomHTML = "<div id='bottom'></div>",
            CustomCSS = Enumerable.Empty<CssAttachmentAsset>(),
            CustomJS = Enumerable.Empty<JavaScriptAttachmentAsset>()
        };

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<MicrositeMasterPage>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        var result = await _service.GetMicrositeMasterPageDtoAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.HeadHtml, Is.EqualTo(page.HeadHTML));
            Assert.That(result.BodyTopHtml, Is.EqualTo(page.BodyTopHTML));
            Assert.That(result.BodyBottomHtml, Is.EqualTo(page.BodyBottomHTML));
        });
    }

    [Test]
    public async Task GetMicrositeMasterPageDtoAsync_WhenPageHasCustomCss_MapsCssLinks()
    {
        var cssAsset = new CssAttachmentAsset
        {
            CssAttachmentAsset1 = new ContentItemAsset { Url = "/styles/custom.css" }
        };

        var page = new MicrositeMasterPage
        {
            CustomCSS = new[] { cssAsset },
            CustomJS = Enumerable.Empty<JavaScriptAttachmentAsset>()
        };

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<MicrositeMasterPage>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        var result = await _service.GetMicrositeMasterPageDtoAsync(CancellationToken.None);

        Assert.That(result.CustomCssLinks, Is.EquivalentTo(ExpectedCustomCss));
    }

    [Test]
    public async Task GetMicrositeMasterPageDtoAsync_WhenPageHasCustomJs_MapsJsLinks()
    {
        var jsAsset = new JavaScriptAttachmentAsset
        {
            JavaScriptAttachmentAsset1 = new ContentItemAsset { Url = "/scripts/custom.js" }
        };

        var page = new MicrositeMasterPage
        {
            CustomCSS = Enumerable.Empty<CssAttachmentAsset>(),
            CustomJS = new[] { jsAsset }
        };

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<MicrositeMasterPage>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        var result = await _service.GetMicrositeMasterPageDtoAsync(CancellationToken.None);

        Assert.That(result.CustomJsLinks, Is.EquivalentTo(ExpectedCustomJs));
    }

    [Test]
    public async Task GetMicrositeMasterPageDtoAsync_WhenCssAssetHasNullUrl_ExcludesFromCssLinks()
    {
        var validCss = new CssAttachmentAsset
        {
            CssAttachmentAsset1 = new ContentItemAsset { Url = "/styles/valid.css" }
        };
        var nullUrlCss = new CssAttachmentAsset
        {
            CssAttachmentAsset1 = new ContentItemAsset { Url = null }
        };

        var page = new MicrositeMasterPage
        {
            CustomCSS = new[] { validCss, nullUrlCss },
            CustomJS = Enumerable.Empty<JavaScriptAttachmentAsset>()
        };

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<MicrositeMasterPage>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        var result = await _service.GetMicrositeMasterPageDtoAsync(CancellationToken.None);

        Assert.That(result.CustomCssLinks, Is.EquivalentTo(ExpectedValidCss));
    }

    [Test]
    public async Task GetMicrositeMasterPageDtoAsync_WhenJsAssetHasNullUrl_ExcludesFromJsLinks()
    {
        var validJs = new JavaScriptAttachmentAsset
        {
            JavaScriptAttachmentAsset1 = new ContentItemAsset { Url = "/scripts/valid.js" }
        };
        var nullUrlJs = new JavaScriptAttachmentAsset
        {
            JavaScriptAttachmentAsset1 = new ContentItemAsset { Url = null }
        };

        var page = new MicrositeMasterPage
        {
            CustomCSS = Enumerable.Empty<CssAttachmentAsset>(),
            CustomJS = new[] { validJs, nullUrlJs }
        };

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<MicrositeMasterPage>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        var result = await _service.GetMicrositeMasterPageDtoAsync(CancellationToken.None);

        Assert.That(result.CustomJsLinks, Is.EquivalentTo(ExpectedValidJs));
    }

    [Test]
    public async Task GetMicrositeMasterPageDtoAsync_WhenCustomCssIsNull_ReturnEmptyCssLinks()
    {
        var page = new MicrositeMasterPage
        {
            CustomCSS = null!,
            CustomJS = Enumerable.Empty<JavaScriptAttachmentAsset>()
        };

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<MicrositeMasterPage>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        var result = await _service.GetMicrositeMasterPageDtoAsync(CancellationToken.None);

        Assert.That(result.CustomCssLinks, Is.Empty);
    }

    [Test]
    public async Task GetMicrositeMasterPageDtoAsync_WhenCustomJsIsNull_ReturnEmptyJsLinks()
    {
        var page = new MicrositeMasterPage
        {
            CustomCSS = Enumerable.Empty<CssAttachmentAsset>(),
            CustomJS = null!
        };

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<MicrositeMasterPage>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        var result = await _service.GetMicrositeMasterPageDtoAsync(CancellationToken.None);

        Assert.That(result.CustomJsLinks, Is.Empty);
    }

    [Test]
    public async Task GetMicrositeMasterPageDtoAsync_RetrievesCurrentPageWithLinkedItemsMaxLevel4()
    {
        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<MicrositeMasterPage>(
                It.Is<RetrieveCurrentPageParameters>(p => p.LinkedItemsMaxLevel == 4),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((MicrositeMasterPage?)null);

        await _service.GetMicrositeMasterPageDtoAsync(CancellationToken.None);

        _contentServiceMock.Verify(
            x => x.RetrieveCurrentPageAsync<MicrositeMasterPage>(
                It.Is<RetrieveCurrentPageParameters>(p => p.LinkedItemsMaxLevel == 4),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetMicrositeMasterPageDtoAsync_PassesCancellationToken_ToContentService()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<MicrositeMasterPage>(
                It.IsAny<RetrieveCurrentPageParameters>(), token))
            .ReturnsAsync((MicrositeMasterPage?)null)
            .Verifiable();

        await _service.GetMicrositeMasterPageDtoAsync(token);

        _contentServiceMock.Verify();
    }
}
