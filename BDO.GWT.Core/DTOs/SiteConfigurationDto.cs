namespace BDO.GWT.Core.DTOs;

public class SiteConfigurationDto
{
    public required IDictionary<string, IDictionary<string, string>> Translations { get; set; }
    public string Hash { get; set; } = null!;
}
