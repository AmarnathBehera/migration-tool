using BDO;
using BDO.GWT.Xperience;
using CMS.Membership;
using GWTXbyKWeb.Pages.BDOIntegrations;
using Kentico.Xperience.Admin.Base;
using System.Threading.Tasks;
[assembly: UIPage(
    parentType: typeof(BdoIntegrationsApplication),
    slug: "BDO-Integrations",
    uiPageType: typeof(BdoIntegrationListingPage),
    name: "BDO Integrations",
    templateName: TemplateNames.LISTING,
    order: 0)]

namespace GWTXbyKWeb.Pages.BDOIntegrations
{

    public class BdoIntegrationListingPage : ListingPage
    {
        protected override string ObjectType => FormSubmissionSubscriptionsInfo.OBJECT_TYPE;

        public override async Task ConfigurePage()
        {
            PageConfiguration.ColumnConfigurations
                        .AddColumn(nameof(FormSubmissionSubscriptionsInfo.Displayname), "Display name")
                        .AddColumn(nameof(FormSubmissionSubscriptionsInfo.Codename), "Code name")
                        .AddColumn(nameof(FormSubmissionSubscriptionsInfo.Form), "Form")
                        .AddColumn(nameof(FormSubmissionSubscriptionsInfo.Method), "Method")
                        .AddColumn(nameof(FormSubmissionSubscriptionsInfo.Endpoint), "Endpoint");

            // Add a "New settings" button at the top. This will navigate the user to a "create" page.
            // We will define it in the next section.
            PageConfiguration.HeaderActions.AddLink<BdoIntegrationsCreatePage>("New setting");

            // Add an "Edit" row action. This will navigate the user to a "edit" section for a specific instance of GlobalSettingsKeyInfo object.
            // We will define it in the next section.
            PageConfiguration.AddEditRowAction<BdoIntegrationsEditSection>();

            // Define a "Delete" action for the user with sufficient permissions to remove an existing setting.
            PageConfiguration.TableActions
                .AddDeleteAction(nameof(Delete));

            await base.ConfigurePage();
        }

        // Ensure only a user with permissions to delete can perform the delete action.
        [PageCommand(Permission = SystemPermissions.DELETE)]
        public override Task<ICommandResponse<RowActionResult>> Delete(int id) => base.Delete(id);
    }
}