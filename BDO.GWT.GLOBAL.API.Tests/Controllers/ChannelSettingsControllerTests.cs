using BDO.GWT.GLOBAL.API.Controllers;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Queries;
using BDO.GWT.GLOBAL.API.Services;
using ChannelSettingInput = BDO.GWT.Web.Core.API.Data.ChannelSettingInput;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.API.Tests.Controllers;

public class ChannelSettingsControllerTests
{
    private Mock<IChannelSettingCrudService> _channelSettingCrudServiceMock = null!;
    private ChannelSettingsController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _channelSettingCrudServiceMock = new Mock<IChannelSettingCrudService>();
        _controller = new ChannelSettingsController(_channelSettingCrudServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        _controller.HttpContext.Items["ChannelId"] = 100;
    }

    [Test]
    public async Task GetChannelSettings_ReturnsOk_WithSettingsArray()
    {
        var query = new SettingsQuery { DisplayName = "ScriptHeadKeyValue" };
        var expected = new[]
        {
            new ChannelSettingOutput
            {
                CodeName = "BDO_ScriptHeadKeyValue",
                DisplayName = "ScriptHeadKeyValue",
                InheritFromGlobalSettings = false,
                Type = "string",
                Value = "test"
            }
        };

        _channelSettingCrudServiceMock
            .Setup(x => x.GetChannelSettings(100, query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetChannelSettings(100, query, CancellationToken.None);

        var okResult = (OkObjectResult)result.Result!;
        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            Assert.That(okResult.Value, Is.EqualTo(expected));
        });

        _channelSettingCrudServiceMock.Verify(
            x => x.GetChannelSettings(100, query, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetChannelSetting_ReturnsOk_WithMatchingSetting()
    {
        const string codeName = "BDO_ScriptHeadKeyValue";
        var expected = new[]
        {
            new ChannelSettingOutput
            {
                CodeName = codeName,
                DisplayName = "ScriptHeadKeyValue",
                InheritFromGlobalSettings = true,
                Type = "string",
                Value = "value"
            }
        };

        _channelSettingCrudServiceMock
            .Setup(x => x.GetChannelSetting(100, codeName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetChannelSetting(100, codeName, CancellationToken.None);

        var okResult = (OkObjectResult)result.Result!;
        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            Assert.That((ChannelSettingOutput[])okResult.Value!, Has.Length.EqualTo(1));
            Assert.That(okResult.Value, Is.EqualTo(expected));
        });
    }

    [Test]
    public async Task PutChannelSetting_ReturnsOk_WhenUpdated()
    {
        const string codeName = "BDO_ScriptHeadKeyValue";
        var request = new ChannelSettingInput
        {
            InheritFromGlobalSettings = true,
            Value = "new testing script"
        };

        var expected = new ChannelSettingOutput
        {
            CodeName = codeName,
            DisplayName = "ScriptHeadKeyValue",
            InheritFromGlobalSettings = true,
            Type = "string",
            Value = "new testing script"
        };

        _channelSettingCrudServiceMock
            .Setup(x => x.UpdateChannelSetting(100, codeName, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.PutChannelSetting(100, codeName, request, CancellationToken.None);

        var okResult = (OkObjectResult)result.Result!;
        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            Assert.That(okResult.Value, Is.EqualTo(expected));
        });

        _channelSettingCrudServiceMock.Verify(
            x => x.UpdateChannelSetting(100, codeName, request, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task PutChannelSetting_ReturnsNotFound_WhenNoSettingUpdated()
    {
        const string codeName = "BDO_Unknown";
        var request = new ChannelSettingInput
        {
            InheritFromGlobalSettings = false,
            Value = "test"
        };

        _channelSettingCrudServiceMock
            .Setup(x => x.UpdateChannelSetting(100, codeName, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChannelSettingOutput?)null);

        var result = await _controller.PutChannelSetting(100, codeName, request, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        });
    }
}
