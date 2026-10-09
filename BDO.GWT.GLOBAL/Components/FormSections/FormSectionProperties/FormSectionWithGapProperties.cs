using Kentico.Xperience.Admin.Base.FormAnnotations;

namespace BDO.GWT.Xperience.Core.LiveSite.Components.FormSections.FormSectionProperties;

public class FormSectionWithGapProperties : BaseFormSectionProperties
{
    [DropDownComponent(
        Label = "Gap between sections",
        Order = 1,
        Options = "gap--none;None\r\ngap--xs;Extra small\r\ngap--sm;Small\r\ngap--md;Medium\r\ngap--lg;Large\r\ngap--xl;Extra large"
    )]
    public string SectionsGap { get; set; } = "gap--none";
}