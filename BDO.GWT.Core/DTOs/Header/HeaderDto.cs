using System.Collections.Generic;
using BDO.GWT.Core.DTOs;

namespace BDO.GWT.Core.DTOs.Header;

public class HeaderDto
{
    public IEnumerable<LinkItemDto> MenuItems { get; set; } = null!;

    public IEnumerable<CultureDto> Cultures { get; set; } = null!;

    public LinkItemDto? Contact { get; set; }

    public string SiteName { get; set; } = null!;

    public string LogoURL { get; set; } = null!;

    public string LogoImageURL { get; set; } = null!;

    public string SearchPageURL { get; set; } = null!;

    public IEnumerable<FirstLevelMenuItemDto> MegaMenuItems { get; set; } = null!;
}
