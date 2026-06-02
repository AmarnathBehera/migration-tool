using BDO.GWT.Core.DTOs.ReusableFieldsDTOs;
using BDO.GWT.Web.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDO.GWT.Core.DTOs
{
    public class BlogPostDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Content { get; set; }
        public bool ShowTitle { get; set; }

        public bool ShowDescription { get; set; }
        public ImageDto? ImageDto { get; set; }
        public ContentSidebarDto? Sidebar { get; set; }

        public bool ShowSidebar { get; set; } = false;

        public bool ShowPintButton { get; set; } = false;

        public bool ShowCallToAction { get; set; } = false;
        public CallToActionDto? CallToAction { get; set; }
        public SearchAndSEODto? SearchAndSEO { get; set; }

       public string? ContentType { get; set; }

        //need to use it based on Rss, if any chnage to type need create it based on Rss
        public string? RssFeedUrl { get; set; }
        public string? RssImageUrl { get; set; }

        
    }
}
