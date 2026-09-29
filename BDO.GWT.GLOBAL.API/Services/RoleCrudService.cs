using BDO.GWT.GLOBAL.API.Data;
using CMS.Base;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.EmailLibrary;
using CMS.Membership;
using CMS.Websites;
using CMS.Websites.Internal;
using CMS.Workspaces;

namespace BDO.GWT.GLOBAL.API.Services;

public interface IRoleCrudService
{
    Task<IEnumerable<RoleOutput>> GetRolesAsync(string? roleName, CancellationToken cancellationToken);
    Task<RoleOutput?> GetRoleAsync(Guid roleGUID, CancellationToken cancellationToken);
    Task<RoleOutput> CreateRoleAsync(RoleInput roleInput, CancellationToken cancellationToken);
    Task<RoleOutput> UpdateRoleAsync(Guid roleGUID, RoleInput roleInput, CancellationToken cancellationToken);
    Task DeleteRoleAsync(Guid roleGUID, CancellationToken cancellationToken);
    Task<RoleOutput> AssignRolePermissionsAsync(AssignRolePermissionsInput input, CancellationToken cancellationToken);
}

public class RoleCrudService : IRoleCrudService
{
    private const string AdministratorRoleName = "Administrator";
    private const string ContentHubApplicationName = "Kentico.Xperience.Application.ContentHub";
    private const string ChannelApplicationPrefix = "Kentico.Xperience.Application.WebPages_";
    private const string EmailChannelApplicationPrefix = "Kentico.Xperience.Application.EmailChannel_";
    private static readonly string[] AllowedPermissions = { "View", "ManagePermissions", "OverrideContentLock" };
    private static readonly string[] DefaultPermissions = { "View" };
    private static readonly string[] AllowedWorkspacePermissions = { "View", "Create", "Update", "Delete", "Synchronize" };
    private static readonly string[] AllowedEmailChannelPermissions = { "View", "Update", "ManagePermissions", "OverrideContentLock" };

    private static string ChannelApplicationName(Guid websiteChannelGuid) =>
        ChannelApplicationPrefix + websiteChannelGuid.ToString("D").ToLowerInvariant();

    private static string EmailChannelApplicationName(Guid emailChannelGuid) =>
        EmailChannelApplicationPrefix + emailChannelGuid.ToString("D").ToLowerInvariant();

    private readonly IInfoProvider<UserInfo> _userInfoProvider;
    private readonly IInfoProvider<UserRoleInfo> _userRoleInfoProvider;
    private readonly IInfoProvider<RoleInfo> _roleInfoProvider;
    private readonly IInfoProvider<ApplicationPermissionInfo> _applicationPermissionInfoProvider;
    private readonly IInfoProvider<WorkspaceDataPermissionInfo> _workspaceDataPermissionInfoProvider;
    private readonly IInfoProvider<WorkspaceInfo> _workspaceInfoProvider;
    private readonly IInfoProvider<EmailChannelInfo> _emailChannelInfoProvider;
    private readonly IInfoProvider<WebPageAclRoleInfo> _webPageAclRoleInfoProvider;
    private readonly IInfoProvider<WebPageAclRolePermissionInfo> _webPageAclRolePermissionInfoProvider;
    private readonly IInfoProvider<WebsiteChannelInfo> _websiteChannelInfoProvider;
    private readonly IInfoProvider<ChannelInfo> _channelInfoProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;

#pragma warning disable S107
    public RoleCrudService(
        IInfoProvider<UserInfo> userInfoProvider,
        IInfoProvider<UserRoleInfo> userRoleInfoProvider,
        IInfoProvider<RoleInfo> roleInfoProvider,
        IInfoProvider<ApplicationPermissionInfo> applicationPermissionInfoProvider,
        IInfoProvider<WorkspaceDataPermissionInfo> workspaceDataPermissionInfoProvider,
        IInfoProvider<WorkspaceInfo> workspaceInfoProvider,
        IInfoProvider<EmailChannelInfo> emailChannelInfoProvider,
        IInfoProvider<WebPageAclRoleInfo> webPageAclRoleInfoProvider,
        IInfoProvider<WebPageAclRolePermissionInfo> webPageAclRolePermissionInfoProvider,
        IInfoProvider<WebsiteChannelInfo> websiteChannelInfoProvider,
        IInfoProvider<ChannelInfo> channelInfoProvider,
        IHttpContextAccessor httpContextAccessor)
#pragma warning restore S107
    {
        _userInfoProvider = userInfoProvider;
        _userRoleInfoProvider = userRoleInfoProvider;
        _roleInfoProvider = roleInfoProvider;
        _applicationPermissionInfoProvider = applicationPermissionInfoProvider;
        _workspaceDataPermissionInfoProvider = workspaceDataPermissionInfoProvider;
        _workspaceInfoProvider = workspaceInfoProvider;
        _emailChannelInfoProvider = emailChannelInfoProvider;
        _webPageAclRoleInfoProvider = webPageAclRoleInfoProvider;
        _webPageAclRolePermissionInfoProvider = webPageAclRolePermissionInfoProvider;
        _websiteChannelInfoProvider = websiteChannelInfoProvider;
        _channelInfoProvider = channelInfoProvider;
        _httpContextAccessor = httpContextAccessor;
    }

