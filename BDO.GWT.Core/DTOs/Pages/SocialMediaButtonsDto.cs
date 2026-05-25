namespace BDO.GWT.Web.Core.LiveSite.DTOs;

public class SocialMediaButtonsDto
{
    public string? EmailButtonUrl { get; set; } = null!;

    public string? FacebookButtonUrl { get; set; } = null!;

    public string? TwitterButtonUrl { get; set; } = null!;

    public string? LinkedInUrl { get; set; } = null!;

    public bool? ShowCopyLinkButton { get; set; } = false;
}
