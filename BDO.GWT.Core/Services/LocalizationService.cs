using BDO.GWT.Core.Entities;
using BDO.GWT.Core.ValueObjects;
using CMS.Base;
using CMS.ContentEngine;
using CMS.Core;
using CMS.DataEngine;
using Kentico.Content.Web.Mvc;
using System.CodeDom.Compiler;

namespace BDO.GWT.Core.Services;

public class LocalizationService : ILocalizationService
{
    public const string LiveSiteResourceStringPrefix = "BDO.LiveSite.";
    public const string CopyrightYear = "<copyrightYear>";

    private readonly ICultureService _cultureService;
    private readonly IContentQueryExecutor _executor;

    public LocalizationService(IContentQueryExecutor executor, ISettingsService settingsService)
    {
        _executor = executor;
        //_settingsService = settingsService;
    }
    public async Task<List<LocalizationKey>> GetAllLocalizationKeyAsync()
    {
        var queryBuilder = new ContentItemQueryBuilder()
            .ForContentType("XperienceCommunity.LocalizationKeyItem", config => config
                .OrderBy("LocalizationKeyItemId"));

        var result = await _executor.GetMappedResult<LocalizationKey>(queryBuilder);

        return result.ToList();
    }

    public async Task<List<LocalizationString>> GetAllLocalizationStringAsync()
    {
        var queryBuilder = new ContentItemQueryBuilder()
            .ForContentType("XperienceCommunity.LocalizationTranslationItem", config => config
                .OrderBy("LocalizationTranslationItemID"));

        var result = await _executor.GetMappedResult<LocalizationString>(queryBuilder);

        return result.ToList();
    }

    public async Task<LocalizationResource> GetAllSiteLocalizationValues()
    {
        var queryBuilder = new ObjectHelper()
            .ForContentType("XperienceCommunity.Localization");

        var result = await _executor.GetMappedResult<LocalizationResource>(queryBuilder);

        return (LocalizationResource)result;
    }

    //private async Task<LanguageTag?> GetDefaultGlobalCulture(CancellationToken cancellationToken)
    //{
    //    LanguageTag? culture = null;

    //    var defaultGlobalCulture = await _settingsService.GetGlobalSettingAsync<string?>(SettingName.CMS.DefaultCultureCode, cancellationToken);

    //    if (defaultGlobalCulture != null)
    //    {
    //        LanguageTag.TryParse(defaultGlobalCulture, out culture);
    //    }

    //    return culture;
    //}
}
