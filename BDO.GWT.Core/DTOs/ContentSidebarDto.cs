using BDO.GWT.Web.Core.DTOs;
using System;

namespace BDO.GWT.Web.Core.DTOs;

public class ContentSidebarDto : ISidebarDto
{
    public string? TypeName { get; set; }

    public DateTime? PublishDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string RelevantContactPeopleTitle { get; set; } = null!;

    public SidebarPersonDto[] RelevantContactPeople { get; set; } = [];

    public SidebarExternalPersonDto[] RelatedExternalPeople { get; set; } = [];

    public bool ShowPrintButton { get; set; }

    public string? DownloadButtonAttachment { get; set; }

    public bool ShowCopyLinkButton { get; set; }

    public SocialMediaButtonsDto? SocialMediaButtons { get; set; }
}
