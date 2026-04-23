using BDO.GWT.Core.ValueObjects;

namespace BDO.GWT.Domain.Core.Entities;

public class Page : Node, IPage
{
    public string Name { get; set; } = null!;

    public Guid PageGUID { get; set; }

    public int? CheckedOutByUserID { get; set; }

    public DateTime? PublishFrom { get; set; }

    public DateTime? PublishTo { get; set; }

    public LanguageTag LanguageTag { get; set; } = null!;

    public bool? IsSearchExcluded { get; set; }

    public bool ShowInMenu { get; set; }

    public bool ShowInSitemap { get; set; }

    public Link? RedirectOnUnpublish { get; set; }

    public DateTime? CreationDate { get; set; }

    public DateTime? LastModificationDate { get; set; }

    public DateTime? LastPublicationDate { get; set; }

    public int? CreatedByUserId { get; set; }

    public string? LastVersionNumber { get; set; }
}
