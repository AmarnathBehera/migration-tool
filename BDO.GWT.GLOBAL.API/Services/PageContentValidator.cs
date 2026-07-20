using BDO.GWT.GLOBAL.API.Constants;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Extensions;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.MacroEngine;
using System.Collections;
using System.Collections.Immutable;

namespace BDO.GWT.GLOBAL.API.Services;

public interface IPageContentValidator
{
    void ValidateAndNormalizePageContent<TPageContent>(TPageContent content, string pageName = default)
        where TPageContent : IPageContent;
}

public class PageContentValidator : IPageContentValidator
{
    private readonly IMacroResolver _macroResolver;
    private readonly IValidationState _validationState;

    public PageContentValidator(IMacroResolver macroResolver, IValidationState validationState)
    {
        _macroResolver = macroResolver;
        _validationState = validationState;
    }

    public void ValidateAndNormalizePageContent<TPageContent>(TPageContent content, string pageName)
        where TPageContent : IPageContent
    {
        var className = GetClassNameFromPageContentType<TPageContent>(pageName);

        var dataClassInfo = DataClassInfoProvider.GetDataClassInfo(className);

        var form = new FormInfo(dataClassInfo.ClassFormDefinition);

        var contentPropertyByName = content
            .GetType()
            .GetProperties()
            .ToImmutableDictionary(x => x.Name, x => x);

        foreach (var item in form.ItemsList)
        {
            if (item is not FormFieldInfo field || field.System || field.PrimaryKey || !field.Visible || field.IsDummyField) continue;

            if (!contentPropertyByName.TryGetValue(field.Name, out var property))
            {
                // Field exists in the Kentico form definition but has no matching
                // property on the generated content type model — nothing to validate.
                continue;
            }

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
            FieldDataType.ContentItemReference => ValidateContentItemReferenceFieldDataType(field, controlName, value),
            FieldDataType.Date => ValidateDateFieldDataType(field, controlName, value),
            //FieldDataType.File => ValidateFileFieldDataType(field, controlName, value),
            FieldDataType.Guid => ValidateGuidFieldDataType(field, controlName, value),
            FieldDataType.Integer => ValidateIntegerFieldDataType(field, controlName, value),
            FieldDataType.LongText => ValidateLongTextFieldDataType(field, controlName, value),
            FieldDataType.Text => ValidateTextFieldDataType(field, controlName, value),
            _ => throw new NotImplementedException($"Validation for field data type '{field.DataType}' is not implemented."),
        };
    }

    #region Field data type validation methods

    private bool? ValidateBooleanFieldDataType(FormFieldInfo field, string componentIdentifier, object? value)
    {
        return componentIdentifier switch
        {
            "Kentico.CheckBox" =>
                ValidateAndNormalizeBool(
                    field,
                    value as bool?,
                    DoNothing),

            "kentico.administration.checkbox" =>
                ValidateAndNormalizeBool(
                    field,
                    value as bool?,
                    DoNothing),


            _ => throw new NotImplementedException(
                $"Validation for '{field.DataType}'.'{componentIdentifier}' is not implemented.")
        };
    }
    private object? ValidateContentItemReferenceFieldDataType(FormFieldInfo field, string controlName, object? value)
    {
        return controlName switch
        {
            "kentico.contentitemselector" or
            "kentico.administration.contentitemselector" =>
                ValidateAndNormalizeMultipleValues(
                    field,
                    value as List<Guid>,
                    DoNothing),

            _ => throw new NotImplementedException(
                $"Validation for '{field.DataType}'.'{controlName}' is not implemented."),
        };
    }
    private DateTime? ValidateDateFieldDataType(FormFieldInfo field, string componentIdentifier, object? value)
    {
        return componentIdentifier switch
        {
            "Kentico.DateTimeInput" =>
                ValidateAndNormalizeDateTime(
                    field,
                    value as DateTime?,
                    DoNothing),

            _ => throw new NotImplementedException(
                $"Validation for '{field.DataType}'.'{componentIdentifier}' is not implemented.")
        };
    }

    private Guid? ValidateFileFieldDataType(FormFieldInfo field, string componentIdentifier, object? value)
    {
        return componentIdentifier switch
        {
            "Kentico.FileUploader" =>
                ValidateAndNormalizeGuid(
                    field,
                    value as Guid?,
                    DoNothing),

            _ => throw new NotImplementedException(
                $"Validation for '{field.DataType}'.'{componentIdentifier}' is not implemented.")
        };
    }

    private Guid? ValidateGuidFieldDataType(FormFieldInfo field, string controlName, object? value)
    {
        if (string.IsNullOrWhiteSpace(controlName))
        {
            throw new ArgumentException(nameof(controlName));
        }

        return ValidateAndNormalizeGuid(
            field,
            value is Guid guid ? guid : value as Guid?,
            DoNothing);
    }

