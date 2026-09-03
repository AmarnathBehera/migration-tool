using CMS.DataEngine;
using CMS.Websites.Routing;

namespace BDO.GWT.Core.Services;

public interface ISettingService
{
    Task<IEnumerable<WebChannelSnippetInfo>?> GetPrintorShareInfo();
    Task<IEnumerable<WebChannelSnippetInfo>?> GetFilteringInfo();
    Task<IEnumerable<WebChannelSnippetInfo>?> GetUsePersonPageTitleInfo();
}

public class SettingService : ISettingService
{
    private readonly IInfoProvider<GlobalsettingskeyInfo> _provider;
    private readonly IInfoProvider<WebChannelSnippetInfo> _siteProvider;
    private readonly IWebsiteChannelContext _websiteChannelContext;
    private readonly IInfoProvider<WebChannelSettingsInfo> _webChannelSettingsInfoProvider;
    private readonly IContentService _contentService;

    public SettingService(
        IInfoProvider<WebChannelSettingsInfo> webChannelSettingsInfoProvider,
        IWebsiteChannelContext websiteChannelContext,
        IInfoProvider<GlobalsettingskeyInfo> provider,
        IInfoProvider<WebChannelSnippetInfo> siteProvider,
        IContentService contentService)
    {
        _provider = provider;
        _siteProvider = siteProvider;
        _websiteChannelContext = websiteChannelContext;
        _webChannelSettingsInfoProvider = webChannelSettingsInfoProvider;
        _contentService = contentService;
    }

    public Task<IEnumerable<WebChannelSnippetInfo>?> GetPrintorShareInfo() => GetMergedSnippetAsync();

    public Task<IEnumerable<WebChannelSnippetInfo>?> GetFilteringInfo() => GetMergedSnippetAsync();

    public Task<IEnumerable<WebChannelSnippetInfo>?> GetUsePersonPageTitleInfo() => GetMergedSnippetAsync();

    private async Task<WebChannelSettingsInfo?> GetCurrentChannelSettingAsync()
    {
        var currentChannelSettings = await _webChannelSettingsInfoProvider
            .Get()
            .WhereEquals(nameof(WebChannelSettingsInfo.WebChannelSettingsChannelID), _contentService.GetCurrentChannelID())
            .TopN(1)
            .GetEnumerableTypedResultAsync();

        return currentChannelSettings?.FirstOrDefault();
    }

    /// <summary>
    /// Returns a single <see cref="WebChannelSnippetInfo"/> whose values are resolved per field:
    /// when the snippet's inherit-from-global flag is <c>true</c>, the value is taken from
    /// <see cref="GlobalsettingskeyInfo"/>; otherwise it is taken from the channel snippet.
    /// </summary>
    private async Task<IEnumerable<WebChannelSnippetInfo>?> GetMergedSnippetAsync()
    {
        var channelSetting = await GetCurrentChannelSettingAsync();
        if (channelSetting == null)
        {
            return null;
        }

        var snippets = await _siteProvider.Get()
            .WhereEquals(nameof(WebChannelSnippetInfo.WebChannelSnippetWebChannelSettingsID), channelSetting.WebChannelSettingsID)
            .TopN(1)
            .GetEnumerableTypedResultAsync();

        var snippet = snippets?.FirstOrDefault();
        if (snippet == null)
        {
            return null;
        }

        var globals = await _provider.Get().TopN(1).GetEnumerableTypedResultAsync();
        var global = globals?.FirstOrDefault();

        return new[] { Merge(snippet, global) };
    }

