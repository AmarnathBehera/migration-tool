namespace BDO.GWT.Core.Data;

public class HeroWidgetData
{
    public required string Layout { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public required string BackgroundImage { get; set; }

    public required string BackgroundImageAltText { get; set; }

    public required string MobileBackgroundImage { get; set; }

    public required string MobileBackgroundImageAltText { get; set; }

    public required string CtaButtonType { get; set; }

    public required string CtaButtonText { get; set; }

    public required string CtaButtonUrl { get; set; }

    public string? WidgetID { get; set; }
}


