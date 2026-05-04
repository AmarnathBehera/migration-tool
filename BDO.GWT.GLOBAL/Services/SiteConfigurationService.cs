using BDO.GWT.Core.Constants;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Core.Services;

public interface ISiteConfigurationService
{
    Task<SiteConfigurationDto> GetSiteConfigurationDtoAsync(CancellationToken cancellationToken);

    string? GetCachedSiteConfigurationHash();
}

public class SiteConfigurationService : ISiteConfigurationService
{
    private readonly ICachingService _cachingService;
    //private readonly IDateFormattingService _dateFormattingService;
    private readonly ILocalizationService _localizationService;
    private readonly ISettingsService _settingService;
    private readonly ISiteContext _siteContext;

    public SiteConfigurationService(ICachingService cachingService, ILocalizationService localizationService, ISettingsService settingService, ISiteContext siteContext) //,IDateFormattingService dateFormattingService)
    {
        _cachingService = cachingService;
        //_dateFormattingService = dateFormattingService;
        _localizationService = localizationService;
        _settingService = settingService;
        _siteContext = siteContext;
    }

    public async Task<SiteConfigurationDto> GetSiteConfigurationDtoAsync(CancellationToken cancellationToken)
    {
        var (translations, translationsCacheDependencies) = await GetLiveSiteTranslationsAsync(cancellationToken);

        var (dateTimeFormats, dateTimeFormatsCacheDependencies) = await GetDateTimeFormatsAsync(cancellationToken);

        var (siteSettings, siteSettingsCacheDependencies) = await GetSiteSettingsAsync(cancellationToken);

        var cacheDependencies = translationsCacheDependencies.Union(dateTimeFormatsCacheDependencies).Union(siteSettingsCacheDependencies).ToArray();

        var result = new SiteConfigurationDto
        {
            Translations = translations,
            DateTimeFormats = dateTimeFormats,
            SiteSettings = siteSettings,
        };

        var calculatedHash = await _cachingService.LoadAsync(
            async _ => await ObjectHelper.CalculateObjectHashAsync(result),
            cacheKey: GetSiteConfigurationHashCacheKey(_siteContext.CurrentSiteID),
            cacheDependencies,
            cancellationToken);

        result.Hash = calculatedHash;

        return result;
    }

    private async Task<(SiteSettingsDto siteSettings, string[] siteSettingsCacheDependencies)> GetSiteSettingsAsync(CancellationToken cancellationToken)
    {
        string[] codeNames = [SettingName.BDO.EnableExternalLinkWarning];

        string[] cacheDependencies =
        [
            $"cms.settingskey|{SettingName.BDO.EnableExternalLinkWarning}",
            $"cms.settingskey|{_siteContext.CurrentSiteID}|{SettingName.BDO.EnableExternalLinkWarning}",
        ];

        var siteSettings = await _settingService.GetCurrentSiteSettingsAsync(codeNames, cancellationToken);

        return (ToSiteSettingsDto(siteSettings), cacheDependencies);
    }

    private static SiteSettingsDto ToSiteSettingsDto(IDictionary<string, object?> siteSettings)
    {
        var siteSettingDto = new SiteSettingsDto
        {
            ExternalLinkWarningEnabled = siteSettings.TryGetValue(SettingName.BDO.EnableExternalLinkWarning, out object? value) && value is true,
        };

        return siteSettingDto;
    }

    public string? GetCachedSiteConfigurationHash()
    {
        return _cachingService.TryGetCachedObject<string>(GetSiteConfigurationHashCacheKey(_siteContext.CurrentSiteID), out var hash) ? hash : null;
    }

    private async Task<(IDictionary<string, IDictionary<string, string>> LiveSiteTranslations, string[] CacheDependencies)> GetLiveSiteTranslationsAsync(CancellationToken cancellationToken)
    {
        string[] cacheDependencies =
        [
            "cms.resourcestring|all",
            "cms.culture|all",
            "cms.resourcetranslation|all",
            "cms.resourcestring|all",
            "bdositelocalization.sitetranslation|all",
            "cms.culturesite|all",
        ];

        var liveSiteTranslations = await _cachingService.LoadAsync(
            async _ => await _localizationService.GetLiveSiteTranslationsAsync(_siteContext.CurrentSiteID, cancellationToken),
            cacheKey: $"{nameof(SiteConfigurationService)}|{nameof(GetLiveSiteTranslationsAsync)}|translations|{_siteContext.CurrentSiteID}",
            cacheDependencies,
            cancellationToken);

        return (liveSiteTranslations, cacheDependencies);
    }

    //private async Task<(DateTimeFormatsDto DateTimeFormats, string[] CacheDependencies)> GetDateTimeFormatsAsync(CancellationToken cancellationToken)
    //{
    //    string[] cacheDependencies =
    //    [
    //        "bdositelocalization.dateformat|all",
    //    ];

    //    var dateTimeFormats = await _cachingService.LoadAsync(
    //        async _ => await _dateFormattingService.GetDateFormatDataAsync(_siteContext.CurrentSiteID, cancellationToken),
    //        cacheKey: $"{nameof(SiteConfigurationService)}|{nameof(GetDateTimeFormatsAsync)}|formats|{_siteContext.CurrentSiteID}",
    //        cacheDependencies,
    //        cancellationToken);

    //    var dateTimeFormatsDto = new DateTimeFormatsDto
    //    {
    //        DateAndTimeFormatByCulture = dateTimeFormats.DateWithTimeFormatByLanguageTag.ToDictionary(x => x.Key.ToString().ToLowerInvariant(), x => x.Value),
    //        DateFormatByCulture = dateTimeFormats.DateFormatByLanguageTag.ToDictionary(x => x.Key.ToString().ToLowerInvariant(), x => x.Value),
    //        TimeFormatByCulture = dateTimeFormats.TimeFormatByLanguageTag.ToDictionary(x => x.Key.ToString().ToLowerInvariant(), x => x.Value),
    //    };

    //    return (dateTimeFormatsDto, cacheDependencies);
    //}

    private static string GetSiteConfigurationHashCacheKey(int siteId)
    {
        return $"{nameof(SiteConfigurationService)}|{nameof(GetSiteConfigurationDtoAsync)}|hash|{siteId}";
    }
}
