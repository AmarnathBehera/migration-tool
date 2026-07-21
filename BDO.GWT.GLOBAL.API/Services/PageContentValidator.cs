using BDO.GWT.Common.Core.Exceptions;
using BDO.GWT.GLOBAL.API.Constants;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Extensions;
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.MacroEngine;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Immutable;
using System.Reflection;

namespace BDO.GWT.GLOBAL.API.Services;

public interface IPageContentValidator
{
    void ValidateAndNormalizePageContent<TPageContent>(TPageContent content)
        where TPageContent : IPageContent;
}

public class PageContentValidator : IPageContentValidator
{
    private static readonly string[] OptionsLineSeparators = { "\n", "\r", "\r\n" };

    private readonly IMacroResolver _macroResolver;
    private readonly IValidationState _validationState;

    public PageContentValidator(IMacroResolver macroResolver, IValidationState validationState)
    {
        _macroResolver = macroResolver;
        _validationState = validationState;
    }

    public void ValidateAndNormalizePageContent<TPageContent>(TPageContent content)
        where TPageContent : IPageContent
    {
        var pageContentType = content.GetType();
        var className = GetClassNameFromPageContentType(pageContentType);
        var contentTypeCodeNameSegment = GetContentTypeCodeNameSegment(className);

        var dataClassInfo = DataClassInfoProvider.GetDataClassInfo(className);

        var form = new FormInfo(dataClassInfo.ClassFormDefinition);

        var contentPropertyByName = content
            .GetType()
            .GetProperties()
            .ToImmutableDictionary(x => x.Name, x => x, StringComparer.OrdinalIgnoreCase);

        foreach (var item in form.ItemsList)
        {
            if (item is not FormFieldInfo field || field.System || field.PrimaryKey || !field.Visible || field.IsDummyField) continue;

            if (!TryGetContentProperty(
                contentPropertyByName,
                contentTypeCodeNameSegment,
                field.Name,
                out var contentProperty))
            {
                continue;
            }

            var value = contentProperty.GetValue(content);

            var controlName = field.Settings.TryGetValue(FormFieldSetting.ControlName, out var settingValue)
                ? Convert.ToString(settingValue)?.Trim().ToLowerInvariant()
                : null;

            if (controlName is null)
            {
                throw new NotFoundException("Field '{field.Name}' control is invalid.");
            }

            var (valueProcessed, newValue) = TryToValidateAndNormalizeValueByControlCssClass(field, value);

            value = valueProcessed ? newValue : ValidateAndNormalizeValueByDataType(field, controlName, value);

            contentProperty.SetValue(content, value);
        }
    }

    private static bool TryGetContentProperty(
        ImmutableDictionary<string, PropertyInfo> contentPropertyByName,
        string contentTypeCodeNameSegment,
        string kenticoFieldName,
        out PropertyInfo contentProperty)
    {
        foreach (var propertyName in GetContentPropertyNameCandidates(contentTypeCodeNameSegment, kenticoFieldName))
        {
            if (contentPropertyByName.TryGetValue(propertyName, out contentProperty!))
            {
                return true;
            }
        }

        contentProperty = null!;
        return false;
    }

    private static IEnumerable<string> GetContentPropertyNameCandidates(string contentTypeCodeNameSegment, string kenticoFieldName)
    {
        var candidates = new[]
        {
            kenticoFieldName,
            TrimPrefix(kenticoFieldName, contentTypeCodeNameSegment),
            TrimSuffix(kenticoFieldName, contentTypeCodeNameSegment),
            TrimPrefix(kenticoFieldName, "Metadata"),
            TrimPrefix(TrimPrefix(kenticoFieldName, contentTypeCodeNameSegment), "Metadata"),
            TrimPrefix(TrimSuffix(kenticoFieldName, contentTypeCodeNameSegment), "Metadata"),
        };

        return candidates
            .Where(candidate => !string.IsNullOrWhiteSpace(candidate))
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }

    private static string TrimPrefix(string value, string prefix)
    {
        return value.StartsWith(prefix, StringComparison.Ordinal)
            ? value[prefix.Length..]
            : value;
    }

    private static string TrimSuffix(string value, string suffix)
    {
        return value.EndsWith(suffix, StringComparison.Ordinal)
            ? value[..^suffix.Length]
            : value;
    }

