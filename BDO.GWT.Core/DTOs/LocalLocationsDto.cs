using System.Collections.Generic;

namespace BDO.GWT.Core.DTOs;

public class LocalLocationsDto
{
    public string? Title { get; set; }

    public IEnumerable<LinkItemDto> Items { get; set; } = new List<LinkItemDto>(); 
}
