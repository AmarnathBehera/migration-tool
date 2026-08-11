using BDO.GWT.Core.Utilities;
using BDO.GWT.Web.Core.Data.Queries;
using BDO.GWT.Web.Core.DTOs.Pages;
using BDO.GWT.Xperience.Core.LiveSite.Services;

namespace BDO.GWT.Web.Core.LiveSite.Services;

public interface ISubscribeRemovePageService
{
    Task<SubscribeRemoveDto> GetSubscribeRemoveDtoAsync(SubscribeRemove? subscribeRemovePage, SubscribeRemoveQuery subscribeRemoveQuery, CancellationToken cancellationToken);
}

public class SubscribeRemovePageService : ISubscribeRemovePageService
{
    private readonly INewsletterSubscriptionService _newsletterSubscriptionService;

    public SubscribeRemovePageService(INewsletterSubscriptionService newsletterSubscriptionService)
    {
        _newsletterSubscriptionService = newsletterSubscriptionService;
    }

    public async Task<SubscribeRemoveDto> GetSubscribeRemoveDtoAsync(SubscribeRemove? subscribeRemovePage, SubscribeRemoveQuery subscribeRemoveQuery, CancellationToken cancellationToken)
    {
        var didSuccessfullyUnsubscribe = await _newsletterSubscriptionService.UnsubscribeFromNewsletterAsync(subscribeRemoveQuery, cancellationToken);

        var rawMessage = didSuccessfullyUnsubscribe
            ? subscribeRemovePage?.SuccessfulUnubscribeMessage
            : subscribeRemovePage?.UnsuccessfulUnsubscribeMessage;

        var message = HtmlUtility.RemoveTildeFromUrlTypeHtmlAttributes(rawMessage ?? string.Empty);
        var subscribeConfirmDto = new SubscribeRemoveDto
        {
            Message = message,
        };

        return subscribeConfirmDto;
    }
}