    private object? ValidateIntegerFieldDataType(FormFieldInfo field, string controlName, object? value)
    {
        return controlName switch
        {
            "Kentico.DropDown" or
            "Kentico.RadioButtons" =>
                ValidateAndNormalizeInt(field, value as int?, ValidateFieldOption),

            "Kentico.ObjectSelector" or
            "Kentico.GeneralSelector" =>
                ValidateAndNormalizeInt(field, value as int?, DoNothing),

            _ =>
                throw new NotImplementedException(
                    $"Validation for '{field.DataType}'.'{controlName}' is not implemented.")
        };
    }

    private object? ValidateLongTextFieldDataType(FormFieldInfo field, string controlName, object? value)
    {
        switch (controlName)
        {
            case "Kentico.DropDown":
                return ValidateAndNormalizeString(
                    field,
                    value as string,
                    ValidateFieldOption);

            case "Kentico.TextInput":
            case "Kentico.TextArea":
            case "Kentico.RichText":
                return ValidateAndNormalizeString(
                    field,
                    value as string,
                    DoNothing);

            case "InsightPeopleSort":
                return ValidateAndNormalizeMultipleValues(
                    field,
                    value as List<Guid>,
                    DoNothing);

            case "Kentico.ObjectSelector":
            case "Kentico.GeneralSelector":
            case "MultiSelector":
                return ValidateAndNormalizeMultipleValues(
                    field,
                    value as List<int>,
                    DoNothing);

            default:
                throw new NotImplementedException(
                    $"Validation for '{field.DataType}'.'{controlName}' is not implemented.");
        }
    }

    private object? ValidateTextFieldDataType(
    FormFieldInfo field,
    string controlName,
    object? value)
    {
        switch (controlName)
        {
            // Single-select option components
            case "Kentico.DropDown":
            case "Kentico.RadioButtons":
                return ValidateAndNormalizeString(
                    field,
                    value as string,
                    ValidateFieldOption);

            // Text-based components
            case "Kentico.TextInput":
            case "kentico.administration.textinput":
            case "Kentico.TextArea":
            case "Kentico.EmailInput":
            case "Kentico.NumericInput":
            case "Kentico.ObjectSelector":
                return ValidateAndNormalizeString(
                    field,
                    value as string,
                    controlName == "Kentico.EmailInput"
                        ? ValidateEmailInput
                        : DoNothing);

            // URL / media-based components
            case "Kentico.MediaFilesSelector":
            case "Kentico.UrlSelector":
            case "MultiRenditionMediaSelector":
                return ValidateAndNormalizeString(
                    field,
                    value as string,
                    ValidateAndNormalizeUri);

            // Multiple choice
            case "Kentico.MultipleChoice":
                return ValidateAndNormalizeMultipleValues(
                    field,
                    value as HashSet<string>,
                    ValidateFieldOptions);

            // Custom Guid-based selector
            case "InsightPeopleSort":
                return ValidateAndNormalizeMultipleValues(
                    field,
                    value as List<Guid>,
                    DoNothing);

            // Multi-selector
            case "MultiSelector":
                return field.DataType switch
                {
                    FieldDataType.Guid =>
                        ValidateAndNormalizeMultipleValues(
                            field,
                            value as List<Guid>,
                            DoNothing),

                    FieldDataType.Integer =>
                        ValidateAndNormalizeMultipleValues(
                            field,
                            value as List<int>,
                            DoNothing),

                    _ =>
                        throw new NotSupportedException(
                            $"Unsupported selector type '{field.DataType}'.")
                };

            // General selector
            case "Kentico.GeneralSelector":
                return ValidateAndNormalizeMultipleValues(
                    field,
                    value as List<int>,
                    DoNothing);

            // Custom SQL-query-backed dropdown. Options come from a SQL query rather
            // than a static macro list, so we can't validate against a fixed option
            // set here (see GetValidFieldOptions TODO) — just normalize the string.
            case "bdo.sqldropdown":
                return ValidateAndNormalizeString(
                    field,
                    value as string,
                    DoNothing);

            // Label/read-only field
            case "Kentico.Label":
                return null;

            default:
                throw new NotImplementedException(
                    $"Validation for '{field.DataType}'.'{controlName}' is not implemented.");
        }
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
            var isValid = Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out var uri);

            if (!isValid)
            {
                if (!field.AllowEmpty)
                {
                    _validationState.AddError(field.Name, FormErrorMessage.InvalidValue);
                }

                return value;
            }

            value = uri!.ToString();

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

    private string GetClassNameFromPageContentType<TPageContent>(string pageName)
    {
        if (pageName == "Person")
        {
            return Person.CONTENT_TYPE_NAME;
        }
        // Need to implement properly, for now just return the Deal content type name
        return Deal.CONTENT_TYPE_NAME;
    }


}