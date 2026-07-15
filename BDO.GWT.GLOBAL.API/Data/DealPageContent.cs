using System.ComponentModel.DataAnnotations;

namespace BDO.GWT.GLOBAL.API.Data;

public class DealPageContent : IMetadataContent
{
    [Required]
    [MaxLength(200)]
    public string MetadataTitle { get; set; } = null!;

    [Required]
    public bool ShowTitle { get; set; }

    public string? MetadataDescription { get; set; }

    public string? DealContent { get; set; }

    [MaxLength(200)]
    public string? MetadataTeaserImage { get; set; }

    [Required]
    [MaxLength(200)]
    public string? MetadataTeaserImageAltText { get; set; }

    [MaxLength(1000)]
    public List<string> Industry { get; set; } = [];

    [MaxLength(1000)]
    public List<string> SubIndustry { get; set; } = [];

    [MaxLength(1000)]
    public List<string> DealType { get; set; } = [];

    [Required]
    public DateTime? DateField { get; set; }

    [MaxLength(1000)]
    public List<string>? Size { get; set; } = [];

    [MaxLength(200)]
    public string? MetadataOfficeLocation { get; set; }

    [MaxLength(99)]
    public string? ClientName { get; set; }

    [MaxLength(2000)]
    public string? MetadataCanonical { get; set; }

    [MaxLength(200)]
    public string RelatedDeals { get; set; } = [];

    [MaxLength(200)]
    public string MetadataRssCategory { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? MetadataRssImage { get; set; }

    [Required]
    [MaxLength(200)]
    public string HeroLayout { get; set; } = null!;

    [MaxLength(200)]
    public string? HeroTitle { get; set; }

    [MaxLength(200)]
    public string? HeroDescription { get; set; }

    [Required]
    [MaxLength(200)]
    public string? HeroBackgroundImage { get; set; }

    [Required]
    [MaxLength(200)]
    public string? HeroBackgroundImageAltText { get; set; }

    [Required]
    [MaxLength(200)]
    public string? HeroBackgroundImage_Mobile { get; set; }

    [Required]
    [MaxLength(200)]
    public string? HeroBackgroundImage_MobileAltText { get; set; }

    [MaxLength(200)]
    public string? HeroCallToActionButtonType { get; set; }

    [Required]
    [MaxLength(200)]
    public string? HeroCallToActionButtonText { get; set; }

    [Required]
    [MaxLength(200)]
    public string? HeroCallToActionButtonUrl { get; set; }

    [Required]
    public bool ShowSidebar { get; set; }

    [Required]
    public bool ShowPrintButton { get; set; }

    [MaxLength(200)]
    public string? DownloadButtonAttachment { get; set; }

    [Required]
    public bool ShowCallToAction { get; set; }

    [MaxLength(200)]
    public string? CallToActionDescription { get; set; }

    [MaxLength(50)]
    public string? CallToActionButtonText { get; set; }

    [MaxLength(200)]
    public string? CallToActionButtonUrl { get; set; }

    [Required]
    public bool ShowSocialMediaButtons { get; set; }

    [Required]
    public bool SharingOptIn { get; set; }

    [MaxLength(200)]
    public string? DealCountry { get; set; }

    public string DisplayedContacts { get; set; }

    [MaxLength(200)]
    public string RelevantContactPerson { get; set; } = string.Empty;

    public List<Guid> PeopleSort { get; set; } = [];

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
