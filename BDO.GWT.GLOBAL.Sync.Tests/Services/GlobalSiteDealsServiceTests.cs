using BDO.GWT.GLOBAL.Sync.Data;
using BDO.GWT.GLOBAL.Sync.Services;
using CMS.ContentEngine;
using CMS.Membership;
using CMS.Websites;
using Microsoft.Extensions.Logging;
using Moq;

namespace BDO.GWT.GLOBAL.Sync.Tests.Services;

[TestFixture]
public class GlobalSiteDealsServiceTests
{
    private Mock<IContentQueryExecutor> _contentQueryExecutorMock = null!;
    private Mock<IWebPageManagerFactory> _webPageManagerFactoryMock = null!;
    private Mock<IUserInfoProvider> _userInfoProviderMock = null!;
    private Mock<ILogger<GlobalSiteDealsService>> _loggerMock = null!;
    private GlobalSiteDealsService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _contentQueryExecutorMock = new Mock<IContentQueryExecutor>();
        _webPageManagerFactoryMock = new Mock<IWebPageManagerFactory>();
        _userInfoProviderMock = new Mock<IUserInfoProvider>();
        _loggerMock = new Mock<ILogger<GlobalSiteDealsService>>();

        _service = new GlobalSiteDealsService(
            _contentQueryExecutorMock.Object,
            _webPageManagerFactoryMock.Object,
            _userInfoProviderMock.Object,
            _loggerMock.Object);
    }

    [Test]
    public async Task SynchronizeChannelGlobalDealsAsync_WhenSourceChannelsEmpty_ReturnsWithoutAnyWork()
    {
        var globalChannel = CreateChannel(Guid.NewGuid(), "global");

        await _service.SynchronizeChannelGlobalDealsAsync(
            globalChannel,
            Array.Empty<ChannelOutput>(),
            forceUpdate: false,
            CancellationToken.None);

        _webPageManagerFactoryMock.VerifyNoOtherCalls();
        _contentQueryExecutorMock.VerifyNoOtherCalls();
        _userInfoProviderMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task SynchronizeChannelGlobalDealsAsync_WhenSourceChannelsEmpty_LogsInformation()
    {
        var globalChannel = CreateChannel(Guid.NewGuid(), "global");

        await _service.SynchronizeChannelGlobalDealsAsync(
            globalChannel,
            Array.Empty<ChannelOutput>(),
            forceUpdate: false,
            CancellationToken.None);

        VerifyLog(LogLevel.Information, Times.Once());
    }

    [Test]
    public async Task SynchronizeChannelGlobalDealsAsync_WhenAdminUserNotFound_ReturnsWithoutCreatingManager()
    {
        var globalChannel = CreateChannel(Guid.NewGuid(), "global");
        var sourceChannel = CreateChannel(Guid.NewGuid(), "source");

        _userInfoProviderMock
            .Setup(p => p.Get(It.IsAny<string>()))
            .Returns((UserInfo)null!);

        await _service.SynchronizeChannelGlobalDealsAsync(
            globalChannel,
            new[] { sourceChannel },
            forceUpdate: false,
            CancellationToken.None);

        _userInfoProviderMock.Verify(p => p.Get("administrator"), Times.Once);
        _webPageManagerFactoryMock.Verify(
            f => f.Create(It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);
        _contentQueryExecutorMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task SynchronizeChannelGlobalDealsAsync_WhenAdminUserNotFound_LogsError()
    {
        var globalChannel = CreateChannel(Guid.NewGuid(), "global");
        var sourceChannel = CreateChannel(Guid.NewGuid(), "source");

        _userInfoProviderMock
            .Setup(p => p.Get(It.IsAny<string>()))
            .Returns((UserInfo)null!);

        await _service.SynchronizeChannelGlobalDealsAsync(
            globalChannel,
            new[] { sourceChannel },
            forceUpdate: false,
            CancellationToken.None);

        VerifyLog(LogLevel.Error, Times.Once());
    }

    [Test]
    public async Task SynchronizeChannelGlobalDealsAsync_WhenCancellationRequested_StillHandlesEmptySourceChannels()
    {
        var globalChannel = CreateChannel(Guid.NewGuid(), "global");
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await _service.SynchronizeChannelGlobalDealsAsync(
            globalChannel,
            Array.Empty<ChannelOutput>(),
            forceUpdate: false,
            cts.Token);

        _webPageManagerFactoryMock.VerifyNoOtherCalls();
        _userInfoProviderMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task SynchronizeChannelGlobalDealsAsync_WithForceUpdateTrue_AndEmptySources_StillReturnsEarly()
    {
        var globalChannel = CreateChannel(Guid.NewGuid(), "global");

        await _service.SynchronizeChannelGlobalDealsAsync(
            globalChannel,
            Array.Empty<ChannelOutput>(),
            forceUpdate: true,
            CancellationToken.None);

        _webPageManagerFactoryMock.VerifyNoOtherCalls();
        _userInfoProviderMock.VerifyNoOtherCalls();
    }

    [Test]
    public void Service_ImplementsIGlobalSiteDealsService()
    {
        Assert.That(_service, Is.InstanceOf<IGlobalSiteDealsService>());
    }

    [Test]
    public void Constructor_WithValidDependencies_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => new GlobalSiteDealsService(
            _contentQueryExecutorMock.Object,
            _webPageManagerFactoryMock.Object,
            _userInfoProviderMock.Object,
            _loggerMock.Object));
    }

    #pragma warning disable CA1873 // Moq Verify expression, not a real log call
        private void VerifyLog(LogLevel level, Times times) =>
            _loggerMock.Verify(
                l => l.Log(
                    level,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                times);
    #pragma warning restore CA1873

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
