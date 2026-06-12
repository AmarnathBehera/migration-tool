namespace BDO.GWT.Core.DTOs.ReusableFieldsDTOs;

public class ItemListDto
{
    public string ItemListTitle { get; set; } = string.Empty;

    public string ItemListDescription { get; set; } = string.Empty;

    public bool ItemListFilteringEnabled { get; set; }

    public string ItemListAdvancedSearchDescription { get; set; } = string.Empty;

    public string ItemListAdvancedSearchLinkText { get; set; } = string.Empty;

    public string ItemListAdvancedSearchLinkUrl { get; set; } = string.Empty;
}