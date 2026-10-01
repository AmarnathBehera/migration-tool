using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BDO.GWT.GLOBAL.API.Data;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum rolesalloweddropdown
{
    Editor,
    Administrator
}

public class RoleInput
{
    /// <summary>Code name of the role (must be unique).</summary>
    [Required, StringLength(100)]
    public string RoleName { get; set; } = string.Empty;

    /// <summary>Display name. Defaults to RoleName when empty.</summary>
    [StringLength(100)]
    public string? DisplayName { get; set; }

    public string? Description { get; set; }
}

public class RoleChannelOutput
{
    public Guid GUID { get; set; }
    public string ChannelName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Channel permissions the role has (View = "Access channel", ManagePermissions = "Manage permissions", OverrideContentLock).</summary>
    public IEnumerable<string> Permissions { get; set; } = Array.Empty<string>();
}

public class RoleApplicationOutput
{
    /// <summary>Raw application identifier, e.g. "Kentico.Xperience.Application.Workflows" or "redirects".</summary>
    public string ApplicationName { get; set; } = string.Empty;

    /// <summary>Raw permission names as stored, e.g. View, Create, Update, Delete, Synchronize, ManageMediaLibrary.</summary>
    public IEnumerable<string> Permissions { get; set; } = Array.Empty<string>();
}

public class RoleEmailChannelOutput
{
    public Guid GUID { get; set; }
    public string ChannelName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Email channel permissions the role has (View, Update, ManagePermissions, OverrideContentLock).</summary>
    public IEnumerable<string> Permissions { get; set; } = Array.Empty<string>();
}

public class RoleWorkspaceOutput
{
    public Guid GUID { get; set; }
    public string WorkspaceName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Content hub permissions in this workspace (View, Create, Update, Delete, Synchronize).</summary>
    public IEnumerable<string> Permissions { get; set; } = Array.Empty<string>();
}

public class RoleOutput
{
    public Guid GUID { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public IEnumerable<RoleChannelOutput> WebsiteChannels { get; set; } = Array.Empty<RoleChannelOutput>();
    public IEnumerable<RoleWorkspaceOutput> ContentHubWorkspaces { get; set; } = Array.Empty<RoleWorkspaceOutput>();
    public IEnumerable<RoleEmailChannelOutput> EmailChannels { get; set; } = Array.Empty<RoleEmailChannelOutput>();

    /// <summary>
    /// Every other application permission set (Media libraries, Event log, Role management, Redirects,
    /// custom applications, etc.) that isn't a website channel, email channel or Content hub workspace.
    /// </summary>
    public IEnumerable<RoleApplicationOutput> ApplicationPermissions { get; set; } = Array.Empty<RoleApplicationOutput>();
}

public class ChannelPermissionInput
{
    /// <summary>Channel name of the website channel, e.g. "Ind".</summary>
    public string? WebsiteChannel { get; set; }

    /// <summary>Defaults to ["View"]. Allowed: View, ManagePermissions, OverrideContentLock.</summary>
    public string[]? Permissions { get; set; }
}

public class ApplicationPermissionInput
{
    /// <summary>Raw application identifier exactly as the admin UI/DB uses it, e.g. "Kentico.Xperience.Application.Workflows" or "redirects".</summary>
    public string? ApplicationName { get; set; }

    /// <summary>Permission names to grant, e.g. ["View", "Update"]. "View" is added automatically if omitted (the admin UI always requires it).</summary>
    public string[]? Permissions { get; set; }
}

public class EmailChannelPermissionInput
{
    /// <summary>Channel name of the email channel (its CMS_Channel.ChannelName).</summary>
    public string? EmailChannel { get; set; }

    /// <summary>Defaults to ["View"]. Allowed: View, Update, ManagePermissions, OverrideContentLock.</summary>
    public string[]? Permissions { get; set; }
}

public class WorkspacePermissionInput
{
    /// <summary>Workspace name ("KenticoDefault") or display name ("Default").</summary>
    public string? Workspace { get; set; }

    /// <summary>Defaults to ["View"]. Allowed: View, Create, Update, Delete, Synchronize.</summary>
    public string[]? Permissions { get; set; }
}

public class AssignRolePermissionsInput
{
    /// <summary>Name of the role to grant permissions to.</summary>
    public string? RoleName { get; set; }

    /// <summary>Optional. Channel permission sets to apply.</summary>
    public List<ChannelPermissionInput>? WebsiteChannels { get; set; }

    /// <summary>Optional. Content hub workspace permission sets to apply.</summary>
    public List<WorkspacePermissionInput>? Workspaces { get; set; }

    /// <summary>Optional. Email channel permission sets to apply.</summary>
    public List<EmailChannelPermissionInput>? EmailChannels { get; set; }

    /// <summary>
    /// Optional. Any other application's permission set (Media libraries, Event log, Role management,
    /// Redirects, a custom application, etc.), by its raw ApplicationName.
    /// </summary>
    public List<ApplicationPermissionInput>? Applications { get; set; }
}

public class    CreateChannelRoleInput
{
    /// <summary>Channel name of the website channel, e.g. "Ind".</summary>
    [Required]
    public string WebsiteChannel { get; set; } = string.Empty;

    /// <summary>Editor or Administrator. Role is named "{WebsiteChannel}-{RoleType}".</summary>
    [Required]
    public rolesalloweddropdown RoleType { get; set; }
}