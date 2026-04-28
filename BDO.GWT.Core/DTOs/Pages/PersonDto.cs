namespace BDO.GWT.Core.DTOs.Pages;

public class PersonDto
{
    public string DisplayName { get; set; } = null!;

    public string? TeaserImageUrl { get; set; }

    public string? TeaserImageAltText { get; set; }

    public string? Summary { get; set; }

    public string? JobTitle { get; set; }

    public IEnumerable<string> Credentials { get; set; } = null!;

    public IEnumerable<string> IndustryCategoryNames { get; set; } = null!;

    public IEnumerable<string> ServiceAreaNames { get; set; } = null!;

    public IEnumerable<string> SpecialtiesCategoryNames { get; set; } = null!;

    public string? VCardUrl { get; set; }

    public IEnumerable<LinkItemDto> OfficeLocations { get; set; } = null!;

    public IEnumerable<LinkItemDto> City { get; set; } = null!;

    public string? FacebookUrl { get; set; }

    public string? TwitterUrl { get; set; }

    public string? LinkedInUrl { get; set; }

    public string? OfficePhoneNumber { get; set; }

    public string? MobilePhoneNumber { get; set; }

    public string? DirectDialNumber { get; set; }

    public string? Email { get; set; }

    public bool DisplayFullEmailAddress { get; set; }

    public bool EnableForm { get; set; }

    public bool ParentEnableForm { get; set; }

 //   public AuthoredContentDto? AuthoredContent { get; set; }
}
