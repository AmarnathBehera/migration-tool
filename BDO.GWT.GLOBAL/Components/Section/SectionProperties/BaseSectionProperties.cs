using BDO.GWT.Xperience.LiveSite.Components.Section;
using Kentico.Forms.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;

namespace BDO.GWT.Xperience.LiveSite.Components.Section;

public class BaseSectionProperties : ISectionProperties
{
    [EditingComponent(DropDownComponent.IDENTIFIER, Label = "Container width", Order = 0)]
    [EditingComponentProperty(nameof(DropDownProperties.DataSource), "section-container;Default\r\nsection-container--md;Medium\r\nsection-container--lg;Large\r\nsection-container--xl;Extra Large\r\nsection-container--xxl;2 Extra Large\r\nsection-container--max;Full Width")]
    public string ContainerPadding { get; set; } = "section-container";
}