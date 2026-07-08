using BDO.GWT.Common.Core.Extensions;
using BDO.GWT.Common.Core.Services;
using BDO.GWT.Domain.Core.ValueObjects;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.Web.Core.API.Data;
using BDO.GWT.Web.Core.API.Services;
using BDO.GWT.Xperience.Constants;
using BDO.GWT.Xperience.Core.API.Extensions;
using BDO.GWT.Xperience.Core.API.Helpers;
using BDO.GWT.Xperience.Core.Constants;
using BDO.GWT.Xperience.Core.Utilities;
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.MacroEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace BDO.GWT.GLOBAL.API.Services;

public interface IPageContentValidator
{
    void ValidateAndNormalizePageContent<TPageContent>(TPageContent content)
        where TPageContent : IPageContent;
}

public class PageContentValidator : IPageContentValidator
{
    private readonly IApiTypeMapper _apiTypeMapper;
    private readonly IMacroResolver _macroResolver;
    private readonly IValidationState _validationState;

    public PageContentValidator(IApiTypeMapper apiTypeMapper, IMacroResolver macroResolver, IValidationState validationState)
    {
        _apiTypeMapper = apiTypeMapper;
        _macroResolver = macroResolver;
        _validationState = validationState;
    }

    public void ValidateAndNormalizePageContent<TPageContent>(TPageContent content)
        where TPageContent : IPageContent
    {
        var className = _apiTypeMapper.GetClassNameFromPageContentType<TPageContent>();

        var dataClassInfo = DataClassInfoProvider.GetDataClassInfo(className);

        var form = new FormInfo(dataClassInfo.ClassFormDefinition);

        var contentPropertyByName = content
            .GetType()
            .GetProperties()
            .ToImmutableDictionary(x => x.Name, x => x);

        foreach (var item in form.ItemsList)
        {
            if (item is not FormFieldInfo field || field.System || field.PrimaryKey || !field.Visible || field.IsDummyField) continue;

            var value = contentPropertyByName[field.Name].GetValue(content);

            var controlName = field.Settings.TryGetValue(FormFieldSetting.ControlName, out var settingValue) ? ((string?)settingValue)?.ToLowerInvariant() : null;

            if (controlName is null)
            {
                throw new Exception($"Field '{field.Name}' control is invalid.");
            }

            var (valueProcessed, newValue) = TryToValidateAndNormalizeValueByControlCssClass(field, value);

            value = valueProcessed ? newValue : ValidateAndNormalizeValueByDataType(field, controlName, value);

            contentPropertyByName[field.Name].SetValue(content, value);
        }
    }

    private static (bool, object?) TryToValidateAndNormalizeValueByControlCssClass(FormFieldInfo field, object? value)
    {
        var valueProcessed = false;

        if (field.Properties.TryGetValue(OtherFormFieldProperty.ControlCssClass, out var controlCssClassObject))
        {
            var controlCssClass = (string?)controlCssClassObject;

            if (controlCssClass is "IndustryControl" or "SubIndustryControl")
            {
                value = ValidateAndNormalizeMultipleValues(field, (List<string>?)value, DoNothing);

                valueProcessed = true;
            }
        }

        return (valueProcessed, value);
    }

    private object? ValidateAndNormalizeValueByDataType(FormFieldInfo field, string controlName, object? value)
    {
        return field.DataType switch
        {
            FieldDataType.Boolean => ValidateBooleanFieldDataType(field, controlName, value),
            FieldDataType.Date => ValidateDateFieldDataType(field, controlName, value),
            FieldDataType.File => ValidateFileFieldDataType(field, controlName, value),
            FieldDataType.Guid => ValidateGuidFieldDataType(field, controlName, value),
            FieldDataType.Integer => ValidateIntegerFieldDataType(field, controlName, value),
            FieldDataType.LongText => ValidateLongTextFieldDataType(field, controlName, value),
            FieldDataType.Text => ValidateTextFieldDataType(field, controlName, value),
            _ => throw new NotImplementedException($"Validation for field data type '{field.DataType}' is not implemented."),
        };
    }

