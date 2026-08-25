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

        public string? ImageURL { get; set; }

        public string? ImageAltText { get; set; }

        public string ReadMoreLink { get; set; } = null!;
    }
}
