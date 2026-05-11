using System;

namespace BDO.GWT.Core.DTOs;


public class MultiLinkCardsGridDto
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public MultiLinkCardDto[]? Cards { get; set; } = Array.Empty<MultiLinkCardDto>();

    public bool IsMultiLinks { get; set; } = true;
}
