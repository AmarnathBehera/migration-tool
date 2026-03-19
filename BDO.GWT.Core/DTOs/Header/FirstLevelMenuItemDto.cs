using System.Collections.Generic;

namespace BDO.GWT.Core.DTOs.Header;

public class FirstLevelMenuItemDto : LinkItemDto
{
    public IEnumerable<FeaturedMenuItemDto>? Featured { get; set; }

    public IEnumerable<SecondLevelMenuItemDto> Children { get; set; }
}