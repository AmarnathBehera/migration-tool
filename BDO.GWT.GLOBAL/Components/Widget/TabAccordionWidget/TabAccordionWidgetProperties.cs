
using CMS.ContentEngine;
using CMS.Websites;
using Kentico.Forms.Web.Mvc;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Websites.FormAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Widgets.TabAccordionWidget;

public class TabAccordionWidgetProperties : BaseWidgetProperties
{
    [TextInputComponent(Label = "Title", ExplanationText = "Title that will be shown over tab / accordion widget.", Order = 0)]
    public string? Title { get; set; }

    [RadioGroupComponent(Order = 1, Label = "Layout", Options = "contentTab;Tab\r\naccordion;Accordion")]
    public string Layout { get; set; } = "contentTab";

    [DropDownComponent(Label = "Page type", ExplanationText = "Type of pages which will be shown as tabs or accordion items.", Order = 2, Options = "Industry\r\nService\r\nMicrosite")]
    public string PageType { get; set; } = "Industry";

    [WebPageSelectorComponent(TreePath = "/services", MaximumPages = 0,Order =3)]
    [VisibleIfEqualTo(nameof(PageType),"Service")]
    public IEnumerable<WebPageRelatedItem> ServicePages { get; set; } = new List<WebPageRelatedItem>();

    [WebPageSelectorComponent(TreePath = "/industry", MaximumPages = 0,Order =3)]
    [VisibleIfEqualTo(nameof(PageType), "Industry")]
    public IEnumerable<WebPageRelatedItem> IndustryPages { get; set; } = new List<WebPageRelatedItem>();

    [WebPageSelectorComponent(TreePath = "/microsites", MaximumPages = 0, Order = 3)]
    [VisibleIfEqualTo(nameof(PageType), "Microsite")]
    public IEnumerable<WebPageRelatedItem> MicrositePages { get; set; } = new List<WebPageRelatedItem>();


    [DropDownComponent(Label = "Top margin", Order = 4, Options = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string TopMargin { get; set; } = null!;

    [DropDownComponent(Label = "Bottom margin", Order = 5, Options = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string BottomMargin { get; set; } = null!;



}
