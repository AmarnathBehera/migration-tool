using BDO.GWT.Core.Services;
using CMS.DataEngine;
using System;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Components.FormComponents.GlobalLocationDefault
{
    public class GlobalLocationDefaultScriptExecutor
    {
        public readonly ISettingService _settingService;

        public GlobalLocationDefaultScriptExecutor(ISettingService settingService)
        {
            _settingService = settingService;
        }
        public async Task<String> ExecuteAsync(string settingName)
        {
            if (string.IsNullOrWhiteSpace(settingName))
                return string.Empty;

            var globalLocationInfo = await _settingService.GetGlobalSettingAsync();
            string results = " ";
            if (settingName == "Title")
            {
                results = globalLocationInfo?.GlobalLocationsTitle ?? string.Empty;
            }
            else if (settingName == "Description")
            {
                results = globalLocationInfo?.GlobalLocationsDescription ?? string.Empty;
            }

            return results;
        }
    }
}