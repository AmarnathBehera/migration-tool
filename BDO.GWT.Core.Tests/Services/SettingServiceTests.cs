using BDO.GWT.Core.Services;
using CMS.DataEngine;
using CMS.Websites.Routing;
using Moq;

namespace BDO.GWT.Core.Tests.Services;

public class SettingServiceTests
{
    private Mock<IInfoProvider<GlobalsettingskeyInfo>> _globalMock = null!;
    private Mock<IInfoProvider<WebChannelSnippetInfo>> _snippetMock = null!;
    private Mock<IWebsiteChannelContext> _websiteChannelContextMock = null!;
    private Mock<IInfoProvider<WebChannelSettingsInfo>> _channelMock = null!;
    private Mock<IContentService> _contentMock = null!;

    private SettingService _service = null!;

    [SetUp]
    public void Setup()
    {
        _globalMock = new Mock<IInfoProvider<GlobalsettingskeyInfo>>();
        _snippetMock = new Mock<IInfoProvider<WebChannelSnippetInfo>>();
        _websiteChannelContextMock = new Mock<IWebsiteChannelContext>();
        _channelMock = new Mock<IInfoProvider<WebChannelSettingsInfo>>();
        _contentMock = new Mock<IContentService>();

        _contentMock.Setup(x => x.GetCurrentChannelID()).Returns(1);

        _service = new SettingService(
            _channelMock.Object,
            _websiteChannelContextMock.Object,
            _globalMock.Object,
            _snippetMock.Object,
            _contentMock.Object);
    }

    // NOTE: Tests here would require mocking ObjectQuery<T> across several types.
    // The provider-level plumbing is exercised by higher-level service tests that
    // mock ISettingService directly, so no unit tests are kept here for the
    // reworked SettingService.
    [Test]
    public void SettingService_CanBeConstructed()
    {
        Assert.That(_service, Is.Not.Null);
    }

    // --- Helpers -----------------------------------------------------------------

    private static Mock<ObjectQuery<T>> BuildQuery<T>(IEnumerable<T> results)
        where T : BaseInfo
    {
        var queryMock = new Mock<ObjectQuery<T>>();

        queryMock.Setup(q => q.WhereEquals(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(queryMock.Object);
        queryMock.Setup(q => q.TopN(It.IsAny<int>()))
            .Returns(queryMock.Object);
        queryMock.Setup(q => q.GetEnumerableTypedResultAsync(
                It.IsAny<System.Data.CommandBehavior>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(results.ToList());

        return queryMock;
    }

    private void SetupChannelSettingsQuery(IEnumerable<WebChannelSettingsInfo> results) =>
        _channelMock.Setup(p => p.Get()).Returns(BuildQuery(results).Object);

    // --- GetPrintorShareInfo / GetFilteringInfo / GetUsePersonPageTitleInfo -------

    [Test]
    public async Task GetPrintorShareInfo_ReturnsNull_WhenNoChannelSettingFound()
    {
        SetupChannelSettingsQuery([]);

        var result = await _service.GetPrintorShareInfo();

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetFilteringInfo_ReturnsNull_WhenNoChannelSettingFound()
    {
        SetupChannelSettingsQuery([]);

        var result = await _service.GetFilteringInfo();

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetUsePersonPageTitleInfo_ReturnsNull_WhenNoChannelSettingFound()
    {
        SetupChannelSettingsQuery([]);

        var result = await _service.GetUsePersonPageTitleInfo();

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetPrintorShareInfo_UsesCurrentChannelIdFromContentService()
    {
        SetupChannelSettingsQuery([]);

        await _service.GetPrintorShareInfo();

        _contentMock.Verify(x => x.GetCurrentChannelID(), Times.Once);
        _channelMock.Verify(p => p.Get(), Times.Once);
    }

    [Test]
    public void GetPrintorShareInfo_PropagatesException_FromChannelProvider()
    {
        _channelMock.Setup(p => p.Get())
            .Throws(new InvalidOperationException("db error"));

        Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetPrintorShareInfo());
    }

}
