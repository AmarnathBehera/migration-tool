using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Queries;
using ChannelSettingInput = BDO.GWT.Web.Core.API.Data.ChannelSettingInput;
using CMS.Base;
using CMS.DataEngine;
using System.Text.Json;

namespace BDO.GWT.GLOBAL.API.Services;

public interface IChannelSettingCrudService
{
    Task<ChannelSettingOutput[]> GetChannelSettings(int webChannelSettingsId, SettingsQuery settingsQuery, CancellationToken cancellationToken);

    Task<ChannelSettingOutput[]?> GetChannelSetting(int webChannelSettingsId, string codeName, CancellationToken cancellationToken);

    Task<ChannelSettingOutput?> UpdateChannelSetting(int webChannelSettingsId, string codeName, ChannelSettingInput request, CancellationToken cancellationToken);
}

public class ChannelSettingCrudService : IChannelSettingCrudService
{
    private readonly HashSet<string> _excludedColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(WebChannelSnippetInfo.WebChannelSnippetID),
        nameof(WebChannelSnippetInfo.WebChannelSnippetWebChannelSettingsID),
        nameof(WebChannelSnippetInfo.WebSettingsGUID)
    };

    private readonly string[] _inheritSuffixes =
    [
        "Inheritfromglobalsettings",
        "Inheritfromglobalsetting",
        "InheritFromGlobal",
        "Inheritfromglobal",
        "Inheritfromlocal"
    ];

    private readonly IInfoProvider<WebChannelSnippetInfo> _webChannelSnippetInfoProvider;

    public ChannelSettingCrudService(IInfoProvider<WebChannelSnippetInfo> webChannelSnippetInfoProvider)
    {
        _webChannelSnippetInfoProvider = webChannelSnippetInfoProvider;
    }

    public Task<ChannelSettingOutput[]> GetChannelSettings(int webChannelSettingsId, SettingsQuery settingsQuery, CancellationToken cancellationToken)
    {
        var settings = GetSettingsByWebChannelSettingsId(webChannelSettingsId);

        if (!string.IsNullOrWhiteSpace(settingsQuery.DisplayName))
        {
            settings = settings
                .Where(x => string.Equals(x.DisplayName, settingsQuery.DisplayName, StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }

        return Task.FromResult(settings);
    }

    public Task<ChannelSettingOutput[]?> GetChannelSetting(int webChannelSettingsId, string codeName, CancellationToken cancellationToken)
    {
        var settings = GetSettingsByWebChannelSettingsId(webChannelSettingsId);

        if (string.Equals(codeName, WebChannelSnippetInfo.OBJECT_TYPE, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<ChannelSettingOutput[]?>(settings);
        }

        var setting = settings.FirstOrDefault(x => string.Equals(x.CodeName, codeName, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult<ChannelSettingOutput[]?>(setting is null ? null : [setting]);
    }

    public async Task<ChannelSettingOutput?> UpdateChannelSetting(int webChannelSettingsId, string codeName, ChannelSettingInput request, CancellationToken cancellationToken)
    {
        var snippet = _webChannelSnippetInfoProvider
            .Get()
            .WhereEquals(nameof(WebChannelSnippetInfo.WebChannelSnippetWebChannelSettingsID), webChannelSettingsId)
            .GetEnumerableTypedResult()
            .FirstOrDefault();

        if (snippet is null)
        {
            return null;
        }

        var columnName = ResolveColumnName(codeName, snippet);
        if (columnName is null)
        {
            return null;
        }

        var normalizedValue = NormalizeInputValue(request.Value, snippet.GetValue(columnName));
        snippet.SetValue(columnName, normalizedValue);

        var inheritColumnName = ResolveInheritColumnName(columnName, snippet);
        if (inheritColumnName is not null)
        {
            snippet.SetValue(inheritColumnName, request.InheritFromGlobalSettings);
        }

        await _webChannelSnippetInfoProvider.SetAsync(snippet, cancellationToken);

        return CreateChannelSettingOutput(columnName, snippet.GetValue(columnName), request.InheritFromGlobalSettings);
    }

    private ChannelSettingOutput[] GetSettingsByWebChannelSettingsId(int webChannelSettingsId)
    {
        var snippet = _webChannelSnippetInfoProvider
            .Get()
            .WhereEquals(nameof(WebChannelSnippetInfo.WebChannelSnippetWebChannelSettingsID), webChannelSettingsId)
            .GetEnumerableTypedResult()
            .FirstOrDefault();

        return snippet is null
            ? []
            : ToChannelSettingOutputs(snippet);
    }

    private ChannelSettingOutput[] ToChannelSettingOutputs(WebChannelSnippetInfo setting)
    {
        return setting.ColumnNames
            .Where(columnName => !_excludedColumns.Contains(columnName) && !IsInheritColumn(columnName))
            .Select(columnName =>
            {
                var value = setting.GetValue(columnName);
                var inheritColumnName = ResolveInheritColumnName(columnName, setting);
                var inheritValue = inheritColumnName is not null && Convert.ToBoolean(setting.GetValue(inheritColumnName));

                return CreateChannelSettingOutput(columnName, value, inheritValue);
            })
            .ToArray();
    }

    private bool IsInheritColumn(string columnName) =>
        _inheritSuffixes.Any(suffix => columnName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));

    private string RemoveInheritSuffix(string columnName)
    {
        foreach (var suffix in _inheritSuffixes)
        {
            if (columnName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                return columnName[..^suffix.Length];
            }
        }

        return columnName;
    }

    private string? ResolveColumnName(string codeName, IDataContainer setting)
    {
        var normalizedCodeName = codeName.StartsWith("BDO_", StringComparison.OrdinalIgnoreCase)
            ? codeName[4..]
            : codeName;

        if (string.Equals(codeName, WebChannelSnippetInfo.OBJECT_TYPE, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return setting.ColumnNames.FirstOrDefault(columnName =>
            string.Equals(columnName, normalizedCodeName, StringComparison.OrdinalIgnoreCase)
            && !_excludedColumns.Contains(columnName)
            && !IsInheritColumn(columnName));
    }

    private string? ResolveInheritColumnName(string baseColumnName, IDataContainer setting)
    {
        var normalizedBaseColumnName = NormalizeColumnNameForInheritMatch(baseColumnName);

        return setting.ColumnNames.FirstOrDefault(columnName =>
        {
            if (!IsInheritColumn(columnName))
            {
                return false;
            }

            var inheritBaseName = NormalizeColumnNameForInheritMatch(RemoveInheritSuffix(columnName));

            return string.Equals(inheritBaseName, normalizedBaseColumnName, StringComparison.OrdinalIgnoreCase)
                || normalizedBaseColumnName.StartsWith(inheritBaseName, StringComparison.OrdinalIgnoreCase)
                || inheritBaseName.StartsWith(normalizedBaseColumnName, StringComparison.OrdinalIgnoreCase);
        });
    }

    private static string NormalizeColumnNameForInheritMatch(string columnName)
    {
        return columnName.EndsWith("KeyValue", StringComparison.OrdinalIgnoreCase)
            ? columnName[..^"KeyValue".Length]
            : columnName;
    }

    private static object? NormalizeInputValue(object? value, object? currentValue)
    {
        if (value is null)
        {
            return null;
        }

        if (value is JsonElement jsonElement)
        {
            return ConvertJsonElement(jsonElement, currentValue);
        }

        return value;
    }

    private static object? ConvertJsonElement(JsonElement element, object? currentValue)
    {
        if (element.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        if (currentValue is bool)
        {
            return element.ValueKind is JsonValueKind.True or JsonValueKind.False
                ? element.GetBoolean()
                : bool.TryParse(element.ToString(), out var boolValue) ? boolValue : currentValue;
        }

        if (currentValue is int)
        {
            return element.ValueKind == JsonValueKind.Number && element.TryGetInt32(out var intValue)
                ? intValue
                : int.TryParse(element.ToString(), out intValue) ? intValue : currentValue;
        }

        if (currentValue is long)
        {
            return element.ValueKind == JsonValueKind.Number && element.TryGetInt64(out var longValue)
                ? longValue
                : long.TryParse(element.ToString(), out longValue) ? longValue : currentValue;
        }

        if (currentValue is decimal)
        {
            return element.ValueKind == JsonValueKind.Number && element.TryGetDecimal(out var decimalValue)
                ? decimalValue
                : decimal.TryParse(element.ToString(), out decimalValue) ? decimalValue : currentValue;
        }

        if (currentValue is Guid)
        {
            return Guid.TryParse(element.ToString(), out var guidValue) ? guidValue : currentValue;
        }

        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.True or JsonValueKind.False => element.GetBoolean(),
            JsonValueKind.Number when element.TryGetInt64(out var integerValue) => integerValue,
            JsonValueKind.Number when element.TryGetDecimal(out var numberValue) => numberValue,
            JsonValueKind.Array or JsonValueKind.Object => element.GetRawText(),
            _ => element.ToString()
        };
    }

    private ChannelSettingOutput CreateChannelSettingOutput(string columnName, object? value, bool inheritFromGlobal)
    {
        return new ChannelSettingOutput
        {
            CodeName = $"BDO_{columnName}",
            DisplayName = columnName,
            InheritFromGlobalSettings = inheritFromGlobal,
            Type = ResolveSettingType(value),
            Value = value is DBNull ? null : value
        };
    }

    private string ResolveSettingType(object? value)
    {
        if (value is null || value is DBNull || value is string)
        {
            return "string";
        }

        return value switch
        {
            bool => "boolean",
            byte or short or int or long => "integer",
            float or double or decimal => "number",
            DateTime => "datetime",
            Guid => "guid",
            _ => value.GetType().Name
        };
    }
}
