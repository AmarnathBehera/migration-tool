using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;

namespace BDO.GWT.Common.Core.LiveSite.Data;

public class QuoteCardData : BaseWidgetData
{
    public QuoteCard Data { get; set; } = null!;

    public string TopMargin { get; set; } = null!;

    public string BottomMargin { get; set; } = null!;
}

public class QuoteCard
{
    public string? AuthorsJobTitle { get; set; }

    public LinkItemDto Button { get; set; } = null!;

    public string? PrimaryButtonLinkTarget { get; set; }

    public string Size { get; set; } = "standard";

    public string? Heading { get; set; }

    public string? ImageURL { get; set; }

    public string? ImageAltText { get; set; }

    public string? SubHeading { get; set; }

    public string QuoteText { get; set; } = null!;

    public string QuotesAuthor { get; set; } = null!;
}
