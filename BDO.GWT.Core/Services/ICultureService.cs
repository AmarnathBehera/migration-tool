using BDO.GWT.Core.ValueObjects;

namespace BDO.GWT.Core.Services;

public interface ICultureService
{
    Task<IEnumerable<LanguageTag>> GetCurrentSiteCulturesAsync(CancellationToken cancellationToken);

    Task<IEnumerable<LanguageTag>> GetSiteCulturesAsync(int siteID, CancellationToken cancellationToken);

    Task<IEnumerable<LanguageTag>> GetSiteCulturesLanguageCodeAsync(int siteID, CancellationToken cancellationToken);
}