using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.GLOBAL.API.Data;
using System.ComponentModel.DataAnnotations;

namespace BDO.GWT.GLOBAL.API.Data;

public class PersonPageContent : IMetadataContent
{
    [Required]
    [MaxLength(200)]
    public string MetadataTitle { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string PersonFirstName { get; set; } = null!;

    [MaxLength(200)]
    public string? PersonSurnamePrefixes { get; set; }

    [Required]
    [MaxLength(200)]
    public string PersonLastName { get; set; } = null!;

    [MaxLength(200)]
    public string? MetadataTeaserImage { get; set; }
            
    [Required]
    [MaxLength(200)]
    public string MetadataTeaserImageAltText { get; set; } = null!;

    [MaxLength(2000)]
    public string? MetadataCanonical { get; set; }

    public string? MetadataDescription { get; set; }

    [MaxLength(200)]
    public string? PersonJobTitle { get; set; }

    [MaxLength(200)]
    public string? PersonPhone { get; set; }

    [MaxLength(200)]
    public string? PersonMobilePhone { get; set; }

    [MaxLength(200)]
    public string? PersonDirectDialNumber { get; set; }

    [MaxLength(200)]
    public string? PersonEmail { get; set; }

    public bool EnableForm { get; set; }

    public bool EnableVCard { get; set; }

    [MaxLength(300)]
    public List<String> PersonCredentials { get; set; } = [];

    [MaxLength(300)]
    public List<String> MetadataCity { get; set; } = [];

    [MaxLength(300)]
    public List<String> MetadataOfficeLocation { get; set; } = [];

    [MaxLength(300)]
    public List<String> MetadataProfileGroups { get; set; } = [];

    [MaxLength(300)]
    public List<String> MetadataBusinessLines { get; set; } = [];

    [MaxLength(300)]
    public List<String> MetadataServiceAreas { get; set; } = [];

    [MaxLength(300)]
    public List<String> MetadataServices { get; set; } = [];

    [MaxLength(300)]
    public List<String> MetadataSubServices { get; set; } = [];

    [MaxLength(300)]
    public List<String> MetadataIndustryCategories { get; set; } = [];

    [MaxLength(300)]
    public List<String> MetadataIndustries { get; set; } = [];

    [MaxLength(300)]
    public List<String> MetadataIndustryServices { get; set; } = [];

    [MaxLength(300)]
    public List<String> MetadataSpecialtiesCategories { get; set; } = [];

    [MaxLength(300)]
    public List<String> MetadataSpecialtiesAreas { get; set; } = [];

    [MaxLength(300)]
    public List<String> MetadataSpecialtiesPages { get; set; } = [];

    [MaxLength(300)]
    public List<String> MicrositeMasterPages { get; set; } = [];

    [MaxLength(300)]
    public List<String> MicrositePages { get; set; } = [];

    [MaxLength(200)]
    public string? Facebook { get; set; }

    [MaxLength(200)]
    public string? Twitter { get; set; }

    [MaxLength(200)]
    public string? LinkedIn { get; set; }

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
