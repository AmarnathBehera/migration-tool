using BDO.GWT.Web.Core.Services;
using BDO.GWT.Web.Services;
using BDO.GWT.Xperience.Core.Services;
using CMS.ContentEngine;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Services;

[TestFixture]
public class SiteConfigurationServiceTests
{
    private Mock<ICachingService> _cachingServiceMock = null!;
    private Mock<IContentQueryExecutor> _executorMock = null!;
    private Mock<ISiteContext> _siteContextMock = null!;
    private SiteConfigurationProviders _providers = null!;
    private SiteConfigurationService _service = null!;

    [SetUp]
    public void Setup()
    {
        _cachingServiceMock = new Mock<ICachingService>();
        _executorMock = new Mock<IContentQueryExecutor>();
        _siteContextMock = new Mock<ISiteContext>();

        // GetCachedSiteConfigurationHash only touches ICachingService + ISiteContext,
        // so the provider dependencies are not exercised by these tests and may be null.
        _providers = new SiteConfigurationProviders(
            LocalizationKey: null!,
            LocalizationTranslation: null!,
            ContentLanguage: null!,
            WebChannelSettings: null!,
            WebsiteChannel: null!,
            provider: null!,
            websiteChannelContext: null!,
            webChannelSnippetInfoProvider: null!);

        _service = new SiteConfigurationService(
            _cachingServiceMock.Object,
            _executorMock.Object,
            _siteContextMock.Object,
            _providers);
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
}
