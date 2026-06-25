using BDO.GWT.Web.Core.DTOs;

namespace BDO.GWT.Core.DTOs;

public class BlogContributorsDto
{
    public FilterItemDto<OptionDto>? SortBy { get; set; }

    public HeroDto Hero { get; set; } = null!;

    public int PageSize { get; set; }

    public string ApiEndpoint { get; set; } = null!;
}
