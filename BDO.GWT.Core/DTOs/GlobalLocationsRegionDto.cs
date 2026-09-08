namespace BDO.GWT.Core.DTOs;

public class GlobalLocationsRegionDto
{
    public string? Region { get; set; }

    public List<LinkItemDto> RegionalLocations { get; set; } = null!;
}
