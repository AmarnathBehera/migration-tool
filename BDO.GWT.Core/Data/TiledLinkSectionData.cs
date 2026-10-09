
using AngleSharp.Dom;
using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;
using CMS.ContentEngine;
using Kentico.Xperience.Admin.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BDO.GWT.Core.Data;

public class TiledLinkSectionData : BaseWidgetData
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? BackgroundImageMediaFileGuid { get; set; }

    public string? BackgroundImageAltText { get; set; }

    public string Theme { get; set; } = null!;

    public string? SeeAllLinkText { get; set; }

    public string? SeeAllLinkUrl { get; set; }

    public string? SeeAllLinkURLTarget { get; set; }

    public string TopMargin { get; set; } = null!;

    public string BottomMargin { get; set; } = null!;

    public List<TilePageDto>? TileNodeGuids { get; set; }
}
