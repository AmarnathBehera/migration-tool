using BDO;
using GWTXbyKWeb.Pages.BDOIntegrations;
using Kentico.Xperience.Admin.Base;

[assembly: UIPage(
    parentType: typeof(BdoIntegrationListingPage),
    slug: PageParameterConstants.PARAMETERIZED_SLUG,
    uiPageType: typeof(BdoIntegrationsEditSection),
    name: "Edit",
    templateName: TemplateNames.SECTION_LAYOUT,
    order: 10)]
namespace GWTXbyKWeb.Pages.BDOIntegrations
{
    public class BdoIntegrationsEditSection : EditSectionPage<FormSubmissionSubscriptionsInfo>
    { }
}