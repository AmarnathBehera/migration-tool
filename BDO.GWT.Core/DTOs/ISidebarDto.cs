namespace BDO.GWT.Core.DTOs;

public interface ISidebarDto
{
    public bool ShowPrintButton { get; set; }

    public string? DownloadButtonAttachment { get; set; }

    public SocialMediaButtonsDto? SocialMediaButtons { get; set; }

    public bool ShowCopyLinkButton { get; set; }
}
