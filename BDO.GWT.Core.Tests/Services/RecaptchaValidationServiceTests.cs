using BDO.GWT.Core.Services;
using CMS.DataEngine;
using CMS.Websites;
using CMS.Websites.Routing;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;

namespace BDO.GWT.Core.Tests.Services;

public class RecaptchaValidationServiceTests
{
    private Mock<IHttpClientFactory> _httpClientFactoryMock = null!;
    private Mock<IWebsiteChannelContext> _websiteChannelContextMock = null!;
    private Mock<IInfoProvider<WebsiteCaptchaSettingsInfo>> _captchaSettingsProviderMock = null!;
    private Mock<HttpMessageHandler> _httpMessageHandlerMock = null!;
    private RecaptchaValidationService _service = null!;

    private const int TestChannelId = 42;
    private const string ValidRecaptchaResponse = "valid-recaptcha-token";
    private const string FakeSecretKey = "fake-secret-key";

    [SetUp]
    public void Setup()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _websiteChannelContextMock = new Mock<IWebsiteChannelContext>();
        _captchaSettingsProviderMock = new Mock<IInfoProvider<WebsiteCaptchaSettingsInfo>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        _websiteChannelContextMock
            .Setup(c => c.WebsiteChannelID)
            .Returns(TestChannelId);

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _httpClientFactoryMock
            .Setup(f => f.CreateClient("Recaptcha"))
            .Returns(httpClient);

