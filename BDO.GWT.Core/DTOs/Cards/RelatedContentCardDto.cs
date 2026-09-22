using BDO.GWT.Core.DTOs;
using System;

namespace BDO.GWT.Core.DTOs.Cards;

public class RelatedContentCardDto
{

public string Title { get; set; } = null!;

public string? Description { get; set; }

public string? ImageURL { get; set; }

public string? ImageAltText { get; set; }

public bool IsFeatured { get; set; }

public bool? isComingSoon { get; set; }

public string? Type { get; set; }

public DateTime? Date { get; set; }

public DateTime? UpdatedOnDate { get; set; } = null!;

public LinkItemDto ReadMoreLink { get; set; } = null!;

public string? DateTextOverride { get; set; }

public DateTime? DateTo { get; set; }
}
