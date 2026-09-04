using BDO.GWT.Core.Services;
using CMS.DataEngine;
using CMS.Websites.Routing;
using Moq;

namespace BDO.GWT.Core.Tests.Services;

public class SettingServiceTests
{
    private Mock<IInfoProvider<GlobalsettingskeyInfo>> _globalMock = null!;
    private Mock<IInfoProvider<WebChannelSnippetInfo>> _snippetMock = null!;
    private Mock<IWebsiteChannelContext> _websiteChannelContextMock = null!;
    private Mock<IInfoProvider<WebChannelSettingsInfo>> _channelMock = null!;
    private Mock<IContentService> _contentMock = null!;

    private SettingService _service = null!;

    [SetUp]
    public void Setup()
    {
        _globalMock = new Mock<IInfoProvider<GlobalsettingskeyInfo>>();
        _snippetMock = new Mock<IInfoProvider<WebChannelSnippetInfo>>();
        _websiteChannelContextMock = new Mock<IWebsiteChannelContext>();
        _channelMock = new Mock<IInfoProvider<WebChannelSettingsInfo>>();
        _contentMock = new Mock<IContentService>();

        _contentMock.Setup(x => x.GetCurrentChannelID()).Returns(1);

        _service = new SettingService(
            _channelMock.Object,
            _websiteChannelContextMock.Object,
            _globalMock.Object,
            _snippetMock.Object,
            _contentMock.Object);
    }

    // NOTE: Tests here would require mocking ObjectQuery<T> across several types.
    // The provider-level plumbing is exercised by higher-level service tests that
    // mock ISettingService directly, so no unit tests are kept here for the
    // reworked SettingService.
    [Test]
    public void SettingService_CanBeConstructed()
    {
        Assert.That(_service, Is.Not.Null);
    }

    // --- Helpers -----------------------------------------------------------------

