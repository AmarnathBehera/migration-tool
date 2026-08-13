using BDO;
using CMS.DataEngine;
using CMS.Membership;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace BDO.GWT.GLOBAL.Services;

public interface IApiPermissionService
{
    IReadOnlyList<UserOption> GetUsers();
    IReadOnlyList<MethodGroupOption> GetMethodGroups();
    IReadOnlyList<MatrixRow> GetMatrixByUser(int userId);
    IReadOnlyList<MatrixRow> GetMatrixByMethodGroup(int methodGroupId);
    void SetPermission(int userId, int methodGroupId, bool allowRead, bool allowManage);
}

public sealed class ApiPermissionService : IApiPermissionService
{
    private static readonly IReadOnlyList<MethodGroupOption> MethodGroups = new List<MethodGroupOption>
    {
        new() { Id = 1,  DisplayName = "Users" },
        new() { Id = 2,  DisplayName = "Events" },
        new() { Id = 3,  DisplayName = "Insights" },
        new() { Id = 4,  DisplayName = "News" },
        new() { Id = 5,  DisplayName = "People" },
        new() { Id = 6,  DisplayName = "Webinars" },
        new() { Id = 7,  DisplayName = "Business lines" },
        new() { Id = 8,  DisplayName = "Services" },
        new() { Id = 9,  DisplayName = "Service sections" },
        new() { Id = 10, DisplayName = "Service areas" },
        new() { Id = 11, DisplayName = "Event sections" },
        new() { Id = 12, DisplayName = "Content" },
        new() { Id = 13, DisplayName = "Career Job Opportunities" },
        new() { Id = 14, DisplayName = "Forms" },
        new() { Id = 15, DisplayName = "Page Attachments" },
        new() { Id = 16, DisplayName = "Location Offices" },
        new() { Id = 17, DisplayName = "Deals" },
        new() { Id = 18, DisplayName = "Media File" },
    };

    private readonly IInfoProvider<UserInfo> userProvider;
    private readonly IInfoProvider<Api_permissionsInfo> permissionProvider;

    public ApiPermissionService(
        IInfoProvider<UserInfo> userProvider,
        IInfoProvider<Api_permissionsInfo> permissionProvider)
    {
        this.userProvider = userProvider;
        this.permissionProvider = permissionProvider;
    }

    public IReadOnlyList<UserOption> GetUsers() =>
        userProvider.Get()
            .OrderBy(nameof(UserInfo.UserName))
            .Select(u => new UserOption { Id = u.UserID, DisplayName = GetDisplayName(u) })
            .ToList();

    public IReadOnlyList<MethodGroupOption> GetMethodGroups() => MethodGroups;

    public IReadOnlyList<MatrixRow> GetMatrixByUser(int userId)
    {
        var permissions = GetPermissionLookup();

        return MethodGroups.Select(g =>
        {
            permissions.TryGetValue((userId, g.Id), out var perm);
            return new MatrixRow
            {
                UserId = userId,
                MethodGroupId = g.Id,
                MethodGroupName = g.DisplayName,
                AllowRead = perm.Read,
                AllowManage = perm.Manage,
            };
        }).ToList();
    }

    public IReadOnlyList<MatrixRow> GetMatrixByMethodGroup(int methodGroupId)
    {
        var group = MethodGroups.FirstOrDefault(g => g.Id == methodGroupId);
        var permissions = GetPermissionLookup();

        var users = userProvider.Get().OrderBy(nameof(UserInfo.UserName)).ToList();

        return users.Select(u =>
        {
            permissions.TryGetValue((u.UserID, methodGroupId), out var perm);
            return new MatrixRow
            {
                UserId = u.UserID,
                MethodGroupId = methodGroupId,
                UserDisplayName = GetDisplayName(u),
                MethodGroupName = group?.DisplayName ?? string.Empty,
                AllowRead = perm.Read,
                AllowManage = perm.Manage,
            };
        }).ToList();
    }

    public void SetPermission(int userId, int methodGroupId, bool allowRead, bool allowManage)
    {
        var record = GetPermissionRecord(userId, methodGroupId)
            ?? new Api_permissionsInfo
            {
                User = userId.ToString(CultureInfo.InvariantCulture),
                MethodGroup = methodGroupId.ToString(CultureInfo.InvariantCulture),
                APIPermissionWebChannelSettingID = ResolveWebChannelSettingId(),
            };

        record.Read = allowRead;
        record.Manage = allowManage;

        permissionProvider.Set(record);
    }

    // The edit page persists the current WebChannelSettingsId on the backing record; reuse it.
    private string ResolveWebChannelSettingId()
    {
        foreach (var record in permissionProvider.Get())
        {
            var settingId = record.APIPermissionWebChannelSettingID;
            if (!string.IsNullOrEmpty(settingId))
            {
                return settingId;
            }
        }

        return "0";
    }

    private Dictionary<(int UserId, int MethodGroupId), (bool Read, bool Manage)> GetPermissionLookup()
    {
        var lookup = new Dictionary<(int, int), (bool, bool)>();

        foreach (var record in permissionProvider.Get())
        {
            if (!TryParseKey(record, out var userId, out var methodGroupId))
            {
                continue;
            }

            lookup[(userId, methodGroupId)] = (record.Read, record.Manage);
        }

        return lookup;
    }

    private Api_permissionsInfo GetPermissionRecord(int userId, int methodGroupId)
    {
        foreach (var record in permissionProvider.Get())
        {
            if (TryParseKey(record, out var u, out var m) && u == userId && m == methodGroupId)
            {
                return record;
            }
        }

        return null;
    }

    private static bool TryParseKey(Api_permissionsInfo record, out int userId, out int methodGroupId)
    {
        methodGroupId = 0;
        return int.TryParse(record.User, out userId)
            && int.TryParse(record.MethodGroup, out methodGroupId);
    }

    private static string GetDisplayName(UserInfo user)
    {
        // Mirrors: FirstName + ' ' + LastName + ' (' + UserName + ')'
        var first = user.GetStringValue("FirstName", string.Empty);
        var last = user.GetStringValue("LastName", string.Empty);
        return $"{first} {last} ({user.UserName})";
    }
}

public sealed class UserOption
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
}

public sealed class MethodGroupOption
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public sealed class MatrixRow
{
    public int UserId { get; set; }
    public int MethodGroupId { get; set; }
    public string UserDisplayName { get; set; } = string.Empty;
    public string MethodGroupName { get; set; } = string.Empty;
    public bool AllowRead { get; set; }
    public bool AllowManage { get; set; }
}
