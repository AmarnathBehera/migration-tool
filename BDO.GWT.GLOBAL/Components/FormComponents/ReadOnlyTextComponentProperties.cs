using Kentico.Forms.Web.Mvc;

namespace BDO.GWT.Xperience.Core.LiveSite.Components.FormComponents;

public class ReadOnlyTextComponentProperties : FormComponentProperties<string?>
{
    public ReadOnlyTextComponentProperties() : base("longtext", -1, -1)
    {
    }

    [EditingComponent(TextInputComponent.IDENTIFIER, Order = 0)]
    public bool Multiline { get; set; }
    
    [DefaultValueEditingComponent("ReadOnlyTextComponent")]
    public override string? DefaultValue { get; set; }
}