    // ---------- READ ----------

    public async Task<IEnumerable<RoleOutput>> GetRolesAsync(string? roleName, CancellationToken cancellationToken)
    {
        var query = _roleInfoProvider.Get();

        if (!string.IsNullOrEmpty(roleName))
            query = query.WhereContains(nameof(RoleInfo.RoleName), roleName);

        var roles = (await query.GetEnumerableTypedResultAsync(cancellationToken: cancellationToken)).ToArray();
        var context = await BuildContextAsync(cancellationToken);

        return roles.Select(r => ToRoleOutput(r, context));
    }

    public async Task<RoleOutput?> GetRoleAsync(Guid roleGUID, CancellationToken cancellationToken)
    {
        var role = await FindRoleByGuidAsync(roleGUID, cancellationToken);

        if (role is null)
            return null;

        var context = await BuildContextAsync(cancellationToken);
        return ToRoleOutput(role, context);
    }

    // ---------- CREATE ----------

    public async Task<RoleOutput> CreateRoleAsync(RoleInput roleInput, CancellationToken cancellationToken)
    {
        var caller = await GetAndValidateCallerAsync(cancellationToken);

        await ValidateRoleInputAsync(null, roleInput, cancellationToken);

        var roleName = roleInput.RoleName.Trim();

        var role = new RoleInfo
        {
            RoleName = roleName,
            RoleDisplayName = string.IsNullOrWhiteSpace(roleInput.DisplayName) ? roleName : roleInput.DisplayName.Trim(),
            RoleDescription = roleInput.Description ?? string.Empty
        };

        using (var actionContext = new CMSActionContext(caller))
        {
            actionContext.UseGlobalAdminContext = true;
            _roleInfoProvider.Set(role);
        }

        var context = await BuildContextAsync(cancellationToken);
        return ToRoleOutput(role, context);
    }

    // ---------- UPDATE ----------

    public async Task<RoleOutput> UpdateRoleAsync(Guid roleGUID, RoleInput roleInput, CancellationToken cancellationToken)
    {
        var caller = await GetAndValidateCallerAsync(cancellationToken);

        var role = await GetValidatedRoleAsync(roleGUID, cancellationToken);

        EnsureNotSystemRole(role);

        await ValidateRoleInputAsync(role.RoleID, roleInput, cancellationToken);

        var roleName = roleInput.RoleName.Trim();

        role.RoleName = roleName;
        role.RoleDisplayName = string.IsNullOrWhiteSpace(roleInput.DisplayName) ? roleName : roleInput.DisplayName.Trim();
        role.RoleDescription = roleInput.Description ?? string.Empty;

        using (var actionContext = new CMSActionContext(caller))
        {
            actionContext.UseGlobalAdminContext = true;
            _roleInfoProvider.Set(role);
        }

        var context = await BuildContextAsync(cancellationToken);
        return ToRoleOutput(role, context);
    }

    // ---------- DELETE ----------

