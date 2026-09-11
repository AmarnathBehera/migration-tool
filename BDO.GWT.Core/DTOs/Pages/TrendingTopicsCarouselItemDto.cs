namespace BDO.GWT.Core.DTOs;

public class TrendingTopicsCarouselItemDto
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? ReadMoreLink { get; set; }

    public string? ImageURL { get; set; } = null!;

    public string? ImageAltText { get; set; } = null!;

    public bool IsFeatured { get; set; }
}
