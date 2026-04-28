using System.Collections.Generic;
using Kentico.Xperience.Admin.Base.Forms;

namespace BDO.GWT.GLOBAL.Components.FormComponents.CustomDropdown;

public class CustomDropdownOption
{
    public string Value { get; set; } = "";
    public string Text { get; set; } = "";
}

public class CustomDropdownClientProperties : FormComponentClientProperties<string>
{
    public IEnumerable<CustomDropdownOption> Options { get; set; } = [];
    public string FieldLabel { get; set; }
}