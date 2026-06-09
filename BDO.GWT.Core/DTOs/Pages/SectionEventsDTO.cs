using BDO.GWT.Core.DTOs.ReusableFieldsDTOs;
using BDO.GWT.Web.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDO.GWT.Core.DTOs.Pages
{
    public class SectionEventsDto
    {
        public HeroDto Hero { get; set; } = null!;

        public CardsGridDto CardsGrid { get; set; } = null!;
        public MetaDataDto? MetaData { get; set; }
        public SearchAndSEODto? SearchAndSEO { get; set; }

        public BaseContent? BaseContent { get; set; }

        public BaseWebPage? BaseWebPage { get; set; }
        public string Title { get; set; } = null!;

        public string Description { get; set; } = null!;

    }
}
