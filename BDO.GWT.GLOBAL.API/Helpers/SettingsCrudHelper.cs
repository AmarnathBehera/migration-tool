using BDO.GWT.Common.Core.Exceptions;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Services;
using CMS.DataEngine;
using CMS.FormEngine;
using System.Globalization;

namespace BDO.GWT.GLOBAL.API.Helpers;

public interface ISettingsCrudHelper
{
    void ValidateAndNormalizeSettingInput(FormFieldInfo? field, string columnName, GlobalSettingInput input);
}

public class SettingsCrudHelper : ISettingsCrudHelper
{
    private readonly IValidationState _validationState;

    public SettingsCrudHelper(IValidationState validationState)
    {
        _validationState = validationState;
    }

    public void ValidateAndNormalizeSettingInput(FormFieldInfo? field, string columnName, GlobalSettingInput input)
    {
        if (field is null)
        {
            _validationState.AddError("codeName", $"Cannot find field definition for setting '{columnName}'.");
        }
        else
        {
            var value = input.Value?.ToString();

            if (value is not null)
            {
                var normalizedValue = NormalizeValue(value, field.DataType);

                if (normalizedValue is null)
                {
                    _validationState.AddError("value", GetErrorMessage(field.DataType));
                }
                else
                {
                    input.Value = normalizedValue;
                }
            }
        }

        if (_validationState.HasErrors())
        {
            throw new ValidationException();
        }
    }

    private static object? NormalizeValue(string value, string cmsDataType)
    {
        return cmsDataType switch
        {
            FieldDataType.Boolean => bool.TryParse(value, out var boolValue) ? (bool?)boolValue : null,
            FieldDataType.Double => double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var doubleValue) ? (double?)doubleValue : null,
            FieldDataType.Integer => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intValue) ? (int?)intValue : null,
            FieldDataType.LongInteger => long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var longValue) ? (long?)longValue : null,
            FieldDataType.DateTime => DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateValue) ? (DateTime?)dateValue : null,
            FieldDataType.Guid => Guid.TryParse(value, out var guidValue) ? (Guid?)guidValue : null,
            FieldDataType.LongText => value,
            FieldDataType.Text => value,
            _ => value,
        };
    }

    private static string GetErrorMessage(string cmsDataType)
    {
        return cmsDataType switch
        {
            FieldDataType.Boolean => "Please enter an boolean value (true/false).",
            FieldDataType.Double => "Please enter a number.",
            FieldDataType.Integer or FieldDataType.LongInteger => "Please enter an integer.",
            FieldDataType.DateTime => "Please enter a valid date/time.",
            FieldDataType.Guid => "Please enter a valid GUID.",
            _ => "Value is in wrong format.",
        };
    }
}