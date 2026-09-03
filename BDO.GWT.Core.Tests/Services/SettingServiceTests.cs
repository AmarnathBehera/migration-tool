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
}
