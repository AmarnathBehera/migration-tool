using BDO.GWT.GLOBAL.API.Controllers;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.API.Tests.Controllers;

[TestFixture]
public class LicenseKeysControllerTests
{
    private Mock<ILicenseKeysCrudService> _licenseKeysCrudServiceMock = null!;
    private LicenseKeysController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _licenseKeysCrudServiceMock = new Mock<ILicenseKeysCrudService>();
        _controller = new LicenseKeysController(_licenseKeysCrudServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    private static LicenseKeyOutput CreateLicenseKeyOutput(int id = 1, string? key = "test-key") =>
        new() { ID = id, Key = key };

    #region Get

    [Test]
    public async Task Get_ReturnsOk_WithLicenseKey()
    {
        var expected = CreateLicenseKeyOutput();
        _licenseKeysCrudServiceMock
            .Setup(x => x.GetLicenseKeyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.Get(CancellationToken.None);

        var okResult = result.Result as OkObjectResult;
        Assert.Multiple(() =>
        {
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.EqualTo(expected));
        });
    }

    [Test]
    public async Task Get_CallsServiceOnce()
    {
        _licenseKeysCrudServiceMock
            .Setup(x => x.GetLicenseKeyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateLicenseKeyOutput());

        await _controller.Get(CancellationToken.None);

        _licenseKeysCrudServiceMock.Verify(
            x => x.GetLicenseKeyAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Get_ReturnsOk_WhenKeyIsNull()
    {
        var expected = CreateLicenseKeyOutput(key: null);
        _licenseKeysCrudServiceMock
            .Setup(x => x.GetLicenseKeyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.Get(CancellationToken.None);

        var okResult = result.Result as OkObjectResult;
        Assert.Multiple(() =>
        {
            Assert.That(okResult, Is.Not.Null);
            Assert.That((okResult!.Value as LicenseKeyOutput)?.Key, Is.Null);
        });
    }

    #endregion

    #region Post

    [Test]
    public async Task Post_ReturnsCreatedAtAction_WithLicenseKey()
    {
        var input = new LicenseKeyInput { Key = "new-key" };
        var expected = CreateLicenseKeyOutput(id: 5, key: "new-key");
        _licenseKeysCrudServiceMock
            .Setup(x => x.SetLicenseKeyAsync(input, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.Post(input, CancellationToken.None);

        var createdResult = result.Result as CreatedAtActionResult;
        Assert.Multiple(() =>
        {
            Assert.That(createdResult, Is.Not.Null);
            Assert.That(createdResult!.Value, Is.EqualTo(expected));
            Assert.That(createdResult.ActionName, Is.EqualTo(nameof(_controller.Get)));
        });
    }

    [Test]
    public async Task Post_PassesInputToService()
    {
        var input = new LicenseKeyInput { Key = "abc123" };
        _licenseKeysCrudServiceMock
            .Setup(x => x.SetLicenseKeyAsync(It.IsAny<LicenseKeyInput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateLicenseKeyOutput());

        await _controller.Post(input, CancellationToken.None);

        _licenseKeysCrudServiceMock.Verify(
            x => x.SetLicenseKeyAsync(input, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Post_CreatedAtAction_RouteValues_ContainLicenseKeyId()
    {
        var input = new LicenseKeyInput { Key = "new-key" };
        var expected = CreateLicenseKeyOutput(id: 42, key: "new-key");
        _licenseKeysCrudServiceMock
            .Setup(x => x.SetLicenseKeyAsync(input, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.Post(input, CancellationToken.None);

        var createdResult = result.Result as CreatedAtActionResult;
        Assert.That(createdResult!.RouteValues!["licenseKeyID"], Is.EqualTo(42));
    }

    #endregion

    #region Delete

    [Test]
    public async Task Delete_ReturnsNoContent()
    {
        _licenseKeysCrudServiceMock
            .Setup(x => x.DeleteLicenseKeyAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.Delete(1, CancellationToken.None);

        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task Delete_CallsServiceOnce()
    {
        _licenseKeysCrudServiceMock
            .Setup(x => x.DeleteLicenseKeyAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _controller.Delete(1, CancellationToken.None);

        _licenseKeysCrudServiceMock.Verify(
            x => x.DeleteLicenseKeyAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion
}
