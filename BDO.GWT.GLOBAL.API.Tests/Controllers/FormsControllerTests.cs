using BDO.GWT.GLOBAL.API.Controllers;
using BDO.GWT.GLOBAL.API.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.API.Tests.Controllers;

public class FormsControllerTests
{
    private Mock<IFormCrudService> _formCrudServiceMock = null!;
    private FormsController _controller = null!;
    private readonly int memberFirmID = 960;

    [SetUp]
    public void Setup()
    {
        _formCrudServiceMock = new Mock<IFormCrudService>();
        _controller = new FormsController(_formCrudServiceMock.Object);
    }

    [Test]
    public async Task GetForms_ReturnsOk_WithForms()
    {
        var forms = new List<FormOutput>
        {
            new() { General = new FormGeneralOutput { FormCodeName = "test", FormDisplayName = "Test" } }
        };
        var cancellationToken = new CancellationTokenSource().Token;

        _formCrudServiceMock
            .Setup(x => x.GetFormsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(forms);

        var result = await _controller.GetForms(memberFirmID, cancellationToken);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result!;
        Assert.That(okResult.Value, Is.SameAs(forms));
        _formCrudServiceMock.Verify(x => x.GetFormsAsync(cancellationToken), Times.Once);
    }

    [Test]
    public async Task GetForms_Returns500_WhenServiceThrows()
    {
        _formCrudServiceMock
            .Setup(x => x.GetFormsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("boom"));

        var result = await _controller.GetForms(memberFirmID, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
            var objectResult = (ObjectResult)result.Result!;
            Assert.That(objectResult.StatusCode, Is.EqualTo(500));
            Assert.That(objectResult.Value?.ToString(), Does.Contain("Error retrieving forms"));
        });
    }

