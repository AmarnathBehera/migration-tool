using BDO.GWT.GLOBAL.Sync.Services;
using CMS.ContentEngine;
using CMS.DataEngine;
using Moq;

namespace BDO.GWT.GLOBAL.Sync.Tests.Services;

[TestFixture]
public class ChannelDirectoryServiceTests
{
    private Mock<IInfoProvider<ChannelInfo>> _channelProviderMock = null!;
    private ChannelDirectoryService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _channelProviderMock = new Mock<IInfoProvider<ChannelInfo>>();
        _service = new ChannelDirectoryService(_channelProviderMock.Object);
    }

    [Test]
    public void Constructor_WithValidDependencies_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => new ChannelDirectoryService(_channelProviderMock.Object));
    }

    [Test]
    public void Service_ImplementsIChannelDirectoryService()
    {
        Assert.That(_service, Is.InstanceOf<IChannelDirectoryService>());
    }

    [Test]
    public void GetChannelAsync_WhenProviderGetThrows_PropagatesException()
    {
        _channelProviderMock.Setup(p => p.Get()).Throws<InvalidOperationException>();

        Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetChannelAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Test]
    public void GetAllChannelsExceptAsync_WhenProviderGetThrows_PropagatesException()
    {
        _channelProviderMock.Setup(p => p.Get()).Throws<InvalidOperationException>();

        Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetAllChannelsExceptAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Test]
    public void GetChannelAsync_WithCancelledToken_CanBeInvoked()
    {
        _channelProviderMock.Setup(p => p.Get()).Throws<OperationCanceledException>();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.ThrowsAsync<OperationCanceledException>(
            () => _service.GetChannelAsync(Guid.NewGuid(), cts.Token));
    }

    [Test]
    public void GetAllChannelsExceptAsync_WithCancelledToken_CanBeInvoked()
    {
        _channelProviderMock.Setup(p => p.Get()).Throws<OperationCanceledException>();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.ThrowsAsync<OperationCanceledException>(
            () => _service.GetAllChannelsExceptAsync(Guid.NewGuid(), cts.Token));
    }
}
