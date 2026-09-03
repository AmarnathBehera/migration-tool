using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.Services;
using Moq;

namespace BDO.GWT.Core.Tests.Helpers;

public class FilteringHelperTests
{
    private readonly Mock<ISettingService> _settingServiceMock;
    private readonly FilteringHelper _filteringHelper;

    public FilteringHelperTests()
    {
        _settingServiceMock = new Mock<ISettingService>();
        _filteringHelper = new FilteringHelper(_settingServiceMock.Object);
    }

    [Test]
    public async Task IsFilteringTooltipEnabledAsync_WhenFilteringDisabled_ReturnsFalse()
    {
        var result = await _filteringHelper.IsFilteringTooltipEnabledAsync(false, CancellationToken.None);

        Assert.That(result, Is.False);
        _settingServiceMock.Verify(x => x.GetFilteringInfo(), Times.Never);
    }

    [Test]
    public async Task IsFilteringTooltipEnabledAsync_WhenFilteringInfoIsNull_ReturnsFalse()
    {
        _settingServiceMock
            .Setup(x => x.GetFilteringInfo())
            .ReturnsAsync((IEnumerable<WebChannelSnippetInfo>?)null);

        var result = await _filteringHelper.IsFilteringTooltipEnabledAsync(true, CancellationToken.None);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task IsFilteringTooltipEnabledAsync_WhenFilteringInfoIsEmpty_ReturnsFalse()
    {
        _settingServiceMock
            .Setup(x => x.GetFilteringInfo())
            .ReturnsAsync(new List<WebChannelSnippetInfo>());

        var result = await _filteringHelper.IsFilteringTooltipEnabledAsync(true, CancellationToken.None);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task IsFilteringTooltipEnabledAsync_WhenFilteringEnabled_CallsSettingService()
    {
        _settingServiceMock
            .Setup(x => x.GetFilteringInfo())
            .ReturnsAsync((IEnumerable<WebChannelSnippetInfo>?)null);

        await _filteringHelper.IsFilteringTooltipEnabledAsync(true, CancellationToken.None);

        _settingServiceMock.Verify(x => x.GetFilteringInfo(), Times.Once);
    }
}
