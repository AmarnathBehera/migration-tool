using Kentico.Components.Web.Mvc.FormComponents;
using Kentico.Forms.Web.Mvc;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BDO.GWT.Web.Widgets.AccordionWidget;

public class AccordionWidgetProperties : BaseWidgetProperties
{
    [TextInputComponent(Label = "Accordion heading", Order = 0)]
    public string Heading { get; set; } = null!;

    [RichTextEditorComponent(Order = 1,Label = "Accordion content")]
    public string Content { get; set; } = null!;

    [TextInputComponent(Label = "Deep link ID", Order = 2)]
    public string? DeepLinkID { get; set; }

    [DropDownComponent(Label = "Top margin", Order = 4, Options = "0px;None\n20px\n40px\n60px\n80px")]
    public string? TopMargin { get; set; }

    [DropDownComponent(Label = "Bottom margin", Order = 5, Options = "0px;None\n20px\n40px\n60px\n80px")]
    public string? BottomMargin { get; set; }
}
