

using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.Utilities;
using BDO.GWT.Web.Core.DTOs.Pages;
using BDO.GWT.Xperience.Core.LiveSite.Services;
using BDO.GWT.Xperience.Core.Services;

namespace BDO.GWT.Web.Core.LiveSite.Services;

public interface ISubscribeConfirmPageService
{
    Task<SubscribeConfirmDto> GetSubscribeConfirmDtoAsync(SubscribeConfirm? subscribeConfirmPage, SubscribeConfirmQuery subscribeConfirmQuery, CancellationToken cancellationToken);
}

public class SubscribeConfirmPageService : ISubscribeConfirmPageService
{
    private readonly ISiteContext _siteContext;
    private readonly INewsletterSubscriptionService _newsletterSubscriptionService;

    public SubscribeConfirmPageService(ISiteContext siteContext, INewsletterSubscriptionService newsletterSubscriptionService)
    {
        _siteContext = siteContext;
        _newsletterSubscriptionService = newsletterSubscriptionService;
    }

    public async Task<SubscribeConfirmDto> GetSubscribeConfirmDtoAsync(SubscribeConfirm? subscribeConfirmPage, SubscribeConfirmQuery subscribeConfirmQuery, CancellationToken cancellationToken)
    {
        var didSuccessfullySubscribe = await _newsletterSubscriptionService.ConfirmSubscriptionForNewsletterAsync(subscribeConfirmQuery, _siteContext.CurrentSiteName, cancellationToken);

        var message = HtmlUtility.RemoveTildeFromUrlTypeHtmlAttributes(didSuccessfullySubscribe ? subscribeConfirmPage.SuccessfulSubscribeMessage : subscribeConfirmPage.UnsuccessfulSubscribeMessage);
        var subscribeConfirmDto = new SubscribeConfirmDto
        {
            Message = message,
        };

        return subscribeConfirmDto;
    }
}
