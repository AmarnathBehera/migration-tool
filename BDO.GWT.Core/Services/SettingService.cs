using CMS.DataEngine;

namespace BDO.GWT.Core.Services;

public interface ISettingService
{
    Task<IEnumerable<PrintorShareInfo>?> GetPrintorShareInfo();
}

public class SettingService : ISettingService
{
    public readonly IInfoProvider<PrintorShareInfo> _infoProvider;
    private readonly IInfoProvider<WebChannelSettingsInfo> _webChannelSettingsInfoProvider;
    private readonly IContentService _contentService;
    
    public SettingService(
        IInfoProvider<PrintorShareInfo> infoProvider,
        IInfoProvider<WebChannelSettingsInfo> webChannelSettingsInfoProvider,
        IContentService contentService)
    {
        _infoProvider = infoProvider;
        _webChannelSettingsInfoProvider = webChannelSettingsInfoProvider;
        _contentService = contentService;
    }
    
    public async Task<IEnumerable<PrintorShareInfo>?> GetPrintorShareInfo()
    {
        var currentChannelSettings = await _webChannelSettingsInfoProvider
            .Get()
            .WhereEquals(nameof(WebChannelSettingsInfo.WebChannelSettingsChannelID), _contentService.GetCurrentChannelID())
            .TopN(1)
            .GetEnumerableTypedResultAsync();

        var channelSetting = currentChannelSettings?.FirstOrDefault();

        if (channelSetting == null)
        {
            return null;
        }

        var printorShareData = await _infoProvider.Get()
            .WhereEquals(nameof(PrintorShareInfo.PnSSettingsWebChannelSettingID), channelSetting.WebChannelSettingsID)
            .GetEnumerableTypedResultAsync();

        return printorShareData;
    }
}
