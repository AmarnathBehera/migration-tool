namespace BDO.GWT.Core.DTOs.Pages;

public class RssSectionDto
{
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public LinkItemDto[] Links { get; set; } = null!;
}
