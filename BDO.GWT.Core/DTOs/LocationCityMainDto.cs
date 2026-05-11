using System.Collections.Generic;

namespace BDO.GWT.Core.DTOs;

public class LocationCityMainDto
{
    public string? Title { get; set; }

    public string? Content { get; set; }

    public string? SectionTitle { get; set; }

    public IEnumerable<OfficeCardDto> OfficeCards { get; set; } = new List<OfficeCardDto>();
}