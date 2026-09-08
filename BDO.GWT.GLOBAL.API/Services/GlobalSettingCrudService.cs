using BDO.GWT.Common.Core.Exceptions;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Helpers;
using BDO.GWT.GLOBAL.API.Queries;
using BDO.GWT.GLOBAL.API.Services;
using CMS.Base;
using CMS.DataEngine;
using CMS.FormEngine;

namespace BDO.GWT.GLOBAL.API.Services;

public interface IGlobalSettingCrudService
{
    Task<GlobalSettingOutput[]> GetGlobalSettings(SettingsQuery settingsQuery, CancellationToken cancellationToken);

    Task<GlobalSettingOutput> GetGlobalSetting(string settingCodeName, CancellationToken cancellationToken);

    Task<GlobalSettingOutput> SetGlobalSetting(string settingCodeName, GlobalSettingInput input, CancellationToken cancellationToken);
}

public class GlobalSettingCrudService : IGlobalSettingCrudService
{
    private readonly IInfoProvider<GlobalsettingskeyInfo> _customScriptinfoProvider;
    private readonly ISettingsCrudHelper _settingsCrudHelper;
    private readonly IValidationState _validationState;

    public GlobalSettingCrudService(
        IInfoProvider<GlobalsettingskeyInfo> customScriptinfoProvider,
        ISettingsCrudHelper settingsCrudHelper,
        IValidationState validationState)
    {
        _customScriptinfoProvider = customScriptinfoProvider;
        _settingsCrudHelper = settingsCrudHelper;
        _validationState = validationState;
    }

    public Task<GlobalSettingOutput[]> GetGlobalSettings(SettingsQuery settingsQuery, CancellationToken cancellationToken)
    {
        var setting = _customScriptinfoProvider.Get().GetEnumerableTypedResult().FirstOrDefault();

        if (setting is null)
        {
            return Task.FromResult(Array.Empty<GlobalSettingOutput>());
        }

        var outputs = ToGlobalSettingOutputs(setting);

        if (settingsQuery.DisplayName is not null)
        {
            outputs = outputs
                .Where(o => string.Equals(o.DisplayName, settingsQuery.DisplayName, StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }

        return Task.FromResult(outputs);
    }

    public Task<GlobalSettingOutput> GetGlobalSetting(string settingCodeName, CancellationToken cancellationToken)
    {
        var (setting, columnName) = GetValidatedSettingAndColumn(settingCodeName);

        var output = ToGlobalSettingOutputs(setting)
            .First(o => string.Equals(o.CodeName, columnName, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(output);
    }

    public async Task<GlobalSettingOutput> SetGlobalSetting(string settingCodeName, GlobalSettingInput input, CancellationToken cancellationToken)
    {
        var (setting, columnName) = GetValidatedSettingAndColumn(settingCodeName);

        var formInfo = FormHelper.GetFormInfo(GlobalsettingskeyInfo.OBJECT_TYPE, true);
        var field = formInfo.GetFormField(columnName);

        _settingsCrudHelper.ValidateAndNormalizeSettingInput(field, columnName, input);

        setting.SetValue(columnName, input.Value);

        await _customScriptinfoProvider.SetAsync(setting, cancellationToken);

        var output = ToGlobalSettingOutputs(setting)
            .First(o => string.Equals(o.CodeName, columnName, StringComparison.OrdinalIgnoreCase));

        return output;
    }

    private (GlobalsettingskeyInfo Setting, string ColumnName) GetValidatedSettingAndColumn(string settingCodeName)
    {
        var setting = _customScriptinfoProvider.Get().GetEnumerableTypedResult().FirstOrDefault();

        var columnName = setting?.ColumnNames
            .FirstOrDefault(c => string.Equals(c, settingCodeName, StringComparison.OrdinalIgnoreCase));

        if (setting is null || columnName is null)
        {
            _validationState.AddError("codeName", $"Cannot find setting with code name '{settingCodeName}'.");

            throw new NotFoundException();
        }

        return (setting, columnName);
    }

    private static GlobalSettingOutput[] ToGlobalSettingOutputs(IDataContainer container)
    {
        var formInfo = FormHelper.GetFormInfo(GlobalsettingskeyInfo.OBJECT_TYPE, true);

        return container.ColumnNames
            .Select(columnName =>
            {
                var field = formInfo.GetFormField(columnName);
                var value = container.GetValue(columnName);

                return new GlobalSettingOutput
                {
                    CodeName = columnName,
                    DisplayName = $"{{$globalsettingskey.{columnName.ToLowerInvariant()}$}}",
                    Type = GetTypeName(field?.DataType, value),
                    Value = value
                };
            })
            .ToArray();
    }

    private static string GetTypeName(string? cmsDataType, object? value)
    {
        if (cmsDataType is not null)
        {
            return cmsDataType switch
            {
                FieldDataType.Boolean => "boolean",
                FieldDataType.Double => "double",
                FieldDataType.Integer => "int32",
                FieldDataType.LongInteger => "int64",
                FieldDataType.LongText => "string",
                FieldDataType.Text => "string",
                FieldDataType.DateTime => "datetime",
                FieldDataType.Guid => "guid",
                _ => "string",
            };
        }

        return value switch
        {
            null => "string",
            bool => "boolean",
            int => "int32",
            long => "int64",
            double => "double",
            decimal => "double",
            float => "double",
            DateTime => "datetime",
            Guid => "guid",
            _ => "string"
        };
    }
}