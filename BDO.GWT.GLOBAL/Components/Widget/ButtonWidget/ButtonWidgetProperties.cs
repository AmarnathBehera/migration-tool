using Kentico.Components.Web.Mvc.FormComponents;
using Kentico.Forms.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Microsoft.Extensions.Options;

namespace BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.ButtonWidget;

public class ButtonWidgetProperties : IWidgetProperties
{

    [TextInputComponent(Label = "Button text", ExplanationText = "Button text", Order = 0)]
    public string? Text { get; set; } = null!;

    [TextInputComponent(Label = "Button link", Order = 1)]
    [UrlValidationRule(AllowRelativeUrl = true, AllowFragmentUrl = true)]
    [ExcludeFromAiraTranslation]
    public string? Link { get; set; }

    [DropDownComponent(Label = "Link targe", Order = 2, Options = "_self;Same tab\n_blank;New tab")]
    public string Target { get; set; } = "_self";

    [RadioGroupComponent(Label = "Button type", Order = 3,Options= "primary;Primary\\r\\nsecondary;Secondary\\r\\ntertiary;Tertiary\"")]
    public string Type { get; set; } = "primary";

   
    [RadioGroupComponent(Label = "Light / dark theme", Order = 4, Options= "light;Light theme\r\ndark;Dark theme")]
    public string Theme { get; set; } = "light";

    [RadioGroupComponent(Label = "Button side", Order = 5,Options = "left;Left\r\nright;Right\r\ncenter;Center")]
    public string ButtonSide { get; set; } = "left";

    [DropDownComponent(Label = "Top margin", Order = 6, Options = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string TopMargin { get; set; } = "0px";

    [DropDownComponent(Label = "Bottom margin", Order = 7 ,Options = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string BottomMargin { get; set; } = "0px";
}
