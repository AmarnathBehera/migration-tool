using BDO;
using BDO.GWT.Domain.Core.Constants;
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
    IReadOnlyList<MatrixRow> GetMatrixByUser(int userId, int webChannelSettingId);
    IReadOnlyList<MatrixRow> GetMatrixByMethodGroup(int methodGroupId, int webChannelSettingId);
    void SetPermission(int userId, int methodGroupId, bool allowRead, bool allowManage, int webChannelSettingId);
}

public sealed class ApiPermissionService : IApiPermissionService
{
    private static readonly IReadOnlyList<MethodGroupOption> MethodGroups = new List<MethodGroupOption>
    {
        new() { Id = 1, DisplayName = ApiMethodGroupName.Users },
        new() { Id = 2, DisplayName = ApiMethodGroupName.Events },
        new() { Id = 3, DisplayName = ApiMethodGroupName.Insights },
        new() { Id = 4, DisplayName = ApiMethodGroupName.News },
        new() { Id = 5, DisplayName = ApiMethodGroupName.People },
        new() { Id = 6, DisplayName = ApiMethodGroupName.Webinars },
        new() { Id = 7, DisplayName = ApiMethodGroupName.BusinessLines },
        new() { Id = 8, DisplayName = ApiMethodGroupName.Services },
        new() { Id = 9, DisplayName = ApiMethodGroupName.ServiceSections },
        new() { Id = 10, DisplayName = ApiMethodGroupName.ServiceAreas },
        new() { Id = 11, DisplayName = ApiMethodGroupName.EventSections },
        new() { Id = 12, DisplayName = ApiMethodGroupName.Content },
        new() { Id = 13, DisplayName = ApiMethodGroupName.CareerJobOpportunities },
        new() { Id = 14, DisplayName = ApiMethodGroupName.Forms },
        new() { Id = 15, DisplayName = ApiMethodGroupName.PageAttachments },
        new() { Id = 16, DisplayName = ApiMethodGroupName.LocationOffices },
        new() { Id = 17, DisplayName = ApiMethodGroupName.Deals },
        new() { Id = 18, DisplayName = ApiMethodGroupName.MediaFile },
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

    public IReadOnlyList<MatrixRow> GetMatrixByUser(int userId, int webChannelSettingId)
    {
        var permissions = GetPermissionLookup(webChannelSettingId);

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

    public IReadOnlyList<MatrixRow> GetMatrixByMethodGroup(int methodGroupId, int webChannelSettingId)
    {
        var group = MethodGroups.FirstOrDefault(g => g.Id == methodGroupId);
        var permissions = GetPermissionLookup(webChannelSettingId);

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

    public void SetPermission(int userId, int methodGroupId, bool allowRead, bool allowManage, int webChannelSettingId)
    {
        var record = GetPermissionRecord(userId, methodGroupId, webChannelSettingId)
            ?? new Api_permissionsInfo
            {
                User = userId.ToString(CultureInfo.InvariantCulture),
                MethodGroup = GetMethodGroupName(methodGroupId),
                APIPermissionWebChannelSettingID = webChannelSettingId.ToString(CultureInfo.InvariantCulture),
            };

        record.MethodGroup = GetMethodGroupName(methodGroupId);
        record.Read = allowRead;
        record.Manage = allowManage;

        permissionProvider.Set(record);
    }

    private Dictionary<(int UserId, int MethodGroupId), (bool Read, bool Manage)> GetPermissionLookup(int webChannelSettingId)
    {
        var lookup = new Dictionary<(int, int), (bool, bool)>();

        foreach (var record in permissionProvider.Get())
        {
            if (!MatchesChannel(record, webChannelSettingId)
                || !TryParseKey(record, out var userId, out var methodGroupId))
            {
                continue;
            }

            lookup[(userId, methodGroupId)] = (record.Read, record.Manage);
        }

        return lookup;
    }

    private Api_permissionsInfo GetPermissionRecord(int userId, int methodGroupId, int webChannelSettingId)
    {
        foreach (var record in permissionProvider.Get())
        {
            if (MatchesChannel(record, webChannelSettingId)
                && TryParseKey(record, out var u, out var m)
                && u == userId && m == methodGroupId)
            {
                return record;
            }
        }

        return null;
    }

    private static bool MatchesChannel(Api_permissionsInfo record, int webChannelSettingId) =>
        int.TryParse(record.APIPermissionWebChannelSettingID, out var id) && id == webChannelSettingId;

    private static bool TryParseKey(Api_permissionsInfo record, out int userId, out int methodGroupId)
    {
        methodGroupId = 0;
        if (!int.TryParse(record.User, out userId))
        {
            return false;
        }

        methodGroupId = MethodGroups.FirstOrDefault(group =>
            group.DisplayName == record.MethodGroup)?.Id ?? 0;
        return methodGroupId > 0;
    }

    private static string GetMethodGroupName(int methodGroupId) =>
        MethodGroups.FirstOrDefault(group => group.Id == methodGroupId)?.DisplayName
        ?? methodGroupId.ToString(CultureInfo.InvariantCulture);

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