    #region Field data type validation methods

    private bool? ValidateBooleanFieldDataType(FormFieldInfo field, string controlName, object? value)
    {
        return controlName switch
        {
            FormFieldControlName.CHECKBOX => ValidateAndNormalizeBool(field, (bool?)value, DoNothing),
            _ => throw new NotImplementedException($"Validation for '{field.DataType}'.'{controlName}' is not implemented."),
        };
    }

    private DateTime? ValidateDateFieldDataType(FormFieldInfo field, string controlName, object? value)
    {
        return controlName switch
        {
            FormFieldControlName.CALENDAR => ValidateAndNormalizeDateTime(field, (DateTime?)value, DoNothing),
            _ => throw new NotImplementedException($"Validation for '{field.DataType}'.'{controlName}' is not implemented."),
        };
    }

    private Guid? ValidateFileFieldDataType(FormFieldInfo field, string controlName, object? value)
    {
        return controlName switch
        {
            FormFieldOtherControlName.DIRECT_UPLOAD => ValidateAndNormalizeGuid(field, (Guid?)value, DoNothing),
            _ => throw new NotImplementedException($"Validation for '{field.DataType}'.'{controlName}' is not implemented."),
        };
    }

    private Guid? ValidateGuidFieldDataType(FormFieldInfo field, string controlName, object? value)
    {
        return controlName switch
        {
            FormFieldControlName.LABEL => ValidateAndNormalizeGuid(field, (Guid?)value, DoNothing),
            _ => throw new NotImplementedException($"Validation for '{field.DataType}'.'{controlName}' is not implemented."),
        };
    }

    private object? ValidateIntegerFieldDataType(FormFieldInfo field, string controlName, object? value)
    {
        return controlName switch
        {
            FormFieldControlName.DROPDOWNLIST => ValidateAndNormalizeInt(field, (int?)value, ValidateFieldOption),
            FormFieldControlName.RADIOBUTTONS => ValidateAndNormalizeInt(field, (int?)value, ValidateFieldOption),
            FormFieldOtherControlName.UNI_SELECTOR => ValidateAndNormalizeInt(field, (int?)value, DoNothing),
            _ => throw new NotImplementedException($"Validation for '{field.DataType}'.'{controlName}' is not implemented."),
        };
    }

    private object? ValidateLongTextFieldDataType(FormFieldInfo field, string controlName, object? value)
    {
        return controlName switch
        {
            FormFieldControlName.DROPDOWNLIST => ValidateAndNormalizeString(field, (string?)value, ValidateFieldOption),
            FormFieldControlName.HTMLAREA => ValidateAndNormalizeString(field, (string?)value, DoNothing),
            FormFieldCustomControlName.INSIGHTPEOPLESORT => ValidateAndNormalizeMultipleValues(field, (List<Guid>?)value, DoNothing),
            FormFieldCustomControlName.MULTISELECTOR => ValidateAndNormalizeMultipleValues(field, (List<int>?)value, DoNothing),
            FormFieldControlName.TEXTAREA => ValidateAndNormalizeString(field, (string?)value, DoNothing),
            FormFieldControlName.TEXTBOX => ValidateAndNormalizeString(field, (string?)value, DoNothing),
            FormFieldOtherControlName.UNI_SELECTOR => ValidateAndNormalizeMultipleValues(field, (List<int>?)value, DoNothing),
            _ => throw new NotImplementedException($"Validation for '{field.DataType}'.'{controlName}' is not implemented."),
        };
    }

