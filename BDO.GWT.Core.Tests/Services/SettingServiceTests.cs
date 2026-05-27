using BDO.GWT.Core.Services;
using Moq;

namespace BDO.GWT.Core.Tests.Services;

public class SettingServiceTests
{
    private readonly Mock<ISettingService> _settingServiceMock;

    public SettingServiceTests()
    {
        _settingServiceMock = new Mock<ISettingService>();
    }

    [Test]
    public async Task GetPrintorShareInfo_WhenNoChannelSettings_ReturnsNull()
    {
        _settingServiceMock
            .Setup(x => x.GetPrintorShareInfo())
            .ReturnsAsync((IEnumerable<PrintorShareInfo>?)null);

        var result = await _settingServiceMock.Object.GetPrintorShareInfo();

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetPrintorShareInfo_WhenChannelSettingsExist_ReturnsEmptyCollection()
    {
        _settingServiceMock
            .Setup(x => x.GetPrintorShareInfo())
            .ReturnsAsync(new List<PrintorShareInfo>());

        var result = await _settingServiceMock.Object.GetPrintorShareInfo();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        });
    }

    [Test]
    public async Task GetPrintorShareInfo_InvokedOnce()
    {
        _settingServiceMock
            .Setup(x => x.GetPrintorShareInfo())
            .ReturnsAsync((IEnumerable<PrintorShareInfo>?)null);

        await _settingServiceMock.Object.GetPrintorShareInfo();

        _settingServiceMock.Verify(x => x.GetPrintorShareInfo(), Times.Once);
    }

    [Test]
    public async Task GetFilteringInfo_WhenNoChannelSettings_ReturnsNull()
    {
        _settingServiceMock
            .Setup(x => x.GetFilteringInfo())
            .ReturnsAsync((IEnumerable<FilteringInfo>?)null);

        var result = await _settingServiceMock.Object.GetFilteringInfo();

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetFilteringInfo_WhenChannelSettingsExist_ReturnsEmptyCollection()
    {
        _settingServiceMock
            .Setup(x => x.GetFilteringInfo())
            .ReturnsAsync(new List<FilteringInfo>());

        var result = await _settingServiceMock.Object.GetFilteringInfo();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        });
    }

    [Test]
    public async Task GetFilteringInfo_InvokedOnce()
    {
        _settingServiceMock
            .Setup(x => x.GetFilteringInfo())
            .ReturnsAsync((IEnumerable<FilteringInfo>?)null);

        await _settingServiceMock.Object.GetFilteringInfo();

        _settingServiceMock.Verify(x => x.GetFilteringInfo(), Times.Once);
    }

    [Test]
    public async Task GetUsePersonPageTitleInfo_WhenNoChannelSettings_ReturnsNull()
    {
        _settingServiceMock
            .Setup(x => x.GetUsePersonPageTitleInfo())
            .ReturnsAsync((IEnumerable<OurPeopleInfo>?)null);

        var result = await _settingServiceMock.Object.GetUsePersonPageTitleInfo();

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetUsePersonPageTitleInfo_WhenChannelSettingsExist_ReturnsEmptyCollection()
    {
        _settingServiceMock
            .Setup(x => x.GetUsePersonPageTitleInfo())
            .ReturnsAsync(new List<OurPeopleInfo>());

        var result = await _settingServiceMock.Object.GetUsePersonPageTitleInfo();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        });
    }

    [Test]
    public async Task GetUsePersonPageTitleInfo_InvokedOnce()
    {
        _settingServiceMock
            .Setup(x => x.GetUsePersonPageTitleInfo())
            .ReturnsAsync((IEnumerable<OurPeopleInfo>?)null);

        await _settingServiceMock.Object.GetUsePersonPageTitleInfo();

        _settingServiceMock.Verify(x => x.GetUsePersonPageTitleInfo(), Times.Once);
    }
}
