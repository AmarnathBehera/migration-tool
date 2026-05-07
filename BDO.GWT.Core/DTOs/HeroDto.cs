namespace BDO.GWT.Web.Core.DTOs;

public class HeroDto
{
    public string Layout { get; set; } = null!;

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? BackgroundImage { get; set; }

    public string? BackgroundImageAltText { get; set; }

    public string? MobileBackgroundImage { get; set; }

    public string? MobileBackgroundImageAltText { get; set; }

    public string? CtaButtonType { get; set; }

    public string? CtaButtonText { get; set; }

    public string? CtaButtonUrl { get; set; }

    public string? CtaButtonUrlTarget { get; set; }

    public string? WidgetID { get; set; }
}