    private object? ValidateTextFieldDataType(FormFieldInfo field, string controlName, object? value)
    {
        return controlName switch
        {
            FormFieldControlName.DROPDOWNLIST => ValidateAndNormalizeString(field, (string?)value, ValidateFieldOption),
            FormFieldOtherControlName.EMAILINPUT => ValidateAndNormalizeString(field, (string?)value, ValidateEmailInput),
            FormFieldCustomControlName.INSIGHTPEOPLESORT => ValidateAndNormalizeMultipleValues(field, (List<Guid>?)value, DoNothing),
            FormFieldControlName.LABEL => null,
            FormFieldControlName.MEDIASELECTION => ValidateAndNormalizeString(field, (string?)value, ValidateAndNormalizeUri),
            FormFieldControlName.MULTIPLECHOICE => ValidateAndNormalizeMultipleValues(field, (HashSet<string>?)value, ValidateFieldOptions),
            FormFieldCustomControlName.MULTIRENDITIONMEDIASELECTOR => ValidateAndNormalizeString(field, (string?)value, ValidateAndNormalizeUri),
            FormFieldCustomControlName.MULTISELECTOR => FormFieldUtility.TryGetOptionTypeFromSqlQuery(field) switch
            {
                { } type when type == typeof(Guid) => ValidateAndNormalizeMultipleValues(field, (List<Guid>?)value, DoNothing),
                { } type when type == typeof(int) => ValidateAndNormalizeMultipleValues(field, (List<int>?)value, DoNothing),
                _ => ValidateAndNormalizeMultipleValues(field, (List<int>?)value, DoNothing),
            },
            FormFieldOtherControlName.NUMERICUPDOWN => ValidateAndNormalizeString(field, (string?)value, DoNothing),
            FormFieldOtherControlName.OBJECTSELECTOR => ValidateAndNormalizeString(field, (string?)value, DoNothing),
            FormFieldControlName.RADIOBUTTONS => ValidateAndNormalizeString(field, (string?)value, ValidateFieldOption),
            FormFieldControlName.TEXTAREA => ValidateAndNormalizeString(field, (string?)value, DoNothing),
            FormFieldControlName.TEXTBOX => ValidateAndNormalizeString(field, (string?)value, DoNothing),
            FormFieldOtherControlName.UNI_SELECTOR => ValidateAndNormalizeMultipleValues(field, (List<int>?)value, DoNothing),
            FormFieldOtherControlName.URLSELECTOR => ValidateAndNormalizeString(field, (string?)value, ValidateAndNormalizeUri),
            _ => throw new NotImplementedException($"Validation for '{field.DataType}'.'{controlName}' is not implemented."),
        };
    }

    #endregion

    #region Value type validation methods

    private bool? ValidateAndNormalizeBool(FormFieldInfo field, bool? value, Func<FormFieldInfo, bool, bool> validateAndNormalizeValue)
    {
        if (!field.AllowEmpty && value == null)
        {
            _validationState.AddError(field.Name, FormErrorMessage.EmptyValue);
        }

        return value is null ? value : validateAndNormalizeValue(field, value.Value);
    }

    private DateTime? ValidateAndNormalizeDateTime(FormFieldInfo field, DateTime? value, Func<FormFieldInfo, DateTime, DateTime> validateAndNormalizeValue)
    {
        if (!field.AllowEmpty && value == null)
        {
            _validationState.AddError(field.Name, FormErrorMessage.EmptyValue);
        }

        return value is null ? value : validateAndNormalizeValue(field, value.Value);
    }

    private Guid? ValidateAndNormalizeGuid(FormFieldInfo field, Guid? value, Func<FormFieldInfo, Guid, Guid> validateAndNormalizeValue)
    {
        if (!field.AllowEmpty && (value == null || value == Guid.Empty))
        {
            _validationState.AddError(field.Name, FormErrorMessage.EmptyValue);
        }

        return value is null ? value : validateAndNormalizeValue(field, value.Value);
    }

    private object? ValidateAndNormalizeInt(FormFieldInfo field, int? value, Func<FormFieldInfo, int, int> validateAndNormalizeValue)
    {
        if (!field.AllowEmpty && value is null)
        {
            _validationState.AddError(field.Name, FormErrorMessage.EmptyValue);
        }

