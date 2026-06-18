using BDO;
using BDO.GWT.Core;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.GLOBAL.Services;
using BDO.GWT.Web.Controllers;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Controllers;

public class MicrositePageControllerTests
{
    private Mock<IContentService> _contentServiceMock = null!;
    private Mock<IMicrositePageService> _micrositePageServiceMock = null!;
    private MicrositePageController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _contentServiceMock = new Mock<IContentService>();
        _micrositePageServiceMock = new Mock<IMicrositePageService>();
        _controller = new MicrositePageController(_contentServiceMock.Object, _micrositePageServiceMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _controller.Dispose();
    }

    [Test]
    public async Task Index_WhenCurrentPageIsNull_ReturnsNotFound()
    {
        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<MicrositePage>(
                It.IsAny<RetrieveCurrentPageParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MicrositePage?)null);

        var result = await _controller.Index(CancellationToken.None);

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Index_WhenCurrentPageExists_ReturnsViewWithMicrositePageDto()
    {
        var currentPage = new MicrositePage();
        var expectedDto = new MicrositePageDto
        {
            HeadHtml = "<script>head</script>",
            BodyTopHtml = "<div>top</div>",
            BodyBottomHtml = "<div>bottom</div>",
            CustomCssLinks = new[] { "/styles/site.css" },
            CustomJsLinks = new[] { "/scripts/site.js" }
        };

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<MicrositePage>(
                It.IsAny<RetrieveCurrentPageParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentPage);

        _micrositePageServiceMock
            .Setup(x => x.GetMicrositePageDtoAsync(currentPage, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var result = await _controller.Index(CancellationToken.None);

        var viewResult = result as ViewResult;
        Assert.Multiple(() =>
        {
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult!.ViewName, Is.EqualTo("~/Views/MicrositePage/Index.cshtml"));
            var model = viewResult.Model as MicrositePageDto;
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.HeadHtml, Is.EqualTo(expectedDto.HeadHtml));
            Assert.That(model.BodyTopHtml, Is.EqualTo(expectedDto.BodyTopHtml));
            Assert.That(model.BodyBottomHtml, Is.EqualTo(expectedDto.BodyBottomHtml));
            Assert.That(model.CustomCssLinks, Is.EquivalentTo(expectedDto.CustomCssLinks));
            Assert.That(model.CustomJsLinks, Is.EquivalentTo(expectedDto.CustomJsLinks));
        });
    }

    [Test]
    public async Task Index_RetrievesCurrentPageWithLinkedItemsMaxLevel4()
    {
        var currentPage = new MicrositePage();

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<MicrositePage>(
                It.Is<RetrieveCurrentPageParameters>(p => p.LinkedItemsMaxLevel == 4),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentPage);

        _micrositePageServiceMock
            .Setup(x => x.GetMicrositePageDtoAsync(currentPage, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MicrositePageDto());

        await _controller.Index(CancellationToken.None);

        _contentServiceMock.Verify(
            x => x.RetrieveCurrentPageAsync<MicrositePage>(
                It.Is<RetrieveCurrentPageParameters>(p => p.LinkedItemsMaxLevel == 4),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Index_WhenCurrentPageIsNull_DoesNotCallMicrositePageService()
    {
        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<MicrositePage>(
                It.IsAny<RetrieveCurrentPageParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MicrositePage?)null);

        await _controller.Index(CancellationToken.None);

        _micrositePageServiceMock.Verify(
            x => x.GetMicrositePageDtoAsync(It.IsAny<MicrositePage>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task Index_PassesCancellationToken_ToContentService()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<MicrositePage>(
                It.IsAny<RetrieveCurrentPageParameters>(), token))
            .ReturnsAsync((MicrositePage?)null);

        await _controller.Index(token);

        _contentServiceMock.Verify(
            x => x.RetrieveCurrentPageAsync<MicrositePage>(
                It.IsAny<RetrieveCurrentPageParameters>(), token),
            Times.Once);
    }

    [Test]
    public async Task Index_PassesCancellationToken_ToMicrositePageService()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;
        var currentPage = new MicrositePage();

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<MicrositePage>(
                It.IsAny<RetrieveCurrentPageParameters>(), token))
            .ReturnsAsync(currentPage);

        _micrositePageServiceMock
            .Setup(x => x.GetMicrositePageDtoAsync(currentPage, token))
            .ReturnsAsync(new MicrositePageDto());

        await _controller.Index(token);

        _micrositePageServiceMock.Verify(
            x => x.GetMicrositePageDtoAsync(currentPage, token),
            Times.Once);
    }
}
