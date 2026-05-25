namespace BDO.GWT.Core.DTOs.Pages;

public class NewsGroupDto
{
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public NewsGroupItemDto[] Links { get; set; } = null!;
}
