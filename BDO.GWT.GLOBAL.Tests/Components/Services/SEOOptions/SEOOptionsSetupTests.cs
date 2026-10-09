using BDO;
using CMS.DataEngine;
using CMS.Websites.Routing;
using GWTXbyKWeb.Components.Services.SEOOptions;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Components.Services.SEOOptions;

[TestFixture]
public class SEOOptionsSetupTests
{
    private Mock<IInfoProvider<GlobalsettingskeyInfo>> _globalProviderMock = null!;
    private Mock<IInfoProvider<WebChannelSnippetInfo>> _snippetProviderMock = null!;
    private Mock<IWebsiteChannelContext> _websiteChannelContextMock = null!;
    private Mock<IInfoProvider<WebChannelSettingsInfo>> _channelSettingsProviderMock = null!;
    private SEOOptionsSetup _setup = null!;

    [SetUp]
    public void Setup()
    {
        _globalProviderMock = new Mock<IInfoProvider<GlobalsettingskeyInfo>>();
        _snippetProviderMock = new Mock<IInfoProvider<WebChannelSnippetInfo>>();
        _websiteChannelContextMock = new Mock<IWebsiteChannelContext>();
        _channelSettingsProviderMock = new Mock<IInfoProvider<WebChannelSettingsInfo>>();

        _websiteChannelContextMock
            .Setup(x => x.WebsiteChannelID)
            .Returns(1);

        _setup = new SEOOptionsSetup(
            _globalProviderMock.Object,
            _snippetProviderMock.Object,
            _websiteChannelContextMock.Object,
            _channelSettingsProviderMock.Object);
    }

    // --- Helpers -----------------------------------------------------------------

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

    private void SetupAllProvidersEmpty()
    {
        _channelSettingsProviderMock.Setup(p => p.Get())
            .Returns(BuildQuery<WebChannelSettingsInfo>(new WebChannelSettingsInfo[0]).Object);
        _snippetProviderMock.Setup(p => p.Get())
            .Returns(BuildQuery<WebChannelSnippetInfo>(new WebChannelSnippetInfo[0]).Object);
        _globalProviderMock.Setup(p => p.Get())
            .Returns(BuildQuery<GlobalsettingskeyInfo>(new GlobalsettingskeyInfo[0]).Object);
    }

    // --- Tests -------------------------------------------------------------------

    [Test]
    public void Setup_ImplementsIConfigureOptions()
    {
        Assert.That(_setup, Is.InstanceOf<Microsoft.Extensions.Options.IConfigureOptions<GWTXbyKWeb.Components.Services.SEOOptions.SEOOptions>>());
    }

    [Test]
    public void Configure_UsesCurrentWebsiteChannelId()
    {
        SetupAllProvidersEmpty();
        var options = new GWTXbyKWeb.Components.Services.SEOOptions.SEOOptions();

        _setup.Configure(options);

        _websiteChannelContextMock.Verify(x => x.WebsiteChannelID, Times.Once);
    }

    [Test]
    public void Configure_SetsEmptyRobotsAndSecurity_WhenNoSettingsExist()
    {
        SetupAllProvidersEmpty();
        var options = new GWTXbyKWeb.Components.Services.SEOOptions.SEOOptions
        {
            RobotsText = "preexisting",
            SecurityText = "preexisting",
        };

        _setup.Configure(options);

        Assert.Multiple(() =>
        {
            Assert.That(options.RobotsText, Is.EqualTo(string.Empty));
            Assert.That(options.SecurityText, Is.EqualTo(string.Empty));
        });
    }

    [Test]
    public void Configure_QueriesAllProviders_WhenSnippetMissing()
    {
        SetupAllProvidersEmpty();
        var options = new GWTXbyKWeb.Components.Services.SEOOptions.SEOOptions();

        _setup.Configure(options);

        Assert.Multiple(() =>
        {
            _channelSettingsProviderMock.Verify(p => p.Get(), Times.Once);
            _snippetProviderMock.Verify(p => p.Get(), Times.Once);
            // Global provider is queried once per inherit branch (robots + security).
            _globalProviderMock.Verify(p => p.Get(), Times.Exactly(2));
        });
    }

