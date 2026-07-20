using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.GLOBAL.API.Data;
using System.ComponentModel.DataAnnotations;

namespace BDO.GWT.GLOBAL.API.Data;

public class PersonContent : IMetadataContent
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
    public string? MetadataTeaserImageAltText { get; set; }

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

    [MaxLength(200)]
    public List<Guid> PersonCredentials { get; set; } = [];

    [MaxLength(200)]
    public List<Guid> MetadataCity { get; set; } = [];

    [MaxLength(200)]
    public List<Guid> MetadataOfficeLocation { get; set; } = [];

    [MaxLength(4000)]
    public List<Guid> MetadataProfileGroups { get; set; } = [];

    [MaxLength(4000)]
    public List<Guid> MetadataBusinessLines { get; set; } = [];

    [MaxLength(200)]
    public List<Guid> MetadataServiceAreas { get; set; } = [];

    [MaxLength(200)]
    public List<Guid> MetadataServices { get; set; } = [];

    [MaxLength(200)]
    public List<Guid> MetadataSubServices { get; set; } = [];

    [MaxLength(200)]
    public List<Guid> MetadataIndustryCategories { get; set; } = [];

    [MaxLength(200)]
    public List<Guid> MetadataIndustries { get; set; } = [];

    [MaxLength(200)]
    public List<Guid> MetadataIndustryServices { get; set; } = [];

    [MaxLength(200)]
    public List<Guid> MetadataSpecialtiesCategories { get; set; } = [];

    [MaxLength(200)]
    public List<Guid> MetadataSpecialtiesAreas { get; set; } = [];

    [MaxLength(200)]
    public List<Guid> MetadataSpecialtiesPages { get; set; } = [];

    [MaxLength(200)]
    public List<Guid> MicrositeMasterPages { get; set; } = [];

    [MaxLength(200)]
    public List<Guid> MicrositePages { get; set; } = [];

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
