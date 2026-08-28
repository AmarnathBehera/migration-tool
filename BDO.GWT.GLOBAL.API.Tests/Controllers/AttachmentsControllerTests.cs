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

    [Test]
    public async Task Post_WhenAttachmentCreated_ReturnsCreatedAtAction()
    {
        var pageGuid = Guid.NewGuid();
        var file = new FormFile(new MemoryStream([1, 2, 3]), 0, 3, "file", "sample.pdf");
        var expected = new PageAttachmentOutput { GUID = Guid.NewGuid(), Name = "sample.pdf" };

        _attachmentServiceMock
            .Setup(x => x.CreateAttachmentAsync(42, pageGuid, "en-gb", file, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var request = new AttachmentRequest { Culture = "en-gb", File = file };
        var result = await _controller.Post(1, pageGuid, request, CancellationToken.None);

        Assert.That(result, Is.InstanceOf<CreatedAtActionResult>());
        var createdResult = (CreatedAtActionResult)result;
        Assert.That(createdResult.Value, Is.EqualTo(expected));
        Assert.That(createdResult.ActionName, Is.EqualTo(nameof(AttachmentsController.Get)));
    }

    [Test]
    public async Task Post_ForwardsArgumentsToService()
    {
        var pageGuid = Guid.NewGuid();
        var file = new FormFile(new MemoryStream([9, 8, 7]), 0, 3, "file", "sample.pdf");
        using var cts = new CancellationTokenSource();

        _attachmentServiceMock
            .Setup(x => x.CreateAttachmentAsync(
                It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<IFormFile>(), cts.Token))
            .ReturnsAsync(new PageAttachmentOutput());

        var request = new AttachmentRequest { Culture = "fr-fr", File = file };
        await _controller.Post(999, pageGuid, request, cts.Token);

        _attachmentServiceMock.Verify(
            x => x.CreateAttachmentAsync(42, pageGuid, "fr-fr", file, cts.Token),
            Times.Once);
    }

    [Test]
    public async Task GetBinary_WhenAttachmentExists_ReturnsFileContentResult()
    {
        var pageGuid = Guid.NewGuid();
        var attachmentGuid = Guid.NewGuid();
        var data = new byte[] { 1, 2, 3 };

        _attachmentServiceMock
            .Setup(x => x.GetAttachmentBinaryAsync(42, pageGuid, attachmentGuid, "en-gb", It.IsAny<CancellationToken>()))
            .ReturnsAsync((data, "application/pdf", "file.pdf"));

        var result = await _controller.GetBinary(1, pageGuid, attachmentGuid, "en-gb", CancellationToken.None);

        Assert.That(result, Is.InstanceOf<FileContentResult>());
        var fileResult = (FileContentResult)result;

        Assert.Multiple(() =>
        {
            Assert.That(fileResult.FileContents, Is.EqualTo(data));
            Assert.That(fileResult.ContentType, Is.EqualTo("application/octet-stream"));
            Assert.That(fileResult.FileDownloadName, Is.EqualTo(string.Empty));
        });
    }

    [Test]
    public async Task GetBinary_WhenAttachmentNotFound_ReturnsNotFound()
    {
        var pageGuid = Guid.NewGuid();
        var attachmentGuid = Guid.NewGuid();

        _attachmentServiceMock
            .Setup(x => x.GetAttachmentBinaryAsync(42, pageGuid, attachmentGuid, "en-gb", It.IsAny<CancellationToken>()))
            .ReturnsAsync(((byte[] Data, string ContentType, string FileName)?)null);

        var result = await _controller.GetBinary(1, pageGuid, attachmentGuid, "en-gb", CancellationToken.None);

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Put_WhenAttachmentUpdated_ReturnsOkWithResult()
    {
        var pageGuid = Guid.NewGuid();
        var attachmentGuid = Guid.NewGuid();
        var file = new FormFile(new MemoryStream([1, 2, 3]), 0, 3, "file", "updated.pdf");
        var expected = new PageAttachmentOutput { GUID = attachmentGuid, Name = "updated.pdf" };

        _attachmentServiceMock
            .Setup(x => x.UpdateAttachmentAsync(42, pageGuid, attachmentGuid, "en-gb", file, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var request = new AttachmentRequest { Culture = "en-gb", File = file };
        var result = await _controller.Put(1, pageGuid, attachmentGuid, request, CancellationToken.None);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        Assert.That(okResult.Value, Is.EqualTo(expected));
    }

    [Test]
    public async Task Put_WhenAttachmentNotFound_ReturnsNotFound()
    {
        var pageGuid = Guid.NewGuid();
        var attachmentGuid = Guid.NewGuid();
        var file = new FormFile(new MemoryStream([1, 2, 3]), 0, 3, "file", "updated.pdf");

        _attachmentServiceMock
            .Setup(x => x.UpdateAttachmentAsync(42, pageGuid, attachmentGuid, "en-gb", file, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PageAttachmentOutput?)null);

        var request = new AttachmentRequest { Culture = "en-gb", File = file };
        var result = await _controller.Put(1, pageGuid, attachmentGuid, request, CancellationToken.None);

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Put_ForwardsArgumentsToService()
    {
        var pageGuid = Guid.NewGuid();
        var attachmentGuid = Guid.NewGuid();
        var file = new FormFile(new MemoryStream([9, 8, 7]), 0, 3, "file", "updated.pdf");
        using var cts = new CancellationTokenSource();

        _attachmentServiceMock
            .Setup(x => x.UpdateAttachmentAsync(
                It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<IFormFile>(), cts.Token))
            .ReturnsAsync(new PageAttachmentOutput());

        var request = new AttachmentRequest { Culture = "fr-fr", File = file };
        await _controller.Put(999, pageGuid, attachmentGuid, request, cts.Token);

        _attachmentServiceMock.Verify(
            x => x.UpdateAttachmentAsync(42, pageGuid, attachmentGuid, "fr-fr", file, cts.Token),
            Times.Once);
    }
}
