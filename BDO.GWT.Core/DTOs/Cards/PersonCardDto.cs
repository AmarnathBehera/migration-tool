using System.Collections.Generic;

namespace BDO.GWT.Core.DTOs.Cards;

public class PersonCardDto
{
    public string DisplayName { get; set; } = null!;

    public string? TeaserImageUrl { get; set; }

    public string? TeaserImageAltText { get; set; }

    public string? JobTitle { get; set; }

    public string? PersonUrl { get; set; }

    public IEnumerable<string>? OfficeNames { get; set; }
}
