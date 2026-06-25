using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using CMS.Websites;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Core.Tests.Services;

public class SocialMediaButtonsServiceTests
{
    private Mock<ISettingService> _settingServiceMock = null!;
    private Mock<IWebPageUrlResolver> _urlResolverMock = null!;
    private SocialMediaButtonsService _service = null!;

    [SetUp]
    public void Setup()
    {
        _settingServiceMock = new Mock<ISettingService>();
        _urlResolverMock = new Mock<IWebPageUrlResolver>();

        _urlResolverMock
            .Setup(x => x.GetAbsoluteUrl(It.IsAny<IWebPageFieldsSource>()))
            .Returns("https://test.com/page");

        _service = new SocialMediaButtonsService(
            _settingServiceMock.Object,
            _urlResolverMock.Object);
    }

    private static TestPrintorShareInfo Create(
        bool email = false,
        bool fb = false,
        bool tw = false,
        bool li = false,
        bool copy = false)
    {
        // NOTE: We cannot use an object initializer (i.e. `new PrintorShareInfo { DisableEmail = ... }`)
        // because the property setters call AbstractInfoBase.SetValue, which lazily resolves the
        // Kentico DataClass through the CMS IoC container. The container is not initialized in unit
        // tests, so any setter call throws ServiceResolutionException. Instead, use a test-only
        // subclass that overrides the virtual getters with simple auto-properties, bypassing SetValue.
        return new TestPrintorShareInfo
        {
            DisableEmail = email,
            DisableFacebook = fb,
            DisableTwitter = tw,
            DisableLinkedIn = li,
            DisableCopyLinkButton = copy,
        };
    }

    private sealed class TestPrintorShareInfo : PrintorShareInfo
    {
        public override bool DisableEmail { get; set; }
        public override bool DisableFacebook { get; set; }
        public override bool DisableTwitter { get; set; }
        public override bool DisableLinkedIn { get; set; }
        public override bool DisableCopyLinkButton { get; set; }
    }

    // -------------------------------
    // BASIC ENABLE/DISABLE COVERAGE
    // -------------------------------

