using BDO.GWT.Web.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDO.GWT.Core.DTOs.Pages
{
    public class SectionEventsDTO
    {
        public FilterItemDto<OptionDto>[]? Filters { get; set; }

        public required bool FilteringTooltipEnabled { get; set; }

        public FilterItemDto<OptionDto>? SortBy { get; set; }

        public HeroDto Hero { get; set; } = null!;

        public int PageSize { get; set; }

        public string ApiEndpoint { get; set; } = null!;
    }
}
