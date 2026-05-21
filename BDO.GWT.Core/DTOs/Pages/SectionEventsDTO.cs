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
        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? AdvancedSearchPrelinkText { get; set; }

        public string? AdvancedSearchLinkText { get; set; }

        public string? AdvancedSearchLink { get; set; }
    }
}
