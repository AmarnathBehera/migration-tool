
using BDO;
using BDO.GWT.Xperience.LiveSite.Components.Section;

using Kentico.Forms.Web.Mvc;

using Kentico.PageBuilder.Web.Mvc;

namespace BDO.GWT.Xperience.LiveSite.Components.Section;

public class SectionWithGapProperties : BaseSectionProperties

{

    [EditingComponent(DropDownComponent.IDENTIFIER, Label = "Gap between sections", Order = 1)]
    [EditingComponentProperty(nameof(DropDownProperties.DataSource), "gap--none;None\r\ngap--xs;Extra small\r\ngap--sm;Small\r\ngap--md;Medium\r\ngap--lg;Large\r\ngap--xl;Extra large")]

    public string SectionsGap { get; set; } = "gap--none";

}
