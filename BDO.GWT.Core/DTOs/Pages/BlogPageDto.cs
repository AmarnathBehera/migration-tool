using BDO.GWT.Core.DTOs.ReusableFieldsDTOs;
using BDO.GWT.Web.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDO.GWT.Core.DTOs
{
    public class BlogPageDto
    {
        public HeroDto Hero { get; set; } = null!;

        public ItemListDto ItemList {  get; set; }   = null!;

        public MetaDataDto? MetaData { get; set; }

        public SearchAndSEODto? SearchAndSEO { get; set; }

        public OGMetaDataDto? OGMetaData { get; set; }

        public BaseContent? BaseContent { get; set; }

        public BaseWebPage? BaseWebPage { get; set; }
    }
}
