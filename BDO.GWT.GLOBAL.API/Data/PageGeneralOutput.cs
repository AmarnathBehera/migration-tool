namespace BDO.GWT.GLOBAL.API.Data;

public class PageGeneralOutput
{
    public string Name { get; set; } = null!;

    public string Culture { get; set; } = null!;

    public int? NodeParentID { get; set; }

    public int NodeID { get; set; }

    public int PageID { get; set; }

    public Guid NodeGUID { get; set; }

    public Guid PageGUID { get; set; }

    public string NodeAliasPath { get; set; } = null!;
}
