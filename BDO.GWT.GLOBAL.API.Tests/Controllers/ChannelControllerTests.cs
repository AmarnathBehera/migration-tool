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
        var query = new ChannelsQuery { AdministrationDomain = "admin.example.com" };
        var expected = new List<ChannelOutput>
        {
            new()
            {
                General = new SiteGeneralOutput
                {
                    ID = 1,
                    GUID = Guid.NewGuid(),
                    DisplayName = "Test Channel",
                    CodeName = "test-channel",
                    WebsitedomainresentationURL = "test.example.com",
                    DefaultContentCulture = "en-GB"
                }
            }
        };

        _channelCrudServiceMock
            .Setup(x => x.GetEntitiesAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetChannelsAsync(query, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = (OkObjectResult)result.Result!;
            Assert.That(okResult.Value, Is.EqualTo(expected));
        });

        _channelCrudServiceMock.Verify(x => x.GetEntitiesAsync(query, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetChannelAsync_ReturnsOk_WithChannel()
    {
        var channelGuid = Guid.NewGuid();
        var expected = new ChannelOutput
        {
            General = new SiteGeneralOutput
            {
                ID = 10,
                GUID = channelGuid,
                DisplayName = "Channel 10",
                CodeName = "channel-10",
                WebsitedomainresentationURL = "channel10.example.com"
            }
        };

        _channelCrudServiceMock
            .Setup(x => x.GetEntityAsync(channelGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetChannelAsync(channelGuid, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = (OkObjectResult)result.Result!;
            Assert.That(okResult.Value, Is.EqualTo(expected));
        });

        _channelCrudServiceMock.Verify(x => x.GetEntityAsync(channelGuid, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task PostChannelAsync_ReturnsCreatedAtRoute_WhenGuidIsGenerated()
    {
        var request = BuildChannelInput("My Channel", "my-channel", "https://my.example.com");
        var created = new ChannelOutput
        {
            General = new SiteGeneralOutput
            {
                ID = 101,
                GUID = Guid.NewGuid(),
                DisplayName = "My Channel",
                CodeName = "my-channel",
                WebsitedomainresentationURL = "my.example.com"
            }
        };

        _channelCrudServiceMock
            .Setup(x => x.CreateEntityAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        var result = await _controller.PostChannelAsync(request, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<CreatedAtRouteResult>());
            var createdResult = (CreatedAtRouteResult)result.Result!;
            Assert.That(createdResult.RouteName, Is.EqualTo("GetChannel"));
            Assert.That(createdResult.RouteValues?["channelGuid"], Is.EqualTo(created.General.GUID));
            Assert.That(createdResult.Value, Is.EqualTo(created));
        });
    }

    [Test]
    public async Task PostChannelAsync_ReturnsBadRequest_WhenGuidIsEmpty()
    {
        var request = BuildChannelInput("Bad Channel", "bad-channel", "https://bad.example.com");
        var created = new ChannelOutput
        {
            General = new SiteGeneralOutput
            {
                ID = 55,
                GUID = Guid.Empty,
                DisplayName = "Bad Channel",
                CodeName = "bad-channel",
                WebsitedomainresentationURL = "bad.example.com"
            }
        };

        _channelCrudServiceMock
            .Setup(x => x.CreateEntityAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        var result = await _controller.PostChannelAsync(request, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequest = (BadRequestObjectResult)result.Result!;
            Assert.That(badRequest.Value, Is.EqualTo("Channel GUID was not generated."));
        });
    }

    [Test]
    public async Task PutChannelAsync_ReturnsOk_WithUpdatedChannel()
    {
        var channelGuid = Guid.NewGuid();
        var request = BuildChannelInput("Updated", "updated", "https://updated.example.com");
        var updated = new ChannelOutput
        {
            General = new SiteGeneralOutput
            {
                ID = 77,
                GUID = channelGuid,
                DisplayName = "Updated",
                CodeName = "updated",
                WebsitedomainresentationURL = "updated.example.com"
            }
        };

        _channelCrudServiceMock
            .Setup(x => x.UpdateEntityAsync(channelGuid, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updated);

        var result = await _controller.PutChannelAsync(channelGuid, request, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = (OkObjectResult)result.Result!;
            Assert.That(okResult.Value, Is.EqualTo(updated));
        });

        _channelCrudServiceMock.Verify(x => x.UpdateEntityAsync(channelGuid, request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task DeleteChannelAsync_ReturnsNoContent_AndCallsService()
    {
        var channelGuid = Guid.NewGuid();

        var result = await _controller.DeleteChannelAsync(channelGuid, CancellationToken.None);

        Assert.That(result, Is.InstanceOf<NoContentResult>());
        _channelCrudServiceMock.Verify(x => x.DeleteEntityAsync(channelGuid, It.IsAny<CancellationToken>()), Times.Once);
    }

    private static ChannelInput BuildChannelInput(string displayName, string codeName, string presentationUrl)
    {
        return new ChannelInput
        {
            General = new ChannelGeneralInput
            {
                DisplayName = displayName,
                CodeName = codeName,
                PresentationURL = presentationUrl,
                DefaultContentCulture = "en-GB"
            },
            DomainAliases = new ChannelDomainAliasesInput
            {
                LiveSite =
                [
                    new ChannelDomainAliasInput { PresentationURL = presentationUrl }
                ]
            },
            Cultures =
            [
                new ChannelCultureInput { Code = "en-GB" }
            ]
        };
    }
}
