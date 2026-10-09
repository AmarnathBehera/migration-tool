namespace BDO.GWT.Core.DTOs.Pages;

public class ContentPageDto
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? Content { get; set; }

    public ContentSidebarDto? Sidebar { get; set; }

    public CallToActionWithPositionDto? CallToActionWithPosition { get; set; }

    public CallToActionDto? CallToAction { get; set; }

    public ContentPageDto() { }

    public ContentPageDto(ContentPageDto contentPageDto)
    {
        Title = contentPageDto.Title;
        Description = contentPageDto.Description;
        Content = contentPageDto.Content;
        Sidebar = contentPageDto.Sidebar;
        CallToAction = contentPageDto.CallToAction;
    }
}
