using BDO.GWT.Web.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDO.GWT.Core.DTOs
{
    public  class BlogContributorDto
    {
        public FilterItemDto<OptionDto>? SortBy { get; set; }

        public HeroDto Hero { get; set; } = null!;

        public int PageSize { get; set; }

        public string ApiEndpoint { get; set; } = null!;
    }
}
