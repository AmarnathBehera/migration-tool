using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Queries;
using CMS.Base;
using CMS.DataEngine;

namespace BDO.GWT.GLOBAL.API.Services;

public interface IGlobalSettingCrudService
{
    Task<GlobalSettingOutput[]> GetGlobalSettings(SettingsQuery settingsQuery, CancellationToken cancellationToken);
    Task<GlobalSettingOutput> GetGlobalSetting(string settingCodeName, CancellationToken cancellationToken);
}

public class GlobalSettingCrudService : IGlobalSettingCrudService
{
    private readonly IInfoProvider<GlobalsettingskeyInfo> _customScriptinfoProvider;
    private readonly IInfoProvider<CookiebotInfo> _cookiebotinfoProvider;
    private readonly IInfoProvider<GlobalEventsnWebinarsInfo> _globalEventsnWebinarinfoProvider;

    public GlobalSettingCrudService(
        IInfoProvider<GlobalsettingskeyInfo> customScriptinfoProvider,
        IInfoProvider<CookiebotInfo> cookiebotinfoProvider,
        IInfoProvider<GlobalEventsnWebinarsInfo> globalEventsnWebinarinfoProvider)
    {
        _customScriptinfoProvider = customScriptinfoProvider;
        _cookiebotinfoProvider = cookiebotinfoProvider;
        _globalEventsnWebinarinfoProvider = globalEventsnWebinarinfoProvider;
    }

    public async Task<GlobalSettingOutput[]> GetGlobalSettings(SettingsQuery settingsQuery, CancellationToken cancellationToken)
    {
        var queryGlobalsettingskeyInfo = _customScriptinfoProvider.Get();
        var queryCookiebotInfo = _cookiebotinfoProvider.Get();
        var queryGlobalEventsnWebinarinfo = _globalEventsnWebinarinfoProvider.Get();

        if (settingsQuery.DisplayName is not null)
        {
            if (settingsQuery.DisplayName == "Include Custom Scripts")
            {
                var globalSettings = queryGlobalsettingskeyInfo.GetEnumerableTypedResult();
                return globalSettings.Select(ToGlobalSettingOutput).ToArray();
            }
            else if (settingsQuery.DisplayName == "Cookiebot")
            {
                var cookiebots = queryCookiebotInfo.GetEnumerableTypedResult();
                return cookiebots.Select(ToGlobalSettingOutput).ToArray();
            }
            else if (settingsQuery.DisplayName == "Events & Webinar")
            {
                var globalEventsnWebinars = queryGlobalEventsnWebinarinfo.GetEnumerableTypedResult();
                return globalEventsnWebinars.Select(ToGlobalSettingOutput).ToArray();
            }
            else
            {
                return System.Array.Empty<GlobalSettingOutput>();
            }
        }

        var allGlobalSettings = queryGlobalsettingskeyInfo.GetEnumerableTypedResult();
        var allCookiebots = queryCookiebotInfo.GetEnumerableTypedResult();
        var allGlobalEventsnWebinars = queryGlobalEventsnWebinarinfo.GetEnumerableTypedResult();

        var outputs = allGlobalSettings.Select(ToGlobalSettingOutput)
            .Concat(allCookiebots.Select(ToGlobalSettingOutput))
            .Concat(allGlobalEventsnWebinars.Select(ToGlobalSettingOutput))
            .ToArray();

        return outputs;
    }
    public async Task<GlobalSettingOutput> GetGlobalSetting(string settingCodeName, CancellationToken cancellationToken)
    {
        if (settingCodeName == "BDO.Globalsettingskey")
        {
            var globalSettings = _customScriptinfoProvider.Get().GetEnumerableTypedResult();
            return globalSettings.Select(ToGlobalSettingOutput).FirstOrDefault();
        }
        else if (settingCodeName == "BDO.Cookiebot")
        {
            var cookiebots = _cookiebotinfoProvider.Get().GetEnumerableTypedResult();
            return cookiebots.Select(ToGlobalSettingOutput).FirstOrDefault();
        }
        else if (settingCodeName == "BDO.GlobalEventsnWebinars")
        {
            var globalEventsnWebinars = _globalEventsnWebinarinfoProvider.Get().GetEnumerableTypedResult();
            return globalEventsnWebinars.Select(ToGlobalSettingOutput).FirstOrDefault();
        }

        return null; // or throw, depending on how you want "unknown code name" handled
    }

    private static GlobalSettingOutput ToGlobalSettingOutput(GlobalsettingskeyInfo setting)
    {
        return new GlobalSettingOutput
        {
            CodeName = setting.ClassName,
            DisplayName = "Include Custom Scripts",
            Value = ToFieldDictionary(setting)
        };
    }

    private static GlobalSettingOutput ToGlobalSettingOutput(CookiebotInfo setting)
    {
        return new GlobalSettingOutput
        {
            CodeName = setting.ClassName,
            DisplayName = "Cookiebot",
            Value = ToFieldDictionary(setting)
        };
    }

    private static GlobalSettingOutput ToGlobalSettingOutput(GlobalEventsnWebinarsInfo setting)
    {
        return new GlobalSettingOutput
        {
            CodeName = setting.ClassName,
            DisplayName = "Events & Webinar",
            Value = ToFieldDictionary(setting)
        };
    }

    private static Dictionary<string, object> ToFieldDictionary(IDataContainer container)
    {
        return container.ColumnNames
            .ToDictionary(columnName => columnName, columnName => container.GetValue(columnName));
    }
}