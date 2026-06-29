using BDO.GWT.Core.DTOs.Cards;


namespace BDO.GWT.Core.DTOs;

public class EditorsPickDto : ICardsCarouselDto<EditorsPickCardDto>, IFilterableComponentDto
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public string ApiEndpoint { get; set; } = "editorsPick/items";

    public IEnumerable<FilterItemDto<OptionDto>>? Filters { get; set; }

    public required bool FilteringTooltipEnabled { get; set; }

    public IEnumerable<EditorsPickCardDto> Items { get; set; } = null!;

    public string? BackgroundImageURL { get; set; }

    public string? BackgroundImageAltText { get; set; }

    public LinkItemDto? SeeAllLink { get; set; }

    public string TopMargin { get; set; } = null!;

    public string BottomMargin { get; set; } = null!;

    public string? WidgetID { get; set; }

    public Guid StateGUID { get; set; }
}
