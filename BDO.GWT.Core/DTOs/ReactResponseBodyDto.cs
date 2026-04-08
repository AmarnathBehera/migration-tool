namespace BDO.GWT.Core.DTOs;

public record ReactResponseBodyDto
{
    public required string HtmlContent { get; set; }

    public required string ContainerId { get; set; }
}