using BDO.GWT.Web.Core.DTOs;

namespace BDO.GWT.Core.DTOs.Pages;

public class NewsSectionDto
{
    public string Title { get; set; } = null!;

    public HeroDto Hero { get; set; } = null!;

    public CardsGridDto CardsGrid { get; set; } = null!;
}
