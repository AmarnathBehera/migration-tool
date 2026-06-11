using BDO.GWT.Web.Core.DTOs;

namespace BDO.GWT.Core.DTOs.Pages
{
    public class SectionDealsDto
    {
        public HeroDto Hero { get; set; } = null!;

        public IEnumerable<FilterItemDto<OptionDto>>? Filters { get; set; }

        public int PageSize { get; set; }

        public string ApiEndpoint { get; set; } = null!;
    }
}
