using BDO.GWT.Core.DTOs;

namespace BDO.GWT.Core.DTOs.Cards;

public class EditorsPickCardDto
{
    public bool IsFeatured { get; set; }

    public string? Type { get; set; }

    public string? ContentType { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? ImageURL { get; set; }

    public string? ImageAltText { get; set; }

    public DateTime? Date { get; set; }

    public DateTime? UpdatedOnDate { get; set; }

    public LinkItemDto? ReadMoreLink { get; set; }

    public string? DateTextOverride { get; set; }

    public DateTime? DateTo { get; set; }
}