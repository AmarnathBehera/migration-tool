using Kentico.Components.Web.Mvc.FormComponents;
using Kentico.Forms.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;

namespace BDO.GWT.GLOBAL.Widgets;
public class ViewGlobalLocationsWidgetProperties : IWidgetProperties
{
    [EditingComponent(TextInputComponent.IDENTIFIER, Order = 0, Label = "Title")]
    public string? Title { get; set; }
    
    [EditingComponent(TextInputComponent.IDENTIFIER, Order = 1, Label = "Description")]
    public string? Description { get; set; }

    [EditingComponent(RadioButtonsComponent.IDENTIFIER, Order = 2, Label = "Light / Dark theme")]
    [EditingComponentProperty(nameof(RadioButtonsProperties.DataSource), "dark;Dark theme\r\nlight;Light theme")]
    public string Theme { get; set; } = "dark";

    [EditingComponent(RadioButtonsComponent.IDENTIFIER, Order = 3, Label = "Layout")]
    [EditingComponentProperty(nameof(RadioButtonsProperties.DataSource), "col-1;One column\r\ncol-1-shadowed;One column shadowed")]
    public string Layout { get; set; } = "col-1";

    #region button

    [EditingComponent(TextInputComponent.IDENTIFIER, Order = 4, Label = "Button text")]
    public string? ButtonText { get; set; }

    [EditingComponent(UrlSelector.IDENTIFIER, Order = 5, Label = "Button URL", ExplanationText = "Please ensure that the provided URL directs to the official BDO Global locations page. This page contains information about BDO offices worldwide.")]
    public string? ButtonUrl { get; set; }

    [EditingComponent(DropDownComponent.IDENTIFIER, Order = 6, Label = "Button link target")]
    [EditingComponentProperty(nameof(DropDownProperties.DataSource), "_self;Same tab\r\n_blank;New tab")]
    public string ButtonLinkTarget { get; set; } = "_self";

    #endregion

    [EditingComponent(DropDownComponent.IDENTIFIER, Label = "Top margin", Order = 7)]
    [EditingComponentProperty(nameof(DropDownProperties.DataSource), "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string TopMargin { get; set; } = "0px";

    [EditingComponent(DropDownComponent.IDENTIFIER, Label = "Bottom margin", Order = 8)]
    [EditingComponentProperty(nameof(DropDownProperties.DataSource), "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string BottomMargin { get; set; } = "0px";

    [EditingComponent(TextInputComponent.IDENTIFIER, Label = "Widget ID", ExplanationText = "ID should be unique from any other ID on the page. If the ID contains multiple words, words must be joined with a '-' (e.g. three-word-id). No other special characters and no spaces.'", Order = 999)]
    public string WidgetID { get; set; }



}
