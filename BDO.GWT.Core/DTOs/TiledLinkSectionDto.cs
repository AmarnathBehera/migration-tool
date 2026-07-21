#nullable enable
using BDO.GWT.Core.DTOs;
using System;
using System.Collections.Generic;

namespace BDO.GWT.Web.Core.DTOs;

public class TiledLinkSectionDto
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? BackgroundImageURL { get; set; }

    public string? BackgroundImageAltText { get; set; }

    public List<TilePageDto> Items { get; set; } = new();

    public string Theme { get; set; } = null!;

    public LinkItemDto? SeeAllLink { get; set; }

    public string? TopMargin { get; set; }

    public string? BottomMargin { get; set; }

    public string? WidgetID { get; set; }
}
