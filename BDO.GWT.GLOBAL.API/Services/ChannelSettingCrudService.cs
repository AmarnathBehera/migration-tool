using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Queries;
using CMS.Base;
using CMS.DataEngine;

namespace BDO.GWT.GLOBAL.API.Services;

public interface IChannelSettingCrudService
{
    Task<ChannelSettingOutput[]> GetChannelSettings(SettingsQuery settingsQuery, CancellationToken cancellationToken);

    Task<ChannelSettingOutput[]?> GetChannelSetting(string codeName, CancellationToken cancellationToken);
}

public class ChannelSettingCrudService : IChannelSettingCrudService
{
    private readonly IInfoProvider<WebChannelSnippetInfo> _webChannelSnippetInfoProvider;

    public ChannelSettingCrudService(IInfoProvider<WebChannelSnippetInfo> webChannelSnippetInfoProvider)
    {
        _webChannelSnippetInfoProvider = webChannelSnippetInfoProvider;
    }

    public Task<ChannelSettingOutput[]> GetChannelSettings(SettingsQuery settingsQuery, CancellationToken cancellationToken)
    {
        if (settingsQuery.DisplayName is not null &&
            !string.Equals(settingsQuery.DisplayName, "Include Custom Scripts", StringComparison.Ordinal))
        {
            return Task.FromResult(Array.Empty<ChannelSettingOutput>());
        }

        return Task.FromResult(_webChannelSnippetInfoProvider.Get().GetEnumerableTypedResult()
            .Select(ToChannelSettingOutput).ToArray());
    }

    public Task<ChannelSettingOutput[]?> GetChannelSetting(string codeName, CancellationToken cancellationToken)
    {
        if (string.Equals(codeName, "BDO.WebChannelSnippet", StringComparison.Ordinal))
        {
            return Task.FromResult<ChannelSettingOutput[]?>(_webChannelSnippetInfoProvider.Get()
                .GetEnumerableTypedResult().Select(ToChannelSettingOutput).ToArray());
        }

        return Task.FromResult<ChannelSettingOutput[]?>(null);
    }

    private static ChannelSettingOutput ToChannelSettingOutput(WebChannelSnippetInfo setting) =>
        new() { CodeName = WebChannelSnippetInfo.OBJECT_TYPE, DisplayName = "Include Custom Scripts", InheritFromGlobalSettings = false, Type = nameof(WebChannelSnippetInfo), Value = ToFieldDictionary(setting) };

    private static Dictionary<string, object> ToFieldDictionary(IDataContainer container) =>
        container.ColumnNames.ToDictionary(columnName => columnName, columnName => container.GetValue(columnName));
}
