using BDO.GWT.Web.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDO.GWT.Core.DTOs.Pages
{
    public class SearchPageDto
    {
        public FilterItemDto<SearchFacetOptionDto> Filters { get; set; } = null!;

        public FilterItemDto<OptionDto> SortBy { get; set; } = null!;

        public string InitialSearchText { get; set; } = null!;
    }
}
