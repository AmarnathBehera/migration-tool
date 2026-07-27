using BDO.GWT.GLOBAL.API.Data;
using System.ComponentModel.DataAnnotations;

namespace BDO.GWT.GLOBAL.API.Data;
public class LocationOfficePageContent : IMetadataContent
{
    [Required]
    [MaxLength(200)]
    public string MetadataTitle { get; set; } = null!;

    public string? MetadataDescription { get; set; }

    [MaxLength(200)]
    public string? MetadataTeaserImage { get; set; }

    [Required]
    [MaxLength(200)]
    public string? MetadataTeaserImageAltText { get; set; }

    [Required]
    [MaxLength(200)]
    public string OfficeImage { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string OfficeImageAltText { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string LocationAddressLine1 { get; set; } = null!;

    [MaxLength(200)]
    public string? LocationAddressLine2 { get; set; }

    [MaxLength(2000)]
    public string? MetadataCanonical { get; set; }

    [Required]
    [MaxLength(200)]
    public string LocationZip { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string LocationZipDisplayType { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string LocationCountry { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string LocationLatitude { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string LocationLongitude { get; set; } = null!;

    [MaxLength(200)]
    public string? LocationEmail { get; set; }

    public int LocationType { get; set; }

    [Required]
    [MaxLength(20)]
    public string LocationPhone { get; set; } = null!;

    [MaxLength(20)]
    public string? LocationFax { get; set; }

    [MaxLength(200)]
    public string? OpeningHours { get; set; }

    [MaxLength(200)]
    public string? LocationPOBoxNumber { get; set; }

    [MaxLength(200)]
    public string? LocationPOCity { get; set; }

    [MaxLength(200)]
    public string? LocationPOZip { get; set; }

    [MaxLength(200)]
    public string? LocationPOZipDisplayType { get; set; }

    [MaxLength(200)]
    public string? LocationPOCountry { get; set; }

    [MaxLength(60)]
    public string? MetadataOGTitle { get; set; }

    [MaxLength(500)]
    public string? MetadataOGDescription { get; set; }

    [MaxLength(500)]
    public string? MetadataOGImage { get; set; }

    public bool MetadataOGTwitterImageSizeCheckbox { get; set; }

    [MaxLength(500)]
    public string? MetadataOGTwitterImage { get; set; }

    public bool MetadataNofollow { get; set; }

    public bool MetadataNoindex { get; set; }

    public bool IncludeInSitemap { get; set; }

    public DateTime? PublishFrom { get; set; }

    public DateTime? PublishTo { get; set; }

    public string? RedirectOnUnpublish { get; set; }
}
