using CMS.Helpers;
using CMS.Membership;
using Kentico.Xperience.Admin.Base;
using System;
using System.Threading.Tasks;

namespace BDO.GWT.Xperience.Admin.Pages.ProjectSettings.GlobalSettings;
[UIEvaluatePermission("Administrator")]
public class GlobalSettingsListingPage : ListingPage
{
    protected override string ObjectType => GlobalsettingskeyInfo.OBJECT_TYPE;

    public override async Task ConfigurePage()
    {
        PageConfiguration.ColumnConfigurations
            .AddColumn(nameof(GlobalsettingskeyInfo.GlobalSettingsKeyDisplayName), "Display name")
            .AddColumn(nameof(GlobalsettingskeyInfo.GlobalSettingsKeyValue), "Value")
            .AddColumn(nameof(GlobalsettingskeyInfo.GlobalSettingsKeyNote), "Note")
            .AddColumn(nameof(GlobalsettingskeyInfo.GlobalSettingsKeyName), "Code name");

        PageConfiguration.AddEditRowAction<GlobalSettingsEditSection>();
        PageConfiguration.TableActions.AddDeleteAction(nameof(Delete));
        await base.ConfigurePage();
    }

    // Ensure only a user with permissions to delete can perform the delete action.
    [PageCommand(Permission = SystemPermissions.DELETE)]
    public override Task<ICommandResponse<RowActionResult>> Delete(int id) => base.Delete(id);
}
