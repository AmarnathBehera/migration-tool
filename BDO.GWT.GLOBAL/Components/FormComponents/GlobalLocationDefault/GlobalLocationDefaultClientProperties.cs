using System.Collections.Generic;
using Kentico.Xperience.Admin.Base.Forms;

namespace BDO.GWT.GLOBAL.Components.FormComponents.GlobalLocationDefault;

public class GlobalLocationDefaultOption
{
    public string Value { get; set; } = "";
    public string Text { get; set; } = "";
}

public class GlobalLocationDefaultClientProperties : FormComponentClientProperties<string>
{
    public string Options { get; set; } = "";
    public string FieldLabel { get; set; }
}