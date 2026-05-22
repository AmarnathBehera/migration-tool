namespace BDO.GWT.Core.DTOs;

public class SiteConfigurationDto
{
    public required IDictionary<string, IDictionary<string, string>> Translations { get; set; }
    public SiteSettingsDto SiteSettings { get; set; } = new SiteSettingsDto();
    public string Hash { get; set; } = null!;
}

public class SiteSettingsDto
{
    public bool ExternalLinkWarningEnabled { get; set; }
}
