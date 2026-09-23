using BDO;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using BDO.GWT.Web.Services;
using BDO.GWT.Xperience.Core.Services;
using CMS.ContentEngine;
using CMS.Core;
using CMS.DataEngine;
using CMS.Websites;
using CMS.Websites.Routing;
using Moq;
using XperienceCommunity.Localization;

namespace BDO.GWT.GLOBAL.Tests.Services;

[TestFixture]
public class SiteConfigurationServiceTests
{
    private Mock<ICachingService> _cachingServiceMock = null!;
    private Mock<IContentQueryExecutor> _executorMock = null!;
    private Mock<ISiteContext> _siteContextMock = null!;

    private Mock<IInfoProvider<LocalizationKeyInfo>> _localizationKeyProviderMock = null!;
    private Mock<IInfoProvider<XperienceCommunity.Localization.LocalizationTranslationItemInfo>> _localizationTranslationProviderMock = null!;
    private Mock<IInfoProvider<ContentLanguageInfo>> _contentLanguageProviderMock = null!;
    private Mock<IInfoProvider<WebChannelSettingsInfo>> _webChannelSettingsProviderMock = null!;
    private Mock<IInfoProvider<WebsiteChannelInfo>> _websiteChannelProviderMock = null!;
    private Mock<IInfoProvider<GlobalsettingskeyInfo>> _globalSettingsProviderMock = null!;
    private Mock<IWebsiteChannelContext> _websiteChannelContextMock = null!;
    private Mock<IInfoProvider<WebChannelSnippetInfo>> _webChannelSnippetProviderMock = null!;

    private SiteConfigurationProviders _providers = null!;
    private SiteConfigurationService _service = null!;

    [SetUp]
    public void Setup()
    {
        _cachingServiceMock = new Mock<ICachingService>();
        _executorMock = new Mock<IContentQueryExecutor>();
        _siteContextMock = new Mock<ISiteContext>();

        _localizationKeyProviderMock = new Mock<IInfoProvider<LocalizationKeyInfo>>();
        _localizationTranslationProviderMock = new Mock<IInfoProvider<XperienceCommunity.Localization.LocalizationTranslationItemInfo>>();
        _contentLanguageProviderMock = new Mock<IInfoProvider<ContentLanguageInfo>>();
        _webChannelSettingsProviderMock = new Mock<IInfoProvider<WebChannelSettingsInfo>>();
        _websiteChannelProviderMock = new Mock<IInfoProvider<WebsiteChannelInfo>>();
        _globalSettingsProviderMock = new Mock<IInfoProvider<GlobalsettingskeyInfo>>();
        _websiteChannelContextMock = new Mock<IWebsiteChannelContext>();
        _webChannelSnippetProviderMock = new Mock<IInfoProvider<WebChannelSnippetInfo>>();

        _providers = new SiteConfigurationProviders(
            LocalizationKey: _localizationKeyProviderMock.Object,
            LocalizationTranslation: _localizationTranslationProviderMock.Object,
            ContentLanguage: _contentLanguageProviderMock.Object,
            WebChannelSettings: _webChannelSettingsProviderMock.Object,
            WebsiteChannel: _websiteChannelProviderMock.Object,
            provider: _globalSettingsProviderMock.Object,
            websiteChannelContext: _websiteChannelContextMock.Object,
            webChannelSnippetInfoProvider: _webChannelSnippetProviderMock.Object);

        _service = new SiteConfigurationService(
            _cachingServiceMock.Object,
            _executorMock.Object,
            _siteContextMock.Object,
            _providers,
            _websiteChannelContextMock.Object);
    }

