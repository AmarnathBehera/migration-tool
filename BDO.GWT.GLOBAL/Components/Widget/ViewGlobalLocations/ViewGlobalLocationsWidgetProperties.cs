using BDO.GWT.Web.Widgets;
using Kentico.Forms.Web.Mvc;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;

namespace BDO.GWT.GLOBAL.Widgets;

public class ViewGlobalLocationsWidgetProperties : BaseWidgetProperties
{
    [TextInputComponent(Label = "Title", ExplanationText = "This field is read-only. The title displayed for the Global Locations widget is automatically populated from the resource string 'BDO.LiveSite.ViewGlobalLocations.Title'.", EditMode = FormEditMode.Disabled, Order = 0)]
    public string Title { get; set; }

    [TextAreaComponent(Label = "Description", ExplanationText = "This field is read-only. The description displayed for the Global Locations widget is automatically populated from the resource string 'BDO.LiveSite.ViewGlobalLocations.Description'.", EditMode = FormEditMode.Disabled, Order = 1)]
    public string Description { get; set; }

    [RadioGroupComponent(Label = "Light / Dark theme", Options = "dark;Dark theme\r\nlight;Light theme", Order = 2)]
    public string Theme { get; set; } = "dark";

    [RadioGroupComponent(Label = "Layout", Options = "col-1;One column\r\ncol-1-shadowed;One column shadowed", Order = 3)]
    public string Layout { get; set; } = "col-1";

    #region button

    [TextInputComponent(Label = "Button text", ExplanationText = "This field is read-only. The text displayed on the button is automatically populated from the resource string 'BDO.LiveSite.ViewGlobalLocations.Button'.", EditMode = FormEditMode.Disabled, Order = 4)]
    public string ButtonText { get; set; }

    [TextInputComponent(Label = "Button URL", ExplanationText = "Please ensure that the provided URL directs to the official BDO Global Locations page. This page contains information regarding BDO offices worldwide.", Order = 5)]
    public string ButtonUrl { get; set; }

    [DropDownComponent(Label = "Button link target", Options = "_self;Same tab\r\n_blank;New tab", Order = 6)]
    public string ButtonLinkTarget { get; set; } = "_self";

    #endregion

    [DropDownComponent(Label = "Top margin", Options = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px", Order = 7)]
    public string TopMargin { get; set; } = "0px";

    [DropDownComponent(Label = "Bottom margin", Options = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px", Order = 8)]
    public string BottomMargin { get; set; } = "0px";
}