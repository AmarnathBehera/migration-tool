using BDO.GWT.GLOBAL.API.Data;
using CMS.DataEngine;

namespace BDO.GWT.GLOBAL.API.Services;

public interface ILicenseKeysCrudService
{
    Task<LicenseKeyOutput> GetLicenseKeyAsync(CancellationToken cancellationToken);

    Task<LicenseKeyOutput> SetLicenseKeyAsync(LicenseKeyInput licenseKeyInput, CancellationToken cancellationToken);

    Task DeleteLicenseKeyAsync(CancellationToken cancellationToken);
}

public class LicenseKeysCrudService : ILicenseKeysCrudService
{
    private const string LicenseKeySettingName = "CMSLicenseKey";

    private readonly IInfoProvider<SettingsKeyInfo> _settingsKeyInfoProvider;

    public LicenseKeysCrudService(IInfoProvider<SettingsKeyInfo> settingsKeyInfoProvider)
    {
        _settingsKeyInfoProvider = settingsKeyInfoProvider;
    }

    public async Task<LicenseKeyOutput> GetLicenseKeyAsync(CancellationToken cancellationToken)
    {
        var settingsKey = await _settingsKeyInfoProvider.GetAsync(LicenseKeySettingName, cancellationToken);

        return ToLicenseKeyOutput(settingsKey);
    }

    public async Task<LicenseKeyOutput> SetLicenseKeyAsync(LicenseKeyInput licenseKeyInput, CancellationToken cancellationToken)
    {
        var settingsKey = await _settingsKeyInfoProvider.GetAsync(LicenseKeySettingName, cancellationToken);

        settingsKey.KeyValue = licenseKeyInput.Key;

        await _settingsKeyInfoProvider.SetAsync(settingsKey, cancellationToken);

        return ToLicenseKeyOutput(settingsKey);
    }

    public async Task DeleteLicenseKeyAsync(CancellationToken cancellationToken)
    {
        var settingsKey = await _settingsKeyInfoProvider.GetAsync(LicenseKeySettingName, cancellationToken);

        settingsKey.KeyValue = null;

        await _settingsKeyInfoProvider.SetAsync(settingsKey, cancellationToken);
    }

    private static LicenseKeyOutput ToLicenseKeyOutput(SettingsKeyInfo settingsKey)
    {
        return new LicenseKeyOutput
        {
            ID = settingsKey.KeyID,
            Key = settingsKey.KeyValue
        };
    }
}