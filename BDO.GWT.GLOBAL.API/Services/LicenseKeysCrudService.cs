using BDO.GWT.GLOBAL.API.Data;
using CMS.DataEngine;
using System.Text;
using System.Text.Json;

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

    private static LicenseKeyPayload? TryDecodePayload(string? rawKeyValue)
    {
        if (string.IsNullOrEmpty(rawKeyValue))
        {
            return null;
        }

        try
        {
            var envelopeJson = Encoding.UTF8.GetString(Convert.FromBase64String(rawKeyValue));
            var envelope = JsonSerializer.Deserialize<LicenseKeyEnvelope>(envelopeJson);

            if (envelope?.Data is null)
            {
                return null;
            }

            return JsonSerializer.Deserialize<LicenseKeyPayload>(envelope.Data);
        }
        catch (Exception)
        {
            // Malformed/unexpected key format — surface as "unknown" rather than throwing,
            // since this is best-effort metadata extraction, not license validation.
            return null;
        }
    }    
}