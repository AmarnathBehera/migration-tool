using BDO.GWT.Domain.Core.Constants;
using BDO.GWT.GLOBAL.API.Data;
using CMS.DataEngine;
using CMS.Membership;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.API.Services;

public interface IUserApiPermissionsCrudService
{
    Task<IDictionary<string, ApiEndpointGroupPermissions>> GetUserApiPermissionsByEndpointGroupNameAsync(int websiteChannelID, Guid userGUID, CancellationToken cancellationToken);

    Task<IDictionary<string, ApiEndpointGroupPermissions>> SetUserApiPermissionsByEndpointGroupNameAsync(int websiteChannelID, Guid userGUID, IDictionary<string, ApiEndpointGroupPermissions> apiPermissionsByEndpointGroupName, CancellationToken cancellationToken);
}

public class UserApiPermissionsCrudService : IUserApiPermissionsCrudService
{

    private static readonly string[] ApiMethodGroupNames = typeof(ApiMethodGroupName)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(field => field.FieldType == typeof(string))
        .Select(field => (string)field.GetValue(null)!)
        .Where(name => !string.IsNullOrEmpty(name))
        .ToArray();

    private readonly IInfoProvider<UserInfo> _userInfoProvider;
    private readonly IInfoProvider<Api_permissionsInfo> _apiPermissionInfoProvider;

    public UserApiPermissionsCrudService(
        IInfoProvider<UserInfo> userInfoProvider,
        IInfoProvider<Api_permissionsInfo> apiPermissionInfoProvider)
    {
        _userInfoProvider = userInfoProvider;
        _apiPermissionInfoProvider = apiPermissionInfoProvider;
    }

    public async Task<IDictionary<string, ApiEndpointGroupPermissions>> GetUserApiPermissionsByEndpointGroupNameAsync(int websiteChannelID, Guid userGUID, CancellationToken cancellationToken)
    {
        var user = await GetValidatedUserAsync(userGUID, cancellationToken);

        var permissions = await GetUserApiPermissionsAsync(websiteChannelID, user.UserID, cancellationToken);

        return permissions;
    }

    public async Task<IDictionary<string, ApiEndpointGroupPermissions>> SetUserApiPermissionsByEndpointGroupNameAsync(int websiteChannelID, Guid userGUID, IDictionary<string, ApiEndpointGroupPermissions> apiPermissionsByEndpointGroupName, CancellationToken cancellationToken)
    {
        var user = await GetValidatedUserAsync(userGUID, cancellationToken);

        await SetUserApiPermissionsAsync(websiteChannelID, user.UserID, apiPermissionsByEndpointGroupName, cancellationToken);

        var permissions = await GetUserApiPermissionsAsync(websiteChannelID, user.UserID, cancellationToken);

        return permissions;
    }