    private static string GetContentTypeCodeNameSegment(string className)
    {
        var separatorIndex = className.LastIndexOf('.');

        return separatorIndex >= 0
            ? className[(separatorIndex + 1)..]
            : className;
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
            FieldDataType.Guid => ValidateGuidFieldDataType(field, controlName, value),
            FieldDataType.Integer => ValidateIntegerFieldDataType(field, controlName, value),
            FieldDataType.LongText => ValidateLongTextFieldDataType(field, controlName, value),
            FieldDataType.Text => ValidateTextFieldDataType(field, controlName, value),
            FieldDataType.ContentItemReference => value switch
            {
                string s => ValidateContentItemReferenceFieldDataType(field, controlName, s),
                IEnumerable<string> list => list
                    .Select(v => ValidateContentItemReferenceFieldDataType(field, controlName, v))
                    .ToList(),
                _ => throw new ArgumentException($"Validation for field data type '{field.DataType}' is not implemented."),
            }
        };
    }
    private string? ValidateContentItemReferenceFieldDataType(FormFieldInfo field, string controlName, object? value)
    {
        if (string.IsNullOrWhiteSpace(controlName))
        {
            throw new ArgumentException("Control name must be provided.", nameof(controlName));
        }

        // Content item reference values are serialized as strings in the page content DTO
        // (e.g. RelatedDeals, DisplayedContacts, RelevantContactPerson). Validate the
        // presence rule and pass the value through unchanged.
        return ValidateAndNormalizeString(
            field,
            value as string,
            DoNothing);
    }

    #region Field data type validation methods

    private bool? ValidateBooleanFieldDataType(FormFieldInfo field, string componentIdentifier, object? value)
    {
        return componentIdentifier switch
        {
            "kentico.checkbox" or "kentico.administration.checkbox" =>
                ValidateAndNormalizeBool(
                    field,
                    value as bool?,
                    DoNothing),

            _ => throw new NotImplementedException(
                $"Validation for '{field.DataType}'.'{componentIdentifier}' is not implemented.")
        };
    }

    private DateTime? ValidateDateFieldDataType(FormFieldInfo field, string componentIdentifier, object? value)
    {
        return componentIdentifier switch
        {
            "kentico.datetimeinput" or "kentico.administration.dateinput" =>
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
            throw new ArgumentException("Control name must be provided.", nameof(controlName));
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
            "kentico.dropdown" or
            "kentico.radiobuttons" =>
                ValidateAndNormalizeInt(field, value as int?, ValidateFieldOption),

            "kentico.objectselector" or
            "kentico.generalselector" =>
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
            case "kentico.dropdown":
                return ValidateAndNormalizeString(
                    field,
                    value as string,
                    ValidateFieldOption);

            case "kentico.textinput":
            case "kentico.textarea":
            case "kentico.richtext":
            case "kentico.administration.richtexteditor":
                return ValidateAndNormalizeString(
                    field,
                    value as string,
                    DoNothing);

            case "insightpeoplesort":
                return ValidateAndNormalizeMultipleValues(
                    field,
                    value as List<Guid>,
                    DoNothing);

            case "kentico.objectselector":
            case "kentico.generalselector":
            case "multiselector":
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
            case "kentico.dropdown":
            case "kentico.radiobuttons":
                return ValidateAndNormalizeString(
                    field,
                    value as string,
                    ValidateFieldOption);

            // Text-based components
            case "kentico.textinput":
            case "kentico.administration.textinput":
            case "kentico.textarea":
            case "kentico.administration.textarea":
            case "kentico.emailinput":
            case "kentico.numericinput":
            case "kentico.objectselector":
            // Admin dropdown selector stores a plain string value.
            case "kentico.administration.dropdownselector":
            // Custom SQL-driven dropdown stores a plain string value; its
            // options are resolved via SQL at runtime, so we normalize the
            // string without validating against static field options.
            case "bdo.sqldropdown":
                return ValidateAndNormalizeString(
                    field,
                    value as string,
                    controlName == "kentico.emailinput"
                        ? ValidateEmailInput
                        : DoNothing);

            // URL / media-based components
            case "kentico.mediafilesselector":
            case "kentico.urlselector":
            case "multirenditionmediaselector":
                return ValidateAndNormalizeString(
                    field,
                    value as string,
                    ValidateAndNormalizeUri);

            // Multiple choice
            case "kentico.multiplechoice":
                return ValidateAndNormalizeMultipleValues(
                    field,
                    value as HashSet<string>,
                    ValidateFieldOptions);

            // Custom Guid-based selector
            case "insightpeoplesort":
                return ValidateAndNormalizeMultipleValues(
                    field,
                    value as List<Guid>,
                    DoNothing);

            // Multi-selector
            case "multiselector":
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
            case "kentico.generalselector":
                return ValidateAndNormalizeMultipleValues(
                    field,
                    value as List<int>,
                    DoNothing);

            // Label/read-only field
            case "kentico.label":
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
            var link = Uri.TryCreate(value?.ToString(), UriKind.RelativeOrAbsolute, out var uri);

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
        var lines = resolvedOptions.Split(OptionsLineSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var validOptions = lines
            .Select(x => x.Split(";", StringSplitOptions.TrimEntries)[0])
            .ToHashSet();

        return validOptions;
    }

    private static string FormatStringValues(IEnumerable<string> stringValues)
    {
        return string.Join(", ", stringValues.Select(x => $"'{x}'"));
    }

    private static string GetClassNameFromPageContentType(Type pageContentType)
    {
        return TryGetContentTypeName(pageContentType, out var className)
            ? className
            : throw new InvalidOperationException(
                $"Could not resolve the XbyK content type name for '{pageContentType.FullName}'.");
    }

    private static bool TryGetContentTypeName(Type type, out string className)
    {
        const string FieldName = "CONTENT_TYPE_NAME";

        var contentTypeField = type.GetField(
            FieldName,
            BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

        if (contentTypeField?.GetValue(null) is string resolvedName && !string.IsNullOrWhiteSpace(resolvedName))
        {
            className = resolvedName;
            return true;
        }

        if (type.Name.EndsWith("PageContent", StringComparison.Ordinal))
        {
            var generatedTypeName = type.Name[..^"PageContent".Length];

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var generatedType = assembly.GetType($"BDO.{generatedTypeName}");

                if (generatedType is not null && TryGetContentTypeName(generatedType, out className))
                {
                    return true;
                }
            }
        }

        className = string.Empty;
        return false;
    }


}
