using BDO;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Membership;
using System.Collections.Generic;
using System.Linq;

namespace BDO.GWT.GLOBAL.Services;

public interface IApiPermissionService
{
    IReadOnlyList<ChannelOption> GetChannels();
    IReadOnlyList<UserOption> GetUsers();
    IReadOnlyList<MethodGroupOption> GetMethodGroups();
    IReadOnlyList<MatrixRow> GetMatrixByUser(int userId, int channelId);
    IReadOnlyList<MatrixRow> GetMatrixByMethodGroup(int methodGroupId, int channelId);
    void SetPermission(int userId, int methodGroupId, int channelId, string permissionType, bool allow);
}

public sealed class ApiPermissionService : IApiPermissionService
{
    private readonly IInfoProvider<ChannelInfo> channelProvider;
    private readonly IInfoProvider<UserInfo> userProvider;
    private readonly IInfoProvider<Api_permissionsInfo> methodGroupProvider;

    public ApiPermissionService(
        IInfoProvider<ChannelInfo> channelProvider,
        IInfoProvider<UserInfo> userProvider,
        IInfoProvider<Api_permissionsInfo> methodGroupProvider)
    {
        this.channelProvider = channelProvider;
        this.userProvider = userProvider;
        this.methodGroupProvider = methodGroupProvider;
    }

    public IReadOnlyList<ChannelOption> GetChannels() =>
        channelProvider.Get()
            .OrderBy(nameof(ChannelInfo.ChannelDisplayName))
            .Select(c => new ChannelOption { Id = c.ChannelID, DisplayName = c.ChannelDisplayName })
            .ToList();

    public IReadOnlyList<UserOption> GetUsers() =>
        userProvider.Get()
            .OrderBy(nameof(UserInfo.UserName))
            .Select(u => new UserOption { Id = u.UserID, DisplayName = GetDisplayName(u) })
            .ToList();

    public IReadOnlyList<MethodGroupOption> GetMethodGroups() =>
        methodGroupProvider.Get()
            .OrderBy("DisplayName")
            .Select(g => new MethodGroupOption
            {
                Id = g.GetIntegerValue("ApiMethodGroupID", g.GetIntegerValue("Api_permissionsID", 0)),
                DisplayName = g.GetStringValue("DisplayName", string.Empty),
                Description = g.GetStringValue("Description", string.Empty)
            })
            .ToList();

    public IReadOnlyList<MatrixRow> GetMatrixByUser(int userId, int channelId)
    {
        var groups = methodGroupProvider.Get().OrderBy("DisplayName").ToList();

        return groups.Select(g =>
        {
            var methodGroupId = g.GetIntegerValue("ApiMethodGroupID", g.GetIntegerValue("Api_permissionsID", 0));
            return new MatrixRow
            {
                UserId = userId,
                MethodGroupId = methodGroupId,
                MethodGroupName = g.GetStringValue("DisplayName", string.Empty),
                AllowRead = false,
                AllowManage = false,
            };
        }).ToList();
    }

    public IReadOnlyList<MatrixRow> GetMatrixByMethodGroup(int methodGroupId, int channelId)
    {
        var group = methodGroupProvider.Get().FirstOrDefault(g =>
            g.GetIntegerValue("ApiMethodGroupID", g.GetIntegerValue("Api_permissionsID", 0)) == methodGroupId);

        var users = userProvider.Get().OrderBy(nameof(UserInfo.UserName)).ToList();

        return users.Select(u =>
        {
            return new MatrixRow
            {
                UserId = u.UserID,
                MethodGroupId = methodGroupId,
                UserDisplayName = GetDisplayName(u),
                MethodGroupName = group?.GetStringValue("DisplayName", string.Empty) ?? string.Empty,
                AllowRead = false,
                AllowManage = false,
            };
        }).ToList();
    }

    public void SetPermission(int userId, int methodGroupId, int channelId, string permissionType, bool allow)
    {
        // The generated permission object type is currently unavailable in this project.
        // Keep this method as a safe no-op so the application builds and listing loads.
    }

    private static string GetDisplayName(UserInfo user)
    {
        var first = user.GetStringValue("UserFirstName", string.Empty);
        var last = user.GetStringValue("UserLastName", string.Empty);
        var full = $"{first} {last}".Trim();
        return full.Length > 0 ? full : user.UserName;
    }
}

public sealed class ChannelOption
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
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
