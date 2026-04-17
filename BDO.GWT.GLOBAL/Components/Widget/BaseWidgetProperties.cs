using Kentico.Forms.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using System.ComponentModel.DataAnnotations;

namespace BDO.GWT.Web.Widgets;

public abstract class BaseWidgetProperties : IWidgetProperties
{
    [TextInputComponent(Label = "Widget ID", ExplanationText = "ID should be unique from any other ID on the page. If the ID contains multiple words, words must be joined with a '-' (e.g. three-word-id). No other special characters and no spaces.'", Order = 999)]
        [RegularExpression(
        @"^[a-z0-9'-]*$",
        ErrorMessage = "Characters are not allowed.")]
    public string? WidgetID { get; set; }
}
