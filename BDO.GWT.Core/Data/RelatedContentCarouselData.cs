using BDO.GWT.Core;
using CMS.ContentEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BDO.GWT.Web.Core.LiveSite.Data;

public class RelatedContentCarouselData : BaseWidgetData
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public bool FilteringEnabled { get; set; }

    public HashSet<string> Filters { get; set; } = new();

    public List<string> SelectedPageTypes { get; set; } = new();

    public IEnumerable<ContentItemReference> FeaturedPageNodeGuids { get; set; } = Enumerable.Empty<ContentItemReference>();

    public string? BackgroundImageUrl { get; set; }

    public string? BackgroundImageAltText { get; set; }

    public string? SeeAllLinkText { get; set; }

    public string? SeeAllLinkUrl { get; set; }

    public string? SeeAllLinkURLTarget { get; set; }

    public string TopMargin { get; set; } = null!;

    public string BottomMargin { get; set; } = null!;
}
