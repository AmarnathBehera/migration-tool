using System.Collections.Generic;

namespace BDO.GWT.Core.DTOs;

public class LocationsMainDto
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? ListTitle { get; set; }

    public IEnumerable<LocalLocationsDto> Locations { get; set; } = new List<LocalLocationsDto>();
}
