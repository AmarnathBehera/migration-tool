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
    private readonly ISettingsService _settingsService;
    public SocialMediaButtonsService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public async Task<SocialMediaButtonsDto> GetSocialMediaButtonsDto(NewsArticle page, CancellationToken cancellationToken)
    {
        var encodedPageTitle = HttpUtility.UrlEncode(page.PageTitle);
        var pageUrl = page.GetUrl().AbsoluteUrl;

        var encodedPageUrl = HttpUtility.UrlEncode(pageUrl);

        //var settingValueByKey = await _settingsService.GetCurrentChannelSettingsAsync([
        //    SettingName.BDO.DisableCopyLink,
        //    SettingName.BDO.DisableEmail,
        //    SettingName.BDO.DisableFacebook,
        //    SettingName.BDO.DisableTwitter,
        //    SettingName.BDO.DisableLinkedIn,
        //    SettingName.BDO.SocialSharing.FacebookAppID,
        //    SettingName.BDO.SocialSharing.TwitterHandle,
        //], cancellationToken);

        var dto = new SocialMediaButtonsDto
        {
            //EmailButtonUrl = settingValueByKey.TryGetValue(SettingName.BDO.DisableEmail, out var disableEmail) && disableEmail is true
            //    ? null
            //    : ConstructEmailButtonUrl(encodedPageTitle, encodedPageUrl),

            //FacebookButtonUrl = settingValueByKey.TryGetValue(SettingName.BDO.DisableFacebook, out var disableFacebook) && disableFacebook is true
            //    ? null
            //    : ConstructFacebookButtonUrl(settingValueByKey, encodedPageTitle, encodedPageUrl),

            //TwitterButtonUrl = settingValueByKey.TryGetValue(SettingName.BDO.DisableTwitter, out var disableTwitter) && disableTwitter is true
            //    ? null
            //    : ConstructTwitterButtonUrl(settingValueByKey, encodedPageTitle, encodedPageUrl),

            //LinkedInUrl = settingValueByKey.TryGetValue(SettingName.BDO.DisableLinkedIn, out var disableLinkedIn) && disableLinkedIn is true
            //    ? null
            //    : ConstructLinkedInButtonUrl(encodedPageTitle, encodedPageUrl),

            //ShowCopyLinkButton = settingValueByKey.TryGetValue(SettingName.BDO.DisableCopyLink, out var disableCopyLink) && disableCopyLink is false,
        };

        return dto;
    }

    private static string ConstructEmailButtonUrl(string encodedPageTitle, string encodedPageUrl)
    {
        return $"mailto:?subject={SanitizeEmailSubject(encodedPageTitle)}&body={encodedPageUrl}";
    }

    private static string? ConstructFacebookButtonUrl(IDictionary<string, object?> settingValueByKey, string encodedPageTitle, string encodedPageUrl)
    {
        return settingValueByKey.TryGetValue(SettingName.BDO.SocialSharing.FacebookAppID, out var facebookAppID)
            ? $"https://www.facebook.com/dialog/feed?app_id={facebookAppID}&display=popup&caption={encodedPageTitle}&link={encodedPageUrl}&redirect_uri={encodedPageUrl}"
            : null;
    }

    private static string? ConstructTwitterButtonUrl(IDictionary<string, object?> settingValueByKey, string encodedPageTitle, string encodedPageUrl)
    {
        return settingValueByKey.TryGetValue(SettingName.BDO.SocialSharing.TwitterHandle, out var twitterHandle)
            ? $"https://twitter.com/intent/tweet?text={encodedPageTitle}&url={encodedPageUrl}&via={twitterHandle}"
            : null;
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
