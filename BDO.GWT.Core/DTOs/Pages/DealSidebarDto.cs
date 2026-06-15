using BDO.GWT.Core.DTOs;

namespace BDO.GWT.Web.Core.DTOs.Pages;

public class DealSidebarDto : ISidebarDto
{
    public DateTime Date { get; set; }

    public SidebarPersonDto[] Contacts { get; set; } = Array.Empty<SidebarPersonDto>();

    public bool ShowPrintButton { get; set; }

    public string? DownloadButtonAttachment { get; set; }

    public bool ShowCopyLinkButton { get; set; }

    public SocialMediaButtonsDto? SocialMediaButtons { get; set; }

    public CallToActionDto? CallToAction { get; set; }
}
