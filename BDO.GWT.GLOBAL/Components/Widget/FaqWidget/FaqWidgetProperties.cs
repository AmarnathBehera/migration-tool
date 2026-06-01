using CMS.ContentEngine;
using Kentico.Forms.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using System.Collections.Generic;

namespace BDO.GWT.Web.Widgets.FaqWidget;

public class FaqWidgetProperties : IWidgetProperties
{
    [TextInputComponent(Label = "Title", ExplanationText = "Title that will be shown over FAQ widget.", Order = 0)]
    public string Title { get; set; }

    [RadioGroupComponent(Label = "Layout", Order = 1, Options = "cards;Cards\r\nlist;List\r\naccordion;Accordion")]
    public string Layout { get; set; } = "cards";

    [ContentItemSelectorComponent(new string[] { FaqFolder.CONTENT_TYPE_NAME, FaqTopic.CONTENT_TYPE_NAME, FaqPage.CONTENT_TYPE_NAME }, Label = "FAQ items", Order = 2, ExplanationText = "Up to 12 items will be displayed.")]
    public IEnumerable<ContentItemReference> Pages { get; set; } = new List<ContentItemReference>();

    [RadioGroupComponent(Label = "Hover colour", Order = 3, Options = "slate;Slate\r\nred;Red", ExplanationText = "Note: Content appearance may be affected by the hover colour. For rich text content, we recommend using the \"accordion\" layout above.")]
    public string HoverColour { get; set; } = "slate";

    [DropDownComponent(Label = "Top margin", Order = 4, Options = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string TopMargin { get; set; } = "0px";

    [DropDownComponent(Label = "Bottom margin", Order = 5, Options = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string BottomMargin { get; set; } = "0px";

    [TextInputComponent(Label = "Widget ID", Order = 18, ExplanationText = "ID should be unique from any other ID on the page. If the ID contains multiple words, words must be joined with a '-' (e.g. three-word-id). No other special characters and no spaces.'")]
    public string WidgetID { get; set; }
}
