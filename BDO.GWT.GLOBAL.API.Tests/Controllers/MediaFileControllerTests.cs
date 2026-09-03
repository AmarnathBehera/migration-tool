using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Services;
using BDO.GWT.Web.Core.API.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Net.Mime;

namespace BDO.GWT.GLOBAL.API.Tests.Controllers;

[TestFixture]
public class MediaFileControllerTests
{
    private Mock<IMediaFileCrudService> _mediaFileCrudServiceMock = null!;
    private MediaFileController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mediaFileCrudServiceMock = new Mock<IMediaFileCrudService>(MockBehavior.Strict);

        _controller = new MediaFileController(_mediaFileCrudServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Test]
    public async Task GetMediaFilesAsync_ReturnsOkWithMediaFiles()
    {
        var expected = new[]
        {
            new MediaFileOutput { Id = 1, Guid = Guid.NewGuid(), Path = "a.jpg", MimeType = "image/jpeg", Extension = ".jpg", Link = "~/getmedia/a.jpg" },
            new MediaFileOutput { Id = 2, Guid = Guid.NewGuid(), Path = "b.png", MimeType = "image/png", Extension = ".png", Link = "~/getmedia/b.png" }
        };

        _mediaFileCrudServiceMock
            .Setup(x => x.GetMediaFilesAsync("Demo", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetMediaFilesAsync(100, "Demo", CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());

            var okResult = (OkObjectResult)result.Result!;
            Assert.That(okResult.Value, Is.EqualTo(expected));
        });

        _mediaFileCrudServiceMock.Verify(x => x.GetMediaFilesAsync("Demo", CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task GetMediaFileAsync_WhenFound_ReturnsOkWithMediaFile()
    {
        var attachmentGuid = Guid.NewGuid();
        var expected = new MediaFileOutput { Id = 3, Guid = attachmentGuid, Path = "c.webp", MimeType = "image/webp", Extension = ".webp", Link = "~/getmedia/c.webp" };

        _mediaFileCrudServiceMock
            .Setup(x => x.GetMediaFileAsync("Demo", attachmentGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetMediaFileAsync(100, "Demo", attachmentGuid, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());

            var okResult = (OkObjectResult)result.Result!;
            Assert.That(okResult.Value, Is.EqualTo(expected));
        });

        _mediaFileCrudServiceMock.Verify(x => x.GetMediaFileAsync("Demo", attachmentGuid, CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task GetMediaFileAsync_WhenNotFound_ReturnsNotFound()
    {
        var attachmentGuid = Guid.NewGuid();

        _mediaFileCrudServiceMock
            .Setup(x => x.GetMediaFileAsync("Demo", attachmentGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MediaFileOutput?)null);

        var result = await _controller.GetMediaFileAsync(100, "Demo", attachmentGuid, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
            Assert.That(result.Value, Is.Null);
        });

        _mediaFileCrudServiceMock.Verify(x => x.GetMediaFileAsync("Demo", attachmentGuid, CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task GetBinaryAsync_WhenNotFound_ReturnsNotFound()
    {
        var attachmentGuid = Guid.NewGuid();

        _mediaFileCrudServiceMock
            .Setup(x => x.DownloadMediaFileAsync("Demo", attachmentGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FileStreamData?)null);

        var result = await _controller.GetBinaryAsync(100, "Demo", attachmentGuid, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        });

        _mediaFileCrudServiceMock.Verify(x => x.DownloadMediaFileAsync("Demo", attachmentGuid, CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task GetBinaryAsync_WhenFound_ReturnsFileStreamResultAndSetsHeaders()
    {
        var attachmentGuid = Guid.NewGuid();
        var data = new byte[] { 1, 2, 3, 4 };
        var fileName = "download.jpg";
        var contentType = "image/jpeg";

        _mediaFileCrudServiceMock
            .Setup(x => x.DownloadMediaFileAsync("Demo", attachmentGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FileStreamData
            {
                Stream = new MemoryStream(data),
                ContentType = contentType,
                FileName = fileName
            });

        var result = await _controller.GetBinaryAsync(100, "Demo", attachmentGuid, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<FileStreamResult>());

            var fileResult = (FileStreamResult)result;
            Assert.That(fileResult.ContentType, Is.EqualTo(contentType));
            Assert.That(fileResult.FileDownloadName, Is.EqualTo(fileName));

            var disposition = _controller.Response.GetTypedHeaders().ContentDisposition;
            Assert.That(disposition, Is.Not.Null);
            Assert.That(disposition!.DispositionType, Is.EqualTo("attachment"));
            Assert.That(disposition.FileNameStar, Is.EqualTo(fileName));
        });

        _mediaFileCrudServiceMock.Verify(x => x.DownloadMediaFileAsync("Demo", attachmentGuid, CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task CreateMediaFileAsync_ReturnsCreatedAtRouteWithPayloadAndRouteValues()
    {
        var createdGuid = Guid.NewGuid();
        var input = new MediaFileInput
        {
            File = new FormFile(new MemoryStream([10, 11]), 0, 2, "File", "new.jpg"),
            DisplayName = "Hero image"
        };

        var expected = new MediaFileOutput
        {
            Id = 11,
            Guid = createdGuid,
            Path = "new.jpg",
            MimeType = "image/jpeg",
            Extension = ".jpg",
            Link = "~/getmedia/new.jpg"
        };

        _mediaFileCrudServiceMock
            .Setup(x => x.CreateMediaFileAsync("Demo", input, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.CreateMediaFileAsync(100, "Demo", input, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<CreatedAtRouteResult>());

            var created = (CreatedAtRouteResult)result.Result!;
            Assert.That(created.RouteName, Is.EqualTo("GetMediaFileByAttachmentGuid"));
            Assert.That(created.Value, Is.EqualTo(expected));
            Assert.That(created.RouteValues, Is.Not.Null);
            Assert.That(created.RouteValues!["memberFirmID"], Is.EqualTo(100));
            Assert.That(created.RouteValues["workspaceName"], Is.EqualTo("Demo"));
            Assert.That(created.RouteValues["attachmentGuid"], Is.EqualTo(createdGuid));
        });

        _mediaFileCrudServiceMock.Verify(x => x.CreateMediaFileAsync("Demo", input, CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task UpdateMediaFileAsync_WhenFound_ReturnsOkWithPayload()
    {
        var attachmentGuid = Guid.NewGuid();
        var input = new MediaFileInput
        {
            File = new FormFile(new MemoryStream([1, 2]), 0, 2, "File", "replace.png"),
            DisplayName = "Replacement"
        };

        var expected = new MediaFileOutput { Id = 12, Guid = attachmentGuid, Path = "replace.png", MimeType = "image/png", Extension = ".png", Link = "~/getmedia/replace.png" };

        _mediaFileCrudServiceMock
            .Setup(x => x.UpdateMediaFileAsync("Demo", attachmentGuid, input, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.UpdateMediaFileAsync(100, "Demo", attachmentGuid, input, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());

            var okResult = (OkObjectResult)result.Result!;
            Assert.That(okResult.Value, Is.EqualTo(expected));
        });

        _mediaFileCrudServiceMock.Verify(x => x.UpdateMediaFileAsync("Demo", attachmentGuid, input, CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task UpdateMediaFileAsync_WhenNotFound_ReturnsNotFound()
    {
        var attachmentGuid = Guid.NewGuid();
        var input = new MediaFileInput
        {
            File = new FormFile(new MemoryStream([1, 2]), 0, 2, "File", "replace.png")
        };

        _mediaFileCrudServiceMock
            .Setup(x => x.UpdateMediaFileAsync("Demo", attachmentGuid, input, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MediaFileOutput?)null);

        var result = await _controller.UpdateMediaFileAsync(100, "Demo", attachmentGuid, input, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
            Assert.That(result.Value, Is.Null);
        });

        _mediaFileCrudServiceMock.Verify(x => x.UpdateMediaFileAsync("Demo", attachmentGuid, input, CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task CreateMediaFileAsync_WhenFileNameIsInvalid_ReturnsBadRequest()
    {
        var input = new MediaFileInput
        {
            File = new FormFile(new MemoryStream([7, 8]), 0, 2, "File", string.Empty)
        };

        var result = await _controller.CreateMediaFileAsync(100, "Demo", input, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
        });

        _mediaFileCrudServiceMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task UpdateMediaFileAsync_WhenFileNameContainsPath_SanitizesBeforeServiceCall()
    {
        var attachmentGuid = Guid.NewGuid();
        var input = new MediaFileInput
        {
            File = new FormFile(new MemoryStream([1, 2, 3]), 0, 3, "File", "folder\\replace.png"),
            DisplayName = " Replacement "
        };

        var expected = new MediaFileOutput { Id = 21, Guid = attachmentGuid, Path = "replace.png", MimeType = "image/png", Extension = ".png", Link = "~/getmedia/replace.png" };

        _mediaFileCrudServiceMock
            .Setup(x => x.UpdateMediaFileAsync(
                "Demo",
                attachmentGuid,
                It.Is<MediaFileInput>(m => m.File.FileName == "replace.png" && m.DisplayName == "Replacement"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.UpdateMediaFileAsync(100, "Demo", attachmentGuid, input, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = (OkObjectResult)result.Result!;
            Assert.That(okResult.Value, Is.EqualTo(expected));
        });

        _mediaFileCrudServiceMock.Verify(x => x.UpdateMediaFileAsync(
            "Demo",
            attachmentGuid,
            It.Is<MediaFileInput>(m => m.File.FileName == "replace.png" && m.DisplayName == "Replacement"),
            CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task DeleteMediaFileAsync_WhenDeleted_ReturnsNoContent()
    {
        var attachmentGuid = Guid.NewGuid();

        _mediaFileCrudServiceMock
            .Setup(x => x.DeleteMediaFileAsync(attachmentGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.DeleteMediaFileAsync(100, "Demo", attachmentGuid, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        });

        _mediaFileCrudServiceMock.Verify(x => x.DeleteMediaFileAsync(attachmentGuid, CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task DeleteMediaFileAsync_WhenNotDeleted_ReturnsNotFound()
    {
        var attachmentGuid = Guid.NewGuid();

        _mediaFileCrudServiceMock
            .Setup(x => x.DeleteMediaFileAsync(attachmentGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _controller.DeleteMediaFileAsync(100, "Demo", attachmentGuid, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        });

        _mediaFileCrudServiceMock.Verify(x => x.DeleteMediaFileAsync(attachmentGuid, CancellationToken.None), Times.Once);
    }
}
