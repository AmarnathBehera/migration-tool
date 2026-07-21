using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Membership;
using CMS.Websites;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;

public interface IChannelFilterService
{
    Task<ICollection<int>> GetAccessibleChannelIdsAsync();
}

public class ChannelFilterService : IChannelFilterService
{
    private const string WEBPAGES_APPLICATION_PREFIX = "Kentico.Xperience.Application.WebPages_";
    private const string VIEW_PERMISSION = "View"; // = "Access channel" in the Role management UI

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserInfoProvider _userInfoProvider;
    private readonly IInfoProvider<UserRoleInfo> _userRoleProvider;
    private readonly IInfoProvider<WebsiteChannelInfo> _websiteChannelProvider;
    private readonly IInfoProvider<ApplicationPermissionInfo> _applicationPermissionProvider;

    public ChannelFilterService(
        IHttpContextAccessor httpContextAccessor,
        IUserInfoProvider userInfoProvider,
        IInfoProvider<UserRoleInfo> userRoleProvider,
        IInfoProvider<WebsiteChannelInfo> websiteChannelProvider,
        IInfoProvider<ApplicationPermissionInfo> applicationPermissionProvider)
    {
        _httpContextAccessor = httpContextAccessor;
        _userInfoProvider = userInfoProvider;
        _userRoleProvider = userRoleProvider;
        _websiteChannelProvider = websiteChannelProvider;
        _applicationPermissionProvider = applicationPermissionProvider;
    }

    private UserInfo GetCurrentUser()
    {
        var userName = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
        if (string.IsNullOrEmpty(userName))
            return null;

        return _userInfoProvider.Get(userName);
    }

    public async Task<ICollection<int>> GetAccessibleChannelIdsAsync()
    {
        var currentUser = GetCurrentUser();
        if (currentUser == null)
            return Array.Empty<int>();

        if (currentUser.IsInRole("Administrator"))
        {
            return null; // global admin = no filtering
        }

        var userRoleIds = _userRoleProvider.Get()
            .WhereEquals(nameof(UserRoleInfo.UserID), currentUser.UserID)
            .Column(nameof(UserRoleInfo.RoleID))
            .GetEnumerableTypedResult()
            .Select(ur => ur.RoleID)
            .ToList();

        if (userRoleIds.Count == 0)
            return Array.Empty<int>();

        // Find "View" grants (= "Access channel") on WebPages-type applications for this user's roles
        var grantedApplicationNames = _applicationPermissionProvider.Get()
            .WhereIn(nameof(ApplicationPermissionInfo.RoleID), userRoleIds)
            .WhereEquals(nameof(ApplicationPermissionInfo.PermissionName), VIEW_PERMISSION)
            .WhereStartsWith(nameof(ApplicationPermissionInfo.ApplicationName), WEBPAGES_APPLICATION_PREFIX)
            .Column(nameof(ApplicationPermissionInfo.ApplicationName))
            .GetEnumerableTypedResult()
            .Select(p => p.ApplicationName)
            .Distinct()
            .ToList();

        if (grantedApplicationNames.Count == 0)
            return Array.Empty<int>();

        var grantedGuids = grantedApplicationNames
     .Select(name => name.Substring(WEBPAGES_APPLICATION_PREFIX.Length))
     .Where(guidStr => Guid.TryParse(guidStr, out _))
     .Select(Guid.Parse)
     .ToList();

        return _websiteChannelProvider.Get()
    .WhereIn(nameof(WebsiteChannelInfo.WebsiteChannelGUID), grantedGuids)
    .Column(nameof(WebsiteChannelInfo.WebsiteChannelChannelID))
    .GetEnumerableTypedResult()
    .Select(wc => wc.WebsiteChannelChannelID)
    .ToList();
    }
}