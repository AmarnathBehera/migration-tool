using System.Collections.Generic;

namespace BDO.GWT.Core.DTOs;

public class LegalPageDto
{
    public string? Title { get; set; }

    public string BodyContent { get; set; } = null!;

    public IEnumerable<LinkItemDto> Links { get; set; } = null!;
}
