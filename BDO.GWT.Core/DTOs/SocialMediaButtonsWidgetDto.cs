namespace BDO.GWT.Core.DTOs;

public class SocialMediaButtonsWidgetDto 
{
    public string TopMargin { get; set; } = null!;

    public string BottomMargin { get; set; } = null!;

    public string? WidgetID { get; set; }

    public string? EmailButtonUrl { get; set; } = null!;

    public string? FacebookButtonUrl { get; set; } = null!;

    public string? TwitterButtonUrl { get; set; } = null!;

    public string? LinkedInUrl { get; set; } = null!;

    public bool? ShowCopyLinkButton { get; set; } = false;
}
