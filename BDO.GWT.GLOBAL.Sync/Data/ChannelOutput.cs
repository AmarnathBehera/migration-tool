
namespace BDO.GWT.GLOBAL.Sync.Data;

public record ChannelOutput
{
    public required SiteGeneralOutput General { get; set; }
}

public record SiteGeneralOutput
{
    public required int ID { get; set; }

    public required Guid GUID { get; set; }

    public required string DisplayName { get; set; }

    public required string CodeName { get; set; }

    public required string WebsitedomainresentationURL { get; set; }
}