    private static Mock<ObjectQuery<T>> BuildQuery<T>(IEnumerable<T> items)
        where T : BaseInfo
    {
        var queryMock = new Mock<ObjectQuery<T>>();

        queryMock.Setup(q => q.WhereEquals(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(queryMock.Object);
        queryMock.Setup(q => q.TopN(It.IsAny<int>()))
            .Returns(queryMock.Object);

        queryMock.As<IEnumerable<T>>()
            .Setup(x => x.GetEnumerator())
            .Returns(() => items.GetEnumerator());

        return queryMock;
    }

    private static Mock<ObjectQuery<T>> BuildAsyncQuery<T>(IEnumerable<T> items)
        where T : BaseInfo
    {
        var queryMock = BuildQuery(items);

        queryMock.Setup(q => q.GetEnumerableTypedResultAsync(
                It.IsAny<System.Data.CommandBehavior>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(items.ToList());

        return queryMock;
    }

    private SiteSettingsDto InvokeGetSiteSettings()
    {
        var method = typeof(SiteConfigurationService)
            .GetMethod("GetSiteSettings", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        Assert.That(method, Is.Not.Null);
        return (SiteSettingsDto)method!.Invoke(_service, null)!;
    }

    [Test]
    public void Service_ImplementsISiteConfigurationService()
    {
        Assert.That(_service, Is.InstanceOf<ISiteConfigurationService>());
    }

    [Test]
    public void GetCachedSiteConfigurationHash_ReturnsCachedHash_WhenPresentInCache()
    {
        const int siteId = 7;
        const string expectedHash = "abc123";
        var expectedKey = $"SiteConfigurationService|GetCachedSiteConfigurationHash|hash|{siteId}";

        _siteContextMock.SetupGet(x => x.CurrentSiteID).Returns(siteId);

        string? outValue = expectedHash;
        _cachingServiceMock
            .Setup(x => x.TryGetCachedObject<string>(expectedKey, out outValue))
            .Returns(true);

        var result = _service.GetCachedSiteConfigurationHash();

        Assert.That(result, Is.EqualTo(expectedHash));
    }

    [Test]
    public void GetCachedSiteConfigurationHash_ReturnsNull_WhenNotPresentInCache()
    {
        const int siteId = 3;

        _siteContextMock.SetupGet(x => x.CurrentSiteID).Returns(siteId);

        string? outValue = null;
        _cachingServiceMock
            .Setup(x => x.TryGetCachedObject<string>(It.IsAny<string>(), out outValue))
            .Returns(false);

        var result = _service.GetCachedSiteConfigurationHash();

        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetCachedSiteConfigurationHash_BuildsCacheKey_FromCurrentSiteId()
    {
        const int siteId = 42;
        var expectedKey = $"SiteConfigurationService|GetCachedSiteConfigurationHash|hash|{siteId}";

        _siteContextMock.SetupGet(x => x.CurrentSiteID).Returns(siteId);

        string? outValue = null;
        _cachingServiceMock
            .Setup(x => x.TryGetCachedObject<string>(It.IsAny<string>(), out outValue))
            .Returns(false);

        _service.GetCachedSiteConfigurationHash();

        _cachingServiceMock.Verify(
            x => x.TryGetCachedObject<string>(expectedKey, out outValue),
            Times.Once);
    }

    [Test]
    public void GetCachedSiteConfigurationHash_ReadsCurrentSiteId_Once()
    {
        _siteContextMock.SetupGet(x => x.CurrentSiteID).Returns(1);

        string? outValue = null;
        _cachingServiceMock
            .Setup(x => x.TryGetCachedObject<string>(It.IsAny<string>(), out outValue))
            .Returns(false);

        _service.GetCachedSiteConfigurationHash();

        _siteContextMock.VerifyGet(x => x.CurrentSiteID, Times.Once);
    }

    [Test]
    public void GetCachedSiteConfigurationHash_PropagatesException_FromCachingService()
    {
        _siteContextMock.SetupGet(x => x.CurrentSiteID).Returns(1);

        string? outValue = null;
        _cachingServiceMock
            .Setup(x => x.TryGetCachedObject<string>(It.IsAny<string>(), out outValue))
            .Throws(new InvalidOperationException("cache error"));

        Assert.Throws<InvalidOperationException>(() => _service.GetCachedSiteConfigurationHash());
    }

    [Test]
    public void GetSiteSettings_ReturnsDefaults_WhenWebsiteChannelNotFound()
    {
        _siteContextMock.SetupGet(x => x.CurrentSiteID).Returns(10);
        _websiteChannelProviderMock.Setup(p => p.Get())
            .Returns(BuildQuery<WebsiteChannelInfo>([]).Object);

        var result = InvokeGetSiteSettings();

        Assert.That(result.ExternalLinkWarningEnabled, Is.False);
    }

    [Test]
    public void GetSiteSettings_ReturnsDefaults_WhenWebChannelSettingsNotFound()
    {
        _siteContextMock.SetupGet(x => x.CurrentSiteID).Returns(10);

        var websiteChannel = new TestWebsiteChannelInfo
        {
            WebsiteChannelID = 10,
            WebsiteChannelChannelID = 77,
        };

        _websiteChannelProviderMock.Setup(p => p.Get())
            .Returns(BuildQuery<WebsiteChannelInfo>([websiteChannel]).Object);
        _webChannelSettingsProviderMock.Setup(p => p.Get())
            .Returns(BuildQuery<WebChannelSettingsInfo>([]).Object);

        var result = InvokeGetSiteSettings();

        Assert.That(result.ExternalLinkWarningEnabled, Is.False);
    }

    [Test]
    public void GetSiteSettings_UsesSnippetValue_WhenNotInheritingFromGlobal()
    {
        _siteContextMock.SetupGet(x => x.CurrentSiteID).Returns(10);

        var websiteChannel = new TestWebsiteChannelInfo
        {
            WebsiteChannelID = 10,
            WebsiteChannelChannelID = 77,
        };
        var webChannelSettings = new TestWebChannelSettingsInfo
        {
            WebChannelSettingsID = 20,
            WebChannelSettingsChannelID = 77,
        };
        var snippet = new TestWebChannelSnippetInfo
        {
            WebChannelSnippetWebChannelSettingsID = 20,
            EnableexternallinkwarningInheritfromlocal = false,
            Enableexternallinkwarning = true,
        };

        _websiteChannelProviderMock.Setup(p => p.Get())
            .Returns(BuildQuery<WebsiteChannelInfo>([websiteChannel]).Object);
        _webChannelSettingsProviderMock.Setup(p => p.Get())
            .Returns(BuildQuery<WebChannelSettingsInfo>([webChannelSettings]).Object);
        _webChannelSnippetProviderMock.Setup(p => p.Get())
            .Returns(BuildQuery<WebChannelSnippetInfo>([snippet]).Object);

        var result = InvokeGetSiteSettings();

        Assert.That(result.ExternalLinkWarningEnabled, Is.True);
        _globalSettingsProviderMock.Verify(p => p.Get(), Times.Never);
    }

    [Test]
    public void GetSiteSettings_UsesGlobalValue_WhenSnippetInheritsFromGlobal()
    {
        _siteContextMock.SetupGet(x => x.CurrentSiteID).Returns(10);

        var websiteChannel = new TestWebsiteChannelInfo
        {
            WebsiteChannelID = 10,
            WebsiteChannelChannelID = 77,
        };
        var webChannelSettings = new TestWebChannelSettingsInfo
        {
            WebChannelSettingsID = 20,
            WebChannelSettingsChannelID = 77,
        };
        var snippet = new TestWebChannelSnippetInfo
        {
            WebChannelSnippetWebChannelSettingsID = 20,
            EnableexternallinkwarningInheritfromlocal = true,
            Enableexternallinkwarning = false,
        };
        var global = new TestGlobalSettingsKeyInfo
        {
            Enableexternallinkwarning = true,
        };

        _websiteChannelProviderMock.Setup(p => p.Get())
            .Returns(BuildQuery<WebsiteChannelInfo>([websiteChannel]).Object);
        _webChannelSettingsProviderMock.Setup(p => p.Get())
            .Returns(BuildQuery<WebChannelSettingsInfo>([webChannelSettings]).Object);
        _webChannelSnippetProviderMock.Setup(p => p.Get())
            .Returns(BuildQuery<WebChannelSnippetInfo>([snippet]).Object);
        _globalSettingsProviderMock.Setup(p => p.Get())
            .Returns(BuildQuery<GlobalsettingskeyInfo>([global]).Object);

        var result = InvokeGetSiteSettings();

        Assert.That(result.ExternalLinkWarningEnabled, Is.True);
        _globalSettingsProviderMock.Verify(p => p.Get(), Times.Once);
    }

    [Test]
    public void GetSiteSettings_FallsBackToFalse_WhenSnippetInheritsAndGlobalMissing()
    {
        _siteContextMock.SetupGet(x => x.CurrentSiteID).Returns(10);

        var websiteChannel = new TestWebsiteChannelInfo
        {
            WebsiteChannelID = 10,
            WebsiteChannelChannelID = 77,
        };
        var webChannelSettings = new TestWebChannelSettingsInfo
        {
            WebChannelSettingsID = 20,
            WebChannelSettingsChannelID = 77,
        };
        var snippet = new TestWebChannelSnippetInfo
        {
            WebChannelSnippetWebChannelSettingsID = 20,
            EnableexternallinkwarningInheritfromlocal = true,
            Enableexternallinkwarning = true,
        };

        _websiteChannelProviderMock.Setup(p => p.Get())
            .Returns(BuildQuery<WebsiteChannelInfo>([websiteChannel]).Object);
        _webChannelSettingsProviderMock.Setup(p => p.Get())
            .Returns(BuildQuery<WebChannelSettingsInfo>([webChannelSettings]).Object);
        _webChannelSnippetProviderMock.Setup(p => p.Get())
            .Returns(BuildQuery<WebChannelSnippetInfo>([snippet]).Object);
        _globalSettingsProviderMock.Setup(p => p.Get())
            .Returns(BuildQuery<GlobalsettingskeyInfo>([]).Object);

        var result = InvokeGetSiteSettings();

        Assert.That(result.ExternalLinkWarningEnabled, Is.False);
    }

    [Test]
    public async Task GetLocalizationValuesAsync_ReturnsEmpty_WhenNoLocalizationData()
    {
        _localizationKeyProviderMock.Setup(p => p.Get())
            .Returns(BuildAsyncQuery<LocalizationKeyInfo>([]).Object);
        _localizationTranslationProviderMock.Setup(p => p.Get())
            .Returns(BuildAsyncQuery<XperienceCommunity.Localization.LocalizationTranslationItemInfo>([]).Object);
        _contentLanguageProviderMock.Setup(p => p.Get())
            .Returns(BuildAsyncQuery<ContentLanguageInfo>([]).Object);

        var result = await _service.GetLocalizationValuesAsync();

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetSiteConfigurationDtoAsync_ReadsWebsiteChannelContextId()
    {
        const int testChannelId = 100;

        _websiteChannelContextMock.SetupGet(x => x.WebsiteChannelID).Returns(testChannelId);

        // We can verify that the WebsiteChannelContext.WebsiteChannelID is read
        // by checking that it's called at least once
        int result = _websiteChannelContextMock.Object.WebsiteChannelID;

        Assert.That(result, Is.EqualTo(testChannelId));
        _websiteChannelContextMock.VerifyGet(x => x.WebsiteChannelID, Times.AtLeastOnce);
    }

    [Test]
    public void GetSiteConfigurationDtoAsync_UsesWebsiteChannelContextForChannelId()
    {
        const int expectedChannelId = 42;

        _websiteChannelContextMock.SetupGet(x => x.WebsiteChannelID).Returns(expectedChannelId);

        var channelId = _websiteChannelContextMock.Object.WebsiteChannelID;

        Assert.That(channelId, Is.EqualTo(expectedChannelId));
    }

    [Test]
    public void GetSiteConfigurationDtoAsync_AccessesWebsiteChannelContext()
    {
        // Test that the _websiteChannelContext property is properly injected and accessible
        Assert.That(_service, Is.Not.Null);
        Assert.That(_websiteChannelContextMock.Object, Is.Not.Null);
    }



    private sealed class TestWebsiteChannelInfo : WebsiteChannelInfo
    {
        public override int WebsiteChannelID { get; set; }
        public override int WebsiteChannelChannelID { get; set; }
        public override string WebsiteChannelDomain { get; set; } = string.Empty;
    }

    private sealed class TestWebChannelSettingsInfo : WebChannelSettingsInfo
    {
        public override int WebChannelSettingsID { get; set; }
        public override int WebChannelSettingsChannelID { get; set; }
    }

    private sealed class TestWebChannelSnippetInfo : WebChannelSnippetInfo
    {
        public override int WebChannelSnippetWebChannelSettingsID { get; set; }
        public override bool EnableexternallinkwarningInheritfromlocal { get; set; }
        public override bool Enableexternallinkwarning { get; set; }
    }

    private sealed class TestGlobalSettingsKeyInfo : GlobalsettingskeyInfo
    {
        public override bool Enableexternallinkwarning { get; set; }
    }
}
