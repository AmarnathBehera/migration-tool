using BDO.GWT.Core;

namespace BDO.GWT.Common.Core.LiveSite.Data;

public class ImageWithTextData : BaseWidgetData
{
    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string? ButtonTitle { get; set; }

    public string? ButtonURL { get; set; }

    public string? PrimaryButtonLinkTarget { get; set; } 

    public string? ButtonType { get; set; }

    public string MediaType { get; set; } = null!;

    public string? EmbedCode { get; set; }

    public string? ImageURL { get; set; }

    public string? ImageAltText { get; set; }

    public string MediaPosition { get; set; } = null!;

    public string? BackgroundImageURL { get; set; }

    public string? BackgroundImageAltText { get; set; }

    public string Theme { get; set; } = null!;

    public string TopMargin { get; set; } = null!;

    public string BottomMargin { get; set; } = null!;
}
