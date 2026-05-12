using BDO.GWT.Xperience.Core.LiveSite.Components.FormComponents;
using Kentico.Forms.Web.Mvc;


namespace BDO.GWT.Xperience.Core.LiveSite.Components.FormComponents;

public class TextBoxReload : Kentico.Forms.Web.Mvc.FormComponent<TextBoxReloadProperties, string>
{
    public const string IDENTIFIER = "BDO.FormComponent.TextBoxReload";

    [BindableProperty]
    public string Value { get; set; }

    public override string GetValue() => Value ?? string.Empty;

    public override void SetValue(string value) => Value = value;
}