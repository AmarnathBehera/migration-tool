using BDO.GWT.Core.DTOs.Cards;
using System.Collections.Generic;

namespace BDO.GWT.Core.DTOs;

public class PeopleGridDto : ICardsCarouselDto<PersonCardDto>
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public IEnumerable<PersonCardDto> Items { get; set; } = null!;

    public IEnumerable<PersonCardDto>? ExternalItems { get; set; }

    public bool? UseNameAsLink { get; set; }

    public LinkItemDto? SeeAllLink { get; set; }

    public string TopMargin { get; set; } = null!;

    public string BottomMargin { get; set; } = null!;

    public string? WidgetID { get; set; }
}
