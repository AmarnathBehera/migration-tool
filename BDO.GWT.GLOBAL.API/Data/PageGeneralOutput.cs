namespace BDO.GWT.GLOBAL.API.Data;

public class PageGeneralOutput
{
    public string Name { get; set; } = null!;

    public string Culture { get; set; } = null!;

    public int? WebPageItemParentID { get; set; }

    public int WebPageItemID { get; set; }

    public int ContentItemID { get; set; }

    public Guid ContentItemGuid { get; set; }

    public Guid WebPageItemGuid { get; set; }

    public string Path { get; set; } = null!;
}
