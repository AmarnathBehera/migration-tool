using CMS.Membership;
using Kentico.Xperience.Admin.Base;
using System.Threading.Tasks;

namespace BDO.GWT.Xperience.Admin.Pages.ProjectSettings.GlobalSettings;

// The class has to inherit from ListingPage.
public class GlobalSettingsListingPage : ListingPage
{
    protected override string ObjectType => GlobalsettingskeyInfo.OBJECT_TYPE;

    public override async Task ConfigurePage()
    {
        // Determine which columns of your GlobalSettingsKeyInfo objects to show the user in the list view.
        PageConfiguration.ColumnConfigurations
                    .AddColumn(nameof(GlobalsettingskeyInfo.GlobalSettingsKeyDisplayName), "Display name")
                    .AddColumn(nameof(GlobalsettingskeyInfo.GlobalSettingsKeyValue), "Value")
                    .AddColumn(nameof(GlobalsettingskeyInfo.GlobalSettingsKeyNote), "Note")
                    .AddColumn(nameof(GlobalsettingskeyInfo.GlobalSettingsKeyName), "Code name");

        // Add a "New settings" button at the top. This will navigate the user to a "create" page.
        // We will define it in the next section.
        PageConfiguration.HeaderActions.AddLink<GlobalSettingsCreatePage>("New setting");

        // Add an "Edit" row action. This will navigate the user to a "edit" section for a specific instance of GlobalSettingsKeyInfo object.
        // We will define it in the next section.
        PageConfiguration.AddEditRowAction<GlobalSettingsEditSection>();

        //// Define a "Delete" action for the user with sufficient permissions to remove an existing setting.
        PageConfiguration.TableActions
            .AddDeleteAction(nameof(Delete));
        await base.ConfigurePage();
    }

    // Ensure only a user with permissions to delete can perform the delete action.
    [PageCommand(Permission = SystemPermissions.DELETE)]
    public override Task<ICommandResponse<RowActionResult>> Delete(int id) => base.Delete(id);
}
