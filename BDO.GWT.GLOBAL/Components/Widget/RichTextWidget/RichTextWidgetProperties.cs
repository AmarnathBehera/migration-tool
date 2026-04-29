using Kentico.Forms.Web.Mvc;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace BDO.GWT.Web.Widgets.RichTextWidget;

public class RichTextWidgetProperties : Kentico.Components.Web.Mvc.Widgets.Internal.RichTextWidgetProperties
{
    [TextInputComponent(Label = "WidgetID", ExplanationText = "ID should be unique from any other ID on the page. If the ID contains multiple words, words must be joined with a '-' (e.g. three-word-id). No other special characters and no spaces.'", Order = 1)]
    [RegularExpression(
    @"^[a-z0-9'-]*$",
    ErrorMessage = "Characters are not allowed.")]
    public string WidgetID { get; set; }

    [DropDownComponent(Label = "Top margin", Order = 2, Options = "0px;None\n20px\n40px\n60px\n80px")]
    public string TopMargin { get; set; }

    [DropDownComponent(Label = "Bottom margin", Order = 3, Options = "0px;None\n20px\n40px\n60px\n80px")]
    public string BottomMargin { get; set; }
}
