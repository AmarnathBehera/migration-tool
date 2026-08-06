using Kentico.Xperience.Admin.Base.FormAnnotations;
using XperienceCommunity.Redirects.Services;

namespace XperienceCommunity.Redirects.UIPages;

public class RedirectBulkImportModel
{
    [RequiredValidationRule]
    [DropDownComponent(
        Label = "Site",
        Order = 1,
        DataProviderType = typeof(ChannelDropdownProvider),
        Tooltip = "The website channel that the target pages of the imported redirects belong to.")]
    public string WebsiteChannelName { get; set; } = string.Empty;

    [RequiredValidationRule]
    [DropDownComponent(
        Label = "CSV delimiter",
        Order = 2,
        Options = "semicolon;Semicolon\r\ncomma;Comma")]
    public string Delimiter { get; set; } = "semicolon";

    [CheckBoxComponent(Label = "First row is a header", Order = 3)]
    public bool HasHeaderRow { get; set; } = true;

    [RequiredValidationRule]
    [CsvFileUploadComponent(
        Label = "CSV file",
        Order = 4,
        ExplanationTextAsHtml = true,
        ExplanationText = "Upload a CSV file. Each row needs two columns separated by the delimiter above: the source URL being redirected from, and the current system URL of the target page (e.g. <code>/section/new-page</code>).")]
    public string CsvData { get; set; } = string.Empty;
}