        return value is null ? value : validateAndNormalizeValue(field, value.Value);
    }

    private string? ValidateAndNormalizeString(FormFieldInfo field, string? value, Func<FormFieldInfo, string, string> validateAndNormalizeValue)
    {
        if (!field.AllowEmpty && string.IsNullOrWhiteSpace(value))
        {
            _validationState.AddError(field.Name, FormErrorMessage.EmptyValue);
        }

        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        if (field.Size > 0 && value.Length > field.Size)
        {
            value = value[..field.Size];
        }

        return validateAndNormalizeValue(field, value);
    }

    private static TValues ValidateAndNormalizeMultipleValues<TValues>(FormFieldInfo field, TValues? values, Func<FormFieldInfo, TValues, TValues> validateAndNormalizeValues)
        where TValues : IEnumerable, new()
    {
        return values is null ? [] : validateAndNormalizeValues(field, values);
    }

    #endregion

    #region Form control validation methods

    private static TValue DoNothing<TValue>(FormFieldInfo field, TValue value)
    {
        return value;
    }

    private string ValidateAndNormalizeUri(FormFieldInfo field, string value)
    {
        try
        {
            var link = Link.Parse(value);

            value = link.ToString();

            if (!field.AllowEmpty && string.IsNullOrWhiteSpace(value))
            {
                _validationState.AddError(field.Name, FormErrorMessage.InvalidValue);
            }
        }
        catch (ArgumentException ex)
        {
            _validationState.AddError(field.Name, ex.Message);
        }

        return value;
    }

    private string ValidateEmailInput(FormFieldInfo field, string value)
    {
        var allowMultipleAddresses = bool.Parse((string)field.Settings[FormFieldSetting.AllowMultipleAddresses]!);

        var isEmailValid = allowMultipleAddresses
            ? ValidationHelper.AreEmails(value, null, true)
            : ValidationHelper.IsEmail(value, true);

        if (!isEmailValid)
        {
            _validationState.AddError(field.Name, FormErrorMessage.InvalidEmailFormat);
        }

        return value;
    }

    private TValue ValidateFieldOption<TValue>(FormFieldInfo field, TValue value) where TValue : notnull
    {
        var validOptions = GetValidFieldOptions(field);

        if (validOptions is not null)
        {
            var stringValue = value.ToString() ?? string.Empty;

            if (!validOptions.Contains(stringValue))
            {
                _validationState.AddError(field.Name, $"Please enter one of available values: {FormatStringValues(validOptions)}.");
            }
        }

        return value;
    }

    private HashSet<string> ValidateFieldOptions(FormFieldInfo field, HashSet<string> values)
    {
        var validOptions = GetValidFieldOptions(field);

        if (validOptions is not null)
        {
            var invalidOptions = values.Except(validOptions).ToArray();

            if (invalidOptions.Length > 0)
            {
                _validationState.AddError(field.Name, $"The following values are invalid: {FormatStringValues(invalidOptions)}. Valid options: {FormatStringValues(validOptions)}.");
            }
        }

        return values;
    }

    #endregion

    private HashSet<string>? GetValidFieldOptions(FormFieldInfo field)
    {
        var options = (string?)field.Settings[FormFieldSetting.Options];
        if (options == null)
        {
            // TODO: Add support for Macro expression and SQL Query options

            return null;
        }

        // List of options
        var resolvedOptions = _macroResolver.ResolveMacros(options);
        var lines = resolvedOptions.Split(new[] { "\n", "\r", "\r\n" }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var validOptions = lines
            .Select(x => x.Split(";", StringSplitOptions.TrimEntries)[0])
            .ToHashSet();

        return validOptions;
    }

    private static string FormatStringValues(IEnumerable<string> stringValues)
    {
        return string.Join(", ", stringValues.Select(x => $"'{x}'"));
    }
}
