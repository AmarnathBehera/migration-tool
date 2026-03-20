namespace BDO.GWT.Core.DTOs.Footer;

public class SocialButtonDto
{
    public string AltText { get; set; } = null!;

    public string AriaLabel { get; set; } = null!;

    public string IconType { get; set; } = null!;

    public string? CustomIcon { get; set; }

    public int Order { get; set; }

    public string URL { get; set; } = null!;
}