    private static Mock<ObjectQuery<T>> BuildQuery<T>(IEnumerable<T> results)
        where T : BaseInfo
    {
        var queryMock = new Mock<ObjectQuery<T>>();

        queryMock.Setup(q => q.WhereEquals(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(queryMock.Object);
        queryMock.Setup(q => q.TopN(It.IsAny<int>()))
            .Returns(queryMock.Object);
        queryMock.Setup(q => q.GetEnumerableTypedResultAsync(
                It.IsAny<System.Data.CommandBehavior>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(results.ToList());

        return queryMock;
    }

    private void SetupChannelSettingsQuery(IEnumerable<WebChannelSettingsInfo> results) =>
        _channelMock.Setup(p => p.Get()).Returns(BuildQuery(results).Object);

    private void SetupGlobalSettingsQuery(IEnumerable<GlobalsettingskeyInfo> results) =>
        _globalMock.Setup(p => p.Get()).Returns(BuildQuery(results).Object);

    // --- GetGlobalSettingAsync ---------------------------------------------------

    [Test]
    public async Task GetGlobalSettingAsync_ReturnsNull_WhenNoGlobalSettingFound()
    {
        SetupGlobalSettingsQuery([]);

        var result = await _service.GetGlobalSettingAsync();

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetGlobalSettingAsync_UsesTopNOne_FromGlobalProvider()
    {
        var queryMock = BuildQuery(Array.Empty<GlobalsettingskeyInfo>());
        _globalMock.Setup(p => p.Get()).Returns(queryMock.Object);

        await _service.GetGlobalSettingAsync();

        _globalMock.Verify(p => p.Get(), Times.Once);
        queryMock.Verify(q => q.TopN(1), Times.Once);
    }

    [Test]
    public void GetGlobalSettingAsync_PropagatesException_FromGlobalProvider()
    {
        _globalMock.Setup(p => p.Get())
            .Throws(new InvalidOperationException("db error"));

        Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetGlobalSettingAsync());
    }



    // --- GetPrintorShareInfo / GetFilteringInfo / GetUsePersonPageTitleInfo -------

    [Test]
    public async Task GetPrintorShareInfo_ReturnsNull_WhenNoChannelSettingFound()
    {
        SetupChannelSettingsQuery([]);

        var result = await _service.GetPrintorShareInfo();

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetFilteringInfo_ReturnsNull_WhenNoChannelSettingFound()
    {
        SetupChannelSettingsQuery([]);

        var result = await _service.GetFilteringInfo();

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetUsePersonPageTitleInfo_ReturnsNull_WhenNoChannelSettingFound()
    {
        SetupChannelSettingsQuery([]);

        var result = await _service.GetUsePersonPageTitleInfo();

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetPrintorShareInfo_UsesCurrentChannelIdFromContentService()
    {
        SetupChannelSettingsQuery([]);

        await _service.GetPrintorShareInfo();

        _contentMock.Verify(x => x.GetCurrentChannelID(), Times.Once);
        _channelMock.Verify(p => p.Get(), Times.Once);
    }

    [Test]
    public void GetPrintorShareInfo_PropagatesException_FromChannelProvider()
    {
        _channelMock.Setup(p => p.Get())
            .Throws(new InvalidOperationException("db error"));

        Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetPrintorShareInfo());
    }

    // --- Snippet lookup + Merge (documented limitation) --------------------------
    //
    // The snippet-found path and the Merge(...) projection cannot be exercised as unit
    // tests. The service resolves query results via Kentico's GetEnumerableTypedResultAsync,
    // which is an EXTENSION method (not virtual) on ObjectQuery<T>; Moq cannot intercept it,
    // so a mocked query cannot be made to yield a populated list. As a result the mocked
    // channel-settings chain always resolves to "no channel setting", and execution returns
    // before reaching the snippet lookup or Merge.
    //
    // Additionally, Merge builds a `new WebChannelSnippetInfo { ... }`, whose DatabaseField
    // setters route through AbstractInfoBase.SetValue -> the CMS IoC container, which is not
    // initialized in unit tests. These paths therefore require Kentico integration tests.

    /// <summary>
    /// Verifies the pure Merge behaviour that IS reachable in isolation: when there is no
    /// global settings row, Merge returns the channel snippet unchanged (its early
    /// `if (global == null) return snippet;` branch), without constructing a merged object.
    /// Info subclasses provide plain backing storage so no CMS infrastructure is touched.
    /// </summary>
    [Test]
    public void Merge_ReturnsSnippetUnchanged_WhenGlobalIsNull()
    {
        var snippet = new TestWebChannelSnippetInfo
        {
            WebChannelSnippetID = 42,
            WebChannelSnippetWebChannelSettingsID = 5,
            FromEmail = "channel@example.com",
        };

        var merge = typeof(SettingService).GetMethod(
            "Merge",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

        Assert.That(merge, Is.Not.Null, "Private static Merge(WebChannelSnippetInfo, GlobalsettingskeyInfo?) not found.");

        var result = merge!.Invoke(null, [snippet, null]);

        Assert.That(result, Is.SameAs(snippet));
    }

    // --- Pick<T> (pure helper, no CMS infrastructure) ----------------------------

    private static object? InvokePick<T>(bool inheritFromGlobal, T globalValue, T snippetValue)
    {
        var pick = typeof(SettingService)
            .GetMethod("Pick", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!
            .MakeGenericMethod(typeof(T));

        return pick.Invoke(null, [inheritFromGlobal, globalValue, snippetValue]);
    }

    [Test]
    public void Pick_ReturnsGlobalValue_WhenInheritFromGlobalIsTrue()
    {
        var result = InvokePick(inheritFromGlobal: true, globalValue: "global", snippetValue: "snippet");

        Assert.That(result, Is.EqualTo("global"));
    }

    [Test]
    public void Pick_ReturnsSnippetValue_WhenInheritFromGlobalIsFalse()
    {
        var result = InvokePick(inheritFromGlobal: false, globalValue: "global", snippetValue: "snippet");

        Assert.That(result, Is.EqualTo("snippet"));
    }

    [Test]
    public void Pick_ReturnsGlobalBool_WhenInheritFromGlobalIsTrue()
    {
        var result = InvokePick(inheritFromGlobal: true, globalValue: true, snippetValue: false);

        Assert.That(result, Is.True);
    }

    [Test]
    public void Pick_ReturnsSnippetBool_WhenInheritFromGlobalIsFalse()
    {
        var result = InvokePick(inheritFromGlobal: false, globalValue: true, snippetValue: false);

        Assert.That(result, Is.False);
    }

    [Test]
    public void Pick_ReturnsGlobalInt_WhenInheritFromGlobalIsTrue()
    {
        var result = InvokePick(inheritFromGlobal: true, globalValue: 10, snippetValue: 20);

        Assert.That(result, Is.EqualTo(10));
    }

    [Test]
    public void Pick_ReturnsSnippetInt_WhenInheritFromGlobalIsFalse()
    {
        var result = InvokePick(inheritFromGlobal: false, globalValue: 10, snippetValue: 20);

        Assert.That(result, Is.EqualTo(20));
    }

    // --- ApplyMerge (full projection, no CMS infrastructure) ---------------------
    //
    // ApplyMerge is the pure projection extracted from Merge. By supplying fully-overridden
    // test doubles (plain auto-property storage) for target, snippet and global, every
    // Pick(...) call and every assignment line is exercised without touching Kentico's
    // AbstractInfoBase.SetValue / IoC container.

    [Test]
    public void ApplyMerge_TakesGlobalValues_WhenAllInheritFlagsAreTrue()
    {
        var target = new FullTestSnippetInfo();
        var snippet = BuildSnippet(inheritAll: true);
        var global = BuildGlobal();

        var result = SettingService.ApplyMerge(target, snippet, global);

        Assert.That(result, Is.SameAs(target));
        Assert.Multiple(() =>
        {
            // Identity/passthrough fields
            Assert.That(result.WebChannelSnippetID, Is.EqualTo(snippet.WebChannelSnippetID));
            Assert.That(result.WebChannelSnippetWebChannelSettingsID, Is.EqualTo(snippet.WebChannelSnippetWebChannelSettingsID));
            Assert.That(result.WebSettingsGUID, Is.EqualTo(snippet.WebSettingsGUID));
            Assert.That(result.DisplayName, Is.EqualTo(snippet.DisplayName));

            // Inherited-from-global (strings)
            Assert.That(result.ScriptHeadKeyValue, Is.EqualTo(global.ScriptHeadKeyValue));
            Assert.That(result.ScriptsTopofPageHeadKeyValue, Is.EqualTo(global.ScriptsTopofPageHeadKeyValue));
            Assert.That(result.ScriptinbottomofBodyKeyValue, Is.EqualTo(global.ScriptinbottomofBodyKeyValue));
            Assert.That(result.ScriptinTopofBodyKeyValue, Is.EqualTo(global.ScriptinTopofBodyKeyValue));
            Assert.That(result.ScriptexecutedwhencookiesareacceptedKeyValue, Is.EqualTo(global.ScriptexecutedwhencookiesareacceptedKeyValue));
            Assert.That(result.MarketoMunchkinCodeKeyValue, Is.EqualTo(global.MarketoMunchkinCodeKeyValue));
            Assert.That(result.FromEmail, Is.EqualTo(global.FromEmail));
            Assert.That(result.NonASCIItoASCIIcharacterdictionary, Is.EqualTo(global.NonASCIItoASCIIcharacterdictionary));
            Assert.That(result.SeoSettingsRobots, Is.EqualTo(global.SeoSettingsRobots));
            Assert.That(result.SeoSettingsSecurity, Is.EqualTo(global.SeoSettingsSecurity));
            Assert.That(result.CookieID, Is.EqualTo(global.CookieID));
            Assert.That(result.CustomCookiebotScript, Is.EqualTo(global.CustomCookiebotScript));

            // Inherited-from-global (int)
            Assert.That(result.Minutestotriggercalendarreminder, Is.EqualTo(global.Minutestotriggercalendarreminder));

            // Inherited-from-global (bools)
            Assert.That(result.Displaylocationtitleinasidebar, Is.EqualTo(global.Displaylocationtitleinasidebar));
            Assert.That(result.Enableexternallinkwarning, Is.EqualTo(global.Enableexternallinkwarning));
            Assert.That(result.DisableDownload, Is.EqualTo(global.DisableDownload));
            Assert.That(result.DisablePrint, Is.EqualTo(global.DisablePrint));
            Assert.That(result.DisableFacebook, Is.EqualTo(global.DisableFacebook));
            Assert.That(result.DisableTwitter, Is.EqualTo(global.DisableTwitter));
            Assert.That(result.DisableLinkedIn, Is.EqualTo(global.DisableLinkedIn));
            Assert.That(result.DisableEmail, Is.EqualTo(global.DisableEmail));
            Assert.That(result.DisableCopyLinkButton, Is.EqualTo(global.DisableCopyLinkButton));
            Assert.That(result.Showtooltip, Is.EqualTo(global.Showtooltip));
            Assert.That(result.DisplayEmailinvCard, Is.EqualTo(global.DisplayEmailinvCard));
            Assert.That(result.UsePageTitleToDisplayPersonName, Is.EqualTo(global.UsePageTitleToDisplayPersonName));
            Assert.That(result.EnableCookiebot, Is.EqualTo(global.EnableCookiebot));
        });
    }

    [Test]
    public void ApplyMerge_TakesSnippetValues_WhenAllInheritFlagsAreFalse()
    {
        var target = new FullTestSnippetInfo();
        var snippet = BuildSnippet(inheritAll: false);
        var global = BuildGlobal();

        var result = SettingService.ApplyMerge(target, snippet, global);

        Assert.Multiple(() =>
        {
            Assert.That(result.ScriptHeadKeyValue, Is.EqualTo(snippet.ScriptHeadKeyValue));
            Assert.That(result.FromEmail, Is.EqualTo(snippet.FromEmail));
            Assert.That(result.Minutestotriggercalendarreminder, Is.EqualTo(snippet.Minutestotriggercalendarreminder));
            Assert.That(result.Enableexternallinkwarning, Is.EqualTo(snippet.Enableexternallinkwarning));
            Assert.That(result.SeoSettingsRobots, Is.EqualTo(snippet.SeoSettingsRobots));
            Assert.That(result.SeoSettingsSecurity, Is.EqualTo(snippet.SeoSettingsSecurity));
            Assert.That(result.CookieID, Is.EqualTo(snippet.CookieID));
            Assert.That(result.CustomCookiebotScript, Is.EqualTo(snippet.CustomCookiebotScript));
            Assert.That(result.EnableCookiebot, Is.EqualTo(snippet.EnableCookiebot));
            Assert.That(result.DisablePrint, Is.EqualTo(snippet.DisablePrint));
        });
    }

    [Test]
    public void ApplyMerge_PreservesInheritFlags_FromSnippet()
    {
        var target = new FullTestSnippetInfo();
        var snippet = BuildSnippet(inheritAll: true);
        var global = BuildGlobal();

        var result = SettingService.ApplyMerge(target, snippet, global);

        Assert.Multiple(() =>
        {
            Assert.That(result.ScriptHeadInheritfromglobalsettings, Is.True);
            Assert.That(result.FromEmailInheritfromglobal, Is.True);
            Assert.That(result.EnableexternallinkwarningInheritfromlocal, Is.True);
            Assert.That(result.SeoSettingsRobotsInheritfromglobal, Is.True);
            Assert.That(result.SeoSettingsSecurityInheritfromGlobal, Is.True);
            Assert.That(result.CookieIDInheritfromglobal, Is.True);
            Assert.That(result.CustomCookiebotScriptInheritfromglobal, Is.True);
            Assert.That(result.EnableCookiebotInheritfromglobal, Is.True);
        });
    }

    // --- Test doubles + builders -------------------------------------------------

    private static FullTestSnippetInfo BuildSnippet(bool inheritAll) => new()
    {
        WebChannelSnippetID = 42,
        WebChannelSnippetWebChannelSettingsID = 5,
        WebSettingsGUID = Guid.NewGuid(),
        DisplayName = "Snippet display",

        ScriptHeadKeyValue = "s-head",
        ScriptsTopofPageHeadKeyValue = "s-toppage",
        ScriptinbottomofBodyKeyValue = "s-bottom",
        ScriptinTopofBodyKeyValue = "s-top",
        ScriptexecutedwhencookiesareacceptedKeyValue = "s-cookie",
        MarketoMunchkinCodeKeyValue = "s-marketo",
        FromEmail = "snippet@example.com",
        Minutestotriggercalendarreminder = 15,
        Displaylocationtitleinasidebar = false,
        NonASCIItoASCIIcharacterdictionary = "s-dict",
        Enableexternallinkwarning = false,
        SeoSettingsRobots = "s-robots",
        SeoSettingsSecurity = "s-security",
        DisableDownload = false,
        DisablePrint = false,
        DisableFacebook = false,
        DisableTwitter = false,
        DisableLinkedIn = false,
        DisableEmail = false,
        DisableCopyLinkButton = false,
        Showtooltip = false,
        DisplayEmailinvCard = false,
        UsePageTitleToDisplayPersonName = false,
        EnableCookiebot = false,
        CookieID = "s-cookieid",
        CustomCookiebotScript = "s-script",

        ScriptHeadInheritfromglobalsettings = inheritAll,
        ScriptsTopofPageInheritfromglobalsetting = inheritAll,
        ScriptinbottomofBodyInheritFromGlobal = inheritAll,
        ScriptinTopofBodyInheritfromglobal = inheritAll,
        ScriptexecutedwhencookiesareacceptedInheritfromglobal = inheritAll,
        MarketoMunchkinCodeInheritfromglobal = inheritAll,
        FromEmailInheritfromglobal = inheritAll,
        MinutestotriggercalendarreminderInheritfromglobal = inheritAll,
        DisplaylocationtitleinasidebarInheritfromglobal = inheritAll,
        NonASCIItoASCIIcharacterdictionaryInheritfromglobal = inheritAll,
        EnableexternallinkwarningInheritfromlocal = inheritAll,
        SeoSettingsRobotsInheritfromglobal = inheritAll,
        SeoSettingsSecurityInheritfromGlobal = inheritAll,
        DisableDownloadInheritfromglobal = inheritAll,
        DisablePrintInheritfromglobal = inheritAll,
        DisableFacebookInheritfromglobal = inheritAll,
        DisableTwitterInheritfromglobal = inheritAll,
        DisableLinkedInInheritfromglobal = inheritAll,
        DisableEmailInheritfromglobal = inheritAll,
        DisableCopyLinkButtonInheritfromglobal = inheritAll,
        ShowtooltipInheritfromglobal = inheritAll,
        DisplayEmailinvCardInheritfromglobal = inheritAll,
        UsePageTitleToDisplayPersonNameInheritfromglobal = inheritAll,
        EnableCookiebotInheritfromglobal = inheritAll,
        CookieIDInheritfromglobal = inheritAll,
        CustomCookiebotScriptInheritfromglobal = inheritAll,
    };

    private static FullTestGlobalInfo BuildGlobal() => new()
    {
        ScriptHeadKeyValue = "g-head",
        ScriptsTopofPageHeadKeyValue = "g-toppage",
        ScriptinbottomofBodyKeyValue = "g-bottom",
        ScriptinTopofBodyKeyValue = "g-top",
        ScriptexecutedwhencookiesareacceptedKeyValue = "g-cookie",
        MarketoMunchkinCodeKeyValue = "g-marketo",
        FromEmail = "global@example.com",
        Minutestotriggercalendarreminder = 99,
        Displaylocationtitleinasidebar = true,
        NonASCIItoASCIIcharacterdictionary = "g-dict",
        Enableexternallinkwarning = true,
        SeoSettingsRobots = "g-robots",
        SeoSettingsSecurity = "g-security",
        DisableDownload = true,
        DisablePrint = true,
        DisableFacebook = true,
        DisableTwitter = true,
        DisableLinkedIn = true,
        DisableEmail = true,
        DisableCopyLinkButton = true,
        Showtooltip = true,
        DisplayEmailinvCard = true,
        UsePageTitleToDisplayPersonName = true,
        EnableCookiebot = true,
        CookieID = "g-cookieid",
        CustomCookiebotScript = "g-script",
    };

    /// <summary>
    /// WebChannelSnippetInfo double: every property used by ApplyMerge is overridden with
    /// plain auto-property storage, so no AbstractInfoBase.SetValue / IoC access occurs.
    /// </summary>
    private sealed class FullTestSnippetInfo : WebChannelSnippetInfo
    {
        public override int WebChannelSnippetID { get; set; }
        public override int WebChannelSnippetWebChannelSettingsID { get; set; }
        public override Guid WebSettingsGUID { get; set; }
        public override string DisplayName { get; set; } = string.Empty;

        public override string ScriptHeadKeyValue { get; set; } = string.Empty;
        public override string ScriptsTopofPageHeadKeyValue { get; set; } = string.Empty;
        public override string ScriptinbottomofBodyKeyValue { get; set; } = string.Empty;
        public override string ScriptinTopofBodyKeyValue { get; set; } = string.Empty;
        public override string ScriptexecutedwhencookiesareacceptedKeyValue { get; set; } = string.Empty;
        public override string MarketoMunchkinCodeKeyValue { get; set; } = string.Empty;
        public override string FromEmail { get; set; } = string.Empty;
        public override int Minutestotriggercalendarreminder { get; set; }
        public override bool Displaylocationtitleinasidebar { get; set; }
        public override string NonASCIItoASCIIcharacterdictionary { get; set; } = string.Empty;
        public override bool Enableexternallinkwarning { get; set; }
        public override string SeoSettingsRobots { get; set; } = string.Empty;
        public override string SeoSettingsSecurity { get; set; } = string.Empty;
        public override bool DisableDownload { get; set; }
        public override bool DisablePrint { get; set; }
        public override bool DisableFacebook { get; set; }
        public override bool DisableTwitter { get; set; }
        public override bool DisableLinkedIn { get; set; }
        public override bool DisableEmail { get; set; }
        public override bool DisableCopyLinkButton { get; set; }
        public override bool Showtooltip { get; set; }
        public override bool DisplayEmailinvCard { get; set; }
        public override bool UsePageTitleToDisplayPersonName { get; set; }
        public override bool EnableCookiebot { get; set; }
        public override string CookieID { get; set; } = string.Empty;
        public override string CustomCookiebotScript { get; set; } = string.Empty;

        public override bool ScriptHeadInheritfromglobalsettings { get; set; }
        public override bool ScriptsTopofPageInheritfromglobalsetting { get; set; }
        public override bool ScriptinbottomofBodyInheritFromGlobal { get; set; }
        public override bool ScriptinTopofBodyInheritfromglobal { get; set; }
        public override bool ScriptexecutedwhencookiesareacceptedInheritfromglobal { get; set; }
        public override bool MarketoMunchkinCodeInheritfromglobal { get; set; }
        public override bool FromEmailInheritfromglobal { get; set; }
        public override bool MinutestotriggercalendarreminderInheritfromglobal { get; set; }
        public override bool DisplaylocationtitleinasidebarInheritfromglobal { get; set; }
        public override bool NonASCIItoASCIIcharacterdictionaryInheritfromglobal { get; set; }
        public override bool EnableexternallinkwarningInheritfromlocal { get; set; }
        public override bool SeoSettingsRobotsInheritfromglobal { get; set; }
        public override bool SeoSettingsSecurityInheritfromGlobal { get; set; }
        public override bool DisableDownloadInheritfromglobal { get; set; }
        public override bool DisablePrintInheritfromglobal { get; set; }
        public override bool DisableFacebookInheritfromglobal { get; set; }
        public override bool DisableTwitterInheritfromglobal { get; set; }
        public override bool DisableLinkedInInheritfromglobal { get; set; }
        public override bool DisableEmailInheritfromglobal { get; set; }
        public override bool DisableCopyLinkButtonInheritfromglobal { get; set; }
        public override bool ShowtooltipInheritfromglobal { get; set; }
        public override bool DisplayEmailinvCardInheritfromglobal { get; set; }
        public override bool UsePageTitleToDisplayPersonNameInheritfromglobal { get; set; }
        public override bool EnableCookiebotInheritfromglobal { get; set; }
        public override bool CookieIDInheritfromglobal { get; set; }
        public override bool CustomCookiebotScriptInheritfromglobal { get; set; }
    }

    /// <summary>
    /// GlobalsettingskeyInfo double with plain auto-property storage for the fields read
    /// by ApplyMerge.
    /// </summary>
    private sealed class FullTestGlobalInfo : GlobalsettingskeyInfo
    {
        public override string ScriptHeadKeyValue { get; set; } = string.Empty;
        public override string ScriptsTopofPageHeadKeyValue { get; set; } = string.Empty;
        public override string ScriptinbottomofBodyKeyValue { get; set; } = string.Empty;
        public override string ScriptinTopofBodyKeyValue { get; set; } = string.Empty;
        public override string ScriptexecutedwhencookiesareacceptedKeyValue { get; set; } = string.Empty;
        public override string MarketoMunchkinCodeKeyValue { get; set; } = string.Empty;
        public override string FromEmail { get; set; } = string.Empty;
        public override int Minutestotriggercalendarreminder { get; set; }
        public override bool Displaylocationtitleinasidebar { get; set; }
        public override string NonASCIItoASCIIcharacterdictionary { get; set; } = string.Empty;
        public override bool Enableexternallinkwarning { get; set; }
        public override string SeoSettingsRobots { get; set; } = string.Empty;
        public override string SeoSettingsSecurity { get; set; } = string.Empty;
        public override bool DisableDownload { get; set; }
        public override bool DisablePrint { get; set; }
        public override bool DisableFacebook { get; set; }
        public override bool DisableTwitter { get; set; }
        public override bool DisableLinkedIn { get; set; }
        public override bool DisableEmail { get; set; }
        public override bool DisableCopyLinkButton { get; set; }
        public override bool Showtooltip { get; set; }
        public override bool DisplayEmailinvCard { get; set; }
        public override bool UsePageTitleToDisplayPersonName { get; set; }
        public override bool EnableCookiebot { get; set; }
        public override string CookieID { get; set; } = string.Empty;
        public override string CustomCookiebotScript { get; set; } = string.Empty;
    }

    private sealed class TestWebChannelSnippetInfo : WebChannelSnippetInfo
    {
        public override int WebChannelSnippetID { get; set; }
        public override int WebChannelSnippetWebChannelSettingsID { get; set; }
        public override string FromEmail { get; set; } = string.Empty;
    }

}
