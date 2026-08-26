using BDO.GWT.GLOBAL.API.Controllers;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Queries;
using BDO.GWT.GLOBAL.API.Services;
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
    }

    [Test]
    public async Task GetChannelSettings_ReturnsOk_WithSettingsArray()
    {
        var query = new SettingsQuery { DisplayName = "Email" };
        var expected = new[]
        {
            new ChannelSettingOutput
            {
                CodeName = "email_setting",
                DisplayName = "Email Setting",
                InheritFromGlobalSettings = false,
                Type = "Email"
            }
        };

        _channelSettingCrudServiceMock
            .Setup(x => x.GetChannelSettings(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetChannelSettings(1, query, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result!;
        Assert.That(okResult.Value, Is.EqualTo(expected));

        _channelSettingCrudServiceMock.Verify(
            x => x.GetChannelSettings(query, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetChannelSettings_ReturnsOk_WithEmptyArray()
    {
        var query = new SettingsQuery();

        _channelSettingCrudServiceMock
            .Setup(x => x.GetChannelSettings(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _controller.GetChannelSettings(1, query, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result!;
        Assert.That((ChannelSettingOutput[])okResult.Value!, Has.Length.EqualTo(0));
    }

    [Test]
    public async Task GetChannelSetting_ReturnsOk_WithMatchingSetting()
    {
        const string codeName = "email_setting";
        var expected = new[]
        {
            new ChannelSettingOutput
            {
                CodeName = codeName,
                DisplayName = "Email Setting",
                InheritFromGlobalSettings = true,
                Type = "Email"
            }
        };

        _channelSettingCrudServiceMock
            .Setup(x => x.GetChannelSetting(codeName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetChannelSetting(1, codeName, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result!;
        Assert.That(okResult.Value, Is.EqualTo(expected));

        _channelSettingCrudServiceMock.Verify(
            x => x.GetChannelSetting(codeName, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetChannelSetting_ReturnsOk_WithNull_WhenNotFound()
    {
        const string codeName = "nonexistent";

        _channelSettingCrudServiceMock
            .Setup(x => x.GetChannelSetting(codeName, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChannelSettingOutput[]?)null);

        var result = await _controller.GetChannelSetting(1, codeName, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result!;
        Assert.That(okResult.Value, Is.Null);
    }
}
