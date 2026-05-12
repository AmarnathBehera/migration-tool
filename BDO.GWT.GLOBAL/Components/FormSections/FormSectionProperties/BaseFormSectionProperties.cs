using Kentico.Forms.Web.Mvc;
using Kentico.Xperience.Admin.Base.FormAnnotations;

namespace BDO.GWT.Xperience.Core.LiveSite.Components.FormSections.FormSectionProperties;

public class BaseFormSectionProperties : IFormSectionProperties
{
    [DropDownComponent(
        Label = "Container width",
        Order = 0,
        Options = "section-container;Default\r\nsection-container--md;Medium\r\nsection-container--lg;Large\r\nsection-container--xl;Extra large\r\nsection-container--xxl;2 Extra large\r\nsection-container--max;Full width"
    )]
    public string ContainerPadding { get; set; } = "section-container";
}