    [Test]
    public async Task GetForm_ReturnsNotFound_WhenKeyNotFoundExceptionIsThrown()
    {
        _formCrudServiceMock
            .Setup(x => x.GetFormAsync("missing", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("not found"));

        var result = await _controller.GetForm(memberFirmID, "missing", CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
        var notFound = (NotFoundObjectResult)result.Result!;
        Assert.That(notFound.Value, Is.EqualTo("not found"));
    }

    [Test]
    public async Task GetFormEntries_ReturnsOk_WithEntries()
    {
        var entries = new List<Dictionary<string, object?>>()
        {
            new() { ["FormEntryID"] = 101, ["Name"] = "John" }
        };

        _formCrudServiceMock
            .Setup(x => x.GetFormEntriesAsync("contact", It.IsAny<CancellationToken>()))
            .ReturnsAsync(entries);

        var result = await _controller.GetFormEntries(memberFirmID, "contact", CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result!;
        Assert.That(okResult.Value, Is.SameAs(entries));
    }

    [Test]
    public async Task GetFormEntry_ReturnsNotFound_WhenEntryDoesNotExist()
    {
        _formCrudServiceMock
            .Setup(x => x.GetFormEntryAsync("contact", 999, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("missing entry"));

        var result = await _controller.GetFormEntry(memberFirmID, "contact", 999, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
        var notFound = (NotFoundObjectResult)result.Result!;
        Assert.That(notFound.Value, Is.EqualTo("missing entry"));
    }

    [Test]
    public async Task GetFormEntryAttachment_ReturnsFileStreamResult()
    {
        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });

        _formCrudServiceMock
            .Setup(x => x.GetFormEntryAttachmentAsync("contact", 10, "resume", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FileStreamData
            {
                Stream = stream,
                ContentType = "application/pdf",
                FileName = "resume.pdf"
            });

        var result = await _controller.GetFormEntryAttachment(1, "contact", 10, "resume", CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<FileStreamResult>());
            var fileResult = (FileStreamResult)result.Result!;
            Assert.That(fileResult.ContentType, Is.EqualTo("application/pdf"));
            Assert.That(fileResult.FileDownloadName, Is.EqualTo("resume.pdf"));
        });
    }

    [Test]
    public async Task DeleteFormEntry_ReturnsOk_WhenDeleteSucceeds()
    {
        _formCrudServiceMock
            .Setup(x => x.DeleteFormEntryAsync("contact", 15, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.DeleteFormEntry(memberFirmID, "contact", 15, CancellationToken.None);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        var message = okResult.Value?.GetType().GetProperty("message")?.GetValue(okResult.Value)?.ToString();
        Assert.That(message, Is.EqualTo("Form entry '15' for form 'contact' was successfully deleted."));
    }

    [Test]
    public async Task DeleteFormEntry_ReturnsNotFound_WhenEntryIsMissing()
    {
        _formCrudServiceMock
            .Setup(x => x.DeleteFormEntryAsync("contact", 15, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("missing"));

        var result = await _controller.DeleteFormEntry(memberFirmID, "contact", 15, CancellationToken.None);

        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        var notFound = (NotFoundObjectResult)result;
        Assert.That(notFound.Value, Is.EqualTo("missing"));
    }

    [Test]
    public async Task GetForm_ReturnsOk_WhenFormExists()
    {
        var form = new FormOutput
        {
            General = new FormGeneralOutput { FormCodeName = "contact", FormDisplayName = "Contact Us" }
        };

        _formCrudServiceMock
            .Setup(x => x.GetFormAsync("contact", It.IsAny<CancellationToken>()))
            .ReturnsAsync(form);

        var result = await _controller.GetForm(memberFirmID, "contact", CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result!;
        Assert.That(okResult.Value, Is.SameAs(form));
    }

    [Test]
    public async Task GetForm_Returns500_WhenServiceThrows()
    {
        _formCrudServiceMock
            .Setup(x => x.GetFormAsync("contact", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("unexpected"));

        var result = await _controller.GetForm(memberFirmID, "contact", CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
            var objectResult = (ObjectResult)result.Result!;
            Assert.That(objectResult.StatusCode, Is.EqualTo(500));
            Assert.That(objectResult.Value?.ToString(), Does.Contain("Error retrieving form"));
        });
    }

    [Test]
    public async Task GetFormEntries_ReturnsNotFound_WhenFormDoesNotExist()
    {
        _formCrudServiceMock
            .Setup(x => x.GetFormEntriesAsync("missing", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("form not found"));

        var result = await _controller.GetFormEntries(memberFirmID, "missing", CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
        var notFound = (NotFoundObjectResult)result.Result!;
        Assert.That(notFound.Value, Is.EqualTo("form not found"));
    }

    [Test]
    public async Task GetFormEntries_Returns500_WhenServiceThrows()
    {
        _formCrudServiceMock
            .Setup(x => x.GetFormEntriesAsync("contact", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("db error"));

        var result = await _controller.GetFormEntries(memberFirmID, "contact", CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
            var objectResult = (ObjectResult)result.Result!;
            Assert.That(objectResult.StatusCode, Is.EqualTo(500));
            Assert.That(objectResult.Value?.ToString(), Does.Contain("Error retrieving form entries"));
        });
    }

    [Test]
    public async Task GetFormEntry_ReturnsOk_WhenEntryExists()
    {
        var entry = new Dictionary<string, object?> { ["FormEntryID"] = 5, ["Email"] = "test@test.com" };

        _formCrudServiceMock
            .Setup(x => x.GetFormEntryAsync("contact", 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entry);

        var result = await _controller.GetFormEntry(memberFirmID, "contact", 5, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result!;
        Assert.That(okResult.Value, Is.SameAs(entry));
    }

    [Test]
    public async Task GetFormEntry_Returns500_WhenServiceThrows()
    {
        _formCrudServiceMock
            .Setup(x => x.GetFormEntryAsync("contact", 5, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("timeout"));

        var result = await _controller.GetFormEntry(memberFirmID, "contact", 5, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
            var objectResult = (ObjectResult)result.Result!;
            Assert.That(objectResult.StatusCode, Is.EqualTo(500));
            Assert.That(objectResult.Value?.ToString(), Does.Contain("Error retrieving form entry"));
        });
    }

    [Test]
    public async Task DeleteFormEntry_Returns500_WhenServiceThrows()
    {
        _formCrudServiceMock
            .Setup(x => x.DeleteFormEntryAsync("contact", 15, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("delete failed"));

        var result = await _controller.DeleteFormEntry(memberFirmID, "contact", 15, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var objectResult = (ObjectResult)result;
            Assert.That(objectResult.StatusCode, Is.EqualTo(500));
            Assert.That(objectResult.Value?.ToString(), Does.Contain("Error deleting form entry"));
        });
    }

    [Test]
    public async Task GetForms_ReturnsOk_WithEmptyList()
    {
        _formCrudServiceMock
            .Setup(x => x.GetFormsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FormOutput>());

        var result = await _controller.GetForms(memberFirmID, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = (OkObjectResult)result.Result!;
            Assert.That(okResult.Value, Is.InstanceOf<IEnumerable<FormOutput>>());
            Assert.That(((IEnumerable<FormOutput>)okResult.Value!).Count(), Is.EqualTo(0));
        });
    }

    [Test]
    public async Task GetForms_ForwardsCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _formCrudServiceMock
            .Setup(x => x.GetFormsAsync(token))
            .ReturnsAsync(new List<FormOutput>());

        await _controller.GetForms(memberFirmID, token);

        _formCrudServiceMock.Verify(x => x.GetFormsAsync(token), Times.Once);
    }

    [Test]
    public async Task GetForm_ForwardsCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _formCrudServiceMock
            .Setup(x => x.GetFormAsync("test", token))
            .ReturnsAsync(new FormOutput { General = new FormGeneralOutput { FormCodeName = "test", FormDisplayName = "Test" } });

        await _controller.GetForm(memberFirmID, "test", token);

        _formCrudServiceMock.Verify(x => x.GetFormAsync("test", token), Times.Once);
    }

    [Test]
    public async Task GetFormEntries_ForwardsCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _formCrudServiceMock
            .Setup(x => x.GetFormEntriesAsync("contact", token))
            .ReturnsAsync(new List<Dictionary<string, object?>>());

        await _controller.GetFormEntries(memberFirmID, "contact", token);

        _formCrudServiceMock.Verify(x => x.GetFormEntriesAsync("contact", token), Times.Once);
    }

    [Test]
    public async Task GetFormEntry_ForwardsCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _formCrudServiceMock
            .Setup(x => x.GetFormEntryAsync("contact", 1, token))
            .ReturnsAsync(new Dictionary<string, object?> { ["ID"] = 1 });

        await _controller.GetFormEntry(memberFirmID, "contact", 1, token);

        _formCrudServiceMock.Verify(x => x.GetFormEntryAsync("contact", 1, token), Times.Once);
    }

    [Test]
    public async Task DeleteFormEntry_ForwardsCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _formCrudServiceMock
            .Setup(x => x.DeleteFormEntryAsync("contact", 1, token))
            .Returns(Task.CompletedTask);

        await _controller.DeleteFormEntry(memberFirmID, "contact", 1, token);

        _formCrudServiceMock.Verify(x => x.DeleteFormEntryAsync("contact", 1, token), Times.Once);
    }

    [Test]
    public async Task GetFormEntryAttachment_ForwardsCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;
        using var stream = new MemoryStream([1]);

        _formCrudServiceMock
            .Setup(x => x.GetFormEntryAttachmentAsync("contact", 1, "file", token))
            .ReturnsAsync(new FileStreamData { Stream = stream, ContentType = "text/plain", FileName = "a.txt" });

        await _controller.GetFormEntryAttachment(1, "contact", 1, "file", token);

        _formCrudServiceMock.Verify(x => x.GetFormEntryAttachmentAsync("contact", 1, "file", token), Times.Once);
    }

    [Test]
    public async Task GetForms_ReturnsOk_WithMultipleForms()
    {
        var forms = new List<FormOutput>
        {
            new() { General = new FormGeneralOutput { FormCodeName = "contact", FormDisplayName = "Contact Us" } },
            new() { General = new FormGeneralOutput { FormCodeName = "feedback", FormDisplayName = "Feedback" } },
            new() { General = new FormGeneralOutput { FormCodeName = "signup", FormDisplayName = "Sign Up" } }
        };

        _formCrudServiceMock
            .Setup(x => x.GetFormsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(forms);

        var result = await _controller.GetForms(memberFirmID, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result!;
        var returnedForms = (IEnumerable<FormOutput>)okResult.Value!;
        Assert.That(returnedForms.Count(), Is.EqualTo(3));
    }

    [Test]
    public async Task GetFormEntries_ReturnsOk_WithEmptyEntries()
    {
        _formCrudServiceMock
            .Setup(x => x.GetFormEntriesAsync("contact", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Dictionary<string, object?>>());

        var result = await _controller.GetFormEntries(memberFirmID, "contact", CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result!;
        var entries = (IEnumerable<Dictionary<string, object?>>)okResult.Value!;
        Assert.That(entries.Count(), Is.EqualTo(0));
    }

    [Test]
    public async Task GetFormEntryAttachment_ReturnsCorrectFileName()
    {
        using var stream = new MemoryStream([0xFF, 0xD8, 0xFF]);

        _formCrudServiceMock
            .Setup(x => x.GetFormEntryAttachmentAsync("contact", 5, "photo", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FileStreamData
            {
                Stream = stream,
                ContentType = "image/jpeg",
                FileName = "profile-photo.jpg"
            });

        var result = await _controller.GetFormEntryAttachment(1, "contact", 5, "photo", CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<FileStreamResult>());
            var fileResult = (FileStreamResult)result.Result!;
            Assert.That(fileResult.ContentType, Is.EqualTo("image/jpeg"));
            Assert.That(fileResult.FileDownloadName, Is.EqualTo("profile-photo.jpg"));
            Assert.That(fileResult.FileStream, Is.SameAs(stream));
        });
    }

    [Test]
    public async Task GetFormEntry_ReturnsOk_WithNullFieldValues()
    {
        var entry = new Dictionary<string, object?>
        {
            ["FormEntryID"] = 1,
            ["Name"] = "Jane",
            ["Phone"] = null,
            ["Notes"] = null
        };

        _formCrudServiceMock
            .Setup(x => x.GetFormEntryAsync("contact", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entry);

        var result = await _controller.GetFormEntry(memberFirmID, "contact", 1, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result!;
        var returned = (Dictionary<string, object?>)okResult.Value!;
        Assert.Multiple(() =>
        {
            Assert.That(returned["Phone"], Is.Null);
            Assert.That(returned["Notes"], Is.Null);
            Assert.That(returned["Name"], Is.EqualTo("Jane"));
        });
    }

    [Test]
    public async Task DeleteFormEntry_ReturnsOk_MessageContainsFormCodeNameAndEntryId()
    {
        _formCrudServiceMock
            .Setup(x => x.DeleteFormEntryAsync("feedback", 42, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.DeleteFormEntry(memberFirmID, "feedback", 42, CancellationToken.None);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        var message = okResult.Value?.GetType().GetProperty("message")?.GetValue(okResult.Value)?.ToString();
        Assert.That(message, Does.Contain("42"));
        Assert.That(message, Does.Contain("feedback"));
    }
}
