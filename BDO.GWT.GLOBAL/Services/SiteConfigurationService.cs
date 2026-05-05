using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Entities;
using BDO.GWT.Core.Helpers;
using BDO.GWT.GLOBAL.Services;
using BDO.GWT.Web.Services;
using CMS.ContentEngine;
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
    private const string LocalizationContentType = "XperienceCommunity.Localization";

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
    private readonly IContentQueryExecutor _executor;
    private readonly ISiteContext _siteContext;

    public SiteConfigurationService(ICachingService cachingService, IContentQueryExecutor executor)
    {
        _cachingService = cachingService;
        _executor = executor;
    }

    public string? GetCachedSiteConfigurationHash()
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
            async _ => await ObjectHelper.CalculateObjectHashAsync(result),
            cacheKey: GetSiteConfigurationHashCacheKey(_siteContext.CurrentSiteID),
            cacheDependencies,
            cancellationToken);

        result.Hash = calculatedHash;

        return result;              
    }

    public async Task<IEnumerable<LocalizationResource>> GetLocalizationValuesAsync()
    {
        var queryBuilder = new ContentItemQueryBuilder()
            .ForContentType(LocalizationContentType);

        var result = await _executor.GetMappedResult<LocalizationResource>(queryBuilder);

        return result;
    }
}
