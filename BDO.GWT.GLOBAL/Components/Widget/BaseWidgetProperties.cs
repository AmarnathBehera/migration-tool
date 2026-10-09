using Kentico.Forms.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Xperience.Admin.Base.FormAnnotations;


namespace BDO.GWT.Web.Widgets;

public abstract class BaseWidgetProperties : IWidgetProperties
{
    [TextInputComponent(Label = "Widget ID", ExplanationText = "ID should be unique from any other ID on the page. If the ID contains multiple words, words must be joined with a '-' (e.g. three-word-id). No other special characters and no spaces.'", Order = 999)]
    [WidgetIdValidationRule]
    public string WidgetID { get; set; }
}
