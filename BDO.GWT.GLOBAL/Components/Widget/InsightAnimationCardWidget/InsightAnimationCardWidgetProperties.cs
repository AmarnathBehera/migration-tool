
using BDO.GWT.Web.Widgets;
using CMS.ContentEngine;
using Kentico.Forms.Web.Mvc;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Websites.FormAnnotations;
using System.Collections.Generic;
using System.Linq;


namespace BDO.GWT.GLOBAL.Widgets.InsightAnimationCardWidget;

public class InsightAnimationCardWidgetProperties : BaseWidgetProperties
{
    [TextInputComponent( Label = "Title", ExplanationText = "Title that will be shown over insight carousel widget.", Order = 0)]
    public string? Title { get; set; }

    [TextInputComponent( Label = "Description", ExplanationText = "Description that will be shown over insight carousel widget but under title.", Order = 1)]
    public string? Description { get; set; }

    [ContentItemSelectorComponent (contentTypeName:InsightAnimationPage.CONTENT_TYPE_NAME, Label = "Insight Animation Cards", ExplanationText = "Insights that will be added as individual cards.", Order = 2)]
    public IEnumerable<ContentItemReference> InsightCards { get; set; } = Enumerable.Empty<ContentItemReference>();

    [DropDownComponent( Label = "See all link Target", Order = 3,Options = "_self;Same tab\r\n_blank;New tab")]
    public string openInNewTab { get; set; } = "_self";

    [TextInputComponent( Label = "See all link text", Order = 6)]
    public string? SeeAllLinkText { get; set; }

    [UrlSelectorComponent( Label = "See all link URL", Order = 7)]
    public string? SeeAllLinkURL { get; set; }

    [DropDownComponent( Label = "Top margin", Order = 8,Options = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string TopMargin { get; set; } = null!;

    [DropDownComponent( Label = "Bottom margin", Order = 9,Options = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string BottomMargin { get; set; } = null!;



}

