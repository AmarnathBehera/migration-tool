namespace BDO.GWT.Core.DTOs;

public class ContentSidebarDto : ISidebarDto
{
    public string? TypeName { get; set; }

    public DateTime? PublishDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string RelevantContactPeopleTitle { get; set; } = null!;

    /// <summary>
    /// List of relevant contact people for the content item.
    /// This is typically used to display the authors or other relevant contacts in the sidebar of a content page.
    /// </summary>
    public SidebarPersonDto[] RelevantContactPeople { get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of external individuals associated with the sidebar.
    /// </summary>
    /// <remarks>Use this property to access or assign external people who are relevant to the sidebar, such
    /// as collaborators or contacts. The array may be empty if there are no associated external individuals.</remarks>
    public SidebarExternalPersonDto[] RelatedExternalPeople { get; set; } = [];

    public bool ShowPrintButton { get; set; }

    public string? DownloadButtonAttachment { get; set; }

    public bool ShowCopyLinkButton { get; set; }

    public SocialMediaButtonsDto? SocialMediaButtons { get; set; }
}
