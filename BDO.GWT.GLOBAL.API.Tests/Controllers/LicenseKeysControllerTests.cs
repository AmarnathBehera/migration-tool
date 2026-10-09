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

    [Test]
    public async Task Get_ReturnsOk_WithLicenseKey()
    {
        var expected = CreateLicenseKeyOutput(id: 5, key: "license-key-value");

        _licenseKeysCrudServiceMock
            .Setup(x => x.GetLicenseKeyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.Get(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = (OkObjectResult)result.Result!;
            Assert.That(okResult.Value, Is.EqualTo(expected));
        });

        _licenseKeysCrudServiceMock.Verify(x => x.GetLicenseKeyAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Get_ReturnsOk_WhenLicenseKeyHasNullKey()
    {
        var expected = CreateLicenseKeyOutput(id: 1, key: null);

        _licenseKeysCrudServiceMock
            .Setup(x => x.GetLicenseKeyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.Get(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = (OkObjectResult)result.Result!;
            Assert.That(okResult.Value, Is.EqualTo(expected));
            Assert.That(((LicenseKeyOutput)okResult.Value!).Key, Is.Null);
        });

        _licenseKeysCrudServiceMock.Verify(x => x.GetLicenseKeyAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Get_PassesCancellationToken_ToService()
    {
        using var cts = new CancellationTokenSource();
        var expected = CreateLicenseKeyOutput();

        _licenseKeysCrudServiceMock
            .Setup(x => x.GetLicenseKeyAsync(cts.Token))
            .ReturnsAsync(expected);

        var result = await _controller.Get(cts.Token);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        _licenseKeysCrudServiceMock.Verify(x => x.GetLicenseKeyAsync(cts.Token), Times.Once);
    }

    [Test]
    public void Get_PropagatesException_WhenServiceThrows()
    {
        _licenseKeysCrudServiceMock
            .Setup(x => x.GetLicenseKeyAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("License key retrieval failed."));

        Assert.That(
            async () => await _controller.Get(CancellationToken.None),
            Throws.InstanceOf<InvalidOperationException>());

        _licenseKeysCrudServiceMock.Verify(x => x.GetLicenseKeyAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}