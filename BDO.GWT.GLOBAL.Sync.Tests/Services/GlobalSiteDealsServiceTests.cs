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
