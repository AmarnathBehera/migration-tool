using BDO.GWT.Core.Data;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.LiveSite.Controllers.API;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Controllers.API;

public class VCardControllerUnitTests
{
    private Mock<IPersonPageService> _personPageServiceMock = null!;
    private VCardController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _personPageServiceMock = new Mock<IPersonPageService>();
        _controller = new VCardController(_personPageServiceMock.Object);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "https";

        var urlHelper = new Mock<IUrlHelper>();
        urlHelper
            .Setup(u => u.Action(It.IsAny<UrlActionContext>()))
            .Returns("https://example.com/vcard/test");

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        _controller.Url = urlHelper.Object;
    }

    [Test]
    public async Task DownloadVCard_ReturnsNotFound_WhenVCardIsNull()
    {
        var personGuid = Guid.NewGuid();

        _personPageServiceMock
            .Setup(s => s.GeneratePersonVCardAsync(personGuid, It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VCardData?)null);

        var result = await _controller.DownloadVCard(personGuid, CancellationToken.None);

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task DownloadVCard_ReturnsFile_WhenVCardExists()
    {
        var personGuid = Guid.NewGuid();
        var vCardData = new VCardData
        {
            FullName = "John Doe",
            EMail = "john.doe@example.com",
            Title = "Partner",
            Organization = "BDO"
        };

        _personPageServiceMock
            .Setup(s => s.GeneratePersonVCardAsync(personGuid, It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vCardData);

        var result = await _controller.DownloadVCard(personGuid, CancellationToken.None);

        Assert.That(result, Is.InstanceOf<FileContentResult>());
        var fileResult = (FileContentResult)result;
        Assert.Multiple(() =>
        {
            Assert.That(fileResult.FileDownloadName, Is.EqualTo("John Doe.vcf"));
            Assert.That(fileResult.ContentType, Is.EqualTo("application/octet-stream"));
            Assert.That(fileResult.FileContents, Is.Not.Empty);
        });
    }

    [Test]
    public async Task DownloadVCard_FileContainsVCardContent()
    {
        var personGuid = Guid.NewGuid();
        var vCardData = new VCardData
        {
            FullName = "Jane Smith",
            EMail = "jane.smith@example.com",
            Title = "Director",
            Organization = "BDO"
        };

        _personPageServiceMock
            .Setup(s => s.GeneratePersonVCardAsync(personGuid, It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vCardData);

        var result = await _controller.DownloadVCard(personGuid, CancellationToken.None);

        var fileResult = (FileContentResult)result;
        var content = System.Text.Encoding.Default.GetString(fileResult.FileContents);
        Assert.Multiple(() =>
        {
            Assert.That(content, Does.Contain("BEGIN:VCARD"));
            Assert.That(content, Does.Contain("END:VCARD"));
            Assert.That(content, Does.Contain("FN"));
            Assert.That(content, Does.Contain("Jane Smith"));
        });
    }

    [Test]
    public async Task DownloadVCard_CallsServiceWithCorrectParameters()
    {
        var personGuid = Guid.NewGuid();

        _personPageServiceMock
            .Setup(s => s.GeneratePersonVCardAsync(personGuid, It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VCardData?)null);

        await _controller.DownloadVCard(personGuid, CancellationToken.None);

        _personPageServiceMock.Verify(
            s => s.GeneratePersonVCardAsync(personGuid, It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}