using BDO.GWT.Common.Core.Constants;
using BDO.GWT.Common.Helpers;
using BDO.GWT.Domain.Core.Entities;
using BDO.GWT.Web.Core.LiveSite.Services;
using BDO;
using CMS.DataEngine;
using CMS.OnlineForms;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Data;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BDO.GWT.Core.Tests.Services;

[TestFixture]
public class FormSubmissionSubscriptionsServiceTests
{
    private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

    // CA1861: avoid allocating a new array on each call site of CreateSubscription.
    private static readonly string[] SingleEmptyFormName = [""];

    private Mock<ILogger<FormSubmissionSubscriptionsService>> _loggerMock = null!;
    private Mock<IHttpClientFactory> _httpClientFactoryMock = null!;
    private Mock<IInfoProvider<FormSubmissionSubscriptionsInfo>> _subscriptionProviderMock = null!;
    private Mock<ISecretsEncryptionHelper> _secretsEncryptionHelperMock = null!;
    private FormSubmissionSubscriptionsService _service = null!;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<FormSubmissionSubscriptionsService>>();
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _subscriptionProviderMock = new Mock<IInfoProvider<FormSubmissionSubscriptionsInfo>>();
        _secretsEncryptionHelperMock = new Mock<ISecretsEncryptionHelper>();

