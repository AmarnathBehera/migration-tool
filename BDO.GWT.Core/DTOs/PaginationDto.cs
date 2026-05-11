namespace BDO.GWT.Core.DTOs;

public class PaginationDto<TCardDto>
{
    public TCardDto[] Data { get; set; } = null!;

    public int TotalCount { get; set; }
}
