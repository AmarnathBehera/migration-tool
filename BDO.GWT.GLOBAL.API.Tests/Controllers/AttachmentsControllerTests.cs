using BDO.GWT.GLOBAL.API.Controllers;
using BDO.GWT.GLOBAL.API.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.API.Tests.Controllers;

[TestFixture]
public class AttachmentsControllerTests
{
    private Mock<IPageAttachmentCrudService> _attachmentServiceMock = null!;
    private AttachmentsController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _attachmentServiceMock = new Mock<IPageAttachmentCrudService>();

        var httpContext = new DefaultHttpContext();
        httpContext.Items["ChannelId"] = 42;

        _controller = new AttachmentsController(_attachmentServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };
    }

    [Test]
    public async Task GetAllAttachments_ReturnsOkWithResults()
    {
        var pageGuid = Guid.NewGuid();
        var expected = new[]
        {
            new PageAttachmentOutput { GUID = Guid.NewGuid(), Name = "a.pdf" },
            new PageAttachmentOutput { GUID = Guid.NewGuid(), Name = "b.png" }
        };

        _attachmentServiceMock
            .Setup(x => x.GetAttachmentsAsync(42, pageGuid, "en-gb", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.Get(1, pageGuid, "en-gb", CancellationToken.None);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        Assert.That(okResult.Value, Is.EqualTo(expected));
    }

    [Test]
    public async Task GetAllAttachments_ForwardsChannelIdPageGuidAndCulture()
    {
        var pageGuid = Guid.NewGuid();

        _attachmentServiceMock
            .Setup(x => x.GetAttachmentsAsync(
                It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<PageAttachmentOutput>());

        await _controller.Get(999, pageGuid, "fr-fr", CancellationToken.None);

        _attachmentServiceMock.Verify(
            x => x.GetAttachmentsAsync(42, pageGuid, "fr-fr", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetAllAttachments_PassesCancellationToken()
    {
        var pageGuid = Guid.NewGuid();
        using var cts = new CancellationTokenSource();

        _attachmentServiceMock
            .Setup(x => x.GetAttachmentsAsync(
                It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<string>(), cts.Token))
            .ReturnsAsync(Array.Empty<PageAttachmentOutput>());

        await _controller.Get(1, pageGuid, "en-gb", cts.Token);

        _attachmentServiceMock.Verify(
            x => x.GetAttachmentsAsync(42, pageGuid, "en-gb", cts.Token),
            Times.Once);
    }

    [Test]
    public async Task Get_WhenAttachmentFound_ReturnsOkWithResult()
    {
        var pageGuid = Guid.NewGuid();
        var attachmentGuid = Guid.NewGuid();
        var expected = new PageAttachmentOutput { GUID = attachmentGuid, Name = "file.pdf" };

        _attachmentServiceMock
            .Setup(x => x.GetAttachmentAsync(42, pageGuid, attachmentGuid, "en-gb", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.Get(1, pageGuid, attachmentGuid, "en-gb", CancellationToken.None);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        Assert.That(okResult.Value, Is.EqualTo(expected));
    }

    [Test]
    public async Task Get_WhenAttachmentNotFound_ReturnsNotFound()
    {
        var pageGuid = Guid.NewGuid();
        var attachmentGuid = Guid.NewGuid();

        _attachmentServiceMock
            .Setup(x => x.GetAttachmentAsync(42, pageGuid, attachmentGuid, "en-gb", It.IsAny<CancellationToken>()))
            .ReturnsAsync((PageAttachmentOutput?)null);

        var result = await _controller.Get(1, pageGuid, attachmentGuid, "en-gb", CancellationToken.None);

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Get_UsesChannelIdFromHttpContextItems_NotFromRouteParameter()
    {
        var pageGuid = Guid.NewGuid();
        var attachmentGuid = Guid.NewGuid();

        _attachmentServiceMock
            .Setup(x => x.GetAttachmentAsync(
                It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<Guid>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PageAttachmentOutput());

        // memberFirmID = 999 but HttpContext.Items["ChannelId"] = 42
        await _controller.Get(999, pageGuid, attachmentGuid, "fr-fr", CancellationToken.None);

        _attachmentServiceMock.Verify(
            x => x.GetAttachmentAsync(42, pageGuid, attachmentGuid, "fr-fr", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Get_ForwardsPageGuidAttachmentGuidAndCultureToService()
    {
        var pageGuid = Guid.NewGuid();
        var attachmentGuid = Guid.NewGuid();

        _attachmentServiceMock
            .Setup(x => x.GetAttachmentAsync(
                It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<Guid>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PageAttachmentOutput());

        await _controller.Get(1, pageGuid, attachmentGuid, "de-de", CancellationToken.None);

        _attachmentServiceMock.Verify(
            x => x.GetAttachmentAsync(42, pageGuid, attachmentGuid, "de-de", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Get_PassesCancellationTokenToService()
    {
        var pageGuid = Guid.NewGuid();
        var attachmentGuid = Guid.NewGuid();
        using var cts = new CancellationTokenSource();

        _attachmentServiceMock
            .Setup(x => x.GetAttachmentAsync(
                It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<Guid>(),
                It.IsAny<string>(), cts.Token))
            .ReturnsAsync(new PageAttachmentOutput());

        await _controller.Get(1, pageGuid, attachmentGuid, "en-gb", cts.Token);

        _attachmentServiceMock.Verify(
            x => x.GetAttachmentAsync(42, pageGuid, attachmentGuid, "en-gb", cts.Token),
            Times.Once);
    }

    [Test]
    public async Task Get_CallsServiceExactlyOnce()
    {
        var pageGuid = Guid.NewGuid();
        var attachmentGuid = Guid.NewGuid();

        _attachmentServiceMock
            .Setup(x => x.GetAttachmentAsync(
                It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<Guid>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PageAttachmentOutput?)null);

        await _controller.Get(1, pageGuid, attachmentGuid, "en-gb", CancellationToken.None);

        _attachmentServiceMock.Verify(
            x => x.GetAttachmentAsync(
                It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<Guid>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
