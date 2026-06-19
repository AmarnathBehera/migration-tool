using System;
using System.Collections.Generic;
using System.Text;

namespace BDO.GWT.Core.DTOs.Cards
{
    public class DealCardDto
    {
        public string? ImageURL { get; set; }

        public string? ImageAltText { get; set; }

        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public string? Country { get; set; }

        public string ReadMoreLink { get; set; } = null!;
    }
}
