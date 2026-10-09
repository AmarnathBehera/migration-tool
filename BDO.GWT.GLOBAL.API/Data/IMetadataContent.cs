using System.ComponentModel.DataAnnotations;

namespace BDO.GWT.GLOBAL.API.Data;

public interface IMetadataContent : IPageContent
{
    [Required]
    public string MetadataTitle { get; set; }

    [Required]
    public string? MetadataDescription { get; set; }

    //[MaxLength(200)]
    //public string? MetadataMenuGroup { get; set; }

    [MaxLength(200)]
    public string? MetadataTeaserImage { get; set; }

    [Required]
    [MaxLength(200)]
    public string MetadataTeaserImageAltText { get; set; }

    [MaxLength(2000)]
    public string? MetadataCanonical { get; set; }

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
}
