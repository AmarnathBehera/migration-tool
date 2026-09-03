using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;

namespace BDO.GWT.GLOBAL.Components.FormComponents.GlobalLocationDefault;

public class GlobalLocationDefaultProperties : FormComponentProperties
{
    [TextAreaComponent(
        Label = "Setting Name",
        Order = 1)]
    public string GlobalSettingName { get; set; } = "";
}