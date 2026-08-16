using CMS.Core;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

[assembly: RegisterFormValidationRule(
    BDO.GWT.Web.Widgets.WidgetIdValidationRule.IDENTIFIER,
    typeof(BDO.GWT.Web.Widgets.WidgetIdValidationRule),
    "Widget ID format",
    "Only lowercase letters, numbers and single hyphens allowed (e.g. three-word-id).")]

namespace BDO.GWT.Web.Widgets;

// ── Properties class ────────────────────────────────────────────────────────

public class WidgetIdValidationRuleProperties : ValidationRuleProperties
{
    public override string GetDescriptionText(ILocalizationService localizationService)
        => "Validates that the Widget ID contains only lowercase letters, digits, and single hyphens.";
}

// ── Rule ────────────────────────────────────────────────────────────────────

[ValidationRuleAttribute(typeof(WidgetIdValidationRuleAttribute))]
public class WidgetIdValidationRule : ValidationRule<WidgetIdValidationRuleProperties, string>
{
    public const string IDENTIFIER = "BDO.GWT.WidgetIdValidationRule";

    private static readonly Regex WidgetIdRegex = new(
        @"^[a-z0-9]+(-[a-z0-9]+)*$",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public override Task<ValidationResult> Validate(string value, IFormFieldValueProvider formFieldValueProvider)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return ValidationResult.SuccessResult();
        }

        return WidgetIdRegex.IsMatch(value)
            ? ValidationResult.SuccessResult()
            : ValidationResult.FailResult("Only lowercase letters, numbers and single hyphens are allowed (e.g. three-word-id).");
    }
}

// ── Attribute ───────────────────────────────────────────────────────────────

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class WidgetIdValidationRuleAttribute : ValidationRuleAttribute
{
}