    private async Task<UserInfo> GetValidatedUserAsync(Guid userGUID, CancellationToken cancellationToken)
    {
        var user = (await _userInfoProvider.Get()
            .WhereEquals(nameof(UserInfo.UserGUID), userGUID)
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
            .FirstOrDefault();

        if (user is null)
        {
            // Matches the error-handling style already used in this project's UserCrudService.
            throw new InvalidOperationException($"Cannot find user with GUID '{userGUID}'.");
        }

        return user;
    }


    private async Task<IDictionary<string, ApiEndpointGroupPermissions>> GetUserApiPermissionsAsync(int websiteChannelID, int userID, CancellationToken cancellationToken)
    {
        var rawPermissions = await GetRawExistingPermissionsAsync(websiteChannelID, userID, cancellationToken);


        var existingPermissionsByGroupName = DeduplicateByGroupName(rawPermissions);

        var result = new Dictionary<string, ApiEndpointGroupPermissions>();

        foreach (var groupName in ApiMethodGroupNames.OrderBy(x => x, StringComparer.Ordinal))
        {
            if (existingPermissionsByGroupName.TryGetValue(groupName, out var permission))
            {
                result[groupName] = new ApiEndpointGroupPermissions
                {
                    Read = permission.Read,
                    Manage = permission.Manage,
                };
            }
            else
            {
                result[groupName] = new ApiEndpointGroupPermissions
                {
                    Read = false,
                    Manage = false,
                };
            }
        }

        return result;
    }


    private async Task SetUserApiPermissionsAsync(int websiteChannelID, int userID, IDictionary<string, ApiEndpointGroupPermissions> apiPermissionsByEndpointGroupName, CancellationToken cancellationToken)
    {
        var rawPermissions = await GetRawExistingPermissionsAsync(websiteChannelID, userID, cancellationToken);


        var existingPermissionsByGroupName = await DeduplicateAndCleanUpAsync(rawPermissions, cancellationToken);

        var userIDValue = userID.ToString();
        var websiteChannelIDValue = websiteChannelID.ToString();

        foreach (var groupName in ApiMethodGroupNames)
        {
            var requestedPermissions = apiPermissionsByEndpointGroupName.TryGetValue(groupName, out var permissions)
                ? permissions
                : new ApiEndpointGroupPermissions { Read = false, Manage = false };

            existingPermissionsByGroupName.TryGetValue(groupName, out var existingPermission);

            if (!requestedPermissions.Read && !requestedPermissions.Manage)
            {
                if (existingPermission is not null)
                {
                    await _apiPermissionInfoProvider.DeleteAsync(existingPermission, cancellationToken);
                }
            }
            else if (existingPermission is null)
            {
                var newPermission = new Api_permissionsInfo
                {
                    APIPermissionWebChannelSettingID = websiteChannelIDValue,
                    MethodGroup = groupName,
                    User = userIDValue,
                    Read = requestedPermissions.Read,
                    Manage = requestedPermissions.Manage,
                };

                await _apiPermissionInfoProvider.SetAsync(newPermission, cancellationToken);
            }
            else
            {
                existingPermission.APIPermissionWebChannelSettingID = websiteChannelIDValue;
                existingPermission.Read = requestedPermissions.Read;
                existingPermission.Manage = requestedPermissions.Manage;

                await _apiPermissionInfoProvider.SetAsync(existingPermission, cancellationToken);
            }
        }
    }


    private async Task<IEnumerable<Api_permissionsInfo>> GetRawExistingPermissionsAsync(int websiteChannelID, int userID, CancellationToken cancellationToken)
    {
        var userIDValue = userID.ToString();
        var websiteChannelIDValue = websiteChannelID.ToString();

        var existingPermissions = await _apiPermissionInfoProvider.Get()
            .WhereEquals(nameof(Api_permissionsInfo.User), userIDValue)
            .WhereEquals(nameof(Api_permissionsInfo.APIPermissionWebChannelSettingID), websiteChannelIDValue)
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken);

        return existingPermissions.Where(x => !string.IsNullOrEmpty(x.MethodGroup));
    }

    private static IDictionary<string, Api_permissionsInfo> DeduplicateByGroupName(IEnumerable<Api_permissionsInfo> permissions)
    {
        return permissions
            .GroupBy(x => x.MethodGroup)
            .ToDictionary(group => group.Key, group => group.OrderByDescending(x => x.Api_permissionsID).First());
    }

    private async Task<IDictionary<string, Api_permissionsInfo>> DeduplicateAndCleanUpAsync(IEnumerable<Api_permissionsInfo> permissions, CancellationToken cancellationToken)
    {
        var result = new Dictionary<string, Api_permissionsInfo>();

        foreach (var group in permissions.GroupBy(x => x.MethodGroup))
        {
            var orderedRows = group.OrderByDescending(x => x.Api_permissionsID).ToArray();

            result[group.Key] = orderedRows[0];

            foreach (var duplicateRow in orderedRows.Skip(1))
            {
                await _apiPermissionInfoProvider.DeleteAsync(duplicateRow, cancellationToken);
            }
        }

        return result;
    }
}