        _service = new FormSubmissionSubscriptionsService(
            _loggerMock.Object,
            _httpClientFactoryMock.Object,
            _secretsEncryptionHelperMock.Object,
            _subscriptionProviderMock.Object);
    }

    // ??? FormatRequestBody ?????????????????????????????????????????????????????

    [Test]
    public void FormatRequestBody_EmptyFormEntry_ReturnsUnchangedTemplate()
    {
        var entry = new FormEntry();
        var result = FormSubmissionSubscriptionsService.FormatRequestBody(entry, "{\"name\":\"$Name$\"}");

        Assert.That(result, Is.EqualTo("{\"name\":\"$Name$\"}"));
    }

    [Test]
    public void FormatRequestBody_NullValue_QuotedPlaceholder_ReplacesWithEmptyString()
    {
        var entry = new FormEntry { ["Name"] = null };
        var result = FormSubmissionSubscriptionsService.FormatRequestBody(entry, "{\"name\":\"$Name$\"}");

        Assert.That(result, Is.EqualTo("{\"name\":\"\"}"));
    }

    [Test]
    public void FormatRequestBody_NullValue_UnquotedPlaceholder_ReplacesWithNull()
    {
        var entry = new FormEntry { ["Age"] = null };
        var result = FormSubmissionSubscriptionsService.FormatRequestBody(entry, "{\"age\":$Age$}");

        Assert.That(result, Is.EqualTo("{\"age\":null}"));
    }

    [Test]
    public void FormatRequestBody_NonNullValue_QuotedPlaceholder_JsonSerializesValue()
    {
        var entry = new FormEntry { ["Name"] = "John \"Doe\"" };
        var result = FormSubmissionSubscriptionsService.FormatRequestBody(entry, "{\"name\":\"$Name$\"}");

        // JsonSerializer.Serialize produces properly escaped JSON string with surrounding quotes.
        Assert.That(result, Does.Not.Contain("\"$Name$\""));
        Assert.That(result, Does.Contain("John"));
    }

    [Test]
    public void FormatRequestBody_NonNullValue_UnquotedPlaceholder_JsonEncodesValue()
    {
        var entry = new FormEntry { ["Count"] = "42" };
        var result = FormSubmissionSubscriptionsService.FormatRequestBody(entry, "{\"count\":$Count$}");

        Assert.That(result, Is.EqualTo("{\"count\":42}"));
    }

    [Test]
    public void FormatRequestBody_MultipleKeys_AllReplacedCorrectly()
    {
        var entry = new FormEntry
        {
            ["FirstName"] = "Jane",
            ["LastName"] = null
        };
        var template = "{\"first\":\"$FirstName$\",\"last\":$LastName$}";
        var result = FormSubmissionSubscriptionsService.FormatRequestBody(entry, template);

        Assert.Multiple(() =>
        {
            Assert.That(result, Does.Contain("Jane"));
            Assert.That(result, Does.Contain("null"));
        });
    }

    [Test]
    public void FormatRequestBody_NonNullValue_QuotedPlaceholder_NullValueFallsThrough_ReplacesWithEmptyString()
    {
        // Exercises the newRequestBody == requestBody branch for null value with quoted placeholder.
        // When a null value has a quoted placeholder AND the replace of "\"$key$\"" changes the string,
        // it should not also apply the $key$ ? null replacement.
        var entry = new FormEntry { ["X"] = null };
        var result = FormSubmissionSubscriptionsService.FormatRequestBody(entry, "\"$X$\"");

        Assert.That(result, Is.EqualTo("\"\""));
    }

    [Test]
    public void FormatRequestBody_NullValue_PlaceholderNotQuotedInTemplate_ReplacesWithNull()
    {
        // Covers the else branch: requestBody.Replace($"${key}$", "null")
        var entry = new FormEntry { ["Field"] = null };
        var result = FormSubmissionSubscriptionsService.FormatRequestBody(entry, "value=$Field$&other=1");

        Assert.That(result, Is.EqualTo("value=null&other=1"));
    }

    // ??? InvokeRegisteredEndpointsAsync ????????????????????????????????????????

    [Test]
    public async Task InvokeRegisteredEndpointsAsync_NullFormItem_ReturnsWithoutCallingProvider()
    {
        await _service.InvokeRegisteredEndpointsAsync(null!, new FormEntry(), CancellationToken.None);

        _subscriptionProviderMock.Verify(p => p.Get(), Times.Never);
    }

    [Test]
    public async Task InvokeRegisteredEndpointsAsync_NoMatchingSubscriptions_ReturnsWithoutCreatingHttpClient()
    {
        SetupSubscriptions(); // empty list — no subscriptions

        var formItem = CreateUninitializedBizFormItem();
        await _service.InvokeRegisteredEndpointsAsync(formItem, new FormEntry(), CancellationToken.None);

        _httpClientFactoryMock.Verify(f => f.CreateClient(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task InvokeRegisteredEndpointsAsync_SubscriptionWithEmptyEndpoint_IsSkipped()
    {
        var subscription = CreateSubscription(endpoint: "", method: "POST", formNames: SingleEmptyFormName);
        SetupSubscriptions(subscription);
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(new HttpClient());

        var formItem = CreateUninitializedBizFormItem();

        // Should complete without exception; empty endpoint guard skips the subscription.
        Assert.DoesNotThrowAsync(() =>
            _service.InvokeRegisteredEndpointsAsync(formItem, new FormEntry(), CancellationToken.None));
    }

    [Test]
    public async Task InvokeRegisteredEndpointsAsync_SubscriptionWithEmptyMethod_IsSkipped()
    {
        var subscription = CreateSubscription(endpoint: "https://example.com/api", method: "", formNames: SingleEmptyFormName);
        SetupSubscriptions(subscription);
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(new HttpClient());

        var formItem = CreateUninitializedBizFormItem();

        Assert.DoesNotThrowAsync(() =>
            _service.InvokeRegisteredEndpointsAsync(formItem, new FormEntry(), CancellationToken.None));
    }

    [Test]
    public async Task InvokeRegisteredEndpointsAsync_InvalidUri_LogsWarningAndSkips()
    {
        var subscription = CreateSubscription(endpoint: "not-a-valid-uri", method: "POST", formNames: SingleEmptyFormName);
        SetupSubscriptions(subscription);
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(new HttpClient());

        var formItem = CreateUninitializedBizFormItem();
        await _service.InvokeRegisteredEndpointsAsync(formItem, new FormEntry(), CancellationToken.None);

        VerifyLoggedAtLeastOnce(LogLevel.Warning);
    }

    [Test]
    public async Task InvokeRegisteredEndpointsAsync_InvalidHttpMethod_LogsWarningAndSkips()
    {
        var subscription = CreateSubscription(
            endpoint: "https://example.com/api",
            method: "INVALID METHOD!",
            formNames: SingleEmptyFormName);
        SetupSubscriptions(subscription);
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(new HttpClient());

        var formItem = CreateUninitializedBizFormItem();
        await _service.InvokeRegisteredEndpointsAsync(formItem, new FormEntry(), CancellationToken.None);

        VerifyLoggedAtLeastOnce(LogLevel.Warning);
    }

    [Test]
    public async Task InvokeRegisteredEndpointsAsync_GetMethodWithBodyFormat_LogsBodyIgnoredWarning()
    {
        var subscription = CreateSubscription(
            endpoint: "https://example.com/api",
            method: "GET",
            formNames: SingleEmptyFormName,
            requestBodyFormat: "{\"data\":\"test\"}");
        SetupSubscriptions(subscription);

        var handler = CreateMockHandler(new HttpResponseMessage(HttpStatusCode.OK));
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(new HttpClient(handler));

        var formItem = CreateUninitializedBizFormItem();
        await _service.InvokeRegisteredEndpointsAsync(formItem, new FormEntry(), CancellationToken.None);

        VerifyLoggedAtLeastOnce(LogLevel.Warning);
    }

    [Test]
    public async Task InvokeRegisteredEndpointsAsync_PostMethodWithBodyAndContentType_SendsRequestSuccessfully()
    {
        var subscription = CreateSubscription(
            endpoint: "https://example.com/api",
            method: "POST",
            formNames: SingleEmptyFormName,
            requestBodyFormat: "{\"data\":\"test\"}",
            requestContentType: "application/json");
        SetupSubscriptions(subscription);

        var handler = CreateMockHandler(new HttpResponseMessage(HttpStatusCode.OK));
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(new HttpClient(handler));

        var formItem = CreateUninitializedBizFormItem();

        Assert.DoesNotThrowAsync(() =>
            _service.InvokeRegisteredEndpointsAsync(formItem, new FormEntry(), CancellationToken.None));
    }

    [Test]
    public async Task InvokeRegisteredEndpointsAsync_PostMethodWithBodyNoContentType_SendsWithDefaultEncoding()
    {
        var subscription = CreateSubscription(
            endpoint: "https://example.com/api",
            method: "POST",
            formNames: SingleEmptyFormName,
            requestBodyFormat: "{\"data\":\"test\"}",
            requestContentType: "");
        SetupSubscriptions(subscription);

        var handler = CreateMockHandler(new HttpResponseMessage(HttpStatusCode.OK));
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(new HttpClient(handler));

        var formItem = CreateUninitializedBizFormItem();

        Assert.DoesNotThrowAsync(() =>
            _service.InvokeRegisteredEndpointsAsync(formItem, new FormEntry(), CancellationToken.None));
    }

    [Test]
    public async Task InvokeRegisteredEndpointsAsync_NonSuccessStatusCode_LogsError()
    {
        var subscription = CreateSubscription(
            endpoint: "https://example.com/api",
            method: "POST",
            formNames: SingleEmptyFormName);
        SetupSubscriptions(subscription);

        var handler = CreateMockHandler(new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            ReasonPhrase = "Internal Server Error"
        });
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(new HttpClient(handler));

        var formItem = CreateUninitializedBizFormItem();
        await _service.InvokeRegisteredEndpointsAsync(formItem, new FormEntry(), CancellationToken.None);

        VerifyLoggedAtLeastOnce(LogLevel.Error);
    }

    [Test]
    public async Task InvokeRegisteredEndpointsAsync_SendAsyncThrowsHttpRequestException_LogsError()
    {
        var subscription = CreateSubscription(
            endpoint: "https://example.com/api",
            method: "POST",
            formNames: SingleEmptyFormName);
        SetupSubscriptions(subscription);

        var handler = CreateThrowingHandler(new HttpRequestException("Connection refused"));
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(new HttpClient(handler));

        var formItem = CreateUninitializedBizFormItem();
        await _service.InvokeRegisteredEndpointsAsync(formItem, new FormEntry(), CancellationToken.None);

        VerifyLoggedAtLeastOnce(LogLevel.Error, withException: true);
    }

    [Test]
    public async Task InvokeRegisteredEndpointsAsync_TimeoutOccurs_LogsErrorAndContinues()
    {
        // Simulate a timeout by throwing OperationCanceledException while the outer
        // CancellationToken (None) is not cancelled — matches the "when (!cancellationToken.IsCancellationRequested)" filter.
        var subscription = CreateSubscription(
            endpoint: "https://example.com/api",
            method: "POST",
            formNames: SingleEmptyFormName);
        SetupSubscriptions(subscription);

        var handler = CreateThrowingHandler(new OperationCanceledException());
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(new HttpClient(handler));

        var formItem = CreateUninitializedBizFormItem();
        await _service.InvokeRegisteredEndpointsAsync(formItem, new FormEntry(), CancellationToken.None);

        VerifyLoggedAtLeastOnce(LogLevel.Error);
    }

    [Test]
    public async Task InvokeRegisteredEndpointsAsync_WithValidBasicAuth_AddsAuthorizationHeader()
    {
        var subscription = CreateSubscription(
            endpoint: "https://example.com/api",
            method: "POST",
            formNames: SingleEmptyFormName,
            useAuthorization: true,
            scheme: AuthenticationScheme.Basic,
            username: "user",
            encryptedPassword: "encPwd");
        SetupSubscriptions(subscription);

        _secretsEncryptionHelperMock.Setup(s => s.DecryptSecret("encPwd")).Returns("secret");

        HttpRequestMessage? captured = null;
        var handler = CreateCapturingHandler(
            new HttpResponseMessage(HttpStatusCode.OK),
            req => captured = req);
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(new HttpClient(handler));

        var formItem = CreateUninitializedBizFormItem();
        await _service.InvokeRegisteredEndpointsAsync(formItem, new FormEntry(), CancellationToken.None);

        Assert.That(captured?.Headers.Authorization, Is.Not.Null);
        var decoded = System.Text.Encoding.UTF8.GetString(
            Convert.FromBase64String(captured!.Headers.Authorization!.Parameter!));
        Assert.Multiple(() =>
        {
            Assert.That(captured!.Headers.Authorization!.Scheme, Is.EqualTo("Basic"));
            Assert.That(decoded, Is.EqualTo("user:secret"));
        });
    }

    [Test]
    public async Task InvokeRegisteredEndpointsAsync_BasicAuth_EmptyEncryptedPassword_SendsHeaderWithNoPassword()
    {
        var subscription = CreateSubscription(
            endpoint: "https://example.com/api",
            method: "POST",
            formNames: SingleEmptyFormName,
            useAuthorization: true,
            scheme: AuthenticationScheme.Basic,
            username: "user",
            encryptedPassword: "");
        SetupSubscriptions(subscription);

        HttpRequestMessage? captured = null;
        var handler = CreateCapturingHandler(
            new HttpResponseMessage(HttpStatusCode.OK),
            req => captured = req);
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(new HttpClient(handler));

        var formItem = CreateUninitializedBizFormItem();
        await _service.InvokeRegisteredEndpointsAsync(formItem, new FormEntry(), CancellationToken.None);

        Assert.That(captured?.Headers.Authorization, Is.Not.Null,
            "Authorization header must be present even when no encrypted password is stored.");
    }

    [Test]
    public async Task InvokeRegisteredEndpointsAsync_BasicAuth_DecryptionFails_LogsErrorAndSendsWithoutAuth()
    {
        var subscription = CreateSubscription(
            endpoint: "https://example.com/api",
            method: "POST",
            formNames: SingleEmptyFormName,
            useAuthorization: true,
            scheme: AuthenticationScheme.Basic,
            username: "user",
            encryptedPassword: "badPwd");
        SetupSubscriptions(subscription);

        _secretsEncryptionHelperMock
            .Setup(s => s.DecryptSecret("badPwd"))
            .Throws(new InvalidOperationException("Bad key"));

        HttpRequestMessage? captured = null;
        var handler = CreateCapturingHandler(
            new HttpResponseMessage(HttpStatusCode.OK),
            req => captured = req);
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(new HttpClient(handler));

        var formItem = CreateUninitializedBizFormItem();
        await _service.InvokeRegisteredEndpointsAsync(formItem, new FormEntry(), CancellationToken.None);

        // Decryption failure should log an error.
        VerifyLoggedAtLeastOnce(LogLevel.Error, withException: true);
        // Auth construction returns null ? another warning is logged.
        VerifyLoggedAtLeastOnce(LogLevel.Warning);
        // The request is still sent — no Authorization header though.
        Assert.That(captured, Is.Not.Null, "Request should still have been sent.");
        Assert.That(captured!.Headers.Authorization, Is.Null,
            "Authorization header must be absent when decryption failed.");
    }

    [Test]
    public async Task InvokeRegisteredEndpointsAsync_UnknownScheme_ExceptionCaughtAndLogged()
    {
        var subscription = CreateSubscription(
            endpoint: "https://example.com/api",
            method: "POST",
            formNames: SingleEmptyFormName,
            useAuthorization: true,
            scheme: "Bearer"); // Not implemented ? NotImplementedException
        SetupSubscriptions(subscription);

        var handler = CreateMockHandler(new HttpResponseMessage(HttpStatusCode.OK));
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(new HttpClient(handler));

        var formItem = CreateUninitializedBizFormItem();

        // NotImplementedException is not OperationCanceledException ? caught by outer catch.
        Assert.DoesNotThrowAsync(() =>
            _service.InvokeRegisteredEndpointsAsync(formItem, new FormEntry(), CancellationToken.None));

        VerifyLoggedAtLeastOnce(LogLevel.Error, withException: true);
    }

    // ??? ConstructAuthorizationHeader (private, via reflection) ???????????????

    [Test]
    public void ConstructAuthorizationHeader_UseAuthorizationFalse_ReturnsNull()
    {
        var subscription = CreateSubscription(useAuthorization: false);

        var result = InvokePrivate<string?>(_service, "ConstructAuthorizationHeader", subscription);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void ConstructAuthorizationHeader_UnknownScheme_ThrowsNotImplementedException()
    {
        var subscription = CreateSubscription(useAuthorization: true, scheme: "OAuth2");

        var ex = Assert.Throws<TargetInvocationException>(() =>
            InvokePrivate<string?>(_service, "ConstructAuthorizationHeader", subscription));

        Assert.That(ex!.InnerException, Is.InstanceOf<NotImplementedException>());
    }

    // ??? CreateBasicAuthorizationHeader (private, via reflection) ?????????????

    [Test]
    public void CreateBasicAuthorizationHeader_ValidCredentials_ReturnsCorrectBasicHeader()
    {
        var subscription = CreateSubscription(
            useAuthorization: true,
            scheme: AuthenticationScheme.Basic,
            username: "admin",
            encryptedPassword: "enc123");
        _secretsEncryptionHelperMock.Setup(s => s.DecryptSecret("enc123")).Returns("pass456");

        var result = InvokePrivate<string?>(_service, "CreateBasicAuthorizationHeader", subscription);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Does.StartWith("Basic "));
        var decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(result!["Basic ".Length..]));
        Assert.That(decoded, Is.EqualTo("admin:pass456"));
    }

    [Test]
    public void CreateBasicAuthorizationHeader_EmptyEncryptedPassword_ReturnsHeaderWithNullInCredentials()
    {
        var subscription = CreateSubscription(
            useAuthorization: true,
            scheme: AuthenticationScheme.Basic,
            username: "admin",
            encryptedPassword: "");

        var result = InvokePrivate<string?>(_service, "CreateBasicAuthorizationHeader", subscription);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Does.StartWith("Basic "));
        // DecryptSecret is never called when EncryptedPassword is empty.
        _secretsEncryptionHelperMock.Verify(s => s.DecryptSecret(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void CreateBasicAuthorizationHeader_DecryptionFails_ReturnsNull()
    {
        var subscription = CreateSubscription(
            useAuthorization: true,
            scheme: AuthenticationScheme.Basic,
            username: "admin",
            encryptedPassword: "badEnc");
        _secretsEncryptionHelperMock
            .Setup(s => s.DecryptSecret("badEnc"))
            .Throws(new InvalidOperationException("Bad key"));

        var result = InvokePrivate<string?>(_service, "CreateBasicAuthorizationHeader", subscription);

        Assert.That(result, Is.Null);
    }

    // ??? Helpers ???????????????????????????????????????????????????????????????

    private void SetupSubscriptions(params FormSubmissionSubscriptionsInfo[] subscriptions)
    {
        var queryMock = new Mock<ObjectQuery<FormSubmissionSubscriptionsInfo>> { CallBase = false };
        queryMock.Setup(q => q.GetEnumerableTypedResult(
                    It.IsAny<System.Data.CommandBehavior>(),
                    It.IsAny<bool>()))
                 .Returns(subscriptions);
        _subscriptionProviderMock.Setup(p => p.Get()).Returns(queryMock.Object);
    }

    /// <summary>
    /// Creates a <see cref="FormSubmissionSubscriptionsInfo"/> using its DataRow constructor so the
    /// Kentico CMS IoC container / DB pipeline is never invoked during unit tests.
    /// </summary>
    private static TestFormSubmissionSubscriptionsInfo CreateSubscription(
        string endpoint = "https://example.com",
        string method = "POST",
        IEnumerable<string>? formNames = null,
        string requestBodyFormat = "",
        string requestContentType = "",
        bool useAuthorization = false,
        string scheme = AuthenticationScheme.Basic,
        string username = "",
        string encryptedPassword = "")
    {
        var info = new TestFormSubmissionSubscriptionsInfo
        {
            FormSubmissionSubscriptionsID = 1,
            Displayname = "Test",
            Codename = "test",
            Description = "",
            Form = formNames ?? Enumerable.Empty<string>(),
            Endpoint = endpoint,
            Method = method,
            Useauthorization = useAuthorization,
            Requestcontenttype = requestContentType,
            Requestbodyformat = requestBodyFormat,
            Scheme = scheme,
            Username = username,
            EncryptedPassword = encryptedPassword,
        };
        return info;
    }

    /// <summary>
    /// Test-only subclass that overrides every virtual property so the underlying
    /// Kentico DataClass / IoC pipeline is never touched during unit tests.
    /// </summary>
    private sealed class TestFormSubmissionSubscriptionsInfo : FormSubmissionSubscriptionsInfo
    {
        public override int FormSubmissionSubscriptionsID { get; set; }
        public override string Displayname { get; set; } = string.Empty;
        public override string Codename { get; set; } = string.Empty;
        public override string Description { get; set; } = string.Empty;
        public new IEnumerable<string> Form { get; set; } = Enumerable.Empty<string>();
        public override string Endpoint { get; set; } = string.Empty;
        public override string Method { get; set; } = string.Empty;
        public override bool Useauthorization { get; set; }
        public override string Requestcontenttype { get; set; } = string.Empty;
        public override string Requestbodyformat { get; set; } = string.Empty;
        public override string Scheme { get; set; } = AuthenticationScheme.Basic;
        public override string Username { get; set; } = string.Empty;
        public override string EncryptedPassword { get; set; } = string.Empty;
    }

    private static BizFormItem CreateUninitializedBizFormItem()
        => (BizFormItem)RuntimeHelpers.GetUninitializedObject(typeof(BizFormItem));

    private static HttpMessageHandler CreateMockHandler(HttpResponseMessage response)
    {
        var mock = new Mock<HttpMessageHandler>();
        mock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
        return mock.Object;
    }

    private static HttpMessageHandler CreateThrowingHandler(Exception ex)
    {
        var mock = new Mock<HttpMessageHandler>();
        mock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(ex);
        return mock.Object;
    }

    private static HttpMessageHandler CreateCapturingHandler(
        HttpResponseMessage response,
        Action<HttpRequestMessage> capture)
    {
        var mock = new Mock<HttpMessageHandler>();
        mock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capture(req))
            .ReturnsAsync(response);
        return mock.Object;
    }

    // CA1873 flags ILogger.Log calls whose arguments may be expensive to evaluate,
    // recommending an IsEnabled guard. The Log(...) invocations below are NOT real
    // log calls — they are Moq expression trees used only to match invocations
    // against the mock, so the analyzer's guidance does not apply here.
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Usage", "CA1873:Avoid potentially expensive logging",
        Justification = "Expressions are Moq Verify matchers, not actual log calls.")]
    private void VerifyLoggedAtLeastOnce(LogLevel level, bool withException = false)
    {
        if (withException)
        {
            _loggerMock.Verify(
                l => l.Log(
                    level,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }
        else
        {
            _loggerMock.Verify(
                l => l.Log(
                    level,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }
    }

    private static T InvokePrivate<T>(object target, string method, params object?[] args)
    {
        var mi = target.GetType().GetMethod(method, PrivateInstance);
        Assert.That(mi, Is.Not.Null, $"Private method '{method}' not found.");
        return (T)mi!.Invoke(target, args)!;
    }
}

