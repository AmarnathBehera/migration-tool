using BDO.GWT.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Domain.Services;

public interface ISettingsService
{
    Task<TValue?> GetGlobalSettingAsync<TValue>(string name, CancellationToken cancellationToken);

    Task<Setting?> GetCurrentWebsiteSettingAsync(string name, CancellationToken cancellationToken);

    Task<IDictionary<string, object?>> GetCurrentWebsiteSettingsAsync(string[] settingNames, CancellationToken cancellationToken);

    Task<Dictionary<int, Setting?>> GetWebsiteSettingByWebsitesAsync(string name, List<int> websiteIds, CancellationToken cancellationToken);
}

public class SettingsService : ISettingsService
{

    public Task<TValue?> GetGlobalSettingAsync<TValue>(string name, CancellationToken cancellationToken)
    {
        throw new System.NotImplementedException();
    }

    public Task<Setting?> GetCurrentWebsiteSettingAsync(string name, CancellationToken cancellationToken)
    {

        object? value = name switch
        {
            

           };

        var setting = new Setting
        {
            Value = value,
        };

        return Task.FromResult<Setting?>(setting);
    }

    public async Task<IDictionary<string, object?>> GetCurrentWebsiteSettingsAsync(string[] settingNames, CancellationToken cancellationToken)
    {
        var settingValueByName = new Dictionary<string, object?>();

        foreach (var settingName in settingNames)
        {
            var setting = await GetCurrentWebsiteSettingAsync(settingName, cancellationToken);

            settingValueByName[settingName] = setting?.Value;
        }

        return settingValueByName;
    }


    public Task<Dictionary<int, Setting?>> GetWebsiteSettingByWebsitesAsync(string name, List<int> websiteIds, CancellationToken cancellationToken)
    {
        throw new System.NotImplementedException();
    }
}

