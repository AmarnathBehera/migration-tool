using BDO.GWT.GLOBAL.API.Data;
using System.ComponentModel.DataAnnotations;

namespace BDO.GWT.GLOBAL.API.Data;

public class InsightPageContent : IMetadataContent
{
    [Required]
    [MaxLength(200)]
    public string MetadataTitle { get; set; } = null!;

    [Required]
    public bool ShowTitle { get; set; }

    public string? MetadataDescription { get; set; }

    [Required]
    public bool ShowDescription { get; set; }

    public string? Content { get; set; }

    [MaxLength(200)]
    public string? MetadataTeaserImage { get; set; }

    [Required]
    [MaxLength(200)]
    public string MetadataTeaserImageAltText { get; set; } = null!;

    [MaxLength(2000)]
    public string? MetadataCanonical { get; set; }

    public bool ComingSoon { get; set; }

    public DateTime? UpdatedOn { get; set; }

    [Required]
    public bool ShowSidebar { get; set; }

    [Required]
    public bool ShowCallToAction { get; set; }

    [MaxLength(200)]
    public string? CallToActionDescription { get; set; }

    [MaxLength(50)]
    public string? CallToActionButtonText { get; set; }

    [MaxLength(200)]
    public string? CallToActionButtonUrl { get; set; }

    [MaxLength(200)]
    public string? CallToActionPosition { get; set; }

    [Required]
    public bool ShowPrintButton { get; set; }

    [MaxLength(200)]
    public string? DownloadButtonAttachment { get; set; }

    [Required]
    public bool ShowSocialMediaButtons { get; set; }

    public bool UseNarrowLayout { get; set; }

    public bool ShowAuthorsAtTheTop { get; set; }

    [Required]
    [MaxLength(250)]
    public string Image { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string ImageAltText { get; set; } = null!;

    [MaxLength(200)]
    public string? InsightAttachment { get; set; }

    [MaxLength(200)]
    public string? DownloadButtonTitle { get; set; }

    [MaxLength(200)]
    public string? DownloadButtonText { get; set; }

    [MaxLength(200)]
    public List<string> MetadataBusinessIssues { get; set; } = [];

    [MaxLength(1000)]
    public List<string> ContentType { get; set; } = [];

    [MaxLength(200)]
    public List<string> MetadataBusinessLines { get; set; } = [];

    [MaxLength(200)]
    public List<string> MetadataServiceAreas { get; set; } = [];

    [MaxLength(200)]
    public List<string> MetadataServices { get; set; } = [];

    [MaxLength(200)]
    public List<string> MetadataIndustryCategories { get; set; } = [];

    [MaxLength(200)]
    public List<string> MetadataIndustries { get; set; } = [];

    [MaxLength(200)]
    public List<string> MetadataSpecialtiesCategories { get; set; } = [];

    [MaxLength(200)]
    public List<string> MetadataSpecialtiesAreas { get; set; } = [];

    [MaxLength(200)]
    public List<string> MetadataSpecialtiesPages { get; set; } = [];

    [MaxLength(200)]
    public List<string> MetadataOfficeLocation { get; set; } = [];

    [MaxLength(200)]
    public List<string> MicrositeMasterPages { get; set; } = [];

    [MaxLength(200)]
    public List<string> MicrositePages { get; set; } = [];

    [MaxLength(200)]
    public List<string> MetadataRssCategory { get; set; } = [];

    [MaxLength(200)]
    public string? MetadataRssImage { get; set; }

    [MaxLength(2000)]
    public List<string> Insights { get; set; } = [];

    [MaxLength(1000)]
    public List<string> RelevantContactPerson { get; set; } = [];

    public List<string> PeopleSort { get; set; } = [];

    [MaxLength(1000)]
    public List<string> RelatedExternalPeople { get; set; } = [];

    [MaxLength(200)]
    public string? SharingExplanation { get; set; }

    public bool SharingOptOut { get; set; }

    public bool SharingHeadline { get; set; }

    [MaxLength(10)]
    public string? OriginalDocumentAttachmentType { get; set; }

    [MaxLength(200)]
    public List<string> OriginalIndustries { get; set; } = [];

    [MaxLength(200)]
    public List<string> OriginalServiceLines { get; set; } = [];

    public bool OriginalInvolvedCountriesAll { get; set; }

    [MaxLength(200)]
    public List<string> OriginalInvolvedCountries { get; set; } = [];

    public bool OriginalDocumentOwnerOptOut { get; set; }

    [MaxLength(200)]
    public string? OriginalDocumentOwnerName { get; set; }

    [MaxLength(200)]
    public string? OriginalDocumentOwnerEmail { get; set; }

    public Guid OriginalSiteGuid { get; set; }

    public Guid OriginalDocumentGuid { get; set; }

    [MaxLength(20)]
    public string? SharingSourceFirm { get; set; }

    public string? SharingInstructions { get; set; }

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
