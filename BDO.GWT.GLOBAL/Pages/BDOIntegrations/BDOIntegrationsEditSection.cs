using BDO;
using GWTXbyKWeb.Pages.BDOIntegrations;
using Kentico.Xperience.Admin.Base;

[assembly: UIPage(
    parentType: typeof(BDOIntegrationListingPage),
    slug: PageParameterConstants.PARAMETERIZED_SLUG,
    uiPageType: typeof(BDOIntegrationsEditSection),
    name: "Edit",
    templateName: TemplateNames.SECTION_LAYOUT,
    order: 10)]
namespace GWTXbyKWeb.Pages.BDOIntegrations
{
    public class BDOIntegrationsEditSection : EditSectionPage<FormSubmissionSubscriptionsInfo>
    { }
}