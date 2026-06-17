using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Web.Controllers;
using BDO.GWT.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Controllers;

public class MicrositeMasterPageControllerTests
{
    private Mock<IMicrositeMasterPageService> _serviceMock = null!;
    private MicrositeMasterPageController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _serviceMock = new Mock<IMicrositeMasterPageService>();
        _controller = new MicrositeMasterPageController(_serviceMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _controller.Dispose();
    }

    [Test]
    public async Task Index_ReturnsViewResult_WithDtoFromService()
    {
        var expectedDto = new MicrositeMasterPageDto
        {
            HeadHtml = "<script>head</script>",
            BodyTopHtml = "<div>top</div>",
            BodyBottomHtml = "<div>bottom</div>",
            CustomCssLinks = new[] { "/styles/custom.css" },
            CustomJsLinks = new[] { "/scripts/custom.js" }
        };

        _serviceMock
            .Setup(s => s.GetMicrositeMasterPageDtoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var result = await _controller.Index(CancellationToken.None);

        var viewResult = result as ViewResult;
        Assert.Multiple(() =>
        {
            Assert.That(viewResult, Is.Not.Null);
            var model = viewResult!.Model as MicrositeMasterPageDto;
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.HeadHtml, Is.EqualTo(expectedDto.HeadHtml));
            Assert.That(model.BodyTopHtml, Is.EqualTo(expectedDto.BodyTopHtml));
            Assert.That(model.BodyBottomHtml, Is.EqualTo(expectedDto.BodyBottomHtml));
            Assert.That(model.CustomCssLinks, Is.EquivalentTo(expectedDto.CustomCssLinks));
            Assert.That(model.CustomJsLinks, Is.EquivalentTo(expectedDto.CustomJsLinks));
        });
    }

    [Test]
    public async Task Index_WhenServiceReturnsEmptyDto_ReturnsViewWithEmptyDto()
    {
        var emptyDto = new MicrositeMasterPageDto();

        _serviceMock
            .Setup(s => s.GetMicrositeMasterPageDtoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyDto);

        var result = await _controller.Index(CancellationToken.None);

        var viewResult = result as ViewResult;
        Assert.Multiple(() =>
        {
            Assert.That(viewResult, Is.Not.Null);
            var model = viewResult!.Model as MicrositeMasterPageDto;
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.HeadHtml, Is.Null);
            Assert.That(model.BodyTopHtml, Is.Null);
            Assert.That(model.BodyBottomHtml, Is.Null);
            Assert.That(model.CustomCssLinks, Is.Empty);
            Assert.That(model.CustomJsLinks, Is.Empty);
        });
    }

    [Test]
    public async Task Index_CallsServiceExactlyOnce()
    {
        _serviceMock
            .Setup(s => s.GetMicrositeMasterPageDtoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MicrositeMasterPageDto());

        await _controller.Index(CancellationToken.None);

        _serviceMock.Verify(
            s => s.GetMicrositeMasterPageDtoAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Index_PassesCancellationToken_ToService()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _serviceMock
            .Setup(s => s.GetMicrositeMasterPageDtoAsync(token))
            .ReturnsAsync(new MicrositeMasterPageDto());

        await _controller.Index(token);

        _serviceMock.Verify(s => s.GetMicrositeMasterPageDtoAsync(token), Times.Once);
    }
}
