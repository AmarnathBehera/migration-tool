using BDO.GWT.Core.Services;
using CMS.DataEngine;
using Moq;

namespace BDO.GWT.Core.Tests.Services;

public class SettingServiceTests
{
    private Mock<IInfoProvider<GlobalsettingskeyInfo>> _globalMock = null!;
    private Mock<IInfoProvider<PrintorShareInfo>> _printMock = null!;
    private Mock<IInfoProvider<FilteringInfo>> _filterMock = null!;
    private Mock<IInfoProvider<OurPeopleInfo>> _peopleMock = null!;
    private Mock<IInfoProvider<WebChannelSettingsInfo>> _channelMock = null!;
    private Mock<IContentService> _contentMock = null!;

    private SettingService _service = null!;

    // NOTE: We cannot use an object initializer (i.e. `new WebChannelSettingsInfo { WebChannelSettingsID = ... }`)
    // because the property setters call AbstractInfoBase.SetValue, which lazily resolves the
    // Kentico DataClass through the CMS IoC container. The container is not initialized in unit
    // tests, so any setter call throws ServiceResolutionException. Instead, use a test-only
    // subclass that overrides the virtual setters with simple auto-properties, bypassing SetValue.
    private sealed class TestWebChannelSettingsInfo : WebChannelSettingsInfo
    {
        public override int WebChannelSettingsID { get; set; }
    }

    [SetUp]
    public void Setup()
    {
        _globalMock = new Mock<IInfoProvider<GlobalsettingskeyInfo>>();
        _printMock = new Mock<IInfoProvider<PrintorShareInfo>>();
        _filterMock = new Mock<IInfoProvider<FilteringInfo>>();
        _peopleMock = new Mock<IInfoProvider<OurPeopleInfo>>();
        _channelMock = new Mock<IInfoProvider<WebChannelSettingsInfo>>();
        _contentMock = new Mock<IContentService>();

        _contentMock.Setup(x => x.GetCurrentChannelID())
            .Returns(1);

        _service = new SettingService(
            _globalMock.Object,
            _printMock.Object,
            _filterMock.Object,
            _peopleMock.Object,
            _channelMock.Object,
            _contentMock.Object
        );
    }

    private void SetupChannel(WebChannelSettingsInfo? channel)
    {
        var queryMock = new Mock<ObjectQuery<WebChannelSettingsInfo>>();

        _channelMock.Setup(x => x.Get())
            .Returns(queryMock.Object);

        queryMock.Setup(x => x.WhereEquals(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .Returns(queryMock.Object);

        queryMock.Setup(x => x.TopN(It.IsAny<int>()))
            .Returns(queryMock.Object);

        queryMock.Setup(x => x.GetEnumerableTypedResultAsync())
            .ReturnsAsync(channel != null
                ? new List<WebChannelSettingsInfo> { channel }
                : new List<WebChannelSettingsInfo>());
    }

    [Test]
    public async Task GetPrintorShareInfo_WhenChannelNull_ReturnsNull()
    {
        SetupChannel(null);

        var result = await _service.GetPrintorShareInfo();

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetPrintorShareInfo_WhenDataExists_ReturnsData()
    {
        var channel = new TestWebChannelSettingsInfo
        {
            WebChannelSettingsID = 10
        };

        SetupChannel(channel);

        var data = new List<PrintorShareInfo>
        {
            new PrintorShareInfo()
        };

        var queryMock = new Mock<ObjectQuery<PrintorShareInfo>>();

        _printMock.Setup(x => x.Get())
            .Returns(queryMock.Object);

        queryMock.Setup(x => x.WhereEquals(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(queryMock.Object);

        queryMock.Setup(x => x.GetEnumerableTypedResultAsync())
            .ReturnsAsync(data);

        var result = await _service.GetPrintorShareInfo();

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetFilteringInfo_WhenChannelNull_ReturnsNull()
    {
        SetupChannel(null);

        var result = await _service.GetFilteringInfo();

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetFilteringInfo_WhenDataExists_ReturnsData()
    {
        var channel = new TestWebChannelSettingsInfo { WebChannelSettingsID = 5 };

        SetupChannel(channel);

        var queryMock = new Mock<ObjectQuery<FilteringInfo>>();

        _filterMock.Setup(x => x.Get())
            .Returns(queryMock.Object);

        queryMock.Setup(x => x.WhereEquals(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(queryMock.Object);

        queryMock.Setup(x => x.GetEnumerableTypedResultAsync())
            .ReturnsAsync(new List<FilteringInfo> { new FilteringInfo() });

        var result = await _service.GetFilteringInfo();

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Any(), Is.True);
    }

    [Test]
    public async Task GetUsePersonPageTitleInfo_WhenChannelNull_ReturnsNull()
    {
        SetupChannel(null);

        var result = await _service.GetUsePersonPageTitleInfo();

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetUsePersonPageTitleInfo_WhenDataExists_ReturnsData()
    {
        var channel = new TestWebChannelSettingsInfo { WebChannelSettingsID = 99 };

        SetupChannel(channel);

        var queryMock = new Mock<ObjectQuery<OurPeopleInfo>>();

        _peopleMock.Setup(x => x.Get())
            .Returns(queryMock.Object);

        queryMock.Setup(x => x.WhereEquals(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(queryMock.Object);

        queryMock.Setup(x => x.GetEnumerableTypedResultAsync())
            .ReturnsAsync(new List<OurPeopleInfo> { new OurPeopleInfo() });

        var result = await _service.GetUsePersonPageTitleInfo();

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Count(), Is.EqualTo(1));
    }
}