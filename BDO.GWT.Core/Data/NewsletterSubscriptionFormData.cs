using BDO.GWT.Web.Core.Data;

namespace BDO.GWT.Web.Core.LiveSite.Data;

public class NewsletterSubscriptionFormData : NewsletterSubscriptionData
{
    public string RecaptchaToken { get; set; } = null!;
}
