using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Web.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDO.GWT.Core.DTOs
{
    public class SearchResultDto : PaginationDto<SearchCardDto>
    {
        public int TotalPages { get; set; }

        public FilterItemDto<SearchFacetOptionDto> Filters { get; set; } = null!;
    }

}