        _service = new RecaptchaValidationService(
            _httpClientFactoryMock.Object,
            _websiteChannelContextMock.Object,
            _captchaSettingsProviderMock.Object
        );
    }

    // ─── ValidateAsync: guard clauses ────────────────────────────────────────

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

    [Test]
    public async Task ValidateAsync_ReturnsFalse_WhenNoCaptchaSettingsFoundForChannel()
    {
        SetupCaptchaSettingsQuery(null);

        var result = await _service.ValidateAsync(
            isV3: false,
            threshold: 0.5m,
            recaptchaResponse: ValidRecaptchaResponse,
            cancellationToken: CancellationToken.None);

        Assert.That(result, Is.False);
    }

    // ─── ValidateAsync: V2 scenarios ─────────────────────────────────────────

    [Test]
    public async Task ValidateAsync_ReturnsTrue_WhenV2AndGoogleReturnsSuccess()
    {
        SetupCaptchaSettingsQuery(FakeSecretKey);
        SetupGoogleVerifyResponse(
            statusCode: HttpStatusCode.OK,
            json: """{"success":true,"score":0.0}""");

        var result = await _service.ValidateAsync(
            isV3: false,
            threshold: 0.5m,
            recaptchaResponse: ValidRecaptchaResponse,
            cancellationToken: CancellationToken.None);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task ValidateAsync_ReturnsFalse_WhenV2AndGoogleReturnsSuccessFalse()
    {
        SetupCaptchaSettingsQuery(FakeSecretKey);
        SetupGoogleVerifyResponse(
            statusCode: HttpStatusCode.OK,
            json: """{"success":false}""");

        var result = await _service.ValidateAsync(
            isV3: false,
            threshold: 0.5m,
            recaptchaResponse: ValidRecaptchaResponse,
            cancellationToken: CancellationToken.None);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task ValidateAsync_ReturnsFalse_WhenV2AndGoogleApiReturnsNonSuccessHttpStatus()
    {
        SetupCaptchaSettingsQuery(FakeSecretKey);
        SetupGoogleVerifyResponse(
            statusCode: HttpStatusCode.InternalServerError,
            json: string.Empty);

        var result = await _service.ValidateAsync(
            isV3: false,
            threshold: 0.5m,
            recaptchaResponse: ValidRecaptchaResponse,
            cancellationToken: CancellationToken.None);

        Assert.That(result, Is.False);
    }

    // ─── ValidateAsync: V3 score threshold ───────────────────────────────────

    [Test]
    public async Task ValidateAsync_ReturnsTrue_WhenV3AndScoreEqualsThreshold()
    {
        SetupCaptchaSettingsQuery(FakeSecretKey, version: ReCaptchaVersion.ReCaptchaV3);
        SetupGoogleVerifyResponse(
            statusCode: HttpStatusCode.OK,
            json: """{"success":true,"score":0.5}""");

        var result = await _service.ValidateAsync(
            isV3: true,
            threshold: 0.5m,
            recaptchaResponse: ValidRecaptchaResponse,
            cancellationToken: CancellationToken.None);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task ValidateAsync_ReturnsTrue_WhenV3AndScoreAboveThreshold()
    {
        SetupCaptchaSettingsQuery(FakeSecretKey, version: ReCaptchaVersion.ReCaptchaV3);
        SetupGoogleVerifyResponse(
            statusCode: HttpStatusCode.OK,
            json: """{"success":true,"score":0.9}""");

        var result = await _service.ValidateAsync(
            isV3: true,
            threshold: 0.5m,
            recaptchaResponse: ValidRecaptchaResponse,
            cancellationToken: CancellationToken.None);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task ValidateAsync_ReturnsFalse_WhenV3AndScoreBelowThreshold()
    {
        SetupCaptchaSettingsQuery(FakeSecretKey, version: ReCaptchaVersion.ReCaptchaV3);
        SetupGoogleVerifyResponse(
            statusCode: HttpStatusCode.OK,
            json: """{"success":true,"score":0.3}""");

        var result = await _service.ValidateAsync(
            isV3: true,
            threshold: 0.5m,
            recaptchaResponse: ValidRecaptchaResponse,
            cancellationToken: CancellationToken.None);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task ValidateAsync_ReturnsFalse_WhenV3AndGoogleReturnsSuccessFalse_RegardlessOfScore()
    {
        SetupCaptchaSettingsQuery(FakeSecretKey, version: ReCaptchaVersion.ReCaptchaV3);
        SetupGoogleVerifyResponse(
            statusCode: HttpStatusCode.OK,
            json: """{"success":false,"score":0.9}""");

        var result = await _service.ValidateAsync(
            isV3: true,
            threshold: 0.5m,
            recaptchaResponse: ValidRecaptchaResponse,
            cancellationToken: CancellationToken.None);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task ValidateAsync_ReturnsFalse_WhenV3AndGoogleApiReturnsNonSuccessHttpStatus()
    {
        SetupCaptchaSettingsQuery(FakeSecretKey, version: ReCaptchaVersion.ReCaptchaV3);
        SetupGoogleVerifyResponse(
            statusCode: HttpStatusCode.BadRequest,
            json: string.Empty);

        var result = await _service.ValidateAsync(
            isV3: true,
            threshold: 0.5m,
            recaptchaResponse: ValidRecaptchaResponse,
            cancellationToken: CancellationToken.None);

        Assert.That(result, Is.False);
    }

    // ─── ValidateAsync: secret key lookup uses correct version ───────────────

    [Test]
    public async Task ValidateAsync_UsesV3SecretKey_WhenIsV3IsTrue()
    {
        SetupCaptchaSettingsQuery(FakeSecretKey, version: ReCaptchaVersion.ReCaptchaV3);
        SetupGoogleVerifyResponse(
            statusCode: HttpStatusCode.OK,
            json: """{"success":true,"score":0.9}""");

        var result = await _service.ValidateAsync(
            isV3: true,
            threshold: 0.5m,
            recaptchaResponse: ValidRecaptchaResponse,
            cancellationToken: CancellationToken.None);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task ValidateAsync_UsesV2SecretKey_WhenIsV3IsFalse()
    {
        SetupCaptchaSettingsQuery(FakeSecretKey, version: ReCaptchaVersion.ReCaptchaV2);
        SetupGoogleVerifyResponse(
            statusCode: HttpStatusCode.OK,
            json: """{"success":true,"score":0.0}""");

        var result = await _service.ValidateAsync(
            isV3: false,
            threshold: 0.5m,
            recaptchaResponse: ValidRecaptchaResponse,
            cancellationToken: CancellationToken.None);

        Assert.That(result, Is.True);
    }

    // ─── ValidateAsync: cancellation token is forwarded ──────────────────────

    [Test]
    public async Task ValidateAsync_ForwardsCancellationToken_ToHttpCall()
    {
        SetupCaptchaSettingsQuery(FakeSecretKey);

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException());

        Assert.ThrowsAsync<TaskCanceledException>(async () =>
            await _service.ValidateAsync(
                isV3: false,
                threshold: 0.5m,
                recaptchaResponse: ValidRecaptchaResponse,
                cancellationToken: cts.Token));
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private void SetupCaptchaSettingsQuery(
        string? secretKey,
        ReCaptchaVersion version = ReCaptchaVersion.ReCaptchaV2)
    {
        if (secretKey is null)
        {
            var emptyQuery = new Mock<ObjectQuery<WebsiteCaptchaSettingsInfo>>();
            emptyQuery
                .Setup(q => q.WhereEquals(It.IsAny<string>(), It.IsAny<object>()))
                .Returns(emptyQuery.Object);
            emptyQuery
                .Setup(q => q.TopN(It.IsAny<int>()))
                .Returns(emptyQuery.Object);
            emptyQuery
                .Setup(q => q.GetEnumerator())
                .Returns(Enumerable.Empty<WebsiteCaptchaSettingsInfo>().GetEnumerator());

            _captchaSettingsProviderMock
                .Setup(p => p.Get())
                .Returns(emptyQuery.Object);

            return;
        }

        var settingsInfo = new Mock<WebsiteCaptchaSettingsInfo>();
        settingsInfo
            .SetupGet(i => i.WebsiteCaptchaSettingsReCaptchaSecretKey)
            .Returns(secretKey);
        settingsInfo
            .SetupGet(i => i.WebsiteCaptchaSettingsReCaptchaVersion)
            .Returns(version);

        var query = new Mock<ObjectQuery<WebsiteCaptchaSettingsInfo>>();
        query
            .Setup(q => q.WhereEquals(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(query.Object);
        query
            .Setup(q => q.TopN(It.IsAny<int>()))
            .Returns(query.Object);
        query
            .Setup(q => q.GetEnumerator())
            .Returns(new List<WebsiteCaptchaSettingsInfo> { settingsInfo.Object }.GetEnumerator());

        _captchaSettingsProviderMock
            .Setup(p => p.Get())
            .Returns(query.Object);
    }

    private void SetupGoogleVerifyResponse(HttpStatusCode statusCode, string json)
    {
        var response = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
    }
}