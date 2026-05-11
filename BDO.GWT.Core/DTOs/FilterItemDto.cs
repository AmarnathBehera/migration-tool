using BDO.GWT.Core.DTOs;

namespace BDO.GWT.Core.DTOs;

public class FilterItemDto<TOptionDto>
    where TOptionDto : OptionDto
{
    public string Title { get; set; } = null!;

    public string Name { get; set; } = null!;

    public TOptionDto[] Options { get; set; } = null!;

    public string Type { get; set; } = null!;

    public string? DefaultValue { get; set; }
}
