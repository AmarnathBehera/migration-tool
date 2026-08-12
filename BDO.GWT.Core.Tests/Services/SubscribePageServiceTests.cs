using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.Services;
using CMS.ContactManagement;
using CMS.DataEngine;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Moq;

namespace BDO.GWT.Core.Tests.Services;

public class SubscribePageServiceTests
{
    private Mock<IInfoProvider<ContactGroupInfo>> _recipientListInfoProviderMock = null!;
    private Mock<IWebsiteChannelContext> _websiteChannelContextMock = null!;
    private Mock<IHeroService> _heroServiceMock = null!;
    private Mock<IContentRetriever> _contentRetrieverMock = null!;
    private Mock<IInfoProvider<WebsiteCaptchaSettingsInfo>> _captchaSettingsProviderMock = null!;
    private SubscribePageService _service = null!;

    [SetUp]
    public void Setup()
    {
        _recipientListInfoProviderMock = new Mock<IInfoProvider<ContactGroupInfo>>();
        _websiteChannelContextMock = new Mock<IWebsiteChannelContext>();
        _heroServiceMock = new Mock<IHeroService>();
        _contentRetrieverMock = new Mock<IContentRetriever>();
        _captchaSettingsProviderMock = new Mock<IInfoProvider<WebsiteCaptchaSettingsInfo>>();

        _service = new SubscribePageService(
            _recipientListInfoProviderMock.Object,
            _websiteChannelContextMock.Object,
            _heroServiceMock.Object,
            _contentRetrieverMock.Object,
            _captchaSettingsProviderMock.Object);
    }

    // --- GetSubscribeDtoAsync ----------------------------------------------------

    [Test]
    public async Task GetSubscribeDtoAsync_WhenPageIsNull_ReturnsDefaultDto()
    {
        var result = await _service.GetSubscribeDtoAsync(null, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.Hero, Is.Not.Null);
            Assert.That(result.Hero.Layout, Is.EqualTo(string.Empty));
            Assert.That(result.FormType, Is.Null);
            Assert.That(result.EmbedCode, Is.Null);
            Assert.That(result.SubscribeFormDto, Is.Null);
        });
    }

    [Test]
    public async Task GetSubscribeDtoAsync_WhenPageIsNull_DoesNotCallHeroService()
    {
        await _service.GetSubscribeDtoAsync(null, CancellationToken.None);

        _heroServiceMock.Verify(
            h => h.GetHeroDtoAsync(It.IsAny<BDO.GWT.Core.Data.HeroData>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GetSubscribeDtoAsync_WhenPageIsNull_DoesNotQueryRecipientLists()
    {
        await _service.GetSubscribeDtoAsync(null, CancellationToken.None);

        _recipientListInfoProviderMock.Verify(p => p.Get(), Times.Never);
    }

    [Test]
    public async Task GetSubscribeDtoAsync_WhenPageIsNull_DoesNotQueryCaptchaSettings()
    {
        await _service.GetSubscribeDtoAsync(null, CancellationToken.None);

        _captchaSettingsProviderMock.Verify(p => p.Get(), Times.Never);
    }

    [Test]
    public async Task GetSubscribeDtoAsync_WhenPageIsNull_DoesNotUseContentRetriever()
    {
        await _service.GetSubscribeDtoAsync(null, CancellationToken.None);

        _contentRetrieverMock.VerifyNoOtherCalls();
    }
}
