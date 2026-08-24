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
    private const string SYNC_API_URL_SETTING = "BDO.SyncApiUrl";
    private const string SYNC_API_KEY_SETTING = "BDO.SyncApiKey";
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

        try
        {
            var response = await httpClient.PostAsync(requestUrl, null, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return new ScheduledTaskExecutionResult($"Successfully created remote deal pages ({(int)response.StatusCode})");
            }

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogError(
                "SynchronizeGlobalDeals API call failed with status {StatusCode}: {Body}",
                response.StatusCode, responseBody);

            return new ScheduledTaskExecutionResult($"SynchronizeGlobalDeals API call failed ({(int)response.StatusCode}): {responseBody}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to call SynchronizeGlobalDeals API");
            return new ScheduledTaskExecutionResult($"Failed to call SynchronizeGlobalDeals API: {ex.Message}");
        }
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