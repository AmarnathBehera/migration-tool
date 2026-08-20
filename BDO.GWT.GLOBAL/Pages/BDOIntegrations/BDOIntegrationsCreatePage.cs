using BDO;
using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.GlobalSettings;
using GWTXbyKWeb.Pages.BDOIntegrations;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using System.Threading.Tasks;

[assembly: UIPage(
    parentType: typeof(BdoIntegrationListingPage),
    slug: "create",
    uiPageType: typeof(BdoIntegrationsCreatePage),
    name: "Create Form Submission Integration",
    templateName: TemplateNames.EDIT,
    order: 20)]
namespace GWTXbyKWeb.Pages.BDOIntegrations
{
    public class BdoIntegrationsCreatePage : CreatePage<FormSubmissionSubscriptionsInfo, BdoIntegrationsEditSection>
    {
        public BdoIntegrationsCreatePage(IFormComponentMapper formComponentMapper,
            IFormDataBinder formDataBinder,
            IPageLinkGenerator pageLinkGenerator)
            : base(formComponentMapper, formDataBinder, pageLinkGenerator)
        {
        }

        public override Task ConfigurePage()
        {
            // Assign your 'GlobalSettingsKeyEdit' UI form code name to the page. The assignment is case-insensitive.
            PageConfiguration.UIFormName = "FormSubmissionSubscriptions";
            return base.ConfigurePage();
        }
    }
}