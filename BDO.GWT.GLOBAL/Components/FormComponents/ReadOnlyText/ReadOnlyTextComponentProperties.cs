using Kentico.Forms.Web.Mvc;
using Kentico.Xperience.Admin.Base.FormAnnotations;

namespace BDO.GWT.Xperience.Core.LiveSite.Components.FormComponents;

public class ReadOnlyTextComponentProperties : FormComponentProperties<string?>
{
    public ReadOnlyTextComponentProperties() : base("longtext", -1, -1)
    {
    }

    // ✅ bool property should use CheckBoxComponent, not TextInputComponent
    [CheckBoxComponent(Label = "Multiline", Order = 0)]
    public bool Multiline { get; set; }

    // ✅ Match nullable string? to match base class FormComponentProperties<string?>
    [TextInputComponent(Label = "Default value", Order = EditingComponentOrder.DEFAULT_VALUE)]
    public override string DefaultValue { get; set; }
}