using BDO.GWT.Core.DTOs;
using System;
using System.Text.Json.Serialization;

namespace BDO.GWT.Core.DTOs.Cards
{
    public class EventWebinarSearchCard
    {
        public bool IsFeatured { get; set; }

        public string? Type { get; set; }

        public DateTime DateTime { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTime? DateTimeTo { get; set; }

        public string? Location { get; set; }

        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public LinkItemDto ReadMoreLink { get; set; } = null!;
    }
}
