namespace BDO.GWT.Core.Data.Queries;

public class SubscribeConfirmQuery
{
    public string SubscriptionHash { get; set; } = null!;

    public string DateTime { get; set; } = null!;

    public int? ContactId { get; set; }

    public int? RecipientListId { get; set; }
}
