using BDO;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using CMS.ContactManagement;
using CMS.DataEngine;
using CMS.Globalization;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Moq;
using System.Data;

namespace BDO.GWT.Core.Tests.Services;

public class SubscribePageServiceTests
{
    private Mock<IInfoProvider<ContactGroupInfo>> _recipientListInfoProviderMock = null!;
    private Mock<IWebsiteChannelContext> _websiteChannelContextMock = null!;
    private Mock<IHeroService> _heroServiceMock = null!;
    private Mock<IContentRetriever> _contentRetrieverMock = null!;
    private Mock<IInfoProvider<WebsiteCaptchaSettingsInfo>> _captchaSettingsProviderMock = null!;
    private Mock<IInfoProvider<CountryInfo>> _countryInfoProviderMock = null!;
    private SubscribePageService _service = null!;

    [SetUp]
    public void Setup()
    {
        _recipientListInfoProviderMock = new Mock<IInfoProvider<ContactGroupInfo>>();
        _websiteChannelContextMock = new Mock<IWebsiteChannelContext>();
        _heroServiceMock = new Mock<IHeroService>();
        _contentRetrieverMock = new Mock<IContentRetriever>();
        _captchaSettingsProviderMock = new Mock<IInfoProvider<WebsiteCaptchaSettingsInfo>>();
        _countryInfoProviderMock = new Mock<IInfoProvider<CountryInfo>>();

        _heroServiceMock
            .Setup(h => h.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto { Layout = "hero" });

        _service = new SubscribePageService(
            _countryInfoProviderMock.Object,
            _recipientListInfoProviderMock.Object,
            _websiteChannelContextMock.Object,
            _heroServiceMock.Object,
            _contentRetrieverMock.Object,
            _captchaSettingsProviderMock.Object);
    }

    [Test]
    public async Task GetSubscribeDtoAsync_WhenPageIsNull_ReturnsDefaultDto()
    {
        var result = await _service.GetSubscribeDtoAsync(null, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Hero.Layout, Is.EqualTo(string.Empty));
            Assert.That(result.FormType, Is.Null);
            Assert.That(result.EmbedCode, Is.Null);
            Assert.That(result.SubscribeFormDto, Is.Null);
        });
    }

    [Test]
    public async Task GetSubscribeDtoAsync_WhenNotFormType_MapsPageAndSkipsFormDto()
    {
        HeroData? capturedHero = null;
        _heroServiceMock
            .Setup(h => h.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .Callback<HeroData, CancellationToken>((hero, _) => capturedHero = hero)
            .ReturnsAsync(new HeroDto { Layout = "banner" });

        var page = new Subscribe
        {
            SystemFields = new WebPageFields { WebPageItemGUID = Guid.NewGuid() },
            HeroLayout = "L1",
            HeroTitle = "Title",
            HeroDescription = "Desc",
            HeroCallToActionButtonType = "primary",
            HeroCallToActionButtonText = "Read more",
            FormType = "embed",
            EmbedCode = "<iframe></iframe>",
            RedirectPageOnFormSubmission = [],
        };

        var dto = await _service.GetSubscribeDtoAsync(page, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(dto.Hero.Layout, Is.EqualTo("banner"));
            Assert.That(dto.FormType, Is.EqualTo("embed"));
            Assert.That(dto.EmbedCode, Is.EqualTo("<iframe></iframe>"));
            Assert.That(dto.SubscribeFormDto, Is.Null);
            Assert.That(capturedHero, Is.Not.Null);
            Assert.That(capturedHero!.Layout, Is.EqualTo("L1"));
            Assert.That(capturedHero.Title, Is.EqualTo("Title"));
            Assert.That(capturedHero.Description, Is.EqualTo("Desc"));
        });

        _recipientListInfoProviderMock.Verify(p => p.Get(), Times.Never);
        _countryInfoProviderMock.Verify(p => p.Get(), Times.Never);
        _captchaSettingsProviderMock.Verify(p => p.Get(), Times.Never);
    }

    [Test]
    public void GetSubscribeDtoAsync_WhenFormType_InvokesCaptchaLookupBeforeProviderFailure()
    {
        _websiteChannelContextMock.SetupGet(x => x.WebsiteChannelID).Returns(10);

        _captchaSettingsProviderMock
            .Setup(p => p.Get())
            .Throws(new InvalidOperationException("captcha query unavailable"));

        _recipientListInfoProviderMock
            .Setup(p => p.Get())
            .Throws(new InvalidOperationException("newsletter query unavailable"));

        _countryInfoProviderMock
            .Setup(p => p.Get())
            .Throws(new InvalidOperationException("country query unavailable"));

        var page = new Subscribe
        {
            SystemFields = new WebPageFields { WebPageItemGUID = Guid.NewGuid() },
            FormType = "form",
            RedirectPageOnFormSubmission = [],
        };

        Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetSubscribeDtoAsync(page, CancellationToken.None));
        _captchaSettingsProviderMock.Verify(p => p.Get(), Times.Once);
    }
}
