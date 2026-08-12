using BDO.GWT.GLOBAL.API.Data;
using System.ComponentModel.DataAnnotations;

namespace BDO.GWT.GLOBAL.API.Data;

public class InsightPageContent : IMetadataContent
{
    [Required]
    [MaxLength(200)]
    public string MetadataTitle { get; set; } = null!;

    public string? MetadataDescription { get; set; }

    [MaxLength(2000)]
    public List<String> InsightContent { get; set; } = [];

    [MaxLength(200)]
    public string? MetadataTeaserImage { get; set; }

    [Required]
    [MaxLength(200)]
    public string MetadataTeaserImageAltText { get; set; } = null!;

    [MaxLength(2000)]
    public string? MetadataCanonical { get; set; }

    public bool ComingSoon { get; set; }

    [Required]
    public bool ShowSidebar { get; set; }

    [Required]
    public bool ShowPrintButton { get; set; }

    public bool ShowInsightPrintButton { get; set; }

    [MaxLength(200)]
    public string? DownloadButtonAttachment { get; set; }

    [Required]
    public bool ShowSocialMediaButtons { get; set; }

    [MaxLength(200)]
    public string? Sharingbuttons { get; set; }

    public bool UseNarrowLayout { get; set; }

    public bool ShowAuthorsAtTheTop { get; set; }

    [MaxLength(200)]
    public string? Layoutwithoutsidebar { get; set; }

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

    [MaxLength(1000)]
    public List<string> RelatedExternalPeople { get; set; } = [];

    public bool IsSearchExcluded { get; set; }

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

    [MaxLength(500)]
    public string? PageDescription { get; set; }

    [MaxLength(500)]
    public string? PageKeywords { get; set; }
}