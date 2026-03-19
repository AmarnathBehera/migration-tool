using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using System.Threading.Tasks;

namespace BDO.GWT.Xperience.Admin.Pages.ProjectSettings.GlobalSettings;

// Specify the entity which this page will create (GlobalSettingsKeyInfo)
// and the page to redirect to after the entity has been cerated (GlobalSettingsEdit page)
public class GlobalSettingsCreatePage : CreatePage<GlobalsettingskeyInfo, GlobalSettingsEditPage>
{
    public GlobalSettingsCreatePage(IFormComponentMapper formComponentMapper,
        IFormDataBinder formDataBinder,
        IPageLinkGenerator pageLinkGenerator)
        : base(formComponentMapper, formDataBinder, pageLinkGenerator)
    {
    }

    public override Task ConfigurePage()
    {
        // Assign your 'GlobalSettingsKeyEdit' UI form code name to the page. The assignment is case-insensitive.
        PageConfiguration.UIFormName = "globalsettingskeyedit";
        return base.ConfigurePage();
    }
}
