
using Kentico.Forms.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;
using System.ComponentModel.DataAnnotations;

namespace BDO.GWT.GLOBAL.Components.Widget.KeyTakeawaysWidget;

public class KeyTakeawaysWidgetProperties : IWidgetProperties
{
    [TextInputComponent( Label = "Title", Order = 0)]
    public string Title { get; set; }

    [RichTextEditorComponent( Label = "Content", Order = 1)]

    public string Content { get; set; }

    [RadioGroupComponent( Order = 2, Label = "Background colour",Options = "white;White\r\npale-charcoal;Pale Charcoal\r\nslate;Slate")]
    public string BackgroundColour { get; set; }

    [RadioGroupComponent( Order = 3, Label = "Content alignment",Options = "left;Left\r\nright;Right\r\ncenter;Center")]
    public string ContentAlignment { get; set; } = "left";

    [DropDownComponent( Label = "Content width", Order = 4,Options = "section-container;Default\r\nsection-container--md;Medium\r\nsection-container--lg;Large\r\nsection-container--xl;Extra Large\r\nsection-container--xxl;2 Extra Large\r\nsection-container--max;Full Width")]
    public string ContentWidth { get; set; } = "section-container";

    [DropDownComponent( Label = "Top margin", Order = 5,Options = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]

    public string TopMargin { get; set; } = "0px";

    [DropDownComponent( Label = "Bottom margin", Order = 6,Options = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string BottomMargin { get; set; } = "0px";

}
