using System;

namespace BDO.GWT.Web.Core.Data.Queries;

public class SubscribeRemoveQuery
{
    public string Email { get; set; } = null!;

    public Guid? IssueGuid { get; set; }

    public Guid NewsletterGuid { get; set; } = Guid.Empty;

    public string Hash { get; set; } = null!;
}
