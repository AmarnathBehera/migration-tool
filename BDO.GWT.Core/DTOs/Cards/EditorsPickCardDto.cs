using System;

namespace BDO.GWT.Core.DTOs.Cards;

public class EditorsPickCardDto : CarouselCardDto
{
    public string? DateTextOverride { get; set; }

    public DateTime? DateTo { get; set; }
}
