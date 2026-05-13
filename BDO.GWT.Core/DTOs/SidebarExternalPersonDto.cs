namespace BDO.GWT.Web.Core.DTOs;

public class SidebarExternalPersonDto
{
    public required ImageDto? Image { get; set; }

    public required string FullName { get; set; } = null!;

    public required string? JobTitle { get; set; }

    public required string? Url { get; set; }
}
