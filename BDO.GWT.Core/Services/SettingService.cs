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

    private static T Pick<T>(bool inheritFromGlobal, T globalValue, T snippetValue) =>
        inheritFromGlobal ? globalValue : snippetValue;

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
            ScriptHeadKeyValue = Pick(snippet.ScriptHeadInheritfromglobalsettings, global.ScriptHeadKeyValue, snippet.ScriptHeadKeyValue),
            ScriptsTopofPageHeadKeyValue = Pick(snippet.ScriptsTopofPageInheritfromglobalsetting, global.ScriptsTopofPageHeadKeyValue, snippet.ScriptsTopofPageHeadKeyValue),
            ScriptinbottomofBodyKeyValue = Pick(snippet.ScriptinbottomofBodyInheritFromGlobal, global.ScriptinbottomofBodyKeyValue, snippet.ScriptinbottomofBodyKeyValue),
            ScriptinTopofBodyKeyValue = Pick(snippet.ScriptinTopofBodyInheritfromglobal, global.ScriptinTopofBodyKeyValue, snippet.ScriptinTopofBodyKeyValue),
            ScriptexecutedwhencookiesareacceptedKeyValue = Pick(snippet.ScriptexecutedwhencookiesareacceptedInheritfromglobal, global.ScriptexecutedwhencookiesareacceptedKeyValue, snippet.ScriptexecutedwhencookiesareacceptedKeyValue),
            MarketoMunchkinCodeKeyValue = Pick(snippet.MarketoMunchkinCodeInheritfromglobal, global.MarketoMunchkinCodeKeyValue, snippet.MarketoMunchkinCodeKeyValue),

            // Email
            FromEmail = Pick(snippet.FromEmailInheritfromglobal, global.FromEmail, snippet.FromEmail),
            DisplayName = snippet.DisplayName,

            // Calendar
            Minutestotriggercalendarreminder = Pick(snippet.MinutestotriggercalendarreminderInheritfromglobal, global.Minutestotriggercalendarreminder, snippet.Minutestotriggercalendarreminder),

            // Sidebar / location
            Displaylocationtitleinasidebar = Pick(snippet.DisplaylocationtitleinasidebarInheritfromglobal, global.Displaylocationtitleinasidebar, snippet.Displaylocationtitleinasidebar),

            // Character dictionary
            NonASCIItoASCIIcharacterdictionary = Pick(snippet.NonASCIItoASCIIcharacterdictionaryInheritfromglobal, global.NonASCIItoASCIIcharacterdictionary, snippet.NonASCIItoASCIIcharacterdictionary),

            // External link warning
            Enableexternallinkwarning = Pick(snippet.EnableexternallinkwarningInheritfromlocal, global.Enableexternallinkwarning, snippet.Enableexternallinkwarning),

            // SEO
            SeoSettingsRobots = Pick(snippet.SeoSettingsRobotsInheritfromglobal, global.SeoSettingsRobots, snippet.SeoSettingsRobots),
            SeoSettingsSecurity = Pick(snippet.SeoSettingsSecurityInheritfromGlobal, global.SeoSettingsSecurity, snippet.SeoSettingsSecurity),

            // Print / share
            DisableDownload = Pick(snippet.DisableDownloadInheritfromglobal, global.DisableDownload, snippet.DisableDownload),
            DisablePrint = Pick(snippet.DisablePrintInheritfromglobal, global.DisablePrint, snippet.DisablePrint),
            DisableFacebook = Pick(snippet.DisableFacebookInheritfromglobal, global.DisableFacebook, snippet.DisableFacebook),
            DisableTwitter = Pick(snippet.DisableTwitterInheritfromglobal, global.DisableTwitter, snippet.DisableTwitter),
            DisableLinkedIn = Pick(snippet.DisableLinkedInInheritfromglobal, global.DisableLinkedIn, snippet.DisableLinkedIn),
            DisableEmail = Pick(snippet.DisableEmailInheritfromglobal, global.DisableEmail, snippet.DisableEmail),
            DisableCopyLinkButton = Pick(snippet.DisableCopyLinkButtonInheritfromglobal, global.DisableCopyLinkButton, snippet.DisableCopyLinkButton),

            // Filtering
            Showtooltip = Pick(snippet.ShowtooltipInheritfromglobal, global.Showtooltip, snippet.Showtooltip),

            // People
            DisplayEmailinvCard = Pick(snippet.DisplayEmailinvCardInheritfromglobal, global.DisplayEmailinvCard, snippet.DisplayEmailinvCard),
            UsePageTitleToDisplayPersonName = Pick(snippet.UsePageTitleToDisplayPersonNameInheritfromglobal, global.UsePageTitleToDisplayPersonName, snippet.UsePageTitleToDisplayPersonName),

            // Cookiebot
            EnableCookiebot = Pick(snippet.EnableCookiebotInheritfromglobal, global.EnableCookiebot, snippet.EnableCookiebot),
            CookieID = Pick(snippet.CookieIDInheritfromglobal, global.CookieID, snippet.CookieID),
            CustomCookiebotScript = Pick(snippet.CustomCookiebotScriptInheritfromglobal, global.CustomCookiebotScript, snippet.CustomCookiebotScript),

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
