using CMS.ContentEngine;

namespace BDO.GWT.Core.DTOs;

public class EventsWebinarsSliderDto : ICardsCarouselDto<EventsWebinarsCardDto>, IFilterableComponentDto
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? BackgroundImageURL { get; set; }

    public string? BackgroundImageAltText { get; set; }

    public string ApiEndpoint { get; set; } = "EventsWebinarsSlider/Items";

    public FilterItemDto<OptionDto>[]? Filters { get; set; }

    public required bool FilteringTooltipEnabled { get; set; }

    public IEnumerable<EventsWebinarsCardDto> Items { get; set; } = null!;

    public LinkItemDto? SeeAllLink { get; set; }

    public string TopMargin { get; set; } = null!;

    public string BottomMargin { get; set; } = null!;

    public string? WidgetID { get; set; }

    public Guid? CurrentPageGuid { get; set; }

    public string? PageType { get; set; }
}
