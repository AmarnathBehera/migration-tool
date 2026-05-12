using BDO.GWT.Web.Core.DTOs;

namespace BDO.GWT.Core.DTOs.Pages;

public class SectionPeopleDto //: IFilterableComponentDto
{
   public FilterItemDto<OptionDto>[]? Filters { get; set; }

    public required bool FilteringTooltipEnabled { get; set; }

    public FilterItemDto<OptionDto>? SortBy { get; set; }

    public HeroDto Hero { get; set; } = null!;

    public int PageSize { get; set; }

    public string ApiEndpoint { get; set; } = null!;
}
