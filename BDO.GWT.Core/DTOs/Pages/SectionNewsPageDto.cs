namespace BDO.GWT.Core.DTOs.Pages;

public class SectionNewsPageDto
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? Content { get; set; }

    public ContentSidebarDto? Sidebar { get; set; }

    public CallToActionDto? CallToAction { get; set; }

    public SectionNewsPageDto() { }

    public SectionNewsPageDto(SectionNewsPageDto newsPageDto)
    {
        Title = newsPageDto.Title;
        Description = newsPageDto.Description;
        Content = newsPageDto.Content;
        Sidebar = newsPageDto.Sidebar;
        CallToAction = newsPageDto.CallToAction;
    }
}
