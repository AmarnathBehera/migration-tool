using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Entities;
using BDO.GWT.Core.Retrievers;
using BDO.GWT.Core.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Core.Services;

public class LocalizationService : ILocalizationService
{
    public const string LiveSiteResourceStringPrefix = "BDO.LiveSite.";
    public const string CopyrightYear = "<copyrightYear>";

    private readonly ICultureService _cultureService;
    private readonly IRetriever<ResourceTranslation> _resourceTranslationRetriever;
    private readonly IRetriever<SiteTranslation> _siteTranslationRetriever;
    private readonly ISettingsService _settingsService;

    public LocalizationService(ICultureService cultureService, IRetriever<ResourceTranslation> resourceTranslationRetriever, IRetriever<SiteTranslation> siteTranslationRetriever, ISettingsService settingsService)
    {
        _cultureService = cultureService;
        _resourceTranslationRetriever = resourceTranslationRetriever;
        _settingsService = settingsService;
        _siteTranslationRetriever = siteTranslationRetriever;
    }

    public async Task<IDictionary<string, IDictionary<string, string>>> GetLiveSiteTranslationsAsync(int siteId, CancellationToken cancellationToken = default)
    {
        var siteLanguageTags = (await _cultureService.GetCurrentSiteCulturesAsync(cancellationToken)).ToList();
        var defaultGlobalCulture = await GetDefaultGlobalCulture(cancellationToken);

        if (defaultGlobalCulture != null && !siteLanguageTags.Contains(defaultGlobalCulture))
        {
            siteLanguageTags.Add(defaultGlobalCulture);
        }

        var resourceTranslations = await _resourceTranslationRetriever.RetrieveJoinedEntitiesAsync(q =>
        {
            q.Include(x => x.ResourceString);
            q.Include(x => x.Culture);
            q.Where<ResourceString>(x => x.Key.StartsWith(LiveSiteResourceStringPrefix));
            q.Where<Culture>(x => siteLanguageTags.Contains(x.LanguageTag));
            q.Select<ResourceString>(x => new { x.Key, x.ID });
            q.Select<Culture>(x => x.LanguageTag);
            q.Select<ResourceTranslation>(x => new { x.Translation });
        }, cancellationToken);

        var siteTranslations = await _siteTranslationRetriever.RetrieveJoinedEntitiesAsync(q =>
            {
                q.Include(x => x.ResourceString);
                q.Where<SiteTranslation>(x => siteLanguageTags.Contains(x.LanguageTag) && x.SiteID == siteId);
                q.Select<SiteTranslation>(x => new { x.Translation, x.ResourceStringKey, x.LanguageTag });
            }
            , cancellationToken);

        var dictionary = resourceTranslations
            .GroupBy(resourceTranslation => resourceTranslation.Culture!.LanguageTag)
            .Where(grouping => grouping.Any())
            .ToDictionary(
                grouping => grouping.Key.ToString().ToLowerInvariant(),
                grouping => grouping.ToDictionary(
                    resourceTranslation => resourceTranslation.ResourceString!.Key,
                    resourceTranslation => resourceTranslation.Translation
                    ) as IDictionary<string, string>
                );

        foreach (var siteTranslation in siteTranslations)
        {
            var cultureCode = siteTranslation.LanguageTag!.ToString().ToLowerInvariant();

            if (dictionary.TryGetValue(cultureCode, out var translations))
            {
                translations[siteTranslation.ResourceStringKey] = siteTranslation.Translation;
            }
            else
            {
                dictionary[cultureCode] = new Dictionary<string, string> { { siteTranslation.ResourceStringKey, siteTranslation.Translation } };
            }
        }

        return UpdateCopyrightYear(dictionary);
    }

    public async Task<string?> GetTranslation(string key, LanguageTag languageTag, int siteId, CancellationToken cancellationToken = default)
    {
        var translation = await GetSiteTranslationAsync(siteId, languageTag, key, cancellationToken);

        if (string.IsNullOrEmpty(translation))
        {
            translation = await GetGlobalTranslationAsync(languageTag, key, cancellationToken);
        }

        return translation;
    }

    public async Task<string?> GetSiteTranslationAsync(int siteId, LanguageTag languageTag, string key, CancellationToken cancellationToken)
    {
        var siteTranslation = (await _siteTranslationRetriever.RetrieveJoinedEntitiesAsync(q =>
        {
            q.Where<SiteTranslation>(x => x.ResourceStringKey == key && x.LanguageTag == languageTag && x.SiteID == siteId);
            q.Select<SiteTranslation>(x => x.Translation);
        }, cancellationToken)).FirstOrDefault();

        return siteTranslation?.Translation;
    }

    public async Task<string?> GetGlobalTranslationAsync(LanguageTag languageTag, string key, CancellationToken cancellationToken)
    {
        string? translation = null;
        var defaultCultureCode = (string)(await _settingsService.GetCurrentSiteSettingAsync(SettingName.CMS.DefaultCultureCode, cancellationToken))?.Value!;

        var defaultCulture = LanguageTag.Parse(defaultCultureCode);
        var defaultGlobalCulture = await GetDefaultGlobalCulture(cancellationToken);

        var resourceTranslations = (await _resourceTranslationRetriever.RetrieveJoinedEntitiesAsync(q =>
        {
            q.Include(x => x.ResourceString);
            q.Include(x => x.Culture);
            q.Where<ResourceString>(x => x.Key == key);
            q.Where<Culture>(x => x.LanguageTag == languageTag || x.LanguageTag == defaultCulture || x.LanguageTag == defaultGlobalCulture);
            q.Select<ResourceString>(x => new { x.Key, x.ID });
            q.Select<Culture>(x => x.LanguageTag);
            q.Select<ResourceTranslation>(x => new { x.Translation });
        }, cancellationToken)).ToDictionary(x => x.Culture!.LanguageTag, x => x.Translation);

        if (!resourceTranslations.TryGetValue(languageTag, out translation) &&
            !resourceTranslations.TryGetValue(defaultCulture, out translation) && defaultGlobalCulture != null)
        {
            resourceTranslations.TryGetValue(defaultGlobalCulture, out translation);
        }
        return translation;
    }

    private async Task<LanguageTag?> GetDefaultGlobalCulture(CancellationToken cancellationToken)
    {
        LanguageTag? culture = null;

        var defaultGlobalCulture = await _settingsService.GetGlobalSettingAsync<string?>(SettingName.CMS.DefaultCultureCode, cancellationToken);

        if (defaultGlobalCulture != null)
        {
            LanguageTag.TryParse(defaultGlobalCulture, out culture);
        }

        return culture;
    }
    private static IDictionary<string, IDictionary<string, string>> UpdateCopyrightYear(IDictionary<string, IDictionary<string, string>> dictionary)
    {
        foreach (var culture in dictionary.Keys)
        {
            if (dictionary[culture].TryGetValue(ResourceStringKey.LiveSite.Footer.Copyright, out string? value))
            {
                dictionary[culture][ResourceStringKey.LiveSite.Footer.Copyright] = value.Replace(CopyrightYear, DateTime.Now.Year.ToString());
            }
        }

        return dictionary;
    }
}
