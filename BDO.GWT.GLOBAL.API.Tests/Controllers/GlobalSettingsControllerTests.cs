using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Queries;
using BDO.GWT.GLOBAL.API.Services;
using BDO.GWT.Web.Core.API.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.API.Tests.Controllers;

public class GlobalSettingsControllerTests
{
    private Mock<IGlobalSettingCrudService> _globalSettingCrudServiceMock = null!;
    private GlobalSettingsController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _globalSettingCrudServiceMock = new Mock<IGlobalSettingCrudService>();
        _controller = new GlobalSettingsController(_globalSettingCrudServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Test]
    public async Task GetGlobalSettings_ReturnsOk_WithSettingsArray()
    {
        var query = new SettingsQuery { DisplayName = "Cookie" };
        var expected = new[]
        {
            new GlobalSettingOutput
            {
                CodeName = "cookiebot",
                DisplayName = "Cookiebot",
                Type="string",
                Value = "abc123"
            }
        };

        _globalSettingCrudServiceMock
            .Setup(x => x.GetGlobalSettings(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetGlobalSettings(query, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result!;
        Assert.That(okResult.Value, Is.EqualTo(expected));

        _globalSettingCrudServiceMock.Verify(
            x => x.GetGlobalSettings(query, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetGlobalSettings_ReturnsOk_WithEmptyArray()
    {
        var query = new SettingsQuery();

        _globalSettingCrudServiceMock
            .Setup(x => x.GetGlobalSettings(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _controller.GetGlobalSettings(query, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result!;
        Assert.That((GlobalSettingOutput[])okResult.Value!, Has.Length.EqualTo(0));
    }

    [Test]
    public async Task GetGlobalSetting_ReturnsOk_WithMatchingSetting()
    {
        const string codeName = "cookiebot";
        var expected = new GlobalSettingOutput
        {
            CodeName = codeName,
            DisplayName = "Cookiebot",
            Type= "string",
            Value = "abc123"
        };

        _globalSettingCrudServiceMock
            .Setup(x => x.GetGlobalSetting(codeName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetGlobalSetting(codeName, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result!;
        Assert.That(okResult.Value, Is.EqualTo(expected));

        _globalSettingCrudServiceMock.Verify(
            x => x.GetGlobalSetting(codeName, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetGlobalSetting_ReturnsOk_WithNull_WhenNotFound()
    {
        const string codeName = "nonexistent";

        _globalSettingCrudServiceMock
            .Setup(x => x.GetGlobalSetting(codeName, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GlobalSettingOutput)null!);

        var result = await _controller.GetGlobalSetting(codeName, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result!;
        Assert.That(okResult.Value, Is.Null);
    }

    [Test]
    public async Task SetGlobalSetting_ReturnsOk_WithUpdatedSetting()
    {
        const string codeName = "cookiebot";
        var input = new GlobalSettingInput { Value = "new-value" };
        var expected = new GlobalSettingOutput
        {
            CodeName = codeName,
            DisplayName = "Cookiebot",
            Type = "string",
            Value = "new-value"
        };

        _globalSettingCrudServiceMock
            .Setup(x => x.SetGlobalSetting(codeName, input, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.SetGlobalSetting(codeName, input, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result!;
        Assert.That(okResult.Value, Is.EqualTo(expected));

        _globalSettingCrudServiceMock.Verify(
            x => x.SetGlobalSetting(codeName, input, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task SetGlobalSetting_PropagatesServiceException()
    {
        const string codeName = "cookiebot";
        var input = new GlobalSettingInput { Value = "bad" };

        _globalSettingCrudServiceMock
            .Setup(x => x.SetGlobalSetting(codeName, input, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _controller.SetGlobalSetting(codeName, input, CancellationToken.None));

        await Task.CompletedTask;
    }
}
