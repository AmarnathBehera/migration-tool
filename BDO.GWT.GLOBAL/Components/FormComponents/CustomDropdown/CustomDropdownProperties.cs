using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;

namespace BDO.GWT.GLOBAL.Components.FormComponents.CustomDropdown;

public class CustomDropdownProperties : FormComponentProperties
{
    [TextAreaComponent(
        Label = "SQL Query",
        Order = 1)]
    public string Script { get; set; } = "";
}