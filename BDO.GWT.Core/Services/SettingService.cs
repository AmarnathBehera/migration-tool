using CMS.DataEngine;

namespace BDO.GWT.Core.Services;

public interface ISettingService
{
    Task<IEnumerable<PrintorShareInfo>?> GetPrintorShareInfo();
}

public class SettingService : ISettingService
{
    public readonly IInfoProvider<PrintorShareInfo> _PrintorShareinfoProvider;
    public readonly IInfoProvider<FilteringInfo> _FilteringinfoProvider;
    private readonly IInfoProvider<WebChannelSettingsInfo> _webChannelSettingsInfoProvider;
    private readonly IContentService _contentService;
    
    public SettingService(
        IInfoProvider<PrintorShareInfo> PrintorShareinfoProvider,
        IInfoProvider<FilteringInfo> FilteringinfoProvider,
    IInfoProvider<WebChannelSettingsInfo> webChannelSettingsInfoProvider,
        IContentService contentService)
    {
        _PrintorShareinfoProvider = PrintorShareinfoProvider;
        _FilteringinfoProvider = FilteringinfoProvider;
        _webChannelSettingsInfoProvider = webChannelSettingsInfoProvider;
        _contentService = contentService;
    }

    private async Task<WebChannelSettingsInfo?> GetCurrentChannelSettingAsync()
    {
        var currentChannelSettings = await _webChannelSettingsInfoProvider
            .Get()
            .WhereEquals(nameof(WebChannelSettingsInfo.WebChannelSettingsChannelID), _contentService.GetCurrentChannelID())
            .TopN(1)
            .GetEnumerableTypedResultAsync();

        return currentChannelSettings?.FirstOrDefault();
    }
    public async Task<IEnumerable<PrintorShareInfo>?> GetPrintorShareInfo()
    {
        var channelSetting = await GetCurrentChannelSettingAsync();

        if (channelSetting == null)
        {
            return null;
        }
        var printorShareData = await _PrintorShareinfoProvider.Get()
            .WhereEquals(nameof(PrintorShareInfo.PnSSettingsWebChannelSettingID), channelSetting.WebChannelSettingsID)
            .GetEnumerableTypedResultAsync();

        return printorShareData;
    }

    public async Task<IEnumerable<FilteringInfo>?> GetFilteringInfo()
    {
        var channelSetting = await GetCurrentChannelSettingAsync();

        if (channelSetting == null)
        {
            return null;
        }
        var filteringData = await _FilteringinfoProvider.Get()
            .WhereEquals(nameof(FilteringInfo.FilteringSettingsWebChannelSettingID), channelSetting.WebChannelSettingsID)
            .GetEnumerableTypedResultAsync();

        return filteringData;
    }
}
