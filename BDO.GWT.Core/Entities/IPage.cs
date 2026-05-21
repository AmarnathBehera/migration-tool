using System;

namespace BDO.GWT.Domain.Core.Entities;

public interface IPage : INode
{
    public string Name { get; set; }

    public Guid PageGUID { get; set; }

    public int? CheckedOutByUserID { get; set; }

    public DateTime? PublishFrom { get; set; }

    public DateTime? PublishTo { get; set; }

    //public LanguageTag LanguageTag { get; set; }

    public bool? IsSearchExcluded { get; set; }

    public bool ShowInMenu { get; set; }

    public bool ShowInSitemap { get; set; }

    //public Link? RedirectOnUnpublish { get; set; }

    public DateTime? LastModificationDate { get; set; }

    public DateTime? CreationDate { get; set; }

    public int? CreatedByUserId { get; set; }
}
