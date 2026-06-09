#nullable enable

using CMS.ContentEngine;
using Kentico.Forms.Web.Mvc;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using System.Collections.Generic;

namespace BDO.GWT.Web.Widgets.TabAccordionWidget;

public class TabAccordionWidgetProperties : BaseWidgetProperties
{
    [TextInputComponent(Label = "Title", ExplanationText = "Title that will be shown over tab / accordion widget.", Order = 0)]
    public string? Title { get; set; }

    [RadioGroupComponent(Order = 1, Label = "Layout", Options = "contentTab;Tab\r\naccordion;Accordion")]
    public string Layout { get; set; } = "contentTab";

    [DropDownComponent(Label = "Page type", ExplanationText = "Type of pages which will be shown as tabs or accordion items.", Order = 2, Options = "Industry\r\nService\r\nMicrosite")]
    public string PageType { get; set; } = "Industry";

    [ContentItemSelectorComponent(contentTypeNames:[SectionServices.CONTENT_TYPE_NAME,Service.CONTENT_TYPE_NAME,BusinessLine.CONTENT_TYPE_NAME,ServiceArea.CONTENT_TYPE_NAME,Sub_Service.CONTENT_TYPE_NAME], Order =3, Label = "Tab / accordion items", ExplanationText = "For tab layout recommendation is to have maximum 4 items as it works best from responsiveness perspective.")]
    [VisibleIfEqualTo(nameof(PageType),"Service")]
    public IEnumerable<ContentItemReference> ServicePages { get; set; } = new List<ContentItemReference>();

    [ContentItemSelectorComponent(contentTypeNames: [SectionIndustries.CONTENT_TYPE_NAME,Industry.CONTENT_TYPE_NAME,IndustryCategory.CONTENT_TYPE_NAME,IndustryService.CONTENT_TYPE_NAME], Order =3, Label = "Tab / accordion items", ExplanationText = "For tab layout recommendation is to have maximum 4 items as it works best from responsiveness perspective.")]
    [VisibleIfEqualTo(nameof(PageType), "Industry")]
    public IEnumerable<ContentItemReference> IndustryPages { get; set; } = new List<ContentItemReference>();

    [ContentItemSelectorComponent(contentTypeNames: [SectionMicrosites.CONTENT_TYPE_NAME,MicrositeMasterPage.CONTENT_TYPE_NAME,MicrositePage.CONTENT_TYPE_NAME], Order = 3, Label = "Tab / accordion items", ExplanationText = "For tab layout recommendation is to have maximum 4 items as it works best from responsiveness perspective.")]
    [VisibleIfEqualTo(nameof(PageType), "Microsite")]
    public IEnumerable<ContentItemReference> MicrositePages { get; set; } = new List<ContentItemReference>();


    [DropDownComponent(Label = "Top margin", Order = 4, Options = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string TopMargin { get; set; } = null!;

    [DropDownComponent(Label = "Bottom margin", Order = 5, Options = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string BottomMargin { get; set; } = null!;
}
