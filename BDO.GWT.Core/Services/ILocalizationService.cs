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

public interface ILocalizationService
{
    Task<IDictionary<string, IDictionary<string, string>>> GetLiveSiteTranslationsAsync(int siteId, CancellationToken cancellationToken = default);

    Task<string?> GetTranslation(string key, LanguageTag languageTag, int siteId, CancellationToken cancellationToken = default);

    Task<string?> GetSiteTranslationAsync(int siteId, LanguageTag languageTag, string key, CancellationToken cancellationToken);

    Task<string?> GetGlobalTranslationAsync(LanguageTag languageTag, string key, CancellationToken cancellationToken);
}
