using System;
using System.Collections.Generic;
using System.Text;

namespace BDO.GWT.Core.DTOs
{
    public class TilePageDto
    {
        public string? IconUrl { get; set; }

        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public LinkItemDto ReadMoreLink { get; set; } = null!;
    }
}
