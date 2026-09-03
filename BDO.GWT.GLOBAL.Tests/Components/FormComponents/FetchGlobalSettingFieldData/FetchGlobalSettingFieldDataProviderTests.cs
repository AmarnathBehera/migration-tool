using BDO.GWT.Core.Services;
using BDO.GWT.GLOBAL.Components.FormComponents.FetchGlobalSettingFieldData;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Components.FormComponents.FetchGlobalSettingFieldData;

public class FetchGlobalSettingFieldDataProviderTests
{
    // GlobalLocationInfo's setters call AbstractInfoBase.SetValue, which resolves the Kentico
    // DataClass through the CMS IoC container. That container isn't initialized in unit tests,
    // so a test-only subclass overrides the virtual setters with plain auto-properties.
    private sealed class TestGlobalLocationInfo : GlobalLocationInfo
    {
        public override string GlobalLocationsTitle { get; set; } = string.Empty;
        public override string GlobalLocationsDescription { get; set; } = string.Empty;
    }

    private Mock<ISettingService> _settingServiceMock = null!;
    private FetchGlobalSettingFieldDataProvider _provider = null!;

    [SetUp]
    public void Setup()
    {
        _settingServiceMock = new Mock<ISettingService>();
        _provider = new FetchGlobalSettingFieldDataProvider(_settingServiceMock.Object);
    }

    [Test]
    public async Task GetFieldValueAsync_WhenFieldNameIsNull_ReturnsEmpty()
    {
        var result = await _provider.GetFieldValueAsync(null!);

        Assert.That(result, Is.EqualTo(string.Empty));
    }

    [Test]
    public async Task GetFieldValueAsync_WhenFieldNameIsUnknown_ReturnsEmpty()
    {
        var result = await _provider.GetFieldValueAsync("SomeUnknownField");

        Assert.That(result, Is.EqualTo(string.Empty));
    }

    [Test]
    public async Task GetFieldValueAsync_WhenGlobalSettingIsMissing_ReturnsEmpty()
    {
        _settingServiceMock.Setup(s => s.GetGlobalSettingAsync()).ReturnsAsync((GlobalLocationInfo?)null);

        var result = await _provider.GetFieldValueAsync(nameof(GlobalLocationInfo.GlobalLocationsTitle));

        Assert.That(result, Is.EqualTo(string.Empty));
    }

    [Test]
    public async Task GetFieldValueAsync_WhenFieldNameIsTitle_ReturnsTitle()
    {
        var globalLocationInfo = new TestGlobalLocationInfo
        {
            GlobalLocationsTitle = "Our locations",
            GlobalLocationsDescription = "Find a BDO office near you"
        };
        _settingServiceMock.Setup(s => s.GetGlobalSettingAsync()).ReturnsAsync(globalLocationInfo);

        var result = await _provider.GetFieldValueAsync(nameof(GlobalLocationInfo.GlobalLocationsTitle));

        Assert.That(result, Is.EqualTo("Our locations"));
    }

    [Test]
    public async Task GetFieldValueAsync_WhenFieldNameIsDescription_ReturnsDescription()
    {
        var globalLocationInfo = new TestGlobalLocationInfo
        {
            GlobalLocationsTitle = "Our locations",
            GlobalLocationsDescription = "Find a BDO office near you"
        };
        _settingServiceMock.Setup(s => s.GetGlobalSettingAsync()).ReturnsAsync(globalLocationInfo);

        var result = await _provider.GetFieldValueAsync(nameof(GlobalLocationInfo.GlobalLocationsDescription));

        Assert.That(result, Is.EqualTo("Find a BDO office near you"));
    }

    [Test]
    public async Task GetFieldValueAsync_FieldNameIsCaseInsensitive()
    {
        var globalLocationInfo = new TestGlobalLocationInfo
        {
            GlobalLocationsTitle = "Our locations"
        };
        _settingServiceMock.Setup(s => s.GetGlobalSettingAsync()).ReturnsAsync(globalLocationInfo);

        var result = await _provider.GetFieldValueAsync("globallocationstitle");

        Assert.That(result, Is.EqualTo("Our locations"));
    }
}
