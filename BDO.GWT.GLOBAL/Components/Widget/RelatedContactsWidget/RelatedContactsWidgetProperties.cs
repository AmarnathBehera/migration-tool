
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Websites.FormAnnotations;

namespace BDO.GWT.Web.Widgets.RelatedContactsWidget;

public class RelatedContactsWidgetProperties : BaseWidgetProperties
{
    [TextInputComponent(Label = "Title", Order = 0)]
    public string Title { get; set; }

    [TextInputComponent(Label = "Description", Order = 1)]
    public string Description { get; set; }
  
    [DropDownComponent(Label = "Order contacts by", Order = 2, Options = "firstName;First name\r\nlastName;Last name\r\njobTitle;Job title")]
    public string OrderCriteria { get; set; }

    [CheckBoxComponent(Label = "Use descending alphabetical order", Order = 3)]
    public bool IsDescendingOrder { get; set; } = false;

    [CheckBoxComponent(Label = "Show office", Order = 4, ExplanationText = "Location can be shown for individuals from the Our People section.")]
    public bool ShowOffice { get; set; } = false;

    [TextInputComponent(Label = "See all link text", Order = 5)]
    public string SeeAllLinkText { get; set; }

    [UrlSelectorComponent(Label = "See all link URL", Order = 6)]
    public string SeeAllLinkURL { get; set; }

    [DropDownComponent(Label = "See all link URL target", Order = 7, Options = "_self;Same tab\n_blank;New tab")]
    public string SeeAllLinkURLTarget { get; set; } = "_self";

    [DropDownComponent(Label = "Top margin", Order = 8, Options = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]

    public string TopMargin { get; set; } = "0px";

    [DropDownComponent(Label = "Bottom margin", Order = 9, Options = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string BottomMargin { get; set; } = "0px";
}
