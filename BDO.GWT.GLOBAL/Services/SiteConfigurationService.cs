using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Entities;
using BDO.GWT.GLOBAL.Services;
using BDO.GWT.Web.Services;
using CMS.ContentEngine;
using CMS.DataEngine;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XperienceCommunity.Localization;

namespace BDO.GWT.Web.Core.Services;

public interface ISiteConfigurationService
{
    Task<SiteConfigurationDto> GetSiteConfigurationDtoAsync(CancellationToken cancellationToken);

    string GetCachedSiteConfigurationHash();
}

public class SiteConfigurationService : ISiteConfigurationService
{
    private static string[] cacheDependencies =
        [
            "cms.resourcestring|all",
            "cms.culture|all",
            "cms.resourcetranslation|all",
            "cms.resourcestring|all",
            "bdositelocalization.sitetranslation|all",
            "cms.culturesite|all",
        ];

    private readonly ICachingService _cachingService;
    private readonly ISiteContext _siteContext;
    private readonly IInfoProvider<LocalizationKeyInfo> _localizationKeyProvider;
    private readonly IInfoProvider<LocalizationTranslationItemInfo> _localizationTranslationProvider;
    private readonly IInfoProvider<ContentLanguageInfo> _contentLanguageProvider;

    public SiteConfigurationService(
        ICachingService cachingService,
        IContentQueryExecutor executor,
        ISiteContext siteContext,
        IInfoProvider<LocalizationKeyInfo> localizationKeyProvider,
        IInfoProvider<LocalizationTranslationItemInfo> localizationTranslationProvider,
        IInfoProvider<ContentLanguageInfo> contentLanguageProvider)
    {
        _cachingService = cachingService;
        _siteContext = siteContext;
        _localizationKeyProvider = localizationKeyProvider;
        _localizationTranslationProvider = localizationTranslationProvider;
        _contentLanguageProvider = contentLanguageProvider;
    }

    public string GetCachedSiteConfigurationHash()
    {
        return _cachingService.TryGetCachedObject<string>(GetSiteConfigurationHashCacheKey(_siteContext.CurrentSiteID), out var hash) ? hash : null;
    }

    private static string GetSiteConfigurationHashCacheKey(int siteId)
    {
        return $"{nameof(SiteConfigurationService)}|{nameof(GetCachedSiteConfigurationHash)}|hash|{siteId}";
    }

    public async Task<SiteConfigurationDto> GetSiteConfigurationDtoAsync(CancellationToken cancellationToken)
    {
        var localizationResources = await GetLocalizationValuesAsync();
        
        var translations = localizationResources.ToDictionary(
            resource => resource.CultureCode,
            resource => (IDictionary<string, string>)resource.LocalizationKey.ToDictionary(
                key => key.LocalizationKeyItemName,
                key => resource.LocalizationValue
                    .FirstOrDefault(v => v.LocalizationTranslationItemLocalizationKeyItemId == key.LocalizationKeyItemId)
                    ?.LocalizationTranslationItemText ?? string.Empty
            )
        );

        var result = new SiteConfigurationDto
        {
            Translations = translations
        };

        var calculatedHash = await _cachingService.LoadAsync(
            async _ => await BDO.GWT.Core.Helpers.ObjectHelper.CalculateObjectHashAsync(result),
            cacheKey: GetSiteConfigurationHashCacheKey(_siteContext.CurrentSiteID),
            cacheDependencies,
            cancellationToken);

        result.Hash = calculatedHash;

        return result;              
    }

    public async Task<IEnumerable<LocalizationResource>> GetLocalizationValuesAsync()
    {
        var keys = (await _localizationKeyProvider.Get().GetEnumerableTypedResultAsync()).ToList();
        var translations = (await _localizationTranslationProvider.Get().GetEnumerableTypedResultAsync()).ToList();
        var languages = (await _contentLanguageProvider.Get().GetEnumerableTypedResultAsync()).ToList();

        var localizationKeys = keys
            .Select(k => new LocalizationKey
            {
                LocalizationKeyItemId = k.LocalizationKeyItemId,
                LocalizationKeyItemGuid = k.LocalizationKeyItemGuid,
                LocalizationKeyItemName = k.LocalizationKeyItemName,
                LocalizationKeyItemDescription = k.LocalizationKeyItemDescription,
            })
            .ToList();

        return languages
            .Select(language => new LocalizationResource
            {
                CultureCode = language.ContentLanguageCultureFormat,
                LocalizationKey = localizationKeys,
                LocalizationValue = translations
                    .Where(t => t.LocalizationTranslationItemContentLanguageId == language.ContentLanguageID)
                    .Select(t => new LocalizationValue
                    {
                        LocalizationTranslationItemID = t.LocalizationTranslationItemID,
                        LocalizationTranslationItemGuid = t.LocalizationTranslationItemGuid,
                        LocalizationTranslationItemLocalizationKeyItemId = t.LocalizationTranslationItemLocalizationKeyItemId,
                        LocalizationTranslationItemContentLanguageId = t.LocalizationTranslationItemContentLanguageId,
                        LocalizationTranslationItemText = t.LocalizationTranslationItemText,
                    })
                    .ToList(),
            })
            .ToList();
    }
}
