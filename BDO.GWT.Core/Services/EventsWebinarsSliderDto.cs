using BDO.GWT.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDO.GWT.Core.Services
{
    public class EventsWebinarsSliderDto
    {
        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? BackgroundImageURL { get; set; }

        public string? BackgroundImageAltText { get; set; }

        public FilterItemDto<OptionDto>[]? Filters { get; set; }

        public required bool FilteringTooltipEnabled { get; set; }

        public IEnumerable<EventsWebinarsCardDto> Items { get; set; } = null!;

        public LinkItemDto? SeeAllLink { get; set; }

        public string TopMargin { get; set; } = null!;

        public string BottomMargin { get; set; } = null!;

        public string? WidgetID { get; set; }

        public Guid StateGUID { get; set; }
        public string? CardTitle { get; set; }

        public string? CardDescription { get; set; }

    }
}
