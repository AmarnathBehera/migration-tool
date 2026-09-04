using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Queries;
using CMS.Base;
using CMS.DataEngine;

namespace BDO.GWT.GLOBAL.API.Services;

public interface IGlobalSettingCrudService
{
    Task<GlobalSettingOutput[]> GetGlobalSettings(SettingsQuery settingsQuery, CancellationToken cancellationToken);
    Task<GlobalSettingOutput?> GetGlobalSetting(string settingCodeName, CancellationToken cancellationToken);
}

public class GlobalSettingCrudService : IGlobalSettingCrudService
{
    private readonly IInfoProvider<GlobalsettingskeyInfo> _customScriptinfoProvider;

    public GlobalSettingCrudService(IInfoProvider<GlobalsettingskeyInfo> customScriptinfoProvider)
    {
        _customScriptinfoProvider = customScriptinfoProvider;
    }

    public Task<GlobalSettingOutput[]> GetGlobalSettings(SettingsQuery settingsQuery, CancellationToken cancellationToken)
    {
        var globalSettings = _customScriptinfoProvider.Get().GetEnumerableTypedResult();

        if (settingsQuery.DisplayName is not null &&
            settingsQuery.DisplayName is not ("Include Custom Scripts" or "Cookiebot" or "Events & Webinar"))
        {
            return Task.FromResult(System.Array.Empty<GlobalSettingOutput>());
        }

        return Task.FromResult(globalSettings.Select(ToGlobalSettingOutput).ToArray());
    }

    public Task<GlobalSettingOutput?> GetGlobalSetting(string settingCodeName, CancellationToken cancellationToken)
    {
        if (settingCodeName is "BDO.Globalsettingskey" or "BDO.Cookiebot" or "BDO.GlobalEventsnWebinars")
        {
            var globalSettings = _customScriptinfoProvider.Get().GetEnumerableTypedResult();
            return Task.FromResult(globalSettings.Select(ToGlobalSettingOutput).FirstOrDefault());
        }

        return Task.FromResult<GlobalSettingOutput?>(null);
    }

    private static GlobalSettingOutput ToGlobalSettingOutput(GlobalsettingskeyInfo setting)
    {
        return new GlobalSettingOutput
        {
            CodeName = GlobalsettingskeyInfo.OBJECT_TYPE,
            DisplayName = "Include Custom Scripts",
            Value = ToFieldDictionary(setting)
        };
    }

    private static Dictionary<string, object> ToFieldDictionary(IDataContainer container)
    {
        return container.ColumnNames
            .ToDictionary(columnName => columnName, columnName => container.GetValue(columnName));
    }
}