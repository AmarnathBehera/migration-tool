using CMS.DataEngine;
using GWTXbyKWeb.Components.FormComponents.ApiPermissions;
using Kentico.Content.Web.Mvc;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using System.Linq;
using System.Threading.Tasks;

namespace BDO.GWT.Xperience.Admin.Pages.ProjectSettings.GlobalSettings;

[UIEvaluatePermission("Administrator")]
public class ApiPermissionEditPage : InfoEditPage<Api_permissionsInfo>
{
    private readonly IInfoProvider<Api_permissionsInfo> apiPermissionInfoProvider;
    protected override string ObjectType => Api_permissionsInfo.OBJECT_TYPE;

    public override int ObjectId
    {
        get
        {
            var existing = apiPermissionInfoProvider.Get()
                .FirstOrDefault();

            if (existing != null)
                return existing.Api_permissionsID;

            // Auto-create an empty api_PermissionsInfo for this channel so the edit form can load.
            var api_PermissionsInfo = new Api_permissionsInfo {Api_permissionsID = 1};
            apiPermissionInfoProvider.Set(api_PermissionsInfo);

            return api_PermissionsInfo.Api_permissionsID;
        }

        set { _ = value; }
    }

    [ApiPermissionComponentAttribute(Label = "")]
    public string ApiPermissionMatrix { get; set; } = string.Empty;

    public ApiPermissionEditPage(
        IFormComponentMapper formComponentMapper,
        IFormDataBinder formDataBinder,
        IInfoProvider<Api_permissionsInfo> apiPermissionInfoProvider)
            : base(formComponentMapper, formDataBinder)
    {
        this.apiPermissionInfoProvider = apiPermissionInfoProvider;
    }

    public override async Task ConfigurePage()
    {
        PageConfiguration.UIFormName = "bdo_api_permission";
        PageConfiguration.Headline = "API Permission";
        
        await base.ConfigurePage();
        PageConfiguration.SubmitConfiguration.Visible = false;
    }
}