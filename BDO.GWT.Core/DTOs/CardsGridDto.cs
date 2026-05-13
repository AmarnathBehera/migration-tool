using BDO.GWT.Core.DTOs;

namespace BDO.GWT.Web.Core.DTOs;

public class CardsGridDto : IFilterableComponentDto
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public FilterItemDto<OptionDto>[]? Filters { get; set; }

    public required bool FilteringTooltipEnabled { get; set; }

    public string ApiEndpointName { get; set; } = null!;

    public int PageSize { get; set; }

    public string? AdvancedSearchPrelinkText { get; set; }

    public string? AdvancedSearchLinkText { get; set; }

    public string? AdvancedSearchLink { get; set; }
}
