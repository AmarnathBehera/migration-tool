using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Entities;
using BDO.GWT.Web.Services;
using BDO.GWT.Xperience.Core.Services;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Websites;
using CMS.Websites.Routing;
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

/// <summary>
/// Groups the <see cref="IInfoProvider{T}"/> dependencies required by
/// <see cref="SiteConfigurationService"/> to keep its constructor concise.
/// </summary>
public sealed record SiteConfigurationProviders(
    IInfoProvider<LocalizationKeyInfo> LocalizationKey,
    IInfoProvider<XperienceCommunity.Localization.LocalizationTranslationItemInfo> LocalizationTranslation,
    IInfoProvider<ContentLanguageInfo> ContentLanguage,
    IInfoProvider<WebChannelSettingsInfo> WebChannelSettings,
    IInfoProvider<WebsiteChannelInfo> WebsiteChannel,
    IInfoProvider<GlobalsettingskeyInfo> provider,
    IWebsiteChannelContext websiteChannelContext,
    IInfoProvider<WebChannelSnippetInfo> webChannelSnippetInfoProvider);

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
            "bdo.webchannelsettings|all",
            "bdo.generalsetting|all",
        ];

    private readonly ICachingService _cachingService;
    private readonly ISiteContext _siteContext;
    private readonly SiteConfigurationProviders _providers;
    public SiteConfigurationService(
        ICachingService cachingService,
        IContentQueryExecutor executor,
        ISiteContext siteContext,
        SiteConfigurationProviders providers)
    {
        _cachingService = cachingService;
        _siteContext = siteContext;
        _providers = providers;
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
            resource => resource.CultureCode.ToLower(),
            resource => (IDictionary<string, string>)resource.LocalizationKey.ToDictionary(
                key => key.LocalizationKeyItemName,
                key => resource.LocalizationValue
                    .FirstOrDefault(v => v.LocalizationTranslationItemLocalizationKeyItemId == key.LocalizationKeyItemId)
                    ?.LocalizationTranslationItemText ?? string.Empty
            )
        );

        var result = new SiteConfigurationDto
        {
            Translations = translations,
            SiteSettings = GetSiteSettings()
        };

        var calculatedHash = await _cachingService.LoadAsync(
            async _ => await GWT.Core.Helper.ObjectHelper.CalculateObjectHashAsync(result),
            cacheKey: GetSiteConfigurationHashCacheKey(_siteContext.CurrentSiteID),
            cacheDependencies,
            cancellationToken);

        result.Hash = calculatedHash;

        return result;
    }

    private SiteSettingsDto GetSiteSettings()
    {
        var settings = new SiteSettingsDto();

        var websiteChannel = _providers.WebsiteChannel.Get()
            .WhereEquals(nameof(WebsiteChannelInfo.WebsiteChannelID), _siteContext.CurrentSiteID)
            .TopN(1)
            .FirstOrDefault();

        if (websiteChannel == null)
        {
            return settings;
        }

        var webChannelSettings = _providers.WebChannelSettings.Get()
            .WhereEquals(nameof(WebChannelSettingsInfo.WebChannelSettingsChannelID), websiteChannel.WebsiteChannelChannelID)
            .TopN(1)
            .FirstOrDefault();

        if (webChannelSettings == null)
        {
            return settings;
        }
       
        var generalSetting = _providers.webChannelSnippetInfoProvider.Get()
            .WhereEquals(nameof(WebChannelSnippetInfo.WebChannelSnippetWebChannelSettingsID), webChannelSettings.WebChannelSettingsID)
            .FirstOrDefault();

        if (generalSetting != null)
        {
            if (generalSetting.EnableexternallinkwarningInheritfromlocal)
            {
                var globalGeneralSetting = _providers.provider.Get().FirstOrDefault();
                settings.ExternalLinkWarningEnabled = globalGeneralSetting?.Enableexternallinkwarning ?? false;
            }
            else
            {
                settings.ExternalLinkWarningEnabled = generalSetting.Enableexternallinkwarning;
            }
        }

        return settings;
    }

    public async Task<IEnumerable<LocalizationResource>> GetLocalizationValuesAsync()
    {
        var keys = (await _providers.LocalizationKey.Get().GetEnumerableTypedResultAsync()).ToList();
        var translations = (await _providers.LocalizationTranslation.Get().GetEnumerableTypedResultAsync()).ToList();
        var languages = (await _providers.ContentLanguage.Get().GetEnumerableTypedResultAsync()).ToList();

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
