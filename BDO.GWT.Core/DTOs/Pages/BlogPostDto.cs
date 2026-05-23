using BDO.GWT.Core.DTOs.ReusableFieldsDTOs;
using BDO.GWT.Web.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDO.GWT.Core.DTOs
{
    public class BlogPostDto
    {
        
        public MetaDataDto? MetaData { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public string ImageAltText { get; set; } = string.Empty;
        public SearchAndSEODto? SearchAndSEO { get; set; }

        public OGMetaDataDto? OGMetaData { get; set; }

        public BaseContent? BaseContent { get; set; }

        public BaseWebPage? BaseWebPage { get; set; }
    }
}
