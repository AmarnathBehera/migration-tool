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

    // --- Snippet lookup + Merge (documented limitation) --------------------------
    //
    // The snippet-found path and the Merge(...) projection cannot be exercised as unit
    // tests. The service resolves query results via Kentico's GetEnumerableTypedResultAsync,
    // which is an EXTENSION method (not virtual) on ObjectQuery<T>; Moq cannot intercept it,
    // so a mocked query cannot be made to yield a populated list. As a result the mocked
    // channel-settings chain always resolves to "no channel setting", and execution returns
    // before reaching the snippet lookup or Merge.
    //
    // Additionally, Merge builds a `new WebChannelSnippetInfo { ... }`, whose DatabaseField
    // setters route through AbstractInfoBase.SetValue -> the CMS IoC container, which is not
    // initialized in unit tests. These paths therefore require Kentico integration tests.

    /// <summary>
    /// Verifies the pure Merge behaviour that IS reachable in isolation: when there is no
    /// global settings row, Merge returns the channel snippet unchanged (its early
    /// `if (global == null) return snippet;` branch), without constructing a merged object.
    /// Info subclasses provide plain backing storage so no CMS infrastructure is touched.
    /// </summary>
    [Test]
    public void Merge_ReturnsSnippetUnchanged_WhenGlobalIsNull()
    {
        var snippet = new TestWebChannelSnippetInfo
        {
            WebChannelSnippetID = 42,
            WebChannelSnippetWebChannelSettingsID = 5,
            FromEmail = "channel@example.com",
        };

        var merge = typeof(SettingService).GetMethod(
            "Merge",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

        Assert.That(merge, Is.Not.Null, "Private static Merge(WebChannelSnippetInfo, GlobalsettingskeyInfo?) not found.");

        var result = merge!.Invoke(null, [snippet, null]);

        Assert.That(result, Is.SameAs(snippet));
    }

    private sealed class TestWebChannelSnippetInfo : WebChannelSnippetInfo
    {
        public override int WebChannelSnippetID { get; set; }
        public override int WebChannelSnippetWebChannelSettingsID { get; set; }
        public override string FromEmail { get; set; } = string.Empty;
    }

}
