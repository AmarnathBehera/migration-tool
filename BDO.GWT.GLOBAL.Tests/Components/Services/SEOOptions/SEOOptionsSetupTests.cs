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

    private static Mock<ObjectQuery<T>> BuildEmptyQuery<T>()
        where T : BaseInfo
    {
        var queryMock = new Mock<ObjectQuery<T>>();

        queryMock.Setup(q => q.WhereEquals(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(queryMock.Object);
        queryMock.Setup(q => q.TopN(It.IsAny<int>()))
            .Returns(queryMock.Object);
        queryMock.As<IEnumerable<T>>()
            .Setup(x => x.GetEnumerator())
            .Returns(() => Enumerable.Empty<T>().GetEnumerator());

        return queryMock;
    }

    private void SetupAllProvidersEmpty()
    {
        _channelSettingsProviderMock.Setup(p => p.Get())
            .Returns(BuildEmptyQuery<WebChannelSettingsInfo>().Object);
        _snippetProviderMock.Setup(p => p.Get())
            .Returns(BuildEmptyQuery<WebChannelSnippetInfo>().Object);
        _globalProviderMock.Setup(p => p.Get())
            .Returns(BuildEmptyQuery<GlobalsettingskeyInfo>().Object);
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
        // No channel setting, no snippet, no global ? both inherit branches (?? true)
        // resolve against an empty global provider, yielding empty strings.
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
    public void Configure_PropagatesException_FromChannelSettingsProvider()
    {
        _channelSettingsProviderMock.Setup(p => p.Get())
            .Throws(new InvalidOperationException("db error"));
        var options = new GWTXbyKWeb.Components.Services.SEOOptions.SEOOptions();

        Assert.Throws<InvalidOperationException>(() => _setup.Configure(options));
    }
}
