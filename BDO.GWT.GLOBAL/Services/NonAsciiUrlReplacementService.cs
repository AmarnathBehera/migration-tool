using BDO.GWT.Web.Services;
using CMS.DataEngine;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Services;

public interface INonAsciiUrlReplacementService
{
    Task<string?> ApplyReplacementsAsync(string? input, int websiteChannelId, CancellationToken cancellationToken);
}

public interface INonAsciiUrlReplacementSettingProvider
{
    Task<string?> GetRawDictionaryAsync(int websiteChannelId, CancellationToken cancellationToken);
}

public interface INonAsciiUrlReplacementCache
{
    Task<T> LoadAsync<T>(string cacheKey, Func<Task<T>> valueFactory, CancellationToken cancellationToken);
}

public sealed record NonAsciiUrlReplacementRule(string Search, string Replacement);

public sealed record NonAsciiUrlReplacementResult(string Value, int ReplacementCount);

public sealed class NonAsciiUrlReplacementCache : INonAsciiUrlReplacementCache
{
    private static readonly string[] CacheDependencies =
    [
        "bdo.globalsettingskey|all",
        "bdo.generalsetting|all",
        "bdo.webchannelsettings|all",
        "bdo.webchannelsnippet|all"
    ];

    private readonly ICachingService _cachingService;

    public NonAsciiUrlReplacementCache(ICachingService cachingService)
    {
        _cachingService = cachingService;
    }

    public Task<T> LoadAsync<T>(string cacheKey, Func<Task<T>> valueFactory, CancellationToken cancellationToken)
    {
        return _cachingService.LoadAsync(async _ => await valueFactory().ConfigureAwait(false), cacheKey, CacheDependencies, cancellationToken);
    }
}

public sealed class NonAsciiUrlReplacementSettingProvider : INonAsciiUrlReplacementSettingProvider
{
    private readonly IInfoProvider<GlobalsettingskeyInfo> _globalSettingsProvider;
    private readonly IInfoProvider<WebChannelSettingsInfo> _webChannelSettingsProvider;
    private readonly IInfoProvider<WebChannelSnippetInfo> _webChannelSnippetProvider;
    private readonly ILogger<NonAsciiUrlReplacementSettingProvider> _logger;

    public NonAsciiUrlReplacementSettingProvider(
        IInfoProvider<GlobalsettingskeyInfo> globalSettingsProvider,
        IInfoProvider<WebChannelSettingsInfo> webChannelSettingsProvider,
        IInfoProvider<WebChannelSnippetInfo> webChannelSnippetProvider,
        ILogger<NonAsciiUrlReplacementSettingProvider> logger)
    {
        _globalSettingsProvider = globalSettingsProvider;
        _webChannelSettingsProvider = webChannelSettingsProvider;
        _webChannelSnippetProvider = webChannelSnippetProvider;
        _logger = logger;
    }

    public Task<string?> GetRawDictionaryAsync(int websiteChannelId, CancellationToken cancellationToken)
    {
        var channelSettings = _webChannelSettingsProvider
            .Get()
            .WhereEquals(nameof(WebChannelSettingsInfo.WebChannelSettingsChannelID), websiteChannelId)
            .TopN(1)
            .FirstOrDefault();

        // If channel settings don't exist, fall back to global settings
        if (channelSettings is null)
        {
            _logger.LogInformation(
                "Channel settings not found for website channel {WebsiteChannelId}; attempting to use global settings.",
                websiteChannelId);

            var globalDict = GetGlobalDictionary();
            if (string.IsNullOrWhiteSpace(globalDict))
            {
                _logger.LogWarning(
                    "Non-ASCII URL replacement setting was missing or empty for website channel {WebsiteChannelId} and no global settings were found.",
                    websiteChannelId);
                return Task.FromResult<string?>(null);
            }

            return Task.FromResult<string?>(globalDict);
        }

        var snippet = _webChannelSnippetProvider
            .Get()
            .WhereEquals(nameof(WebChannelSnippetInfo.WebChannelSnippetWebChannelSettingsID), channelSettings.WebChannelSettingsID)
            .TopN(1)
            .FirstOrDefault();

        if (snippet is null)
        {
            _logger.LogInformation(
                "Channel snippet not found for website channel {WebsiteChannelId}; attempting to use global settings.",
                websiteChannelId);

            var globalDict = GetGlobalDictionary();
            if (string.IsNullOrWhiteSpace(globalDict))
            {
                _logger.LogWarning(
                    "Non-ASCII URL replacement setting was missing or empty for website channel {WebsiteChannelId} and no global settings were found.",
                    websiteChannelId);
                return Task.FromResult<string?>(null);
            }

            return Task.FromResult<string?>(globalDict);
        }

        // Check if the snippet inherits from global or has its own setting
        string? rawDictionary = snippet.NonASCIItoASCIIcharacterdictionaryInheritfromglobal
            ? GetGlobalDictionary()
            : snippet.NonASCIItoASCIIcharacterdictionary;

        // If the channel setting is empty and inheritance is enabled, use global
        if (string.IsNullOrWhiteSpace(rawDictionary))
        {
            _logger.LogInformation(
                "Channel setting was empty for website channel {WebsiteChannelId}; checking global settings.",
                websiteChannelId);

            rawDictionary = GetGlobalDictionary();
        }

        if (string.IsNullOrWhiteSpace(rawDictionary))
        {
            _logger.LogWarning(
                "Non-ASCII URL replacement setting was missing or empty for website channel {WebsiteChannelId}.",
                websiteChannelId);

            return Task.FromResult<string?>(null);
        }

        return Task.FromResult<string?>(rawDictionary);
    }

