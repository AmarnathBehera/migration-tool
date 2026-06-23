using Kentico.Forms.Web.Mvc;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using System.ComponentModel.DataAnnotations;


namespace BDO.GWT.Web.Widgets.SocialMediaButtonsWidget;

public class SocialMediaButtonsWidgetProperties : BaseWidgetProperties
{
    [DropDownComponent(Label = "Top margin", Order = 0, Options = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string TopMargin { get; set; } = null!;

    [DropDownComponent(Label = "Bottom margin", Order = 1, Options = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string BottomMargin { get; set; } = null!;


}
