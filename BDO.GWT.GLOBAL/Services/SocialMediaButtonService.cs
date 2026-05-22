using BDO.GWT.GLOBAL.Services;
using BDO.GWT.Web.Core.DTOs;
using CMS.Websites;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace BDO.GWT.Web.Core.LiveSite.Services;

public interface ISocialMediaButtonsService
{
    Task<SocialMediaButtonsDto> GetSocialMediaButtonsDto(Career page, CancellationToken cancellationToken);
}

public class SocialMediaButtonsService : ISocialMediaButtonsService
{

    //private readonly ISettingsService _settingsService;
    private readonly ISiteContext _siteContext;

    public SocialMediaButtonsService( ISiteContext siteContext)
    {

        _siteContext = siteContext;
    }

    public async Task<SocialMediaButtonsDto> GetSocialMediaButtonsDto(Career page, CancellationToken cancellationToken)
    {
        var encodedPageTitle = HttpUtility.UrlEncode(page.MetadataTitle);



        var pageUrl = page.GetUrl().AbsoluteUrl;

        var encodedPageUrl = HttpUtility.UrlEncode(pageUrl);

        //var settingValueByKey = await _settingsService.GetCurrentSiteSettingsAsync([
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
            EmailButtonUrl = ConstructEmailButtonUrl(encodedPageTitle, encodedPageUrl),

            FacebookButtonUrl = ConstructFacebookButtonUrl(encodedPageTitle, encodedPageUrl),

            TwitterButtonUrl = ConstructTwitterButtonUrl(encodedPageTitle, encodedPageUrl),

            LinkedInUrl = ConstructLinkedInButtonUrl(encodedPageTitle, encodedPageUrl),

            ShowCopyLinkButton = true //temp
        };

        return dto;
    }

    private static string ConstructEmailButtonUrl(string encodedPageTitle, string encodedPageUrl)
    {
        return $"mailto:?subject={SanitizeEmailSubject(encodedPageTitle)}&body={encodedPageUrl}";
    }

    private static string? ConstructFacebookButtonUrl(string encodedPageTitle, string encodedPageUrl)
    {
        string facebookAppID = "";
        return $"https://www.facebook.com/dialog/feed?app_id={facebookAppID}&display=popup&caption={encodedPageTitle}&link={encodedPageUrl}&redirect_uri={encodedPageUrl}";
    }

    private static string? ConstructTwitterButtonUrl(string encodedPageTitle, string encodedPageUrl)
    {
        string twitterHandle = "";
        return $"https://twitter.com/intent/tweet?text={encodedPageTitle}&url={encodedPageUrl}&via={twitterHandle}";
        //temp removed for setting setup {twitterHandle}

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