    [Test]
    public async Task AllEnabled_ReturnsAllUrls()
    {
        _settingServiceMock
            .Setup(x => x.GetPrintorShareInfo())
            .ReturnsAsync(new[] { Create() });

        var page = new Mock<IWebPageFieldsSource>();

        var result = await _service.GetSocialMediaButtonsDto(page.Object, "Title", CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.EmailButtonUrl, Is.Not.Null);
            Assert.That(result.FacebookButtonUrl, Is.Not.Null);
            Assert.That(result.TwitterButtonUrl, Is.Not.Null);
            Assert.That(result.LinkedInUrl, Is.Not.Null);
            Assert.That(result.ShowCopyLinkButton, Is.True);
        });
    }

    [Test]
    public async Task DisableEmail_ReturnsNull()
    {
        _settingServiceMock
            .Setup(x => x.GetPrintorShareInfo())
            .ReturnsAsync(new[] { Create(email: true) });

        var page = new Mock<IWebPageFieldsSource>();

        var result = await _service.GetSocialMediaButtonsDto(page.Object, "Title", CancellationToken.None);

        Assert.That(result.EmailButtonUrl, Is.Null);
    }

    [Test]
    public async Task DisableFacebook_ReturnsNull()
    {
        _settingServiceMock
            .Setup(x => x.GetPrintorShareInfo())
            .ReturnsAsync(new[] { Create(fb: true) });

        var page = new Mock<IWebPageFieldsSource>();

        var result = await _service.GetSocialMediaButtonsDto(page.Object, "Title", CancellationToken.None);

        Assert.That(result.FacebookButtonUrl, Is.Null);
    }

    [Test]
    public async Task DisableTwitter_ReturnsNull()
    {
        _settingServiceMock
            .Setup(x => x.GetPrintorShareInfo())
            .ReturnsAsync(new[] { Create(tw: true) });

        var page = new Mock<IWebPageFieldsSource>();

        var result = await _service.GetSocialMediaButtonsDto(page.Object, "Title", CancellationToken.None);

        Assert.That(result.TwitterButtonUrl, Is.Null);
    }

    [Test]
    public async Task DisableLinkedIn_ReturnsNull()
    {
        _settingServiceMock
            .Setup(x => x.GetPrintorShareInfo())
            .ReturnsAsync(new[] { Create(li: true) });

        var page = new Mock<IWebPageFieldsSource>();

        var result = await _service.GetSocialMediaButtonsDto(page.Object, "Title", CancellationToken.None);

        Assert.That(result.LinkedInUrl, Is.Null);
    }

    [Test]
    public async Task DisableCopyLink_ReturnsFalse()
    {
        _settingServiceMock
            .Setup(x => x.GetPrintorShareInfo())
            .ReturnsAsync(new[] { Create(copy: true) });

        var page = new Mock<IWebPageFieldsSource>();

        var result = await _service.GetSocialMediaButtonsDto(page.Object, "Title", CancellationToken.None);

        Assert.That(result.ShowCopyLinkButton, Is.False);
    }

    [Test]
    public async Task NullSettings_ReturnsDefaults()
    {
        _settingServiceMock
            .Setup(x => x.GetPrintorShareInfo())
            .ReturnsAsync((IEnumerable<PrintorShareInfo>?)null);

        var page = new Mock<IWebPageFieldsSource>();

        var result = await _service.GetSocialMediaButtonsDto(page.Object, "Title", CancellationToken.None);

        Assert.That(result.ShowCopyLinkButton, Is.True);
    }

    // -------------------------------
    // URL RESOLVER COVERAGE
    // -------------------------------

    [Test]
    public async Task GetDto_UsesPageUrlResolver_Once()
    {
        _settingServiceMock
            .Setup(x => x.GetPrintorShareInfo())
            .ReturnsAsync(new[] { Create() });

        var page = new Mock<IWebPageFieldsSource>();

        await _service.GetSocialMediaButtonsDto(page.Object, "Title", CancellationToken.None);

        _urlResolverMock.Verify(
            x => x.GetAbsoluteUrl(page.Object),
            Times.Once);
    }

    [Test]
    public async Task GetDto_UsesResolvedUrl_InShareLinks()
    {
        _settingServiceMock
            .Setup(x => x.GetPrintorShareInfo())
            .ReturnsAsync(new[] { Create() });

        _urlResolverMock
            .Setup(x => x.GetAbsoluteUrl(It.IsAny<IWebPageFieldsSource>()))
            .Returns("https://resolved.com/page");

        var page = new Mock<IWebPageFieldsSource>();

        var result = await _service.GetSocialMediaButtonsDto(page.Object, "Title", CancellationToken.None);

        // The service URL-encodes the resolved URL before embedding it in share links.
        var encoded = System.Web.HttpUtility.UrlEncode("https://resolved.com/page");
        Assert.Multiple(() =>
        {
            Assert.That(result.EmailButtonUrl, Does.Contain(encoded));
            Assert.That(result.LinkedInUrl, Does.Contain(encoded));
        });
    }

    // -------------------------------
    // URL ENCODING SAFETY
    // -------------------------------

    [Test]
    public async Task ShareUrls_EncodesTitleAndUrl_Safely()
    {
        _settingServiceMock
            .Setup(x => x.GetPrintorShareInfo())
            .ReturnsAsync(new[] { Create() });

        _urlResolverMock
            .Setup(x => x.GetAbsoluteUrl(It.IsAny<IWebPageFieldsSource>()))
            .Returns("https://example.com/page?x=1&y=2");

        var page = new Mock<IWebPageFieldsSource>();

        var result = await _service.GetSocialMediaButtonsDto(
            page.Object,
            "Test Title & Special #Chars",
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.EmailButtonUrl, Does.Contain("%"));
            Assert.That(result.TwitterButtonUrl, Does.Contain("%"));
            Assert.That(result.EmailButtonUrl, Does.Not.Contain("Test Title & Special"));
        });
    }

    // -------------------------------
    // MULTIPLE SETTINGS PRECEDENCE
    // -------------------------------

    [Test]
    public async Task MultipleSettings_FirstOneWins()
    {
        _settingServiceMock
            .Setup(x => x.GetPrintorShareInfo())
            .ReturnsAsync(new[]
            {
                Create(email: true),
                Create(email: false)
            });

        var page = new Mock<IWebPageFieldsSource>();

        var result = await _service.GetSocialMediaButtonsDto(page.Object, "Title", CancellationToken.None);

        Assert.That(result.EmailButtonUrl, Is.Null);
    }

    [Test]
    public async Task MultipleSettings_FirstOneWins_TrailingSettingIsIgnored()
    {
        // The service selects the first setting via FirstOrDefault(), so the trailing
        // setting (email: true) must be ignored and the email URL should be generated.
        _settingServiceMock
            .Setup(x => x.GetPrintorShareInfo())
            .ReturnsAsync(new[]
            {
                Create(email: false),
                Create(email: true)
            });

        var page = new Mock<IWebPageFieldsSource>();

        var result = await _service.GetSocialMediaButtonsDto(page.Object, "Title", CancellationToken.None);

        Assert.That(result.EmailButtonUrl, Is.Not.Null);
    }
}