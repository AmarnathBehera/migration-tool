using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;

namespace BDO.GWT.GLOBAL.Components.FormComponents.FetchGlobalSettingFieldData;

public class FetchGlobalSettingFieldDataProperties : FormComponentProperties
{
    [TextInputComponent(
        Label = "Global setting field name",
        ExplanationText = "Name of the Global Location setting field to display, e.g. GlobalLocationsTitle or GlobalLocationsDescription.",
        Order = 1)]
    public string FieldName { get; set; } = string.Empty;
}
