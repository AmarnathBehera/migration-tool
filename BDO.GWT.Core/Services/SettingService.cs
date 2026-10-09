using CMS.DataEngine;
using CMS.Websites.Routing;

namespace BDO.GWT.Core.Services;

public interface ISettingService
{
    Task<IEnumerable<WebChannelSnippetInfo>?> GetPrintorShareInfo();
    Task<IEnumerable<WebChannelSnippetInfo>?> GetFilteringInfo();
    Task<IEnumerable<WebChannelSnippetInfo>?> GetUsePersonPageTitleInfo();
    Task<IEnumerable<WebChannelSnippetInfo>?> GetShowAdvancedWidgetsInfo();
    Task<GlobalsettingskeyInfo?> GetGlobalSettingAsync();
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

    public Task<IEnumerable<WebChannelSnippetInfo>?> GetShowAdvancedWidgetsInfo() => GetMergedSnippetAsync();

    public async Task<GlobalsettingskeyInfo?> GetGlobalSettingAsync()
    {
        var currentChannelSettings = await _provider
            .Get()
            .TopN(1)
            .GetEnumerableTypedResultAsync();

        return currentChannelSettings?.FirstOrDefault();
    }
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

        return BuildMergedResult(snippet, global);
    }

    internal static IEnumerable<WebChannelSnippetInfo> BuildMergedResult(WebChannelSnippetInfo snippet, GlobalsettingskeyInfo? global)
    {
        return [Merge(snippet, global)];
    }

    private static T Pick<T>(bool inheritFromGlobal, T globalValue, T snippetValue) =>
        inheritFromGlobal ? globalValue : snippetValue;

    private static WebChannelSnippetInfo Merge(WebChannelSnippetInfo snippet, GlobalsettingskeyInfo? global)
    {
        if (global == null)
        {
            return snippet;
        }

        return ApplyMerge(new WebChannelSnippetInfo(), snippet, global);
    }

    /// <summary>
    /// Projects the per-field inherit-from-global resolution onto <paramref name="target"/>.
    /// Extracted so it can be unit-tested with lightweight overridden test doubles that do
    /// not touch the Kentico IoC container.
    /// </summary>
    internal static WebChannelSnippetInfo ApplyMerge(WebChannelSnippetInfo target, WebChannelSnippetInfo snippet, GlobalsettingskeyInfo global)
    {
        target.WebChannelSnippetID = snippet.WebChannelSnippetID;
        target.WebChannelSnippetWebChannelSettingsID = snippet.WebChannelSnippetWebChannelSettingsID;
        target.WebSettingsGUID = snippet.WebSettingsGUID;

        //Show Advanced Widgets
        target.ShowAdvancedWidgets = Pick(snippet.ShowAdvancedWidgetsInheritfromglobalsettings, global.ShowAdvancedWidgets, snippet.ShowAdvancedWidgets);

        // Script fields
        target.ScriptHeadKeyValue = Pick(snippet.ScriptHeadInheritfromglobalsettings, global.ScriptHeadKeyValue, snippet.ScriptHeadKeyValue);
        target.ScriptsTopofPageHeadKeyValue = Pick(snippet.ScriptsTopofPageInheritfromglobalsetting, global.ScriptsTopofPageHeadKeyValue, snippet.ScriptsTopofPageHeadKeyValue);
        target.ScriptinbottomofBodyKeyValue = Pick(snippet.ScriptinbottomofBodyInheritFromGlobal, global.ScriptinbottomofBodyKeyValue, snippet.ScriptinbottomofBodyKeyValue);
        target.ScriptinTopofBodyKeyValue = Pick(snippet.ScriptinTopofBodyInheritfromglobal, global.ScriptinTopofBodyKeyValue, snippet.ScriptinTopofBodyKeyValue);
        target.ScriptexecutedwhencookiesareacceptedKeyValue = Pick(snippet.ScriptexecutedwhencookiesareacceptedInheritfromglobal, global.ScriptexecutedwhencookiesareacceptedKeyValue, snippet.ScriptexecutedwhencookiesareacceptedKeyValue);
        target.MarketoMunchkinCodeKeyValue = Pick(snippet.MarketoMunchkinCodeInheritfromglobal, global.MarketoMunchkinCodeKeyValue, snippet.MarketoMunchkinCodeKeyValue);

        // Email
        target.FromEmail = Pick(snippet.FromEmailInheritfromglobal, global.FromEmail, snippet.FromEmail);
        target.DisplayName = snippet.DisplayName;

        // Calendar
        target.Minutestotriggercalendarreminder = Pick(snippet.MinutestotriggercalendarreminderInheritfromglobal, global.Minutestotriggercalendarreminder, snippet.Minutestotriggercalendarreminder);

        // Sidebar / location
        target.Displaylocationtitleinasidebar = Pick(snippet.DisplaylocationtitleinasidebarInheritfromglobal, global.Displaylocationtitleinasidebar, snippet.Displaylocationtitleinasidebar);

        // Character dictionary
        target.NonASCIItoASCIIcharacterdictionary = Pick(snippet.NonASCIItoASCIIcharacterdictionaryInheritfromglobal, global.NonASCIItoASCIIcharacterdictionary, snippet.NonASCIItoASCIIcharacterdictionary);

        // External link warning
        target.Enableexternallinkwarning = Pick(snippet.EnableexternallinkwarningInheritfromlocal, global.Enableexternallinkwarning, snippet.Enableexternallinkwarning);

        // SEO
        target.SeoSettingsRobots = Pick(snippet.SeoSettingsRobotsInheritfromglobal, global.SeoSettingsRobots, snippet.SeoSettingsRobots);
        target.SeoSettingsSecurity = Pick(snippet.SeoSettingsSecurityInheritfromGlobal, global.SeoSettingsSecurity, snippet.SeoSettingsSecurity);

        // Print / share
        target.DisableDownload = Pick(snippet.DisableDownloadInheritfromglobal, global.DisableDownload, snippet.DisableDownload);
        target.DisablePrint = Pick(snippet.DisablePrintInheritfromglobal, global.DisablePrint, snippet.DisablePrint);
        target.DisableFacebook = Pick(snippet.DisableFacebookInheritfromglobal, global.DisableFacebook, snippet.DisableFacebook);
        target.DisableTwitter = Pick(snippet.DisableTwitterInheritfromglobal, global.DisableTwitter, snippet.DisableTwitter);
        target.DisableLinkedIn = Pick(snippet.DisableLinkedInInheritfromglobal, global.DisableLinkedIn, snippet.DisableLinkedIn);
        target.DisableEmail = Pick(snippet.DisableEmailInheritfromglobal, global.DisableEmail, snippet.DisableEmail);
        target.DisableCopyLinkButton = Pick(snippet.DisableCopyLinkButtonInheritfromglobal, global.DisableCopyLinkButton, snippet.DisableCopyLinkButton);

        // Filtering
        target.Showtooltip = Pick(snippet.ShowtooltipInheritfromglobal, global.Showtooltip, snippet.Showtooltip);

        // People
        target.DisplayEmailinvCard = Pick(snippet.DisplayEmailinvCardInheritfromglobal, global.DisplayEmailinvCard, snippet.DisplayEmailinvCard);
        target.UsePageTitleToDisplayPersonName = Pick(snippet.UsePageTitleToDisplayPersonNameInheritfromglobal, global.UsePageTitleToDisplayPersonName, snippet.UsePageTitleToDisplayPersonName);

        // Cookiebot
        target.EnableCookiebot = Pick(snippet.EnableCookiebotInheritfromglobal, global.EnableCookiebot, snippet.EnableCookiebot);
        target.CookieID = Pick(snippet.CookieIDInheritfromglobal, global.CookieID, snippet.CookieID);
        target.CustomCookiebotScript = Pick(snippet.CustomCookiebotScriptInheritfromglobal, global.CustomCookiebotScript, snippet.CustomCookiebotScript);

        // Preserve inherit flags as-is (informational)
        target.ScriptHeadInheritfromglobalsettings = snippet.ScriptHeadInheritfromglobalsettings;
        target.ScriptsTopofPageInheritfromglobalsetting = snippet.ScriptsTopofPageInheritfromglobalsetting;
        target.ScriptinbottomofBodyInheritFromGlobal = snippet.ScriptinbottomofBodyInheritFromGlobal;
        target.ScriptinTopofBodyInheritfromglobal = snippet.ScriptinTopofBodyInheritfromglobal;
        target.ScriptexecutedwhencookiesareacceptedInheritfromglobal = snippet.ScriptexecutedwhencookiesareacceptedInheritfromglobal;
        target.MarketoMunchkinCodeInheritfromglobal = snippet.MarketoMunchkinCodeInheritfromglobal;
        target.FromEmailInheritfromglobal = snippet.FromEmailInheritfromglobal;
        target.MinutestotriggercalendarreminderInheritfromglobal = snippet.MinutestotriggercalendarreminderInheritfromglobal;
        target.DisplaylocationtitleinasidebarInheritfromglobal = snippet.DisplaylocationtitleinasidebarInheritfromglobal;
        target.NonASCIItoASCIIcharacterdictionaryInheritfromglobal = snippet.NonASCIItoASCIIcharacterdictionaryInheritfromglobal;
        target.EnableexternallinkwarningInheritfromlocal = snippet.EnableexternallinkwarningInheritfromlocal;
        target.SeoSettingsRobotsInheritfromglobal = snippet.SeoSettingsRobotsInheritfromglobal;
        target.SeoSettingsSecurityInheritfromGlobal = snippet.SeoSettingsSecurityInheritfromGlobal;
        target.DisableDownloadInheritfromglobal = snippet.DisableDownloadInheritfromglobal;
        target.DisablePrintInheritfromglobal = snippet.DisablePrintInheritfromglobal;
        target.DisableFacebookInheritfromglobal = snippet.DisableFacebookInheritfromglobal;
        target.DisableTwitterInheritfromglobal = snippet.DisableTwitterInheritfromglobal;
        target.DisableLinkedInInheritfromglobal = snippet.DisableLinkedInInheritfromglobal;
        target.DisableEmailInheritfromglobal = snippet.DisableEmailInheritfromglobal;
        target.DisableCopyLinkButtonInheritfromglobal = snippet.DisableCopyLinkButtonInheritfromglobal;
        target.ShowtooltipInheritfromglobal = snippet.ShowtooltipInheritfromglobal;
        target.DisplayEmailinvCardInheritfromglobal = snippet.DisplayEmailinvCardInheritfromglobal;
        target.UsePageTitleToDisplayPersonNameInheritfromglobal = snippet.UsePageTitleToDisplayPersonNameInheritfromglobal;
        target.EnableCookiebotInheritfromglobal = snippet.EnableCookiebotInheritfromglobal;
        target.CookieIDInheritfromglobal = snippet.CookieIDInheritfromglobal;
        target.CustomCookiebotScriptInheritfromglobal = snippet.CustomCookiebotScriptInheritfromglobal;

        return target;
    }
}
