namespace BDO.GWT.Core.DTOs;

public class SiteConfigurationDto
{
    public required IDictionary<string, IDictionary<string, string>> Translations { get; set; }

    public required DateTimeFormatsDto DateTimeFormats { get; set; }

    public required SiteSettingsDto SiteSettings { get; set; }

    public string Hash { get; set; } = null!;
}
