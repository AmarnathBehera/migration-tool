using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using System.Threading.Tasks;

namespace BDO.GWT.Xperience.Admin.Pages.ProjectSettings.GlobalSettings;

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
        PageConfiguration.UIFormName = "globalsettingskeyedit";
        return base.ConfigurePage();
    }
}
