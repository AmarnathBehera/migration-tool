#nullable enable
using CMS.Scheduler;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

[assembly: RegisterScheduledTask(identifier: "BDO.SynchronizeGlobalDealsScheduler", typeof(BDO.GWT.GLOBAL.Scheduler.SynchronizeGlobalDealsScheduler))]

namespace BDO.GWT.GLOBAL.Scheduler;

public class SynchronizeGlobalDealsScheduler : IScheduledTask
{
    // Matches the keys actually present in appsettings.json.
    private const string SYNC_API_URL_SETTING = "BDOSyncApiUrl";
    private const string SYNC_API_KEY_SETTING = "BDOSyncApiKey";
    private const string API_KEY_HEADER = "X-API-Key";

    private static readonly JsonSerializerOptions TaskOptionsJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SynchronizeGlobalDealsScheduler> _logger;

    public SynchronizeGlobalDealsScheduler(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<SynchronizeGlobalDealsScheduler> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ScheduledTaskExecutionResult> Execute(ScheduledTaskConfigurationInfo task, CancellationToken cancellationToken)
    {
        var syncApiUrl = _configuration[SYNC_API_URL_SETTING];
        var syncApiKey = _configuration[SYNC_API_KEY_SETTING];

        if (string.IsNullOrEmpty(syncApiUrl))
        {
            _logger.LogError("Missing required app setting '{Setting}'", SYNC_API_URL_SETTING);
            return new ScheduledTaskExecutionResult($"Missing required app setting '{SYNC_API_URL_SETTING}'.");
        }

        if (string.IsNullOrEmpty(syncApiKey))
        {
            _logger.LogError("Missing required app setting '{Setting}'", SYNC_API_KEY_SETTING);
            return new ScheduledTaskExecutionResult($"Missing required app setting '{SYNC_API_KEY_SETTING}'.");
        }

        var forceUpdate = ParseForceUpdate(task.ScheduledTaskConfigurationData);

        var httpClient = _httpClientFactory.CreateClient();
        httpClient.Timeout = TimeSpan.FromMinutes(10);
        httpClient.DefaultRequestHeaders.Add(API_KEY_HEADER, syncApiKey);

        var requestUrl = $"{syncApiUrl.TrimEnd('/')}/GlobalSite/SynchronizeGlobalDeals?forceUpdate={forceUpdate}";
        var maskedApiKey = MaskSecret(syncApiKey);
        var machineName = Environment.MachineName;
        var startedUtc = DateTime.UtcNow;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var response = await httpClient.PostAsync(requestUrl, null, cancellationToken);
            stopwatch.Stop();

            if (response.IsSuccessStatusCode)
            {
                return new ScheduledTaskExecutionResult($"Successfully created remote deal pages ({(int)response.StatusCode}) in {stopwatch.ElapsedMilliseconds} ms.");
            }

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            var traceId = TryExtractTraceId(responseBody);
            var responseHeaders = FormatHeaders(response.Headers);
            var contentHeaders = FormatHeaders(response.Content.Headers);
            var wwwAuthenticate = response.Headers.WwwAuthenticate.Count > 0
                ? string.Join("; ", response.Headers.WwwAuthenticate)
                : "(none)";

            var details =
                $"SynchronizeGlobalDeals API call failed.{Environment.NewLine}" +
                $"  Status       : {(int)response.StatusCode} {response.StatusCode} ({response.ReasonPhrase}){Environment.NewLine}" +
                $"  Request URL  : POST {requestUrl}{Environment.NewLine}" +
                $"  ForceUpdate  : {forceUpdate}{Environment.NewLine}" +
                $"  API Key Hdr  : {API_KEY_HEADER}={maskedApiKey} (length={syncApiKey.Length}){Environment.NewLine}" +
                $"  Elapsed      : {stopwatch.ElapsedMilliseconds} ms{Environment.NewLine}" +
                $"  Started (UTC): {startedUtc:O}{Environment.NewLine}" +
                $"  Machine      : {machineName}{Environment.NewLine}" +
                $"  TraceId      : {traceId ?? "(not present)"}{Environment.NewLine}" +
                $"  WWW-Auth     : {wwwAuthenticate}{Environment.NewLine}" +
                $"  Resp Headers : {responseHeaders}{Environment.NewLine}" +
                $"  Cont Headers : {contentHeaders}{Environment.NewLine}" +
                $"  Response Body: {responseBody}";

            _logger.LogError(
                "SynchronizeGlobalDeals API call failed. Status={StatusCode} ({StatusName}) Reason={Reason}, Url={Url}, ForceUpdate={ForceUpdate}, ApiKeyHeader={ApiKeyHeader}={ApiKeyMasked}, ElapsedMs={ElapsedMs}, StartedUtc={StartedUtc:O}, Machine={Machine}, TraceId={TraceId}, WwwAuthenticate={WwwAuthenticate}, ResponseHeaders={ResponseHeaders}, ContentHeaders={ContentHeaders}, Body={Body}",
                (int)response.StatusCode, response.StatusCode, response.ReasonPhrase, requestUrl, forceUpdate,
                API_KEY_HEADER, maskedApiKey, stopwatch.ElapsedMilliseconds, startedUtc, machineName,
                traceId, wwwAuthenticate, responseHeaders, contentHeaders, responseBody);

            return new ScheduledTaskExecutionResult(details);
        }
        catch (OperationCanceledException ex) when (cancellationToken.IsCancellationRequested)
        {
            stopwatch.Stop();
            _logger.LogWarning(ex,
                "SynchronizeGlobalDeals API call was canceled. Url={Url}, ForceUpdate={ForceUpdate}, ElapsedMs={ElapsedMs}, Machine={Machine}",
                requestUrl, forceUpdate, stopwatch.ElapsedMilliseconds, machineName);
            return new ScheduledTaskExecutionResult($"SynchronizeGlobalDeals API call was canceled after {stopwatch.ElapsedMilliseconds} ms. Url={requestUrl}");
        }
        catch (TaskCanceledException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "SynchronizeGlobalDeals API call timed out. Url={Url}, ForceUpdate={ForceUpdate}, Timeout={Timeout}, ElapsedMs={ElapsedMs}, Machine={Machine}",
                requestUrl, forceUpdate, httpClient.Timeout, stopwatch.ElapsedMilliseconds, machineName);
            return new ScheduledTaskExecutionResult($"SynchronizeGlobalDeals API call timed out after {stopwatch.ElapsedMilliseconds} ms (Timeout={httpClient.Timeout}). Url={requestUrl}. Error: {ex.Message}");
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "SynchronizeGlobalDeals API HTTP request failed. Url={Url}, ForceUpdate={ForceUpdate}, StatusCode={StatusCode}, ElapsedMs={ElapsedMs}, Machine={Machine}",
                requestUrl, forceUpdate, ex.StatusCode, stopwatch.ElapsedMilliseconds, machineName);
            return new ScheduledTaskExecutionResult($"SynchronizeGlobalDeals API HTTP request failed. Url={requestUrl}, StatusCode={ex.StatusCode}, ElapsedMs={stopwatch.ElapsedMilliseconds}. Error: {ex.Message}{(ex.InnerException != null ? $" | Inner: {ex.InnerException.Message}" : string.Empty)}");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "Failed to call SynchronizeGlobalDeals API. Url={Url}, ForceUpdate={ForceUpdate}, ElapsedMs={ElapsedMs}, Machine={Machine}",
                requestUrl, forceUpdate, stopwatch.ElapsedMilliseconds, machineName);
            return new ScheduledTaskExecutionResult($"Failed to call SynchronizeGlobalDeals API. Url={requestUrl}, ElapsedMs={stopwatch.ElapsedMilliseconds}. {ex.GetType().Name}: {ex.Message}{(ex.InnerException != null ? $" | Inner ({ex.InnerException.GetType().Name}): {ex.InnerException.Message}" : string.Empty)}");
        }
    }

    private static string MaskSecret(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "(empty)";
        }

        if (value.Length <= 4)
        {
            return new string('*', value.Length);
        }

        return $"{value[..2]}***{value[^2..]}";
    }

    private static string FormatHeaders(System.Net.Http.Headers.HttpHeaders headers)
    {
        var parts = new System.Collections.Generic.List<string>();
        foreach (var header in headers)
        {
            var name = header.Key;
            // Avoid dumping sensitive headers.
            var value = name.Equals("Authorization", StringComparison.OrdinalIgnoreCase) ||
                        name.Equals(API_KEY_HEADER, StringComparison.OrdinalIgnoreCase) ||
                        name.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase)
                ? "(hidden)"
                : string.Join(",", header.Value);
            parts.Add($"{name}={value}");
        }
        return parts.Count == 0 ? "(none)" : string.Join("; ", parts);
    }

    private static string? TryExtractTraceId(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.ValueKind == JsonValueKind.Object &&
                doc.RootElement.TryGetProperty("traceId", out var traceId) &&
                traceId.ValueKind == JsonValueKind.String)
            {
                return traceId.GetString();
            }
        }
        catch (JsonException)
        {
            // Body is not JSON; ignore.
        }

        return null;
    }

    private static bool ParseForceUpdate(string? taskData)
    {
        if (string.IsNullOrWhiteSpace(taskData))
        {
            return false;
        }

        try
        {
            var options = JsonSerializer.Deserialize<TaskOptions>(taskData, TaskOptionsJsonOptions);

            return options?.ForceUpdate ?? false;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private sealed record TaskOptions(bool ForceUpdate);
}