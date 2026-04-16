using Kentico.Components.Web.Mvc.FormComponents;
using Kentico.Forms.Web.Mvc;

namespace BDO.GWT.Web.Widgets.AccordionWidget;

public class AccordionWidgetProperties : BaseWidgetProperties
{
    [EditingComponent(TextInputComponent.IDENTIFIER, Order = 0, Label = "Accordion heading")]
    public string Heading { get; set; } = null!;

    [EditingComponent(RichTextComponent.IDENTIFIER, Order = 1, Label = "Accordion content")]
    public string Content { get; set; } = null!;

    [EditingComponent(TextInputComponent.IDENTIFIER, Order = 2, Label = "Deep link ID")]
    public string? DeepLinkID { get; set; }

    [EditingComponent(DropDownComponent.IDENTIFIER, Label = "Top margin", Order = 3)]
    [EditingComponentProperty(nameof(DropDownProperties.DataSource), "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string TopMargin { get; set; } = null!;

    [EditingComponent(DropDownComponent.IDENTIFIER, Label = "Bottom margin", Order = 4)]
    [EditingComponentProperty(nameof(DropDownProperties.DataSource), "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string BottomMargin { get; set; } = null!;
}
