using CMS.DataEngine;
using Kentico.Forms.Web.Mvc;                      // ✅ Required - FormComponentProperties lives here
using Kentico.Xperience.Admin.Base.FormAnnotations; // ✅ Required - annotation attributes live here

namespace BDO.GWT.Xperience.Core.LiveSite.Components.FormComponents;

public class TextBoxReloadProperties : FormComponentProperties<string>
{
    // ✅ Correct: two args — dataType + size (no third arg)
    public TextBoxReloadProperties() : base(FieldDataType.Text, 500)
    {
    }

    [CheckBoxComponent(Label = "Trim", Order = 0)]
    public bool Trim { get; set; }

    [TextInputComponent(Label = "Value access function name", Order = 10)]
    public string? ValueAccessFunctionName { get; set; }

    [TextInputComponent(Label = "Watermark text", Order = 20)]
    public string? WatermarkText { get; set; }

    [TextInputComponent(Label = "Watermark CSS class", Order = 30)]
    public string? WatermarkCssClass { get; set; }

    [RadioGroupComponent(Label = "Filter type", Order = 40,
        Options = "0;Numbers\r\n1;Lowercase letters\r\n2;Uppercase letters\r\n3;Custom")]
    public string? FilterType { get; set; }

    [RadioGroupComponent(Label = "Filter mode", Order = 50,
        Options = "0;Valid characters\r\n1;Invalid characters")]
    public string? FilterMode { get; set; }

    [TextInputComponent(Label = "Valid characters", Order = 60)]
    public string? ValidChars { get; set; }

    [TextInputComponent(Label = "Invalid characters", Order = 70)]
    public string? InvalidChars { get; set; }

    [TextInputComponent(Label = "Autocomplete service path", Order = 80)]
    public string? AutoCompleteServicePath { get; set; }

    [TextInputComponent(Label = "Autocomplete service method", Order = 90)]
    public string? AutoCompleteServiceMethod { get; set; }

    [TextInputComponent(Label = "Autocomplete minimum prefix length", Order = 100)]
    public string? AutoCompleteMinimumPrefixLength { get; set; }

    [TextInputComponent(Label = "Autocomplete completion set count", Order = 110)]
    public string? AutoCompleteCompletionSetCount { get; set; }

    [TextInputComponent(Label = "Autocomplete completion interval", Order = 120)]
    public string? AutoCompleteCompletionInterval { get; set; }

    [CheckBoxComponent(Label = "Autocomplete first row selected", Order = 130)]
    public bool AutoCompleteFirstRowSelected { get; set; }

    [TextInputComponent(Label = "Autocomplete completion list CSS class", Order = 140)]
    public string? AutoCompleteCompletionListCssClass { get; set; }

    [TextInputComponent(Label = "Autocomplete completion list item CSS class", Order = 150)]
    public string? AutoCompleteCompletionListItemCssClass { get; set; }

    [TextInputComponent(Label = "Autocomplete highlighted item CSS class", Order = 160)]
    public string? AutoCompleteCompletionListHighlightedItemCssClass { get; set; }

    [TextInputComponent(Label = "Autocomplete context key", Order = 170)]
    public string? AutoCompleteContextKey { get; set; }

    [TextInputComponent(Label = "Autocomplete delimiter characters", Order = 180)]
    public string? AutoCompleteDelimiterCharacters { get; set; }

    [CheckBoxComponent(Label = "Show only current word in completion list item", Order = 190)]
    public bool AutoCompleteShowOnlyCurrentWordInCompletionListItem { get; set; }

    [CheckBoxComponent(Label = "Enable caching", Order = 200)]
    public bool AutoCompleteEnableCaching { get; set; }

    // ✅ Use EditingComponentOrder.DEFAULT_VALUE for ordering the default value field
    [TextInputComponent(Label = "Default value", Order = EditingComponentOrder.DEFAULT_VALUE)]
    public override string? DefaultValue { get; set; }
}