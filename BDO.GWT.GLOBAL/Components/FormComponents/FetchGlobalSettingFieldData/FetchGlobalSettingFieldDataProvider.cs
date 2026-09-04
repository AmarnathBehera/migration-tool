using BDO.GWT.Core.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Components.FormComponents.FetchGlobalSettingFieldData;

public interface IFetchGlobalSettingFieldDataProvider
{
    Task<string> GetFieldValueAsync(string fieldName);
}

public class FetchGlobalSettingFieldDataProvider : IFetchGlobalSettingFieldDataProvider
{
    // Maps the field name entered on the component to the Global Location setting field it should display.
    private static readonly Dictionary<string, Func<GlobalsettingskeyInfo, string>> FieldSelectors = new(StringComparer.OrdinalIgnoreCase)
    {
        [nameof(GlobalsettingskeyInfo.GlobalLocationsTitle)] = info => info.GlobalLocationsTitle,
        [nameof(GlobalsettingskeyInfo.GlobalLocationsDescription)] = info => info.GlobalLocationsDescription,
    };

    private readonly ISettingService _settingService;

    public FetchGlobalSettingFieldDataProvider(ISettingService settingService)
    {
        _settingService = settingService;
    }

    public async Task<string> GetFieldValueAsync(string fieldName)
    {
        if (string.IsNullOrWhiteSpace(fieldName) || !FieldSelectors.TryGetValue(fieldName, out var selector))
        {
            return string.Empty;
        }

        var globalLocationInfo = await _settingService.GetGlobalSettingAsync();

        return globalLocationInfo is null ? string.Empty : selector(globalLocationInfo) ?? string.Empty;
    }
}
