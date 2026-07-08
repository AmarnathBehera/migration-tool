namespace BDO.GWT.GLOBAL.API.Data;

public interface IPageContent
{
    public DateTime? PublishFrom { get; set; }

    public DateTime? PublishTo { get; set; }

    public string? RedirectOnUnpublish { get; set; }
}
