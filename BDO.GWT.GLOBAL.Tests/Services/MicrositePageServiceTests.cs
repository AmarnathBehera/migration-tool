using BDO;
using BDO.GWT.Core;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.GLOBAL.Services;
using CMS.ContentEngine;
using Kentico.Content.Web.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Services;

public class MicrositePageServiceTests
{
    private static readonly string[] ExpectedMasterCss = ["/styles/master.css"];
    private static readonly string[] ExpectedMasterJs = ["/scripts/master.js"];
    private static readonly string[] ExpectedValidCss = ["/styles/valid.css"];
    private static readonly string[] ExpectedValidJs = ["/scripts/valid.js"];

    private Mock<IContentService> _contentServiceMock = null!;
    private MicrositePageService _service = null!;

    [SetUp]
    public void Setup()
    {
        _contentServiceMock = new Mock<IContentService>();
        _service = new MicrositePageService(_contentServiceMock.Object);
    }

    [Test]
    public async Task GetMicrositePageDtoAsync_WhenMasterPageIsNull_UsesOnlyCurrentPageHtmlFields()
    {
        var currentPage = new MicrositePage
        {
            PageHeadHTML = "<meta name='page' />",
            PageTopBodyHTML = "<div>page top</div>",
            PageBottomBodyHTML = "<div>page bottom</div>"
        };

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<MicrositeMasterPage>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((MicrositeMasterPage?)null);

        var result = await _service.GetMicrositePageDtoAsync(currentPage, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.HeadHtml, Is.EqualTo(currentPage.PageHeadHTML));
            Assert.That(result.BodyTopHtml, Is.EqualTo(currentPage.PageTopBodyHTML));
            Assert.That(result.BodyBottomHtml, Is.EqualTo(currentPage.PageBottomBodyHTML));
            Assert.That(result.CustomCssLinks, Is.Empty);
            Assert.That(result.CustomJsLinks, Is.Empty);
        });
    }

    [Test]
    public async Task GetMicrositePageDtoAsync_WhenBothPagesHaveHtml_CombinesWithNewline()
    {
        var currentPage = new MicrositePage
        {
            PageHeadHTML = "<meta name='page' />",
            PageTopBodyHTML = "<div>page top</div>",
            PageBottomBodyHTML = "<div>page bottom</div>"
        };

        var masterPage = new MicrositeMasterPage
        {
            HeadHTML = "<script>master head</script>",
            BodyTopHTML = "<div>master top</div>",
            BodyBottomHTML = "<div>master bottom</div>",
            CustomCSS = Enumerable.Empty<CssAttachmentAsset>(),
            CustomJS = Enumerable.Empty<JavaScriptAttachmentAsset>()
        };

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<MicrositeMasterPage>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(masterPage);

        var result = await _service.GetMicrositePageDtoAsync(currentPage, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.HeadHtml, Is.EqualTo(masterPage.HeadHTML + '\n' + currentPage.PageHeadHTML));
            Assert.That(result.BodyTopHtml, Is.EqualTo(masterPage.BodyTopHTML + '\n' + currentPage.PageTopBodyHTML));
            Assert.That(result.BodyBottomHtml, Is.EqualTo(masterPage.BodyBottomHTML + '\n' + currentPage.PageBottomBodyHTML));
        });
    }

    [Test]
    public async Task GetMicrositePageDtoAsync_WhenOnlyMasterPageHasHtml_ReturnsMasterPageHtml()
    {
        var currentPage = new MicrositePage
        {
            PageHeadHTML = null,
            PageTopBodyHTML = null,
            PageBottomBodyHTML = null
        };

        var masterPage = new MicrositeMasterPage
        {
            HeadHTML = "<script>master head</script>",
            BodyTopHTML = "<div>master top</div>",
            BodyBottomHTML = "<div>master bottom</div>",
            CustomCSS = Enumerable.Empty<CssAttachmentAsset>(),
            CustomJS = Enumerable.Empty<JavaScriptAttachmentAsset>()
        };

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<MicrositeMasterPage>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(masterPage);

        var result = await _service.GetMicrositePageDtoAsync(currentPage, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.HeadHtml, Is.EqualTo(masterPage.HeadHTML));
            Assert.That(result.BodyTopHtml, Is.EqualTo(masterPage.BodyTopHTML));
            Assert.That(result.BodyBottomHtml, Is.EqualTo(masterPage.BodyBottomHTML));
        });
    }

    [Test]
    public async Task GetMicrositePageDtoAsync_WhenBothPagesHaveEmptyHtml_ReturnsNull()
    {
        var currentPage = new MicrositePage
        {
            PageHeadHTML = null,
            PageTopBodyHTML = null,
            PageBottomBodyHTML = null
        };

        var masterPage = new MicrositeMasterPage
        {
            HeadHTML = null,
            BodyTopHTML = null,
            BodyBottomHTML = null,
            CustomCSS = Enumerable.Empty<CssAttachmentAsset>(),
            CustomJS = Enumerable.Empty<JavaScriptAttachmentAsset>()
        };

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<MicrositeMasterPage>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(masterPage);

        var result = await _service.GetMicrositePageDtoAsync(currentPage, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.HeadHtml, Is.Null);
            Assert.That(result.BodyTopHtml, Is.Null);
            Assert.That(result.BodyBottomHtml, Is.Null);
        });
    }

    [Test]
    public async Task GetMicrositePageDtoAsync_MapsCustomCssLinksFromMasterPage()
    {
        var cssAsset = new CssAttachmentAsset
        {
            CssAttachmentAsset1 = new ContentItemAsset { Url = "/styles/master.css" }
        };

        var masterPage = new MicrositeMasterPage
        {
            CustomCSS = new[] { cssAsset },
            CustomJS = Enumerable.Empty<JavaScriptAttachmentAsset>()
        };

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<MicrositeMasterPage>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(masterPage);

        var result = await _service.GetMicrositePageDtoAsync(new MicrositePage(), CancellationToken.None);

        Assert.That(result.CustomCssLinks, Is.EquivalentTo(ExpectedMasterCss));
    }

    [Test]
    public async Task GetMicrositePageDtoAsync_MapsCustomJsLinksFromMasterPage()
    {
        var jsAsset = new JavaScriptAttachmentAsset
        {
            JavaScriptAttachmentAsset1 = new ContentItemAsset { Url = "/scripts/master.js" }
        };

        var masterPage = new MicrositeMasterPage
        {
            CustomCSS = Enumerable.Empty<CssAttachmentAsset>(),
            CustomJS = new[] { jsAsset }
        };

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<MicrositeMasterPage>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(masterPage);

        var result = await _service.GetMicrositePageDtoAsync(new MicrositePage(), CancellationToken.None);

        Assert.That(result.CustomJsLinks, Is.EquivalentTo(ExpectedMasterJs));
    }

    [Test]
    public async Task GetMicrositePageDtoAsync_ExcludesCssAssetsWithNullUrl()
    {
        var validCss = new CssAttachmentAsset
        {
            CssAttachmentAsset1 = new ContentItemAsset { Url = "/styles/valid.css" }
        };
        var nullUrlCss = new CssAttachmentAsset
        {
            CssAttachmentAsset1 = new ContentItemAsset { Url = null }
        };

        var masterPage = new MicrositeMasterPage
        {
            CustomCSS = new[] { validCss, nullUrlCss },
            CustomJS = Enumerable.Empty<JavaScriptAttachmentAsset>()
        };

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<MicrositeMasterPage>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(masterPage);

        var result = await _service.GetMicrositePageDtoAsync(new MicrositePage(), CancellationToken.None);

        Assert.That(result.CustomCssLinks, Is.EquivalentTo(ExpectedValidCss));
    }

    [Test]
    public async Task GetMicrositePageDtoAsync_ExcludesJsAssetsWithNullUrl()
    {
        var validJs = new JavaScriptAttachmentAsset
        {
            JavaScriptAttachmentAsset1 = new ContentItemAsset { Url = "/scripts/valid.js" }
        };
        var nullUrlJs = new JavaScriptAttachmentAsset
        {
            JavaScriptAttachmentAsset1 = new ContentItemAsset { Url = null }
        };

        var masterPage = new MicrositeMasterPage
        {
            CustomCSS = Enumerable.Empty<CssAttachmentAsset>(),
            CustomJS = new[] { validJs, nullUrlJs }
        };

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<MicrositeMasterPage>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(masterPage);

        var result = await _service.GetMicrositePageDtoAsync(new MicrositePage(), CancellationToken.None);

        Assert.That(result.CustomJsLinks, Is.EquivalentTo(ExpectedValidJs));
    }

    [Test]
    public async Task GetMicrositePageDtoAsync_WhenMasterPageIsNull_ReturnsEmptyCssAndJsLinks()
    {
        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<MicrositeMasterPage>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((MicrositeMasterPage?)null);

        var result = await _service.GetMicrositePageDtoAsync(new MicrositePage(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.CustomCssLinks, Is.Empty);
            Assert.That(result.CustomJsLinks, Is.Empty);
        });
    }

    [Test]
    public async Task GetMicrositePageDtoAsync_RetrievesMasterPageWithLinkedItemsMaxLevel4()
    {
        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<MicrositeMasterPage>(
                It.Is<RetrieveCurrentPageParameters>(p => p.LinkedItemsMaxLevel == 4),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((MicrositeMasterPage?)null);

        await _service.GetMicrositePageDtoAsync(new MicrositePage(), CancellationToken.None);

        _contentServiceMock.Verify(
            x => x.RetrieveCurrentPageAsync<MicrositeMasterPage>(
                It.Is<RetrieveCurrentPageParameters>(p => p.LinkedItemsMaxLevel == 4),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetMicrositePageDtoAsync_PassesCancellationToken_ToContentService()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<MicrositeMasterPage>(
                It.IsAny<RetrieveCurrentPageParameters>(), token))
            .ReturnsAsync((MicrositeMasterPage?)null)
            .Verifiable();

        await _service.GetMicrositePageDtoAsync(new MicrositePage(), token);

        _contentServiceMock.Verify();
    }
}
