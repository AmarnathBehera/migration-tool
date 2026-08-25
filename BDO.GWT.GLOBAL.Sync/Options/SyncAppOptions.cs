using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace BDO.GWT.GLOBAL.Sync.Options;

public record SyncAppOptions
{
    [Required]
    public required string SyncApiKey { get; set; }

    [Required]
    public required Region[] Regions { get; set; }

    [NotNull]
    public required Guid GlobalSiteGuid { get; set; }

    public required TimeSpan RegionSitesCacheDuration { get; set; } = TimeSpan.FromHours(1);
}
