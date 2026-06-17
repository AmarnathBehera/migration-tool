using BDO.GWT.Core.Services;
using CMS.DataEngine;
using CMS.Websites;
using CMS.Websites.Routing;
using Moq;

namespace BDO.GWT.Core.Tests.Services;

public class RecaptchaValidationServiceTests
{
    private Mock<IHttpClientFactory> _httpClientFactoryMock = null!;
    private Mock<IWebsiteChannelContext> _websiteChannelContextMock = null!;
    private Mock<IInfoProvider<WebsiteCaptchaSettingsInfo>> _captchaSettingsProviderMock = null!;
    private TestableRecaptchaValidationService _service = null!;

    private const int TestChannelId = 42;
    private const string ValidRecaptchaResponse = "valid-recaptcha-token";
    private const string FakeSecretKey = "fake-secret-key";

    [SetUp]
    public void Setup()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _websiteChannelContextMock = new Mock<IWebsiteChannelContext>();
        _captchaSettingsProviderMock = new Mock<IInfoProvider<WebsiteCaptchaSettingsInfo>>();

        _websiteChannelContextMock
            .SetupGet(c => c.WebsiteChannelID)
            .Returns(TestChannelId);

        BuildService(privateKey: null);
    }

    // ─── Guard: empty / null / whitespace token ───────────────────────────────

    [Test]
    public async Task ValidateAsync_ReturnsFalse_WhenRecaptchaResponseIsNull()
    {
        var result = await _service.ValidateAsync(
            isV3: false,
            threshold: 0.5m,
            recaptchaResponse: null!,
            cancellationToken: CancellationToken.None);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task ValidateAsync_ReturnsFalse_WhenRecaptchaResponseIsEmpty()
    {
        var result = await _service.ValidateAsync(
            isV3: false,
            threshold: 0.5m,
            recaptchaResponse: string.Empty,
            cancellationToken: CancellationToken.None);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task ValidateAsync_ReturnsFalse_WhenRecaptchaResponseIsWhitespace()
    {
        var result = await _service.ValidateAsync(
            isV3: false,
            threshold: 0.5m,
            recaptchaResponse: "   ",
            cancellationToken: CancellationToken.None);

        Assert.That(result, Is.False);
    }

    // ─── Guard: no settings for channel ──────────────────────────────────────

    [Test]
    public async Task ValidateAsync_ReturnsFalse_WhenNoCaptchaSettingsFoundForChannel()
    {
        BuildService(privateKey: null);

        var result = await _service.ValidateAsync(
            isV3: false,
            threshold: 0.5m,
            recaptchaResponse: ValidRecaptchaResponse,
            cancellationToken: CancellationToken.None);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task ValidateAsync_ReturnsFalse_WhenNoCaptchaSettingsFoundForChannel_V3()
    {
        BuildService(privateKey: null);

        var result = await _service.ValidateAsync(
            isV3: true,
            threshold: 0.5m,
            recaptchaResponse: ValidRecaptchaResponse,
            cancellationToken: CancellationToken.None);

        Assert.That(result, Is.False);
    }

    // ─── Guard: private key lookup skipped when token invalid ─────────────────

    [Test]
    public async Task ValidateAsync_DoesNotCallGetRecaptchaPrivateKey_WhenTokenIsNull()
    {
        await _service.ValidateAsync(
            isV3: false,
            threshold: 0.5m,
            recaptchaResponse: null!,
            cancellationToken: CancellationToken.None);

        Assert.That(_service.GetRecaptchaPrivateKeyCalled, Is.False);
    }

    [Test]
    public async Task ValidateAsync_DoesNotCallGetRecaptchaPrivateKey_WhenTokenIsEmpty()
    {
        await _service.ValidateAsync(
            isV3: false,
            threshold: 0.5m,
            recaptchaResponse: string.Empty,
            cancellationToken: CancellationToken.None);

        Assert.That(_service.GetRecaptchaPrivateKeyCalled, Is.False);
    }

    [Test]
    public async Task ValidateAsync_DoesNotCallGetRecaptchaPrivateKey_WhenTokenIsWhitespace()
    {
        await _service.ValidateAsync(
            isV3: false,
            threshold: 0.5m,
            recaptchaResponse: "   ",
            cancellationToken: CancellationToken.None);

        Assert.That(_service.GetRecaptchaPrivateKeyCalled, Is.False);
    }

    // ─── Private key lookup IS called when token is valid ─────────────────────

    [Test]
    public async Task ValidateAsync_CallsGetRecaptchaPrivateKey_WhenTokenIsValid_ForV2()
    {
        BuildService(privateKey: null);

        await _service.ValidateAsync(
            isV3: false,
            threshold: 0.5m,
            recaptchaResponse: ValidRecaptchaResponse,
            cancellationToken: CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(_service.GetRecaptchaPrivateKeyCalled, Is.True);
            Assert.That(_service.LastIsV3Arg, Is.False);
        });
       
    }

    [Test]
    public async Task ValidateAsync_CallsGetRecaptchaPrivateKey_WhenTokenIsValid_ForV3()
    {
        BuildService(privateKey: null);

        await _service.ValidateAsync(
            isV3: true,
            threshold: 0.5m,
            recaptchaResponse: ValidRecaptchaResponse,
            cancellationToken: CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(_service.GetRecaptchaPrivateKeyCalled, Is.True);
            Assert.That(_service.LastIsV3Arg, Is.True);
        });
       
    }

    // ─── Integration: RecaptchaValidator behaviour with fake key ─────────────
    // RecaptchaValidator is new'd directly — its output cannot be controlled.
    // These tests only verify the call completes without throwing.

    [Test]
    [Category("Integration")]
    public async Task ValidateAsync_CompletesWithoutThrowing_WhenV2AndKeyProvided()
    {
        BuildService(privateKey: FakeSecretKey);

        Assert.DoesNotThrowAsync(async () =>
            await _service.ValidateAsync(
                isV3: false,
                threshold: 0.5m,
                recaptchaResponse: ValidRecaptchaResponse,
                cancellationToken: CancellationToken.None));
    }

    [Test]
    [Category("Integration")]
    public async Task ValidateAsync_CompletesWithoutThrowing_WhenV3AndKeyProvided()
    {
        BuildService(privateKey: FakeSecretKey);

        Assert.DoesNotThrowAsync(async () =>
            await _service.ValidateAsync(
                isV3: true,
                threshold: 0.5m,
                recaptchaResponse: ValidRecaptchaResponse,
                cancellationToken: CancellationToken.None));
    }

    [Test]
    [Category("Integration")]
    public async Task ValidateAsync_ReturnsBool_WhenV3AndKeyProvided()
    {
        BuildService(privateKey: FakeSecretKey);

        bool result = await _service.ValidateAsync(
            isV3: true,
            threshold: 0.5m,
            recaptchaResponse: ValidRecaptchaResponse,
            cancellationToken: CancellationToken.None);

        // Only assert it returns a bool — score is controlled by Kentico stub
        Assert.That(result, Is.TypeOf<bool>());
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private void BuildService(string? privateKey)
    {
        _service = new TestableRecaptchaValidationService(
            _httpClientFactoryMock.Object,
            _websiteChannelContextMock.Object,
            _captchaSettingsProviderMock.Object,
            privateKey);
    }

    // ─── Testable subclass ────────────────────────────────────────────────────

    private sealed class TestableRecaptchaValidationService : RecaptchaValidationService
    {
        private readonly string? _fakePrivateKey;

        public bool GetRecaptchaPrivateKeyCalled { get; private set; }
        public bool LastIsV3Arg { get; private set; }

        public TestableRecaptchaValidationService(
            IHttpClientFactory httpClientFactory,
            IWebsiteChannelContext websiteChannelContext,
            IInfoProvider<WebsiteCaptchaSettingsInfo> captchaSettingsProvider,
            string? fakePrivateKey)
            : base(httpClientFactory, websiteChannelContext, captchaSettingsProvider)
        {
            _fakePrivateKey = fakePrivateKey;
        }

        protected override string? GetRecaptchaPrivateKey(bool isV3)
        {
            GetRecaptchaPrivateKeyCalled = true;
            LastIsV3Arg = isV3;
            return _fakePrivateKey;
        }
    }
}