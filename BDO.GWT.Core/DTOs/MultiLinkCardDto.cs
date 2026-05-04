using System;

namespace BDO.GWT.Core.DTOs;


public class MultiLinkCardDto
{
    public string? ImageURL { get; set; } = null!;

    public string? ImageAltText { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? Url { get; set; }
    public bool isComingSoon { get; set; }

    public LinkItemDto[] ReadMoreLinks { get; set; } = Array.Empty<LinkItemDto>();
}
