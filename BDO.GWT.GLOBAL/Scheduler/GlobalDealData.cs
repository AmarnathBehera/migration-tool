using System;
using System.Collections.Generic;

namespace BDO.GWT.GLOBAL.API.Data;

public record GlobalDealData
{
    public required string Title { get; set; }

    public required string? Description { get; set; }

    public required string? TeaserImage { get; set; }

    public required string? TeaserImageAltText { get; set; }

    public required List<string> Industries { get; set; }

    public required List<string> SubIndustries { get; set; }

    public required List<string> DealTypes { get; set; }

    public required DateTime Date { get; set; }

    public required string Country { get; set; }

    public required Guid OriginalSiteGUID { get; set; }

    public required Guid OriginalPageGUID { get; set; }

    public required string OriginalPageUrl { get; set; }

    public required DateTime? OriginalModifiedWhen { get; set; }

    public required bool IncludeInSitemap { get; set; }

    public required DateTime? PublishFrom { get; set; }

    public required DateTime? PublishTo { get; set; }

    public required string? RedirectOnUnpublish { get; set; }

    public required bool? IsSearchExcluded { get; set; }
}
