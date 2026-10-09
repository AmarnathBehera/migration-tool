namespace BDO.GWT.Core.DTOs;

public class BusinessStrategiesMenuDto
{
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? ImageURL { get; set; } = null!;

    public LinkItemDto ReadMoreLink { get; set; } = null!;
}
