using BDO.GWT.Core.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDO.GWT.Core.DTOs.Cards
{
    public class InsightSearchCardDto
    {
       
        public string Title { get; set; } = null!;

        public string Description { get; set; } = null!;

        public DateTime? Date { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public string? ImageURL { get; set; }

        public string? ImageAltText { get; set; }

        public LinkItemDto ReadMoreLink { get; set; } = null!;
    }
}
