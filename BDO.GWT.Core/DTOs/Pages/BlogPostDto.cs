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

        public ImageDto? ImageDto { get; set; }
        public ContentSidebarDto? ContentSidebarDto { get; set; }

        public CallToActionDto? CallToAction { get; set; }
        public SearchAndSEODto? SearchAndSEO { get; set; }

        public OGMetaDataDto? OGMetaData { get; set; }

    }
}
