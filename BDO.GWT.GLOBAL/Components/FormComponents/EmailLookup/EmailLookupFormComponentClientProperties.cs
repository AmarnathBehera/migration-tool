using Kentico.Xperience.Admin.Base.Forms;

namespace BDO.GWT.GLOBAL.Components.FormComponents.EmailLookup;

public class EmailLookupFormComponentClientProperties : FormComponentClientProperties<string>
{
    public string Email { get; set; } = string.Empty;
    public string LinkText { get; set; } = string.Empty;
    public string LinkXml { get; set; } = string.Empty;
    public string Results { get; set; } = string.Empty;
    public string LinkDelete { get; set; } = string.Empty;
    public string LinkCopy { get; set; } = string.Empty;
}