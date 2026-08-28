using BDO;
using BDO.GWT.Core;
using BDO.GWT.Core.Services;
using BDO.GWT.Domain.Core.Utilities;
using BDO.GWT.Web.Core.Data;
using BDO.GWT.Web.Core.LiveSite.Data;
using BDO.GWT.Xperience.Core.LiveSite.Services;
using BDO.GWT.Xperience.Core.Services;

namespace BDO.GWT.Web.Core.LiveSite.Services;

public interface ISubscribeFormService
{
    Task<bool> SubscribeToNewslettersAsync(Guid pageGuid, NewsletterSubscriptionFormData newsletterSubscriptionFormData, CancellationToken cancellationToken);
}

public class SubscribeFormService : ISubscribeFormService
{
    private readonly INewsletterSubscriptionService _newsletterSubscriptionService;
    private readonly IContentService _contentService;
    private readonly IRecaptchaValidationService _recaptchaValidationService;

    public SubscribeFormService(INewsletterSubscriptionService newsletterSubscriptionService, IContentService contentService, IRecaptchaValidationService recaptchaValidationService)
    {
        _newsletterSubscriptionService = newsletterSubscriptionService;
        _contentService = contentService;
        _recaptchaValidationService = recaptchaValidationService;
    }

    public async Task<bool> SubscribeToNewslettersAsync(Guid pageGuid, NewsletterSubscriptionFormData newsletterSubscriptionFormData, CancellationToken cancellationToken)
    {
        if (!await ValidateSubscribeFormData(pageGuid, newsletterSubscriptionFormData, cancellationToken))
        {
            return false;
        }

        var subscriptionSuccessful = await _newsletterSubscriptionService.SubscribeToNewslettersAsync(newsletterSubscriptionFormData, cancellationToken, pageGuid);

        return subscriptionSuccessful;
    }

    private async Task<bool> ValidateSubscribeFormData(Guid pageGuid, NewsletterSubscriptionFormData newsletterSubscriptionFormData, CancellationToken cancellationToken)
    {
        var pages = await _contentService.GetPagesByGuidsAsync<Subscribe>([pageGuid], cancellationToken);
        var subscribePage = pages.FirstOrDefault();

        if (subscribePage is null || !ValidateFields(subscribePage, newsletterSubscriptionFormData))
        {
            return false;
        }

        var recaptchaValidationSuccessful = await _recaptchaValidationService.ValidateAsync(subscribePage.RecaptchaUseV3, subscribePage.RecaptchaScoreTreshhold, newsletterSubscriptionFormData.RecaptchaToken, cancellationToken);

        return recaptchaValidationSuccessful;
    }

    private static bool ValidateFields(Subscribe subscribePage, NewsletterSubscriptionData newsletterSubscriptionFormData)
    {
       
        if (!RegexUtilities.IsValidEmail(newsletterSubscriptionFormData.Email) || string.IsNullOrWhiteSpace(newsletterSubscriptionFormData.FirstName) || string.IsNullOrWhiteSpace(newsletterSubscriptionFormData.LastName))
        {
            return false;
        }
        switch (subscribePage)
        {
            case { ShowCompanyField: true, IsCompanyRequired: true } when string.IsNullOrWhiteSpace(newsletterSubscriptionFormData.Company):
            case { ShowJobTitleField: true, IsJobTitleRequired: true } when string.IsNullOrWhiteSpace(newsletterSubscriptionFormData.JobTitle):
            case { ShowCountryField: true, IsCountryRequired: true } when !newsletterSubscriptionFormData.CountryGuid.HasValue || newsletterSubscriptionFormData.CountryGuid.Value == Guid.Empty:
                return false;

            default:
                return true;
        }
    }

}