    [Test]
    public void Configure_UsesSnippetValues_WhenBothInheritFlagsAreFalse()
    {
        var channel = new TestWebChannelSettingsInfo { WebChannelSettingsID = 11, WebChannelSettingsChannelID = 1 };
        var snippet = new TestWebChannelSnippetInfo
        {
            WebChannelSnippetWebChannelSettingsID = 11,
            SeoSettingsRobotsInheritfromglobal = false,
            SeoSettingsRobots = "robots-local",
            SeoSettingsSecurityInheritfromGlobal = false,
            SeoSettingsSecurity = "security-local"
        };

        _channelSettingsProviderMock.Setup(p => p.Get())
            .Returns(BuildQuery<WebChannelSettingsInfo>(new[] { channel }).Object);
        _snippetProviderMock.Setup(p => p.Get())
            .Returns(BuildQuery<WebChannelSnippetInfo>(new[] { snippet }).Object);

        var options = new GWTXbyKWeb.Components.Services.SEOOptions.SEOOptions();

        _setup.Configure(options);

        Assert.Multiple(() =>
        {
            Assert.That(options.RobotsText, Is.EqualTo("robots-local"));
            Assert.That(options.SecurityText, Is.EqualTo("security-local"));
            _globalProviderMock.Verify(p => p.Get(), Times.Never);
        });
    }

    [Test]
    public void Configure_UsesGlobalValues_WhenBothInheritFlagsAreTrue()
    {
        var channel = new TestWebChannelSettingsInfo { WebChannelSettingsID = 11, WebChannelSettingsChannelID = 1 };
        var snippet = new TestWebChannelSnippetInfo
        {
            WebChannelSnippetWebChannelSettingsID = 11,
            SeoSettingsRobotsInheritfromglobal = true,
            SeoSettingsSecurityInheritfromGlobal = true,
            SeoSettingsRobots = "robots-local",
            SeoSettingsSecurity = "security-local"
        };
        var global = new TestGlobalSettingsInfo
        {
            SeoSettingsRobots = "robots-global",
            SeoSettingsSecurity = "security-global"
        };

        _channelSettingsProviderMock.Setup(p => p.Get())
            .Returns(BuildQuery<WebChannelSettingsInfo>(new[] { channel }).Object);
        _snippetProviderMock.Setup(p => p.Get())
            .Returns(BuildQuery<WebChannelSnippetInfo>(new[] { snippet }).Object);
        _globalProviderMock.Setup(p => p.Get())
            .Returns(BuildQuery<GlobalsettingskeyInfo>(new[] { global }).Object);

        var options = new GWTXbyKWeb.Components.Services.SEOOptions.SEOOptions();

        _setup.Configure(options);

        Assert.Multiple(() =>
        {
            Assert.That(options.RobotsText, Is.EqualTo("robots-global"));
            Assert.That(options.SecurityText, Is.EqualTo("security-global"));
            _globalProviderMock.Verify(p => p.Get(), Times.Exactly(2));
        });
    }

    [Test]
    public void Configure_PropagatesException_FromChannelSettingsProvider()
    {
        _channelSettingsProviderMock.Setup(p => p.Get())
            .Throws(new InvalidOperationException("db error"));
        var options = new GWTXbyKWeb.Components.Services.SEOOptions.SEOOptions();

        Assert.Throws<InvalidOperationException>(() => _setup.Configure(options));
    }

    private sealed class TestWebChannelSettingsInfo : WebChannelSettingsInfo
    {
        public override int WebChannelSettingsID { get; set; }
        public override int WebChannelSettingsChannelID { get; set; }
    }

    private sealed class TestWebChannelSnippetInfo : WebChannelSnippetInfo
    {
        public override int WebChannelSnippetWebChannelSettingsID { get; set; }
        public override bool SeoSettingsRobotsInheritfromglobal { get; set; }
        public override string SeoSettingsRobots { get; set; } = string.Empty;
        public override bool SeoSettingsSecurityInheritfromGlobal { get; set; }
        public override string SeoSettingsSecurity { get; set; } = string.Empty;
    }

    private sealed class TestGlobalSettingsInfo : GlobalsettingskeyInfo
    {
        public override string SeoSettingsRobots { get; set; } = string.Empty;
        public override string SeoSettingsSecurity { get; set; } = string.Empty;
    }
}
