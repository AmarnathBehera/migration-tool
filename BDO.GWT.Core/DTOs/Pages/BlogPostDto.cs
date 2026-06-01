using BDO.GWT.Core.DTOs.ReusableFieldsDTOs;
using BDO.GWT.Web.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDO.GWT.Core.DTOs
{
    public class BlogPostDto
    {
        public ContentWithShowOption? ContentWithShowOption { get; set; }
        public MetaDataDto? MetaData { get; set; }

        public ImageDto? ImageDto { get; set; }
        public ContentSidebarDto? ContentSidebarDto { get; set; }

        public bool ShowSidebar { get; set; } = false;

        public bool ShowPintButton { get; set; } = false;

        public CallToActionDto? CallToAction { get; set; }
        public SearchAndSEODto? SearchAndSEO { get; set; }

       public string? ContentType { get; set; }

        //need to use it based on Rss, if any chnage to type need create it based on Rss
        public string? RssFeedUrl { get; set; }
        public string? RssImageUrl { get; set; }


    }
}
