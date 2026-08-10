using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Services;
using BDO.GWT.Web.Core.API.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.API.Tests.Controllers;

public class ChannelControllerTests
{
    private Mock<IChannelCrudService> _channelCrudServiceMock = null!;
    private ChannelController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _channelCrudServiceMock = new Mock<IChannelCrudService>();
        _controller = new ChannelController(_channelCrudServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Test]
    public async Task GetChannelsAsync_ReturnsOk_WithChannelList()
    {
        var query = new ChannelsQuery();
        var expected = new List<ChannelOutput>
        {
            new ChannelOutput
            {
                General = new SiteGeneralOutput
                {
                    ID = 1,
                    GUID = Guid.NewGuid(),
                    DisplayName = "Test Channel",
                    CodeName = "test-channel",
                    WebsitedomainresentationURL = "https://test.com"
                }
            }
        };

        _channelCrudServiceMock
            .Setup(x => x.GetEntitiesAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetChannelsAsync(query, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result!;
        Assert.That(okResult.Value, Is.EqualTo(expected));

        _channelCrudServiceMock.Verify(
            x => x.GetEntitiesAsync(query, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetChannelsAsync_ReturnsOk_WithEmptyList()
    {
        var query = new ChannelsQuery();
        var expected = new List<ChannelOutput>();

        _channelCrudServiceMock
            .Setup(x => x.GetEntitiesAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetChannelsAsync(query, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task GetChannelAsync_ReturnsOk_WithChannel()
    {
        var channelGuid = Guid.NewGuid();
        var expected = new ChannelOutput
        {
            General = new SiteGeneralOutput
            {
                ID = 1,
                GUID = channelGuid,
                DisplayName = "Test Channel",
                CodeName = "test-channel",
                WebsitedomainresentationURL = "https://test.com"
            }
        };

        _channelCrudServiceMock
            .Setup(x => x.GetEntityAsync(channelGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetChannelAsync(channelGuid, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result!;
        Assert.That(okResult.Value, Is.EqualTo(expected));

        _channelCrudServiceMock.Verify(
            x => x.GetEntityAsync(channelGuid, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetChannelAsync_ReturnsOk_WithNull_WhenNotFound()
    {
        var channelGuid = Guid.NewGuid();

        _channelCrudServiceMock
            .Setup(x => x.GetEntityAsync(channelGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChannelOutput?)null);

        var result = await _controller.GetChannelAsync(channelGuid, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result!;
        Assert.That(okResult.Value, Is.Null);
    }
}
