using BDO.GWT.GLOBAL.Sync.Data;
using BDO.GWT.GLOBAL.Sync.Options;
using BDO.GWT.GLOBAL.Sync.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;

namespace BDO.GWT.GLOBAL.Sync.Tests.Services;

[TestFixture]
public class GlobalDealsSynchronizerTests
{
    private static readonly Guid GlobalSiteGuid = Guid.NewGuid();

    private Mock<IChannelDirectoryService> _channelDirectoryMock = null!;
    private Mock<IGlobalSiteDealsService> _globalSiteDealsMock = null!;
    private Mock<IServiceScopeFactory> _scopeFactoryMock = null!;
    private GlobalDealsSynchronizer _synchronizer = null!;

    [SetUp]
    public void SetUp()
    {
        _channelDirectoryMock = new Mock<IChannelDirectoryService>();
        _globalSiteDealsMock = new Mock<IGlobalSiteDealsService>();

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IChannelDirectoryService)))
            .Returns(_channelDirectoryMock.Object);
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IGlobalSiteDealsService)))
            .Returns(_globalSiteDealsMock.Object);

        var scopeMock = new Mock<IServiceScope>();
        scopeMock.SetupGet(s => s.ServiceProvider).Returns(serviceProviderMock.Object);

        _scopeFactoryMock = new Mock<IServiceScopeFactory>();
        _scopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);

        var options = Microsoft.Extensions.Options.Options.Create(new SyncAppOptions
        {
            SyncApiKey = "test-key",
            Regions = Array.Empty<Region>(),
            GlobalSiteGuid = GlobalSiteGuid,
            RegionSitesCacheDuration = TimeSpan.FromMinutes(5),
        });

        _synchronizer = new GlobalDealsSynchronizer(options, _scopeFactoryMock.Object);
    }

    [Test]
    public async Task SynchronizeGlobalDealsAsync_WhenGlobalChannelIsNull_DoesNotSynchronize()
    {
        _channelDirectoryMock
            .Setup(c => c.GetChannelAsync(GlobalSiteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChannelOutput?)null);

        await _synchronizer.SynchronizeGlobalDealsAsync(forceUpdate: false, CancellationToken.None);

        _channelDirectoryMock.Verify(
            c => c.GetAllChannelsExceptAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _globalSiteDealsMock.Verify(
            s => s.SynchronizeChannelGlobalDealsAsync(
                It.IsAny<ChannelOutput>(),
                It.IsAny<IReadOnlyCollection<ChannelOutput>>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task SynchronizeGlobalDealsAsync_WhenSourceChannelsEmpty_DoesNotSynchronize()
    {
        var globalChannel = CreateChannel(GlobalSiteGuid, "global");

        _channelDirectoryMock
            .Setup(c => c.GetChannelAsync(GlobalSiteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(globalChannel);
        _channelDirectoryMock
            .Setup(c => c.GetAllChannelsExceptAsync(GlobalSiteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ChannelOutput>());

        await _synchronizer.SynchronizeGlobalDealsAsync(forceUpdate: true, CancellationToken.None);

        _globalSiteDealsMock.Verify(
            s => s.SynchronizeChannelGlobalDealsAsync(
                It.IsAny<ChannelOutput>(),
                It.IsAny<IReadOnlyCollection<ChannelOutput>>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task SynchronizeGlobalDealsAsync_WhenValidInput_DelegatesToGlobalSiteDealsService()
    {
        var globalChannel = CreateChannel(GlobalSiteGuid, "global");
        var sourceChannels = new List<ChannelOutput>
        {
            CreateChannel(Guid.NewGuid(), "channel-a"),
            CreateChannel(Guid.NewGuid(), "channel-b"),
        };

        _channelDirectoryMock
            .Setup(c => c.GetChannelAsync(GlobalSiteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(globalChannel);
        _channelDirectoryMock
            .Setup(c => c.GetAllChannelsExceptAsync(GlobalSiteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sourceChannels);

        await _synchronizer.SynchronizeGlobalDealsAsync(forceUpdate: true, CancellationToken.None);

        _globalSiteDealsMock.Verify(
            s => s.SynchronizeChannelGlobalDealsAsync(
                globalChannel,
                sourceChannels,
                true,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task SynchronizeGlobalDealsAsync_CreatesScopeFromFactory()
    {
        _channelDirectoryMock
            .Setup(c => c.GetChannelAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChannelOutput?)null);

        await _synchronizer.SynchronizeGlobalDealsAsync(false, CancellationToken.None);

        _scopeFactoryMock.Verify(f => f.CreateScope(), Times.Once);
    }

    [Test]
    public async Task SynchronizeGlobalDealsAsync_ForwardsForceUpdateFalse()
    {
        var globalChannel = CreateChannel(GlobalSiteGuid, "global");
        var sourceChannels = new List<ChannelOutput> { CreateChannel(Guid.NewGuid(), "channel-a") };

        _channelDirectoryMock
            .Setup(c => c.GetChannelAsync(GlobalSiteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(globalChannel);
        _channelDirectoryMock
            .Setup(c => c.GetAllChannelsExceptAsync(GlobalSiteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sourceChannels);

        await _synchronizer.SynchronizeGlobalDealsAsync(forceUpdate: false, CancellationToken.None);

        _globalSiteDealsMock.Verify(
            s => s.SynchronizeChannelGlobalDealsAsync(
                globalChannel, sourceChannels, false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task SynchronizeGlobalDealsAsync_PropagatesCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _channelDirectoryMock
            .Setup(c => c.GetChannelAsync(GlobalSiteGuid, token))
            .ReturnsAsync((ChannelOutput?)null);

        await _synchronizer.SynchronizeGlobalDealsAsync(false, token);

        _channelDirectoryMock.Verify(c => c.GetChannelAsync(GlobalSiteGuid, token), Times.Once);
    }

    [Test]
    public void SynchronizeGlobalDealsAsync_WhenChannelDirectoryThrows_PropagatesException()
    {
        var expected = new InvalidOperationException("boom");
        _channelDirectoryMock
            .Setup(c => c.GetChannelAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expected);

        var actual = Assert.ThrowsAsync<InvalidOperationException>(
            () => _synchronizer.SynchronizeGlobalDealsAsync(false, CancellationToken.None));

        Assert.That(actual, Is.SameAs(expected));
    }

    private static ChannelOutput CreateChannel(Guid guid, string codeName) => new()
    {
        General = new SiteGeneralOutput
        {
            ID = 1,
            GUID = guid,
            DisplayName = codeName,
            CodeName = codeName,
            WebsitedomainresentationURL = $"https://{codeName}.example.com",
        },
    };
}
