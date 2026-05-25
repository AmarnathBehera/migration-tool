using BDO.GWT.Core.Constants;
using BDO.GWT.Core.DTOs;
using CMS.Core;
using CMS.Websites;
using System.Web;

namespace BDO.GWT.Core.Services;

public interface ISocialMediaButtonsService
{
    Task<SocialMediaButtonsDto> GetSocialMediaButtonsDto(NewsArticle page, CancellationToken cancellationToken);
}

public class SocialMediaButtonsService : ISocialMediaButtonsService
{
    private readonly ISettingService _settingsService;
    public SocialMediaButtonsService(ISettingService settingsService)
    {
        _settingsService = settingsService;
    }

    public async Task<SocialMediaButtonsDto> GetSocialMediaButtonsDto(NewsArticle page, CancellationToken cancellationToken)
    {
        var encodedPageTitle = HttpUtility.UrlEncode(page.PageTitle);
        var pageUrl = page.GetUrl().AbsoluteUrl;

        var encodedPageUrl = HttpUtility.UrlEncode(pageUrl);

        var settings = await _settingsService.GetPrintorShareInfo();
        var settingInfo = settings?.FirstOrDefault();

        var dto = new SocialMediaButtonsDto
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

        return dto;
    }

    private static string ConstructEmailButtonUrl(string encodedPageTitle, string encodedPageUrl)
    {
        return $"mailto:?subject={SanitizeEmailSubject(encodedPageTitle)}&body={encodedPageUrl}";
    }

    private static string? ConstructFacebookButtonUrl(string encodedPageTitle, string encodedPageUrl)
    {
        return $"https://www.facebook.com/dialog/feed?display=popup&caption={encodedPageTitle}&link={encodedPageUrl}&redirect_uri={encodedPageUrl}";
    }

    private static string? ConstructTwitterButtonUrl(string encodedPageTitle, string encodedPageUrl)
    {
        return $"https://twitter.com/intent/tweet?text={encodedPageTitle}&url={encodedPageUrl}";
    }

    private static string ConstructLinkedInButtonUrl(string encodedPageTitle, string encodedPageUrl)
    {
        return $"https://www.linkedin.com/shareArticle?url={encodedPageUrl}&title={encodedPageTitle}";
    }

    private static string? SanitizeEmailSubject(string? subject)
    {
        return subject?.Replace("+", "%20");
    }
}
