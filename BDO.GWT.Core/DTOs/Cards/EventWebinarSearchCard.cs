using BDO.GWT.Core.DTOs;
using System;

namespace BDO.GWT.Core.DTOs.Cards
{
    public class EventWebinarSearchCard
    {
        public int? NodeId { get; set; }
        public string? EventType { get; set; }

        public DateTime? EventDateFrom { get; set; }

        public DateTime? EventDateTo { get; set; }

        public string? EventLocationCity { get; set; }

        public string EventTitle { get; set; } = null!;

        public string? EventDescription { get; set; }       
        
        public string? EventLocationTitle { get; set; }

        public string? ReadMoreLink { get; set; } = null!;
        public LinkItemDto EventLink { get; set; } = null!;
    }
}
