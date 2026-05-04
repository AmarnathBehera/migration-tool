namespace BDO.GWT.Core.DTOs;

public record SiteSettingsDto
{
    public required bool ExternalLinkWarningEnabled { get; init; }
}