using Kentico.Forms.Web.Mvc;

namespace BDO.GWT.Xperience.Core.LiveSite.Components.FormComponents;

public class ReadOnlyTextComponent : FormComponent<ReadOnlyTextComponentProperties, string?>
{
    public const string IDENTIFIER = "ReadOnlyTextComponent";
    
    [BindableProperty]
    public string? Value { get; set; }

    public override string? GetValue() => Value;

    public override void SetValue(string? value) => Value = value;
}
