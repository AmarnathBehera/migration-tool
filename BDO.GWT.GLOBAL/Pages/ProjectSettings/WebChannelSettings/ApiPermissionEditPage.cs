using BDO;
using BDO.GWT.GLOBAL.Services;
using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;
using CMS.DataEngine;
using GWTXbyKWeb.Components.FormComponents.ApiPermissions;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GWTXbyKWeb.Pages.ProjectSettings.WebChannelSettings;

public class ApiPermissionEditPage : InfoEditPage<Api_permissionsInfo>
{
    private readonly IInfoProvider<Api_permissionsInfo> apiPermissionInfoProvider;
    private readonly IChannelAccessService channelAccessService;
    private readonly IApiPermissionService apiPermissionService;

    protected override string ObjectType => Api_permissionsInfo.OBJECT_TYPE;

    [PageParameter(typeof(IntPageModelBinder))]
    public int WebChannelSettingsId { get; set; }

    public override int ObjectId
    {
        get
        {
            var existing = apiPermissionInfoProvider.Get()
                .WhereEquals(nameof(Api_permissionsInfo.APIPermissionWebChannelSettingID), WebChannelSettingsId)
                .LastOrDefault();

            if (existing != null)
                return existing.Api_permissionsID;

            if (WebChannelSettingsId <= 0)
                return 0;

            // Auto-create an empty record so the edit form can load.
            var api_PermissionsInfo = new Api_permissionsInfo{APIPermissionWebChannelSettingID = WebChannelSettingsId.ToString()};
            apiPermissionInfoProvider.Set(api_PermissionsInfo);

            return api_PermissionsInfo.Api_permissionsID;
        }

        set { _ = value; }
    }

    // The single field on this page: renders the custom widget
    [ApiPermissionComponentAttribute(Label = "")]
    public string ApiPermissionMatrix { get; set; } = string.Empty;

    public ApiPermissionEditPage(
        IFormComponentMapper formComponentMapper,
        IFormDataBinder formDataBinder,
        IChannelAccessService channelAccessService,
        IInfoProvider<Api_permissionsInfo> apiPermissionInfoProvider,
        IApiPermissionService apiPermissionService)
            : base(formComponentMapper, formDataBinder)
    {
        this.channelAccessService = channelAccessService;
        this.apiPermissionInfoProvider = apiPermissionInfoProvider;
        this.apiPermissionService = apiPermissionService;
    }

    public override async Task ConfigurePage()
    {
        bool canAccess = await channelAccessService.CanAccessAsync(WebChannelSettingsId);
        if (!canAccess)
        {
            PageConfiguration.SubmitConfiguration.Visible = false;
            PageConfiguration.Headline = "Access Denied";
            return;
        }

        PageConfiguration.UIFormName = "bdoapipermission";
        PageConfiguration.Headline = "API Permission";

        await base.ConfigurePage();

        PageConfiguration.SubmitConfiguration.Visible = false;
    }

    // Propagates the current WebChannelSettingsId to the matrix form component so commands can scope by channel.
    protected override async Task<ICollection<IFormItem>> GetFormItems()
    {
        var items = await base.GetFormItems();
        foreach (var component in items.OfType<ApiPermissionComponent>())
        {
            component.Properties.WebChannelSettingId = WebChannelSettingsId;
        }

        return items;
    }

    // Returns channels, users, and method groups to populate the filter dropdowns.
    [PageCommand]
    public Task<ApiPermissionsInitResult> Initialize(string _)
        => Task.FromResult(ApiPermissionCommandHandler.HandleInitialize(apiPermissionService));

    // Loads the permission matrix for the selected channel, filter type, and user/method group.
    [PageCommand]
    public Task<LoadMatrixResult> LoadMatrix(string filterJson)
        => Task.FromResult(ApiPermissionCommandHandler.HandleLoadMatrix(apiPermissionService, filterJson));

    // Toggles a single Read/Manage permission cell in the matrix.
    [PageCommand]
    public Task<TogglePermissionResult> TogglePermission(string toggleJson)
        => Task.FromResult(ApiPermissionCommandHandler.HandleTogglePermission(apiPermissionService, toggleJson));
}