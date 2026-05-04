using BDO.GWT.Web.Core.DTOs;

namespace BDO.GWT.Core.DTOs;

public class MultiLinkPageDto
{
    public HeroDto Hero { get; set; } = null!;

    public MultiLinkCardsGridDto MultiLinksCardsGrid { get; set; } = null!;
}
