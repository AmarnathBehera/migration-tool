namespace BDO.GWT.Core.DTOs;

public interface ICardsCarouselDto<TCardDto>
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public IEnumerable<TCardDto> Items { get; set; }
}
