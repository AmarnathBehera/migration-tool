
namespace BDO.GWT.Core.Helpers;

public interface IFilteringHelper
{
    Task<bool> IsFilteringTooltipEnabledAsync(bool isFilteringEnabled, CancellationToken cancellationToken);
}

public class FilteringHelper : IFilteringHelper
{

    public FilteringHelper()
    {
    }

    public async Task<bool> IsFilteringTooltipEnabledAsync(bool isFilteringEnabled, CancellationToken cancellationToken)
    {
        if (!isFilteringEnabled)
        {
            return false;
        }

        //var setting = await _settingsService.GetCurrentSiteSettingAsync(SettingName.BDO.Filtering.ShowTooltip, cancellationToken);

        return true;
    }
}
