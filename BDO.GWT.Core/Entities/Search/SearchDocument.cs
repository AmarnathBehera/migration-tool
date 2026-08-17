using BDO.GWT.Core.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDO.GWT.Core.Entities.Search
{
    public class SearchDocument
    {
        public string ChannelName { get; set; } = null!;

        public string LanguageTag { get; set; } = null!;

        public string Content { get; set; } = null!;

        public int NodeId { get; set; }

        public string NodeAliasPath { get; set; } = null!;

        public int DocumentId { get; set; }

        public string ClassName { get; set; } = null!;

        public string Title { get; set; } = null!;

        public string MetadataTitle { get; set; } = null!;

        public string? MetadataDescription { get; set; }

        public string? EventType { get; set; }

        public DateTime? EventDateFrom { get; set; }

        public DateTime? EventDateTo { get; set; }

        public string? EventLocationTitle { get; set; }

        public List<int>? InsightContentType { get; set; }

        public Link? InsightImage { get; set; }

        public string? InsightImageAltText { get; set; }

        public DateTimeOffset? PublishFrom { get; set; }

        public DateTimeOffset? PublishTo { get; set; }

        public DateTime CreatedWhen { get; set; }

        public DateTime? SortDate { get; set; }

        public bool? EventIsPrivate { get; set; }

        public string? EventLocationCity { get; set; }
       
    }
}
