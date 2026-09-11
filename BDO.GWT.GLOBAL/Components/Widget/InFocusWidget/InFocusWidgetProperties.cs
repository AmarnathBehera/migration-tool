using BDO.GWT.Web.Widgets;
using CMS.ContentEngine;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace BDO.GWT.GLOBAL.Widgets;

public class InFocusWidgetProperties : BaseWidgetProperties
{
    [TextInputComponent(Label = "Title", ExplanationText = "Title that will be shown over in focus widget.", Order = 0)]
    public string Title { get; set; }

    [TextInputComponent(Label = "Subtitle", ExplanationText = "Subtitle that will be shown over in focus widget.", Order = 1)]
    public string Subtitle { get; set; }

    [ContentItemSelectorComponent(
        ContentAttachment.CONTENT_TYPE_NAME,
        Label = "Image",
        ExplanationText = "Recommended size: 720x1280px",
        Order = 2,
        MaximumItems = 1)]
    public IEnumerable<ContentItemReference> ImageURL { get; set; } = Enumerable.Empty<ContentItemReference>();

    [TextInputComponent(Label = "Image alt text", Order = 3)]
    public string ImageAltText { get; set; } = "";

    [ContentItemSelectorComponent(
        InFocusPage.CONTENT_TYPE_NAME,
        Label = "In focus items",
        ExplanationText = "Recommended menu tabs number 3 - 6.",
        Order = 4)]
    public IEnumerable<ContentItemReference> Pages { get; set; } = Enumerable.Empty<ContentItemReference>();

    [DropDownComponent(Label = "Top margin", Order = 5, Options = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string TopMargin { get; set; } = "0px";

    [DropDownComponent(Label = "Bottom margin", Order = 6, Options = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string BottomMargin { get; set; } = "0px";


}