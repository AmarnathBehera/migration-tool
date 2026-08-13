using BDO.GWT.GLOBAL.Services;
using GWTXbyKWeb.Components.FormComponents.ApiPermissions;
using Kentico.Xperience.Admin.Base.Forms;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("BDO.GWT.GLOBAL.Tests")]
[assembly: RegisterFormComponent(
    ApiPermissionComponent.IDENTIFIER,
    typeof(ApiPermissionComponent),
    "API Permissions Matrix"
)]

namespace GWTXbyKWeb.Components.FormComponents.ApiPermissions;

[ComponentAttribute(typeof(ApiPermissionComponentAttribute))]
public class ApiPermissionComponent
    : FormComponent<ApiPermissionProperties, ApiPermissionClientProperties, string>
{
    public const string IDENTIFIER = "BDO.ApiPermissionMatrix";

    private readonly IApiPermissionService apiPermissionService;

    public ApiPermissionComponent(IApiPermissionService apiPermissionService)
    {
        this.apiPermissionService = apiPermissionService;
    }

    public override string ClientComponentName => "@bdo/custom-components/ApiPermissionsControl";

    protected override async Task ConfigureClientProperties(ApiPermissionClientProperties clientProperties)
    {
        await base.ConfigureClientProperties(clientProperties);
    }

    [FormComponentCommand]
    public Task<ApiPermissionsInitResult> Initialize(string _)
        => Task.FromResult(ApiPermissionCommandHandler.HandleInitialize(apiPermissionService));

    [FormComponentCommand]
    public Task<LoadMatrixResult> LoadMatrix(string filterJson)
        => Task.FromResult(ApiPermissionCommandHandler.HandleLoadMatrix(apiPermissionService, filterJson));

    [FormComponentCommand]
    public Task<TogglePermissionResult> TogglePermission(string toggleJson)
        => Task.FromResult(ApiPermissionCommandHandler.HandleTogglePermission(apiPermissionService, toggleJson));

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
}

public class ApiPermissionProperties : FormComponentProperties { }

public class ApiPermissionClientProperties : FormComponentClientProperties<string> { }

// Extracted so command logic is testable without Kentico's IoC container.
internal static class ApiPermissionCommandHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    internal static ApiPermissionsInitResult HandleInitialize(IApiPermissionService service)
    {
        return new ApiPermissionsInitResult
        {
            Users = new List<UserOption>(service.GetUsers()),
            MethodGroups = new List<MethodGroupOption>(service.GetMethodGroups()),
        };
    }

    internal static LoadMatrixResult HandleLoadMatrix(IApiPermissionService service, string filterJson)
    {
        var request = JsonSerializer.Deserialize<LoadMatrixRequest>(filterJson, JsonOptions);
        if (request is null)
        {
            return new LoadMatrixResult();
        }

        var rows = request.FilterType == "methodgroup"
            ? service.GetMatrixByMethodGroup(request.SelectedId)
            : service.GetMatrixByUser(request.SelectedId);

        return new LoadMatrixResult { Rows = new List<MatrixRow>(rows) };
    }

    internal static TogglePermissionResult HandleTogglePermission(IApiPermissionService service, string toggleJson)
    {
        var request = JsonSerializer.Deserialize<TogglePermissionRequest>(toggleJson, JsonOptions);
        if (request is null)
        {
            return new TogglePermissionResult { Success = false, Message = "Invalid request." };
        }

        service.SetPermission(
            request.UserId,
            request.MethodGroupId,
            request.AllowRead,
            request.AllowManage);

        return new TogglePermissionResult { Success = true, Message = string.Empty };
    }
}

public class ApiPermissionsInitResult
{
    public List<UserOption> Users { get; set; } = new();
    public List<MethodGroupOption> MethodGroups { get; set; } = new();
}

public class LoadMatrixRequest
{
    public string FilterType { get; set; } = "users";
    public int SelectedId { get; set; }
}

public class LoadMatrixResult
{
    public List<MatrixRow> Rows { get; set; } = new();
}

public class TogglePermissionRequest
{
    public int UserId { get; set; }
    public int MethodGroupId { get; set; }
    public bool AllowRead { get; set; }
    public bool AllowManage { get; set; }
}

public class TogglePermissionResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