    public async Task DeleteRoleAsync(Guid roleGUID, CancellationToken cancellationToken)
    {
        var caller = await GetAndValidateCallerAsync(cancellationToken);

        var role = await GetValidatedRoleAsync(roleGUID, cancellationToken);

        EnsureNotSystemRole(role);

        var assignedUsers = (await _userRoleInfoProvider.Get()
            .WhereEquals(nameof(UserRoleInfo.RoleID), role.RoleID)
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
            .Count();

        if (assignedUsers > 0)
            throw new InvalidOperationException(
                $"Role '{role.RoleName}' is assigned to {assignedUsers} user(s). Remove it from those users before deleting.");

        var aclLinks = (await _webPageAclRoleInfoProvider.Get()
            .WhereEquals(nameof(WebPageAclRoleInfo.WebPageAclRoleRoleID), role.RoleID)
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
            .ToArray();

        var linkIds = aclLinks.Select(l => l.WebPageAclRoleID).ToArray();

        var permissionRows = linkIds.Length == 0
            ? Array.Empty<WebPageAclRolePermissionInfo>()
            : (await _webPageAclRolePermissionInfoProvider.Get()
                .WhereIn(nameof(WebPageAclRolePermissionInfo.WebPageAclRolePermissionWebPageAclRoleID), linkIds)
                .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
                .ToArray();

        var applicationPermissions = (await _applicationPermissionInfoProvider.Get()
            .WhereEquals(nameof(ApplicationPermissionInfo.RoleID), role.RoleID)
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
            .ToArray();

        var workspacePermissions = (await _workspaceDataPermissionInfoProvider.Get()
            .WhereEquals(nameof(WorkspaceDataPermissionInfo.WorkspaceDataPermissionRoleID), role.RoleID)
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
            .ToArray();

        using (var actionContext = new CMSActionContext(caller))
        {
            actionContext.UseGlobalAdminContext = true;

            foreach (var workspacePermission in workspacePermissions)
                await _workspaceDataPermissionInfoProvider.DeleteAsync(workspacePermission, cancellationToken);

            foreach (var applicationPermission in applicationPermissions)
                await _applicationPermissionInfoProvider.DeleteAsync(applicationPermission, cancellationToken);

            foreach (var permissionRow in permissionRows)
                await _webPageAclRolePermissionInfoProvider.DeleteAsync(permissionRow, cancellationToken);

            foreach (var link in aclLinks)
                await _webPageAclRoleInfoProvider.DeleteAsync(link, cancellationToken);

            _roleInfoProvider.Delete(role);
        }
    }

    // ---------- ASSIGN PERMISSIONS (CHANNELS + CONTENT HUB WORKSPACES) ----------

    public async Task<RoleOutput> AssignRolePermissionsAsync(AssignRolePermissionsInput input, CancellationToken cancellationToken)
    {
        var caller = await GetAndValidateCallerAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(input.RoleName))
            throw new InvalidOperationException("Role name is required.");

        var channelInputs = input.WebsiteChannels ?? new List<ChannelPermissionInput>();
        var workspaceInputs = input.Workspaces ?? new List<WorkspacePermissionInput>();
        var emailChannelInputs = input.EmailChannels ?? new List<EmailChannelPermissionInput>();
        var applicationInputs = input.Applications ?? new List<ApplicationPermissionInput>();

        if (channelInputs.Count == 0 && workspaceInputs.Count == 0 && emailChannelInputs.Count == 0 && applicationInputs.Count == 0)
            throw new InvalidOperationException("Nothing to assign: provide 'websiteChannels', 'workspaces', 'emailChannels' and/or 'applications'.");

        var role = (await _roleInfoProvider.Get()
            .WhereEquals(nameof(RoleInfo.RoleName), input.RoleName.Trim())
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
            .FirstOrDefault()
            ?? throw new InvalidOperationException($"Cannot find role '{input.RoleName}'.");

        // ---- Resolve and validate everything first, so nothing is written if any entry is invalid ----

        var channelGrants = new List<(string ApplicationName, string[] Permissions)>();

        for (var i = 0; i < channelInputs.Count; i++)
        {
            var entry = channelInputs[i];

            if (string.IsNullOrWhiteSpace(entry.WebsiteChannel))
                throw new InvalidOperationException($"websiteChannels[{i}].websiteChannel is required.");

            var permissions = NormalizePermissions(entry.Permissions, AllowedPermissions, DefaultPermissions);

            var channel = (await _channelInfoProvider.Get()
                .WhereEquals(nameof(ChannelInfo.ChannelName), entry.WebsiteChannel.Trim())
                .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
                .FirstOrDefault()
                ?? throw new InvalidOperationException($"Cannot find channel '{entry.WebsiteChannel}'.");

            var websiteChannel = (await _websiteChannelInfoProvider.Get()
                .WhereEquals(nameof(WebsiteChannelInfo.WebsiteChannelChannelID), channel.ChannelID)
                .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
                .FirstOrDefault()
                ?? throw new InvalidOperationException($"Channel '{entry.WebsiteChannel}' is not a website channel.");

            channelGrants.Add((ChannelApplicationName(websiteChannel.WebsiteChannelGUID), permissions));
        }

        var emailChannelGrants = new List<(string ApplicationName, string[] Permissions)>();

        for (var i = 0; i < emailChannelInputs.Count; i++)
        {
            var entry = emailChannelInputs[i];

            if (string.IsNullOrWhiteSpace(entry.EmailChannel))
                throw new InvalidOperationException($"emailChannels[{i}].emailChannel is required.");

            var permissions = NormalizePermissions(entry.Permissions, AllowedEmailChannelPermissions, DefaultPermissions);

            var channel = (await _channelInfoProvider.Get()
                .WhereEquals(nameof(ChannelInfo.ChannelName), entry.EmailChannel.Trim())
                .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
                .FirstOrDefault()
                ?? throw new InvalidOperationException($"Cannot find channel '{entry.EmailChannel}'.");

            var emailChannel = (await _emailChannelInfoProvider.Get()
                .WhereEquals(nameof(EmailChannelInfo.EmailChannelChannelID), channel.ChannelID)
                .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
                .FirstOrDefault()
                ?? throw new InvalidOperationException($"Channel '{entry.EmailChannel}' is not an email channel.");

            emailChannelGrants.Add((EmailChannelApplicationName(emailChannel.EmailChannelGUID), permissions));
        }

        var genericApplicationGrants = new List<(string ApplicationName, string[] Permissions)>();

        for (var i = 0; i < applicationInputs.Count; i++)
        {
            var entry = applicationInputs[i];

            if (string.IsNullOrWhiteSpace(entry.ApplicationName))
                throw new InvalidOperationException($"applications[{i}].applicationName is required.");

            if (entry.ApplicationName.StartsWith(ChannelApplicationPrefix, StringComparison.OrdinalIgnoreCase)
                || entry.ApplicationName.StartsWith(EmailChannelApplicationPrefix, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(
                    $"applications[{i}].applicationName looks like a channel application. Use 'websiteChannels' or 'emailChannels' instead.");

            // Permission names for generic applications vary per application (View/Create/Update/Delete plus
            // application-specific ones), so they aren't validated against a fixed list here. "View" is required
            // by the admin UI for any application to be accessible, so it's always included.
            var permissions = (entry.Permissions ?? Array.Empty<string>())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.Trim())
                .Append("View")
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            genericApplicationGrants.Add((entry.ApplicationName.Trim(), permissions));
        }

        var workspaceGrants = new List<(int WorkspaceId, string[] Permissions)>();

        if (workspaceInputs.Count > 0)
        {
            var allWorkspaces = (await _workspaceInfoProvider.Get()
                .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
                .ToArray();

            for (var i = 0; i < workspaceInputs.Count; i++)
            {
                var entry = workspaceInputs[i];

                if (string.IsNullOrWhiteSpace(entry.Workspace))
                    throw new InvalidOperationException($"workspaces[{i}].workspace is required.");

                var permissions = NormalizePermissions(entry.Permissions, AllowedWorkspacePermissions, DefaultPermissions);
                var key = entry.Workspace.Trim();

                var matches = allWorkspaces
                    .Where(w => string.Equals(w.WorkspaceName, key, StringComparison.OrdinalIgnoreCase)
                             || string.Equals(w.WorkspaceDisplayName, key, StringComparison.OrdinalIgnoreCase))
                    .ToArray();

                if (matches.Length == 0)
                    throw new InvalidOperationException($"Cannot find workspace '{entry.Workspace}'.");

                var workspace = matches.Length == 1
                    ? matches[0]
                    : matches.SingleOrDefault(w => string.Equals(w.WorkspaceName, key, StringComparison.OrdinalIgnoreCase))
                      ?? throw new InvalidOperationException($"Workspace '{entry.Workspace}' is ambiguous. Use the workspace name instead.");

                workspaceGrants.Add((workspace.WorkspaceID, permissions));
            }
        }

        // ---- Existing rows, so repeated calls never create duplicates ----

        var existingChannelPermissions = (await _applicationPermissionInfoProvider.Get()
            .WhereEquals(nameof(ApplicationPermissionInfo.RoleID), role.RoleID)
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
            .GroupBy(p => p.ApplicationName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => g.Select(p => p.PermissionName).ToHashSet(StringComparer.OrdinalIgnoreCase),
                StringComparer.OrdinalIgnoreCase);

        var existingEmailChannelPermissions = existingChannelPermissions; // same table, same dictionary keyed by ApplicationName

        var existingWorkspacePermissions = (await _workspaceDataPermissionInfoProvider.Get()
            .WhereEquals(nameof(WorkspaceDataPermissionInfo.WorkspaceDataPermissionRoleID), role.RoleID)
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
            .Where(p => string.Equals(p.WorkspaceDataPermissionApplicationName, ContentHubApplicationName, StringComparison.OrdinalIgnoreCase))
            .GroupBy(p => p.WorkspaceDataPermissionWorkspaceID)
            .ToDictionary(
                g => g.Key,
                g => g.Select(p => p.WorkspaceDataPermissionPermissionName).ToHashSet(StringComparer.OrdinalIgnoreCase));

        // ---- Write ----

        using (var actionContext = new CMSActionContext(caller))
        {
            actionContext.UseGlobalAdminContext = true;

            foreach (var (applicationName, permissions) in channelGrants)
            {
                if (!existingChannelPermissions.TryGetValue(applicationName, out var have))
                    existingChannelPermissions[applicationName] = have = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var permission in permissions.Where(p => !have.Contains(p)))
                {
                    await _applicationPermissionInfoProvider.SetAsync(new ApplicationPermissionInfo
                    {
                        RoleID = role.RoleID,
                        ApplicationName = applicationName,
                        PermissionName = permission
                    }, cancellationToken);

                    have.Add(permission);
                }
            }

            foreach (var (applicationName, permissions) in emailChannelGrants)
            {
                if (!existingEmailChannelPermissions.TryGetValue(applicationName, out var have))
                    existingEmailChannelPermissions[applicationName] = have = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var permission in permissions.Where(p => !have.Contains(p)))
                {
                    await _applicationPermissionInfoProvider.SetAsync(new ApplicationPermissionInfo
                    {
                        RoleID = role.RoleID,
                        ApplicationName = applicationName,
                        PermissionName = permission
                    }, cancellationToken);

                    have.Add(permission);
                }
            }

            foreach (var (applicationName, permissions) in genericApplicationGrants)
            {
                if (!existingChannelPermissions.TryGetValue(applicationName, out var have))
                    existingChannelPermissions[applicationName] = have = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var permission in permissions.Where(p => !have.Contains(p)))
                {
                    await _applicationPermissionInfoProvider.SetAsync(new ApplicationPermissionInfo
                    {
                        RoleID = role.RoleID,
                        ApplicationName = applicationName,
                        PermissionName = permission
                    }, cancellationToken);

                    have.Add(permission);
                }
            }

            foreach (var (workspaceId, permissions) in workspaceGrants)
            {
                if (!existingWorkspacePermissions.TryGetValue(workspaceId, out var have))
                    existingWorkspacePermissions[workspaceId] = have = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var permission in permissions.Where(p => !have.Contains(p)))
                {
                    await _workspaceDataPermissionInfoProvider.SetAsync(new WorkspaceDataPermissionInfo
                    {
                        WorkspaceDataPermissionWorkspaceID = workspaceId,
                        WorkspaceDataPermissionRoleID = role.RoleID,
                        WorkspaceDataPermissionApplicationName = ContentHubApplicationName,
                        WorkspaceDataPermissionPermissionName = permission
                    }, cancellationToken);

                    have.Add(permission);
                }
            }
        }

        var context = await BuildContextAsync(cancellationToken);
        return ToRoleOutput(role, context);
    }

    private static string[] NormalizePermissions(string[]? requested, string[] allowed, string[] defaults)
    {
        var list = (requested ?? Array.Empty<string>())
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => p.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (list.Length == 0)
            return defaults;

        var unknown = list.Where(p => !allowed.Contains(p, StringComparer.OrdinalIgnoreCase)).ToArray();

        if (unknown.Length > 0)
            throw new InvalidOperationException(
                $"Unknown permission(s): {string.Join(", ", unknown)}. Allowed: {string.Join(", ", allowed)}.");

        return list
            .Select(p => allowed.First(a => string.Equals(a, p, StringComparison.OrdinalIgnoreCase)))
            .ToArray();
    }

    // ---------- CALLER VALIDATION ----------

    private async Task<UserInfo> GetAndValidateCallerAsync(CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new UnauthorizedAccessException("HTTP context was not available.");

        if (httpContext.User?.Identity?.IsAuthenticated != true)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var username = httpContext.User.Identity?.Name;

        if (string.IsNullOrWhiteSpace(username))
            throw new UnauthorizedAccessException("Authenticated username was not found in the request.");

        var caller = (await _userInfoProvider.Get()
            .WhereEquals(nameof(UserInfo.UserName), username)
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
            .FirstOrDefault();

        if (caller is null)
            throw new UnauthorizedAccessException($"Xperience user '{username}' was not found.");

        if (!caller.UserEnabled)
            throw new UnauthorizedAccessException($"Xperience user '{caller.UserName}' is disabled.");

        if (!caller.UserAdministrationAccess)
            throw new UnauthorizedAccessException($"Xperience user '{caller.UserName}' does not have administration access.");

        return caller;
    }

    // ---------- VALIDATION HELPERS ----------

    private async Task<RoleInfo?> FindRoleByGuidAsync(Guid roleGUID, CancellationToken cancellationToken) =>
        (await _roleInfoProvider.Get()
            .WhereEquals(nameof(RoleInfo.RoleGUID), roleGUID)
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
            .FirstOrDefault();

    private async Task<RoleInfo> GetValidatedRoleAsync(Guid roleGUID, CancellationToken cancellationToken) =>
        await FindRoleByGuidAsync(roleGUID, cancellationToken)
            ?? throw new System.Collections.Generic.KeyNotFoundException($"Cannot find role with GUID '{roleGUID}'.");

    private static void EnsureNotSystemRole(RoleInfo role)
    {
        if (string.Equals(role.RoleName, AdministratorRoleName, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("The Administrator role is a system role and cannot be modified or deleted.");
    }

    private async Task ValidateRoleInputAsync(int? existingRoleId, RoleInput roleInput, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(roleInput.RoleName))
            throw new InvalidOperationException("Role name is required.");

        var roleName = roleInput.RoleName.Trim();

        var query = _roleInfoProvider.Get()
            .WhereEquals(nameof(RoleInfo.RoleName), roleName);

        if (existingRoleId.HasValue)
            query = query.WhereNotEquals(nameof(RoleInfo.RoleID), existingRoleId.Value);

        if ((await query.GetEnumerableTypedResultAsync(cancellationToken: cancellationToken)).Any())
            throw new InvalidOperationException($"Role name '{roleName}' already exists.");
    }

    // ---------- MAPPING ----------

    private sealed record MappingContext(
        // RoleID -> (website channel GUID -> permission names)
        Dictionary<int, Dictionary<Guid, string[]>> ChannelPermissionsByRoleId,
        Dictionary<Guid, ChannelInfo> ChannelsByWebsiteChannelGuid,
        // RoleID -> (email channel GUID -> permission names)
        Dictionary<int, Dictionary<Guid, string[]>> EmailChannelPermissionsByRoleId,
        Dictionary<Guid, ChannelInfo> ChannelsByEmailChannelGuid,
        // RoleID -> (WorkspaceID -> permission names) for Content hub
        Dictionary<int, Dictionary<int, string[]>> WorkspacePermissionsByRoleId,
        Dictionary<int, WorkspaceInfo> WorkspacesById,
        // RoleID -> (ApplicationName -> permission names) for every other application
        Dictionary<int, Dictionary<string, string[]>> GenericApplicationPermissionsByRoleId);

    private async Task<MappingContext> BuildContextAsync(CancellationToken cancellationToken)
    {
        // Filter in memory: the tables are small and this avoids LIKE-wildcard escaping issues with "_".
        var allApplicationPermissions = (await _applicationPermissionInfoProvider.Get()
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
            .ToArray();

        var channelPermissionsByRoleId = BuildGuidPermissionMap(allApplicationPermissions, ChannelApplicationPrefix);
        var emailChannelPermissionsByRoleId = BuildGuidPermissionMap(allApplicationPermissions, EmailChannelApplicationPrefix);

        var genericApplicationPermissionsByRoleId = allApplicationPermissions
            .Where(p => p.ApplicationName != null
                        && !p.ApplicationName.StartsWith(ChannelApplicationPrefix, StringComparison.OrdinalIgnoreCase)
                        && !p.ApplicationName.StartsWith(EmailChannelApplicationPrefix, StringComparison.OrdinalIgnoreCase))
            .GroupBy(p => p.RoleID)
            .ToDictionary(
                g => g.Key,
                g => g.GroupBy(p => p.ApplicationName, StringComparer.OrdinalIgnoreCase)
                      .ToDictionary(
                          a => a.Key,
                          a => a.Select(p => p.PermissionName)
                                .Distinct(StringComparer.OrdinalIgnoreCase)
                                .OrderBy(n => n)
                                .ToArray(),
                          StringComparer.OrdinalIgnoreCase));

        var websiteChannels = await _websiteChannelInfoProvider.Get()
            .Columns(nameof(WebsiteChannelInfo.WebsiteChannelGUID), nameof(WebsiteChannelInfo.WebsiteChannelChannelID))
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken);

        var channels = (await _channelInfoProvider.Get()
            .Columns(nameof(ChannelInfo.ChannelID), nameof(ChannelInfo.ChannelGUID), nameof(ChannelInfo.ChannelName), nameof(ChannelInfo.ChannelDisplayName))
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
            .ToDictionary(c => c.ChannelID);

        var channelsByWebsiteGuid = websiteChannels
            .Where(w => channels.ContainsKey(w.WebsiteChannelChannelID))
            .ToDictionary(w => w.WebsiteChannelGUID, w => channels[w.WebsiteChannelChannelID]);

        var emailChannels = await _emailChannelInfoProvider.Get()
            .Columns(nameof(EmailChannelInfo.EmailChannelGUID), nameof(EmailChannelInfo.EmailChannelChannelID))
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken);

        var channelsByEmailGuid = emailChannels
            .Where(e => channels.ContainsKey(e.EmailChannelChannelID))
            .ToDictionary(e => e.EmailChannelGUID, e => channels[e.EmailChannelChannelID]);

        // Content hub (workspace data permissions)
        var workspacePermissionsByRoleId = (await _workspaceDataPermissionInfoProvider.Get()
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
            .Where(p => string.Equals(p.WorkspaceDataPermissionApplicationName, ContentHubApplicationName, StringComparison.OrdinalIgnoreCase))
            .GroupBy(p => p.WorkspaceDataPermissionRoleID)
            .ToDictionary(
                g => g.Key,
                g => g.GroupBy(p => p.WorkspaceDataPermissionWorkspaceID)
                      .ToDictionary(
                          w => w.Key,
                          w => w.Select(p => p.WorkspaceDataPermissionPermissionName)
                                .Distinct(StringComparer.OrdinalIgnoreCase)
                                .OrderBy(n => n)
                                .ToArray()));

        var workspacesById = (await _workspaceInfoProvider.Get()
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
            .ToDictionary(w => w.WorkspaceID);

        return new MappingContext(
            channelPermissionsByRoleId, channelsByWebsiteGuid,
            emailChannelPermissionsByRoleId, channelsByEmailGuid,
            workspacePermissionsByRoleId, workspacesById,
            genericApplicationPermissionsByRoleId);
    }

    private static Dictionary<int, Dictionary<Guid, string[]>> BuildGuidPermissionMap(
        IEnumerable<ApplicationPermissionInfo> permissions, string applicationPrefix) =>
        permissions
            .Where(p => p.ApplicationName != null
                        && p.ApplicationName.StartsWith(applicationPrefix, StringComparison.OrdinalIgnoreCase))
            .Select(p => new
            {
                p.RoleID,
                p.PermissionName,
                Parsed = Guid.TryParse(p.ApplicationName[applicationPrefix.Length..], out var g) ? g : (Guid?)null
            })
            .Where(x => x.Parsed.HasValue)
            .GroupBy(x => x.RoleID)
            .ToDictionary(
                g => g.Key,
                g => g.GroupBy(x => x.Parsed!.Value)
                      .ToDictionary(
                          c => c.Key,
                          c => c.Select(x => x.PermissionName)
                                .Distinct(StringComparer.OrdinalIgnoreCase)
                                .OrderBy(n => n)
                                .ToArray()));

    private static RoleOutput ToRoleOutput(RoleInfo role, MappingContext ctx)
    {
        var channels = ctx.ChannelPermissionsByRoleId.TryGetValue(role.RoleID, out var byChannel)
            ? byChannel
                .Where(kv => ctx.ChannelsByWebsiteChannelGuid.ContainsKey(kv.Key))
                .Select(kv =>
                {
                    var c = ctx.ChannelsByWebsiteChannelGuid[kv.Key];
                    return new RoleChannelOutput
                    {
                        GUID = c.ChannelGUID,
                        ChannelName = c.ChannelName,
                        DisplayName = c.ChannelDisplayName,
                        Permissions = kv.Value
                    };
                })
                .OrderBy(c => c.ChannelName)
                .ToArray()
            : Array.Empty<RoleChannelOutput>();

        var workspaces = ctx.WorkspacePermissionsByRoleId.TryGetValue(role.RoleID, out var byWorkspace)
            ? byWorkspace
                .Where(kv => ctx.WorkspacesById.ContainsKey(kv.Key))
                .Select(kv =>
                {
                    var w = ctx.WorkspacesById[kv.Key];
                    return new RoleWorkspaceOutput
                    {
                        GUID = w.WorkspaceGUID,
                        WorkspaceName = w.WorkspaceName,
                        DisplayName = w.WorkspaceDisplayName,
                        Permissions = kv.Value
                    };
                })
                .OrderBy(w => w.WorkspaceName)
                .ToArray()
            : Array.Empty<RoleWorkspaceOutput>();

        var emailChannels = ctx.EmailChannelPermissionsByRoleId.TryGetValue(role.RoleID, out var byEmailChannel)
            ? byEmailChannel
                .Where(kv => ctx.ChannelsByEmailChannelGuid.ContainsKey(kv.Key))
                .Select(kv =>
                {
                    var c = ctx.ChannelsByEmailChannelGuid[kv.Key];
                    return new RoleEmailChannelOutput
                    {
                        GUID = c.ChannelGUID,
                        ChannelName = c.ChannelName,
                        DisplayName = c.ChannelDisplayName,
                        Permissions = kv.Value
                    };
                })
                .OrderBy(c => c.ChannelName)
                .ToArray()
            : Array.Empty<RoleEmailChannelOutput>();

        var applicationPermissions = ctx.GenericApplicationPermissionsByRoleId.TryGetValue(role.RoleID, out var byApplication)
            ? byApplication
                .Select(kv => new RoleApplicationOutput { ApplicationName = kv.Key, Permissions = kv.Value })
                .OrderBy(a => a.ApplicationName)
                .ToArray()
            : Array.Empty<RoleApplicationOutput>();

        return new RoleOutput
        {
            GUID = role.RoleGUID,
            RoleName = role.RoleName,
            DisplayName = role.RoleDisplayName,
            Description = role.RoleDescription,
            WebsiteChannels = channels,
            ContentHubWorkspaces = workspaces,
            EmailChannels = emailChannels,
            ApplicationPermissions = applicationPermissions
        };
    }
}