using BDO.GWT.Core.DTOs;
using System;

namespace BDO.GWT.Core.DTOs;

public class EventsWebinarsCardDto
{
    public bool IsFeatured { get; set; }

    public string? Type { get; set; }

    public DateTime DateTime { get; set; }

    public DateTime? DateTimeTo { get; set; }

    public string? Location { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public LinkItemDto Link { get; set; } = null!;
}
