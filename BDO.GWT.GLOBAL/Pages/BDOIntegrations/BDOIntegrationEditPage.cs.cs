using BDO;
using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.GlobalSettings;
using GWTXbyKWeb.Pages.BDOIntegrations;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using System.Threading.Tasks;

[assembly: UIPage(
    parentType: typeof(BDOIntegrationsEditSection),
    slug: "edit",
    uiPageType: typeof(BDOIntegrationEditPage),
    name: "Edit Form Submission subscription",
    templateName: TemplateNames.EDIT,
    order: 0)]
namespace GWTXbyKWeb.Pages.BDOIntegrations
{
    public class BDOIntegrationEditPage : InfoEditPage<FormSubmissionSubscriptionsInfo>
    {
        [PageParameter(typeof(IntPageModelBinder))]
        public override int ObjectId { get; set; }

        public BDOIntegrationEditPage(IFormComponentMapper formComponentMapper,
            IFormDataBinder formDataBinder)
            : base(formComponentMapper, formDataBinder)
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
