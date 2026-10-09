using BDO.GWT.Core.DTOs;
using System.Collections.Generic;

namespace BDO.GWT.Core.DTOs;

public class SideBoxDto
{
    public string? Title { get; set; }

    public IEnumerable<LinkItemDto> Links { get; set; } = null!;
}
