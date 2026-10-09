namespace BDO.GWT.Core.DTOs;

public class GlobalLocationsGridDto
{
    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public List<GlobalLocationsRegionDto> Locations { get; set; } = null!;
}
