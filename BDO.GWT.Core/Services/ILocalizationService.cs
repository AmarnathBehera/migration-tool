using BDO.GWT.Core.Entities;

namespace BDO.GWT.Core.Services;

public interface ILocalizationService
{
    Task<List<LocalizationKey>> GetAllLocalizationKeyAsync();
    Task<List<LocalizationString>> GetAllLocalizationStringAsync();
    Task<LocalizationResource> GetAllSiteLocalizationValues();
}
