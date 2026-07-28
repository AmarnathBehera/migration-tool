using BDO;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Membership;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;

public interface IChannelAccessService
{
    /// <summary>
    /// Returns accessible WebChannelSettingsIDs for the current user.
    /// Returns null if user is global admin (access to everything).
    /// </summary>
    Task<ICollection<int>> GetAccessibleWebChannelSettingsIdsAsync();

    Task<bool> CanAccessAsync(int webChannelSettingsId);
}

public class ChannelAccessService : IChannelAccessService
{
    private readonly IInfoProvider<WebChannelSettingsInfo> _webChannelSettingsProvider;
    private readonly IInfoProvider<ChannelInfo> _channelProvider;
    private readonly IUserInfoProvider _userInfoProvider;
    private readonly IInfoProvider<UserRoleInfo> _userRoleProvider;
    private readonly IInfoProvider<RoleInfo> _roleProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ChannelAccessService(
        IInfoProvider<WebChannelSettingsInfo> webChannelSettingsProvider,
        IInfoProvider<ChannelInfo> channelProvider,
        IUserInfoProvider userInfoProvider,
        IInfoProvider<UserRoleInfo> userRoleProvider,
        IInfoProvider<RoleInfo> roleProvider,
        IHttpContextAccessor httpContextAccessor)
    {
        _webChannelSettingsProvider = webChannelSettingsProvider;
        _channelProvider = channelProvider;
        _userInfoProvider = userInfoProvider;
        _userRoleProvider = userRoleProvider;
        _roleProvider = roleProvider;
        _httpContextAccessor = httpContextAccessor;
    }

    private UserInfo GetCurrentUser()
    {
        var userName = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
        if (string.IsNullOrEmpty(userName))
            return null;

        return _userInfoProvider.Get(userName);
    }

    public async Task<ICollection<int>> GetAccessibleWebChannelSettingsIdsAsync()
    {
        var currentUser = GetCurrentUser();
        if (currentUser == null)
            return Array.Empty<int>();

        bool isAdminRole = currentUser.IsInRole("Administrator");

        if (isAdminRole)
        {
            return null;
        }

        var userRoleIds = _userRoleProvider.Get()
            .WhereEquals(nameof(UserRoleInfo.UserID), currentUser.UserID)
            .Column(nameof(UserRoleInfo.RoleID))
            .GetEnumerableTypedResult()
            .Select(ur => ur.RoleID)
            .ToList();

        if (userRoleIds.Count == 0)
            return Array.Empty<int>();

        // Get role names for this user
        var userRoleNames = _roleProvider.Get()
            .WhereIn(nameof(RoleInfo.RoleID), userRoleIds)
            .Column(nameof(RoleInfo.RoleName))
            .GetEnumerableTypedResult()
            .Select(r => r.RoleName.ToLowerInvariant())
            .ToList();

          var allChannels = _channelProvider.Get().GetEnumerableTypedResult();

        var accessibleChannelIds = allChannels
            .Where(ch =>
            {
                var channelKey = ch.ChannelName.ToLowerInvariant();
                return userRoleNames.Any(role =>
                    role.StartsWith(channelKey + "-") || role == channelKey);
            })
            .Select(ch => ch.ChannelID)
            .ToList();

        if (accessibleChannelIds.Count == 0)
            return Array.Empty<int>();

        return _webChannelSettingsProvider.Get()
            .WhereIn(nameof(WebChannelSettingsInfo.WebChannelSettingsChannelID), accessibleChannelIds)
            .GetEnumerableTypedResult()
            .Select(s => s.WebChannelSettingsID)
            .ToList();
    }

    public async Task<bool> CanAccessAsync(int webChannelSettingsId)
    {
        var accessibleIds = await GetAccessibleWebChannelSettingsIdsAsync();
        // null = global admin = can access everything
        return accessibleIds == null || accessibleIds.Contains(webChannelSettingsId);
    }
}