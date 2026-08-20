using BDO.GWT.Xperience.Admin.Pages.ProjectSettings;
using CMS.Membership;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.UIPages;

namespace BDO.GWT.Xperience;

[UIPermission(SystemPermissions.VIEW)]
[UIPermission(SystemPermissions.CREATE)]
[UIPermission(SystemPermissions.DELETE)]
[UIPermission(SystemPermissions.UPDATE)]
// The class must inherit from the ApplicationPage base class.
public class BDOIntegrationsApplication : ApplicationPage
{
    public const string IDENTIFIER = "BDO.BDOIntegrations";
}
