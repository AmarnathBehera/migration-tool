using BDO.GWT.Core.ValueObjects;
namespace BDO.GWT.Core.Data;

public class HeroData
{
    public string? Layout { get; set; } = null!;

    public string? Title { get; set; }

    public string? Description { get; set; }

    public Link? BackgroundImage { get; set; }

    public string? BackgroundImageAltText { get; set; }

    public Link? MobileBackgroundImage { get; set; }

    public string? MobileBackgroundImageAltText { get; set; }

    public string? CallToActionButtonType { get; set; }

    public string? CallToActionButtonText { get; set; }

    public Link? CallToActionButtonUrl { get; set; }

    public string? CtaButtonUrlTarget { get; set; }

    public string? WidgetID { get; set; }
}