    private string? GetGlobalDictionary()
    {
        var globalSettings = _globalSettingsProvider
            .Get()
            .TopN(1)
            .FirstOrDefault();

        return globalSettings?.NonASCIItoASCIIcharacterdictionary;
    }
}

public class NonAsciiUrlReplacementService : INonAsciiUrlReplacementService
{
    private readonly INonAsciiUrlReplacementSettingProvider _settingProvider;
    private readonly INonAsciiUrlReplacementCache _cache;
    private readonly ILogger<NonAsciiUrlReplacementService> _logger;

    public NonAsciiUrlReplacementService(
        INonAsciiUrlReplacementSettingProvider settingProvider,
        INonAsciiUrlReplacementCache cache,
        ILogger<NonAsciiUrlReplacementService> logger)
    {
        _settingProvider = settingProvider;
        _cache = cache;
        _logger = logger;
    }

    public async Task<string?> ApplyReplacementsAsync(string? input, int websiteChannelId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        var cacheKey = GetCacheKey(websiteChannelId);
        var rules = await _cache.LoadAsync(cacheKey, async () =>
        {
            var rawDictionary = await _settingProvider.GetRawDictionaryAsync(websiteChannelId, cancellationToken).ConfigureAwait(false);
            return ParseRules(rawDictionary, _logger);
        }, cancellationToken).ConfigureAwait(false);

        if (rules.Count == 0)
        {
            return input;
        }

        var outcome = ApplyRules(input, rules);

        if (outcome.ReplacementCount > 0)
        {
            _logger.LogInformation(
                "Applied non-ASCII URL replacements for website channel {WebsiteChannelId}; {ReplacementCount} replacement(s) were made.",
                websiteChannelId,
                outcome.ReplacementCount);
        }

        return outcome.Value;
    }

    internal static string GetCacheKey(int websiteChannelId)
        => $"{nameof(NonAsciiUrlReplacementService)}|channel|{websiteChannelId}";

    public static IReadOnlyList<NonAsciiUrlReplacementRule> ParseRules(string? rawDictionary, ILogger? logger = null)
    {
        if (string.IsNullOrWhiteSpace(rawDictionary))
        {
            return Array.Empty<NonAsciiUrlReplacementRule>();
        }

        var replacements = new Dictionary<string, string>(StringComparer.Ordinal);
        var lines = rawDictionary.Split(['\r', '\n', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        for (var index = 0; index < lines.Length; index++)
        {
            var line = lines[index].Trim();

            if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal) || line.StartsWith("//", StringComparison.Ordinal))
            {
                continue;
            }

            var equalsIndex = line.IndexOf('=');
            if (equalsIndex <= 0 || equalsIndex == line.Length - 1)
            {
                logger?.LogWarning(
                    "Ignored invalid non-ASCII URL replacement entry at line {LineNumber}.",
                    index + 1);
                continue;
            }

            var key = line[..equalsIndex].Trim();
            var value = line[(equalsIndex + 1)..].Trim();

            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
            {
                logger?.LogWarning(
                    "Ignored invalid non-ASCII URL replacement entry at line {LineNumber}.",
                    index + 1);
                continue;
            }

            if (replacements.ContainsKey(key))
            {
                logger?.LogWarning(
                    "Duplicate non-ASCII URL replacement key {ReplacementKey} was overwritten by a later entry.",
                    key);
            }

            replacements[key] = value;
        }

        if (replacements.Count == 0)
        {
            logger?.LogWarning("No valid non-ASCII URL replacement entries were found in the configured dictionary.");
            return Array.Empty<NonAsciiUrlReplacementRule>();
        }

        return replacements
            .OrderByDescending(pair => pair.Key.Length)
            .ThenBy(pair => pair.Key, StringComparer.Ordinal)
            .Select(pair => new NonAsciiUrlReplacementRule(pair.Key, pair.Value))
            .ToArray();
    }

    public static NonAsciiUrlReplacementResult ApplyRules(string input, IReadOnlyList<NonAsciiUrlReplacementRule> rules)
    {
        if (string.IsNullOrEmpty(input) || rules.Count == 0)
        {
            return new NonAsciiUrlReplacementResult(input, 0);
        }

        var current = input;
        var replacementCount = 0;

        foreach (var rule in rules)
        {
            var replaced = ReplaceAll(current, rule.Search, rule.Replacement, out var ruleReplacementCount);
            if (ruleReplacementCount > 0)
            {
                current = replaced;
                replacementCount += ruleReplacementCount;
            }
        }

        return new NonAsciiUrlReplacementResult(current, replacementCount);
    }

    private static string ReplaceAll(string input, string search, string replacement, out int replacementCount)
    {
        replacementCount = 0;

        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(search))
        {
            return input;
        }

        var firstMatch = input.IndexOf(search, StringComparison.Ordinal);
        if (firstMatch < 0)
        {
            return input;
        }

        var builder = new StringBuilder(input.Length);
        var currentIndex = 0;

        while (firstMatch >= 0)
        {
            builder.Append(input, currentIndex, firstMatch - currentIndex);
            builder.Append(replacement);
            currentIndex = firstMatch + search.Length;
            replacementCount++;
            firstMatch = input.IndexOf(search, currentIndex, StringComparison.Ordinal);
        }

        builder.Append(input, currentIndex, input.Length - currentIndex);
        return builder.ToString();
    }
}
