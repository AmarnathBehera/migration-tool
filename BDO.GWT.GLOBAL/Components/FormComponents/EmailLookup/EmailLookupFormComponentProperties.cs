using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;

namespace BDO.GWT.GLOBAL.Components.FormComponents.EmailLookup;

public class EmailLookupFormComponentProperties : FormComponentProperties
{
    [TextInputComponent(
        Label = "Email Address:",
        Order = 0)]
    public string Email { get; set; } = string.Empty;

    [LinkComponent(
        Label = "Search to Text",
        Order = 1)]
    public string LinkText { get; set; } = string.Empty;

    [LinkComponent(
        Label = "Search to XML",
        Order = 2)]
    public string LinkXml { get; set; } = string.Empty;

    [TextAreaComponent(
        Label = "Results:",
        Order = 3,
        EditMode = FormEditMode.ReadOnly)]
    public string Results { get; set; } = string.Empty;

    [LinkComponent(
        Label = "Delete",
        Order = 4)]
    public string LinkDelete { get; set; } = string.Empty;

    [LinkComponent(
        Label = "Copy",
        Order = 5)]
    public string LinkCopy { get; set; } = string.Empty;
}
