using BDO.GWT.Core.Constants;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Web.Core.LiveSite.DTOs;
using CMS.Websites;
using System.Web;

namespace BDO.GWT.Core.Services;

public interface ISocialMediaButtonsService
{
    Task<SocialMediaButtonsDto> GetSocialMediaButtonsDto(IWebPageFieldsSource page, string pageTitle, CancellationToken cancellationToken);
    Task<SocialMediaButtonsDto> GetEventSocialMediaButtonsDto(IEventBase page, string pageTitle, CancellationToken cancellationToken);
}

public class SocialMediaButtonsService : ISocialMediaButtonsService
{
    private readonly ISettingService _settingsService;

    public SocialMediaButtonsService(ISettingService settingsService)
    {
        _settingsService = settingsService;
    }


    public async Task<SocialMediaButtonsDto> GetSocialMediaButtonsDto(
      IWebPageFieldsSource page,
      string pageTitle,
      CancellationToken cancellationToken)
    {
        var encodedPageTitle = HttpUtility.UrlEncode(pageTitle);
        var pageUrl = page.GetUrl().AbsoluteUrl;
        var encodedPageUrl = HttpUtility.UrlEncode(pageUrl);

        var settings = await _settingsService.GetPrintorShareInfo();
        var settingInfo = settings?.FirstOrDefault();

        return new SocialMediaButtonsDto
        {
            EmailButtonUrl = settingInfo?.DisableEmail is true
                ? null
                : ConstructEmailButtonUrl(encodedPageTitle, encodedPageUrl),

            FacebookButtonUrl = settingInfo?.DisableFacebook is true
                ? null
                : ConstructFacebookButtonUrl(encodedPageTitle, encodedPageUrl),

            TwitterButtonUrl = settingInfo?.DisableTwitter is true
                ? null
                : ConstructTwitterButtonUrl(encodedPageTitle, encodedPageUrl),

            LinkedInUrl = settingInfo?.DisableLinkedIn is true
                ? null
                : ConstructLinkedInButtonUrl(encodedPageTitle, encodedPageUrl),

            ShowCopyLinkButton = settingInfo?.DisableCopyLinkButton is not true,
        };
    }


    public async Task<SocialMediaButtonsDto> GetEventSocialMediaButtonsDto(
       IEventBase page,
      string pageTitle,
       CancellationToken cancellationToken)
    {
        var encodedPageTitle = HttpUtility.UrlEncode(pageTitle);
        var pageUrl = ((IWebPageFieldsSource)page).GetUrl().AbsoluteUrl;
        var encodedPageUrl = HttpUtility.UrlEncode(pageUrl);

        var settings = await _settingsService.GetPrintorShareInfo();
        var settingInfo = settings?.FirstOrDefault();

        return new SocialMediaButtonsDto
        {
            EmailButtonUrl = settingInfo?.DisableEmail is true
                ? null
                : ConstructEmailButtonUrl(encodedPageTitle, encodedPageUrl),

            FacebookButtonUrl = settingInfo?.DisableFacebook is true
                ? null
                : ConstructFacebookButtonUrl(encodedPageTitle, encodedPageUrl),

            TwitterButtonUrl = settingInfo?.DisableTwitter is true
                ? null
                : ConstructTwitterButtonUrl(encodedPageTitle, encodedPageUrl),

            LinkedInUrl = settingInfo?.DisableLinkedIn is true
                ? null
                : ConstructLinkedInButtonUrl(encodedPageTitle, encodedPageUrl),

            ShowCopyLinkButton = settingInfo?.DisableCopyLinkButton is not true,
        };
    }

    private static string ConstructEmailButtonUrl(string encodedPageTitle, string encodedPageUrl)
        => $"mailto:?subject={SanitizeEmailSubject(encodedPageTitle)}&body={encodedPageUrl}";

    private static string? ConstructFacebookButtonUrl(string encodedPageTitle, string encodedPageUrl)
        => $"https://www.facebook.com/dialog/feed?display=popup&caption={encodedPageTitle}&link={encodedPageUrl}";


    private static string? ConstructTwitterButtonUrl(string encodedPageTitle, string encodedPageUrl)
        => $"https://twitter.com/intent/tweet?text={encodedPageTitle}&url={encodedPageUrl}";

    private static string ConstructLinkedInButtonUrl(string encodedPageTitle, string encodedPageUrl)
        => $"https://www.linkedin.com/shareArticle?url={encodedPageUrl}&title={encodedPageTitle}";

    private static string? SanitizeEmailSubject(string? subject)
        => subject?.Replace("+", "%20");
}