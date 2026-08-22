using BDO.GWT.Core.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDO.GWT.Core.DTOs.Cards
{
    public class InsightCardDto
    {
        //public string ChannelName { get; set; } = null!;
        //public string LanguageTag { get; set; } = null!;
        //public string Content { get; set; } = null!;
        //public int NodeId { get; set; }
        //public string NodeAliasPath { get; set; } = null!;
        //public int DocumentId { get; set; }
        //public string ClassName { get; set; } = null!;
        //public string Title { get; set; } = null!;
        //public string MetadataTitle { get; set; } = null!;
        //public string? MetadataDescription { get; set; }
        //public Link? InsightImage { get; set; }
        //public string? InsightImageAltText { get; set; }
        public string Title { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string? ImageURL { get; set; }

        public string? ImageAltText { get; set; }

        public string ReadMoreLink { get; set; } = null!;
    }
}
