using BDO.GWT.Core.Services;

namespace BDO.GWT.Core.Helpers;
public interface IFilteringHelper
{
    Task<bool> IsFilteringTooltipEnabledAsync(bool isFilteringEnabled, CancellationToken cancellationToken);
}
public class FilteringHelper : IFilteringHelper
{
    private readonly ISettingService _settingService;
    public FilteringHelper(ISettingService settingService)
    {
        _settingService = settingService;
    }

    public async Task<bool> IsFilteringTooltipEnabledAsync(bool isFilteringEnabled, CancellationToken cancellationToken)
    {        if (!isFilteringEnabled)
        {
            return false;
        }
        var filteringInfos = await _settingService.GetFilteringInfo();
        var setting = filteringInfos?.FirstOrDefault();        
        return setting?.Showtooltip ?? false;
    }
}
