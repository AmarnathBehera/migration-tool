using BDO.GWT.GLOBAL.API.Data;
using System.ComponentModel.DataAnnotations;

namespace BDO.GWT.GLOBAL.API.Data;

public class SectionEventsPageContent : IMetadataContent
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

    [MaxLength(2000)]
    public string? MetadataCanonical { get; set; }

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

    [MaxLength(200)]
    public string? EventsWebinarsListTitle { get; set; }

    [MaxLength(200)]
    public string? EventsWebinarsListDescription { get; set; }

    public bool EventsWebinarsListFilteringEnabled { get; set; }

    [Required]
    [MaxLength(200)]
    public HashSet<string> EventsWebinarsListFilters { get; set; } = [];

    [MaxLength(200)]
    public List<string> EventsWebinarsListFeaturedPages { get; set; } = [];

    [MaxLength(200)]
    public string? EventsWebinarsListAdvancedSearchDescription { get; set; }

    [MaxLength(200)]
    public string? EventsWebinarsListAdvancedSearchLinkText { get; set; }

    [MaxLength(200)]
    public string? EventsWebinarsListAdvancedSearchLinkUrl { get; set; }

    public bool ShowPastEvents { get; set; }

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
