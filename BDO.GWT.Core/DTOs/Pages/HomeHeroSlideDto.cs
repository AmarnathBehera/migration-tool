

namespace BDO.GWT.Core.DTOs;
public class HomeHeroSlideDto
{
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public LinkItemDto? Button { get; set; }

    public string ImageURL { get; set; } = null!;

    public string ImageAltText { get; set; } = null!;

    public string MobileImageURL { get; set; } = null!;

    public string MobileImageAltText { get; set; } = null!;

    public string ThumbnailImageURL { get; set; } = null!;

    public string ThumbnailImageAltText { get; set; } = null!;
}
