using BDO.GWT.GLOBAL.Scheduler;
using CMS.Scheduler;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Tests.Scheduler;

[TestFixture]
public class SynchronizeGlobalDealsSchedulerTests
{
    private Mock<IHttpClientFactory> _httpClientFactoryMock = null!;
    private Mock<IConfiguration> _configurationMock = null!;
    private Mock<ILogger<SynchronizeGlobalDealsScheduler>> _loggerMock = null!;
    private Mock<HttpMessageHandler> _httpMessageHandlerMock = null!;
    private SynchronizeGlobalDealsScheduler _scheduler = null!;

    private const string ValidSyncApiUrl = "https://sync.example.com/";
    private const string ValidSyncApiKey = "test-api-key";

    [SetUp]
    public void Setup()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<SynchronizeGlobalDealsScheduler>>();
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _httpClientFactoryMock
            .Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        _scheduler = new SynchronizeGlobalDealsScheduler(
            _httpClientFactoryMock.Object,
            _configurationMock.Object,
            _loggerMock.Object);
    }

    private void SetupConfig(string? url, string? apiKey)
    {
        _configurationMock.Setup(c => c["BDOSyncApiUrl"]).Returns(url);
        _configurationMock.Setup(c => c["BDOSyncApiKey"]).Returns(apiKey);
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, string content = "")
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content)
            });
    }

    private static string GetResultMessage(ScheduledTaskExecutionResult result)
        => (string?)typeof(ScheduledTaskExecutionResult)
            .GetProperty("Result", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.GetValue(result) ?? string.Empty;

    private static TestScheduledTaskConfigurationInfo CreateTaskInfo(string? data = null)
        => new TestScheduledTaskConfigurationInfo { Data = data };

    private sealed class TestScheduledTaskConfigurationInfo : ScheduledTaskConfigurationInfo
    {
        public string? Data { get; set; }

        public override string ScheduledTaskConfigurationData
        {
            get => Data ?? string.Empty;
            set => Data = value;
        }
    }

    #region Missing configuration

    [Test]
    public async Task Execute_ReturnsMissingUrlError_WhenSyncApiUrlIsNull()
    {
        SetupConfig(url: null, apiKey: ValidSyncApiKey);
        var task = CreateTaskInfo();

        var result = await _scheduler.Execute(task, CancellationToken.None);

        Assert.That(GetResultMessage(result), Does.Contain("BDOSyncApiUrl"));

    }

    [Test]
    public async Task Execute_ReturnsMissingUrlError_WhenSyncApiUrlIsEmpty()
    {
        SetupConfig(url: string.Empty, apiKey: ValidSyncApiKey);
        var task = CreateTaskInfo();

        var result = await _scheduler.Execute(task, CancellationToken.None);

        Assert.That(GetResultMessage(result), Does.Contain("BDOSyncApiUrl"));
    }

    [Test]
    public async Task Execute_DoesNotCallHttp_WhenSyncApiUrlIsMissing()
    {
        SetupConfig(url: null, apiKey: ValidSyncApiKey);
        var task = CreateTaskInfo();

        await _scheduler.Execute(task, CancellationToken.None);

        _httpMessageHandlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Never(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
    }

    [Test]
    public async Task Execute_ReturnsMissingKeyError_WhenSyncApiKeyIsNull()
    {
        SetupConfig(url: ValidSyncApiUrl, apiKey: null);
        var task = CreateTaskInfo();

        var result = await _scheduler.Execute(task, CancellationToken.None);

        Assert.That(GetResultMessage(result), Does.Contain("BDOSyncApiKey"));
    }

    [Test]
    public async Task Execute_ReturnsMissingKeyError_WhenSyncApiKeyIsEmpty()
    {
        SetupConfig(url: ValidSyncApiUrl, apiKey: string.Empty);
        var task = CreateTaskInfo();

        var result = await _scheduler.Execute(task, CancellationToken.None);

        Assert.That(GetResultMessage(result), Does.Contain("BDOSyncApiKey"));
    }

    [Test]
    public async Task Execute_DoesNotCallHttp_WhenSyncApiKeyIsMissing()
    {
        SetupConfig(url: ValidSyncApiUrl, apiKey: null);
        var task = CreateTaskInfo();

        await _scheduler.Execute(task, CancellationToken.None);

        _httpMessageHandlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Never(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
    }

    #endregion

    #region Successful API call

    [Test]
    public async Task Execute_ReturnsSuccessMessage_WhenApiReturns200()
    {
        SetupConfig(url: ValidSyncApiUrl, apiKey: ValidSyncApiKey);
        SetupHttpResponse(HttpStatusCode.OK);
        var task = CreateTaskInfo();

        var result = await _scheduler.Execute(task, CancellationToken.None);

        Assert.That(GetResultMessage(result), Does.Contain("Successfully created remote deal pages"));
        Assert.That(GetResultMessage(result), Does.Contain("200"));
    }

    [Test]
    public async Task Execute_AddsApiKeyHeader_WhenMakingRequest()
    {
        SetupConfig(url: ValidSyncApiUrl, apiKey: ValidSyncApiKey);
        SetupHttpResponse(HttpStatusCode.OK);
        var task = CreateTaskInfo();

        await _scheduler.Execute(task, CancellationToken.None);

        _httpMessageHandlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Headers.Contains("X-API-Key") &&
                    req.Headers.GetValues("X-API-Key").Contains(ValidSyncApiKey)),
                ItExpr.IsAny<CancellationToken>());
    }

    [Test]
    public async Task Execute_CallsCorrectEndpoint()
    {
        SetupConfig(url: "https://sync.example.com/", apiKey: ValidSyncApiKey);
        SetupHttpResponse(HttpStatusCode.OK);
        var task = CreateTaskInfo();

        await _scheduler.Execute(task, CancellationToken.None);

        _httpMessageHandlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.AbsolutePath.Contains("GlobalSite/SynchronizeGlobalDeals")),
                ItExpr.IsAny<CancellationToken>());
    }

    #endregion

    #region Failed API call

    [Test]
    public async Task Execute_ReturnsFailureMessage_WhenApiReturns500()
    {
        SetupConfig(url: ValidSyncApiUrl, apiKey: ValidSyncApiKey);
        SetupHttpResponse(HttpStatusCode.InternalServerError, "Internal error occurred");
        var task = CreateTaskInfo();

        var result = await _scheduler.Execute(task, CancellationToken.None);

        Assert.That(GetResultMessage(result), Does.Contain("SynchronizeGlobalDeals API call failed"));
        Assert.That(GetResultMessage(result), Does.Contain("500"));
        Assert.That(GetResultMessage(result), Does.Contain("Internal error occurred"));
    }

    [Test]
    public async Task Execute_ReturnsFailureMessage_WhenApiReturns404()
    {
        SetupConfig(url: ValidSyncApiUrl, apiKey: ValidSyncApiKey);
        SetupHttpResponse(HttpStatusCode.NotFound, "Not found");
        var task = CreateTaskInfo();

        var result = await _scheduler.Execute(task, CancellationToken.None);

        Assert.That(GetResultMessage(result), Does.Contain("SynchronizeGlobalDeals API call failed"));
        Assert.That(GetResultMessage(result), Does.Contain("404"));
    }

    [Test]
    public async Task Execute_ReturnsExceptionMessage_WhenHttpClientThrows()
    {
        SetupConfig(url: ValidSyncApiUrl, apiKey: ValidSyncApiKey);
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));
        var task = CreateTaskInfo();

        var result = await _scheduler.Execute(task, CancellationToken.None);

        Assert.That(GetResultMessage(result), Does.Contain("SynchronizeGlobalDeals API HTTP request failed"));
        Assert.That(GetResultMessage(result), Does.Contain("Connection refused"));
    }

    #endregion

    #region ForceUpdate parsing

    [Test]
    public async Task Execute_SendsForceUpdateFalse_WhenTaskDataIsNull()
    {
        SetupConfig(url: ValidSyncApiUrl, apiKey: ValidSyncApiKey);
        SetupHttpResponse(HttpStatusCode.OK);
        var task = CreateTaskInfo(data: null);

        await _scheduler.Execute(task, CancellationToken.None);

        _httpMessageHandlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri!.Query.Contains("forceUpdate=False")),
                ItExpr.IsAny<CancellationToken>());
    }

    [Test]
    public async Task Execute_SendsForceUpdateFalse_WhenTaskDataIsWhitespace()
    {
        SetupConfig(url: ValidSyncApiUrl, apiKey: ValidSyncApiKey);
        SetupHttpResponse(HttpStatusCode.OK);
        var task = CreateTaskInfo(data: "   ");

        await _scheduler.Execute(task, CancellationToken.None);

        _httpMessageHandlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri!.Query.Contains("forceUpdate=False")),
                ItExpr.IsAny<CancellationToken>());
    }

    [Test]
    public async Task Execute_SendsForceUpdateTrue_WhenTaskDataHasForceUpdateTrue()
    {
        SetupConfig(url: ValidSyncApiUrl, apiKey: ValidSyncApiKey);
        SetupHttpResponse(HttpStatusCode.OK);
        var task = CreateTaskInfo(data: """{"ForceUpdate": true}""");

        await _scheduler.Execute(task, CancellationToken.None);

        _httpMessageHandlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri!.Query.Contains("forceUpdate=True")),
                ItExpr.IsAny<CancellationToken>());
    }

    [Test]
    public async Task Execute_SendsForceUpdateFalse_WhenTaskDataHasForceUpdateFalse()
    {
        SetupConfig(url: ValidSyncApiUrl, apiKey: ValidSyncApiKey);
        SetupHttpResponse(HttpStatusCode.OK);
        var task = CreateTaskInfo(data: """{"ForceUpdate": false}""");

        await _scheduler.Execute(task, CancellationToken.None);

        _httpMessageHandlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri!.Query.Contains("forceUpdate=False")),
                ItExpr.IsAny<CancellationToken>());
    }

    [Test]
    public async Task Execute_SendsForceUpdateFalse_WhenTaskDataIsInvalidJson()
    {
        SetupConfig(url: ValidSyncApiUrl, apiKey: ValidSyncApiKey);
        SetupHttpResponse(HttpStatusCode.OK);
        var task = CreateTaskInfo(data: "not-valid-json");

        await _scheduler.Execute(task, CancellationToken.None);

        _httpMessageHandlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri!.Query.Contains("forceUpdate=False")),
                ItExpr.IsAny<CancellationToken>());
    }

    [Test]
    public async Task Execute_SendsForceUpdateFalse_WhenTaskDataJsonHasNoForceUpdateField()
    {
        SetupConfig(url: ValidSyncApiUrl, apiKey: ValidSyncApiKey);
        SetupHttpResponse(HttpStatusCode.OK);
        var task = CreateTaskInfo(data: """{"OtherField": "value"}""");

        await _scheduler.Execute(task, CancellationToken.None);

        _httpMessageHandlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri!.Query.Contains("forceUpdate=False")),
                ItExpr.IsAny<CancellationToken>());
    }

    #endregion
}
