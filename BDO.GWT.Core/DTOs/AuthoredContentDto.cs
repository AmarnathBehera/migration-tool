using BDO.GWT.Core.DTOs.Cards;

namespace BDO.GWT.Core.DTOs;

public class AuthoredContentDto : ICardsCarouselDto<AuthoredContentCardDto>
{
    public string? BackgroundImageURL { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public IEnumerable<AuthoredContentCardDto> Items { get; set; } = Array.Empty<AuthoredContentCardDto>();
}
