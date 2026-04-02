using System.Collections.Generic;

namespace BDO.GWT.Core.DTOs.Header;

public class SecondLevelMenuItemDto : LinkItemDto
{
    public string? Description { get; set; }

    public IEnumerable<ThirdLevelMenuItemDto> Children { get; set; }
}