    private static WebChannelSnippetInfo Merge(WebChannelSnippetInfo snippet, GlobalsettingskeyInfo? global)
    {
        if (global == null)
        {
            return snippet;
        }

        return new WebChannelSnippetInfo
        {
            WebChannelSnippetID = snippet.WebChannelSnippetID,
            WebChannelSnippetWebChannelSettingsID = snippet.WebChannelSnippetWebChannelSettingsID,
            WebSettingsGUID = snippet.WebSettingsGUID,

            // Script fields
            ScriptHeadKeyValue = snippet.ScriptHeadInheritfromglobalsettings ? global.ScriptHeadKeyValue : snippet.ScriptHeadKeyValue,
            ScriptsTopofPageHeadKeyValue = snippet.ScriptsTopofPageInheritfromglobalsetting ? global.ScriptsTopofPageHeadKeyValue : snippet.ScriptsTopofPageHeadKeyValue,
            ScriptinbottomofBodyKeyValue = snippet.ScriptinbottomofBodyInheritFromGlobal ? global.ScriptinbottomofBodyKeyValue : snippet.ScriptinbottomofBodyKeyValue,
            ScriptinTopofBodyKeyValue = snippet.ScriptinTopofBodyInheritfromglobal ? global.ScriptinTopofBodyKeyValue : snippet.ScriptinTopofBodyKeyValue,
            ScriptexecutedwhencookiesareacceptedKeyValue = snippet.ScriptexecutedwhencookiesareacceptedInheritfromglobal ? global.ScriptexecutedwhencookiesareacceptedKeyValue : snippet.ScriptexecutedwhencookiesareacceptedKeyValue,
            MarketoMunchkinCodeKeyValue = snippet.MarketoMunchkinCodeInheritfromglobal ? global.MarketoMunchkinCodeKeyValue : snippet.MarketoMunchkinCodeKeyValue,

            // Email
            FromEmail = snippet.FromEmailInheritfromglobal ? global.FromEmail : snippet.FromEmail,
            DisplayName = snippet.DisplayName,

            // Calendar
            Minutestotriggercalendarreminder = snippet.MinutestotriggercalendarreminderInheritfromglobal ? global.Minutestotriggercalendarreminder : snippet.Minutestotriggercalendarreminder,

            // Sidebar / location
            Displaylocationtitleinasidebar = snippet.DisplaylocationtitleinasidebarInheritfromglobal ? global.Displaylocationtitleinasidebar : snippet.Displaylocationtitleinasidebar,

            // Character dictionary
            NonASCIItoASCIIcharacterdictionary = snippet.NonASCIItoASCIIcharacterdictionaryInheritfromglobal ? global.NonASCIItoASCIIcharacterdictionary : snippet.NonASCIItoASCIIcharacterdictionary,

            // External link warning
            Enableexternallinkwarning = snippet.EnableexternallinkwarningInheritfromlocal ? global.Enableexternallinkwarning : snippet.Enableexternallinkwarning,

            // SEO
            SeoSettingsRobots = snippet.SeoSettingsRobotsInheritfromglobal ? global.SeoSettingsRobots : snippet.SeoSettingsRobots,
            SeoSettingsSecurity = snippet.SeoSettingsSecurityInheritfromGlobal ? global.SeoSettingsSecurity : snippet.SeoSettingsSecurity,

            // Print / share
            DisableDownload = snippet.DisableDownloadInheritfromglobal ? global.DisableDownload : snippet.DisableDownload,
            DisablePrint = snippet.DisablePrintInheritfromglobal ? global.DisablePrint : snippet.DisablePrint,
            DisableFacebook = snippet.DisableFacebookInheritfromglobal ? global.DisableFacebook : snippet.DisableFacebook,
            DisableTwitter = snippet.DisableTwitterInheritfromglobal ? global.DisableTwitter : snippet.DisableTwitter,
            DisableLinkedIn = snippet.DisableLinkedInInheritfromglobal ? global.DisableLinkedIn : snippet.DisableLinkedIn,
            DisableEmail = snippet.DisableEmailInheritfromglobal ? global.DisableEmail : snippet.DisableEmail,
            DisableCopyLinkButton = snippet.DisableCopyLinkButtonInheritfromglobal ? global.DisableCopyLinkButton : snippet.DisableCopyLinkButton,

            // Filtering
            Showtooltip = snippet.ShowtooltipInheritfromglobal ? global.Showtooltip : snippet.Showtooltip,

            // People
            DisplayEmailinvCard = snippet.DisplayEmailinvCardInheritfromglobal ? global.DisplayEmailinvCard : snippet.DisplayEmailinvCard,
            UsePageTitleToDisplayPersonName = snippet.UsePageTitleToDisplayPersonNameInheritfromglobal ? global.UsePageTitleToDisplayPersonName : snippet.UsePageTitleToDisplayPersonName,

            // Cookiebot
            EnableCookiebot = snippet.EnableCookiebotInheritfromglobal ? global.EnableCookiebot : snippet.EnableCookiebot,
            CookieID = snippet.CookieIDInheritfromglobal ? global.CookieID : snippet.CookieID,
            CustomCookiebotScript = snippet.CustomCookiebotScriptInheritfromglobal ? global.CustomCookiebotScript : snippet.CustomCookiebotScript,

            // Preserve inherit flags as-is (informational)
            ScriptHeadInheritfromglobalsettings = snippet.ScriptHeadInheritfromglobalsettings,
            ScriptsTopofPageInheritfromglobalsetting = snippet.ScriptsTopofPageInheritfromglobalsetting,
            ScriptinbottomofBodyInheritFromGlobal = snippet.ScriptinbottomofBodyInheritFromGlobal,
            ScriptinTopofBodyInheritfromglobal = snippet.ScriptinTopofBodyInheritfromglobal,
            ScriptexecutedwhencookiesareacceptedInheritfromglobal = snippet.ScriptexecutedwhencookiesareacceptedInheritfromglobal,
            MarketoMunchkinCodeInheritfromglobal = snippet.MarketoMunchkinCodeInheritfromglobal,
            FromEmailInheritfromglobal = snippet.FromEmailInheritfromglobal,
            MinutestotriggercalendarreminderInheritfromglobal = snippet.MinutestotriggercalendarreminderInheritfromglobal,
            DisplaylocationtitleinasidebarInheritfromglobal = snippet.DisplaylocationtitleinasidebarInheritfromglobal,
            NonASCIItoASCIIcharacterdictionaryInheritfromglobal = snippet.NonASCIItoASCIIcharacterdictionaryInheritfromglobal,
            EnableexternallinkwarningInheritfromlocal = snippet.EnableexternallinkwarningInheritfromlocal,
            SeoSettingsRobotsInheritfromglobal = snippet.SeoSettingsRobotsInheritfromglobal,
            SeoSettingsSecurityInheritfromGlobal = snippet.SeoSettingsSecurityInheritfromGlobal,
            DisableDownloadInheritfromglobal = snippet.DisableDownloadInheritfromglobal,
            DisablePrintInheritfromglobal = snippet.DisablePrintInheritfromglobal,
            DisableFacebookInheritfromglobal = snippet.DisableFacebookInheritfromglobal,
            DisableTwitterInheritfromglobal = snippet.DisableTwitterInheritfromglobal,
            DisableLinkedInInheritfromglobal = snippet.DisableLinkedInInheritfromglobal,
            DisableEmailInheritfromglobal = snippet.DisableEmailInheritfromglobal,
            DisableCopyLinkButtonInheritfromglobal = snippet.DisableCopyLinkButtonInheritfromglobal,
            ShowtooltipInheritfromglobal = snippet.ShowtooltipInheritfromglobal,
            DisplayEmailinvCardInheritfromglobal = snippet.DisplayEmailinvCardInheritfromglobal,
            UsePageTitleToDisplayPersonNameInheritfromglobal = snippet.UsePageTitleToDisplayPersonNameInheritfromglobal,
            EnableCookiebotInheritfromglobal = snippet.EnableCookiebotInheritfromglobal,
            CookieIDInheritfromglobal = snippet.CookieIDInheritfromglobal,
            CustomCookiebotScriptInheritfromglobal = snippet.CustomCookiebotScriptInheritfromglobal,
        };
    }
}
