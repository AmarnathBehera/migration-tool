using BDO.GWT.Core.Services;
using CMS.ContentEngine;
using CMS.DataEngine;
using XperienceCommunity.Localization;

namespace BDO.GWT.Core.Helper;

public interface ILocalizationHelper
{
    Task<string> GetTranslationAsync(string cultureCode, string key, CancellationToken cancellationToken);
}

public class LocalizationHelper : ILocalizationHelper
{ 
    private readonly IInfoProvider<LocalizationKeyInfo> _localizationKeyProvider;
    private readonly IInfoProvider<XperienceCommunity.Localization.LocalizationTranslationItemInfo> _localizationTranslationProvider;
    private readonly IInfoProvider<ContentLanguageInfo> _contentLanguageProvider;
    public LocalizationHelper( 
        IInfoProvider<LocalizationKeyInfo> localizationKeyProvider,
        IInfoProvider<XperienceCommunity.Localization.LocalizationTranslationItemInfo> localizationTranslationProvider,
        IInfoProvider<ContentLanguageInfo> contentLanguageProvider)
    { 
        _localizationKeyProvider = localizationKeyProvider;
        _localizationTranslationProvider = localizationTranslationProvider;
        _contentLanguageProvider = contentLanguageProvider;
    }
    public async Task<string> GetTranslationAsync(string cultureCode, string key, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return key;
        }

        var language = (await _contentLanguageProvider
            .Get()
            .WhereEquals(nameof(ContentLanguageInfo.ContentLanguageCultureFormat), cultureCode)
            .TopN(1)
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
            .FirstOrDefault();

        var localizationKey = (await _localizationKeyProvider
            .Get()
            .WhereEquals(nameof(LocalizationKeyInfo.LocalizationKeyItemName), key)
            .TopN(1)
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
            .FirstOrDefault();

        if (language is null || localizationKey is null)
        {
            return key;
        }

        var translation = (await _localizationTranslationProvider
            .Get()
            .WhereEquals(nameof(LocalizationTranslationItemInfo.LocalizationTranslationItemLocalizationKeyItemId), localizationKey.LocalizationKeyItemId)
            .WhereEquals(nameof(LocalizationTranslationItemInfo.LocalizationTranslationItemContentLanguageId), language.ContentLanguageID)
            .TopN(1)
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
            .FirstOrDefault();

        return string.IsNullOrEmpty(translation?.LocalizationTranslationItemText)
            ? key
            : translation.LocalizationTranslationItemText;
    }
}
