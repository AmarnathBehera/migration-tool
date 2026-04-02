using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using System.Threading.Tasks;

namespace BDO.GWT.Xperience.Admin.Pages.ProjectSettings.GlobalSettings;

public class GlobalSettingsEditPage : InfoEditPage<GlobalsettingskeyInfo>
{
    [PageParameter(typeof(IntPageModelBinder))]
    public override int ObjectId { get; set; }

    public GlobalSettingsEditPage(IFormComponentMapper formComponentMapper,
        IFormDataBinder formDataBinder)
        : base(formComponentMapper, formDataBinder)
    {
    }

    public override Task ConfigurePage()
    {
        // Assign your 'GlobalSettingsKeyEdit' UI form code name to the page. The assignment is case-insensitive.
        PageConfiguration.UIFormName = "globalsettingskeyedit";
        return base.ConfigurePage();
    }
}
