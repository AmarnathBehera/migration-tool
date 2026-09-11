namespace BDO.GWT.GLOBAL.API.Data;

public class PageOutput<TPageContent> where TPageContent : IPageContent
{
    public PageGeneralOutput General { get; set; } = null!;

    public TPageContent Content { get; set; } = default!;

    public PageMetadataOutput Metadata { get; set; } = null!;

    public PageNavigationOutput Navigation { get; set; } = null!;
}
