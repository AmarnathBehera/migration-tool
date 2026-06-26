using BDO.GWT.Common.Core.Exceptions;
using BDO.GWT.Common.Core.Services;
using BDO.GWT.Domain.Core.Entities;
using BDO.GWT.Domain.Core.Repositories;
using BDO.GWT.Domain.Core.Retrievers;
using BDO.GWT.Domain.Core.Services;
using BDO.GWT.Web.Core.API.Data;
using BDO.GWT.Xperience.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Core.API.Services;

public interface IUserApiPermissionsCrudService
{
    Task<IDictionary<string, ApiEndpointGroupPermissions>> GetUserApiPermissionsByEndpointGroupNameAsync(Guid userGUID, CancellationToken cancellationToken);

    Task<IDictionary<string, ApiEndpointGroupPermissions>> SetUserApiPermissionsByEndpointGroupNameAsync(Guid userGUID, IDictionary<string, ApiEndpointGroupPermissions> apiPermissionsByEndpointGroupName, CancellationToken cancellationToken);
}

public class UserApiPermissionsCrudService : IUserApiPermissionsCrudService
{
    private readonly IRepository<UserApiPermission> _userApiPermissionRepository;
    private readonly IRetriever<ApiMethodGroup> _apiMethodGroupRetriever;
    private readonly IRetriever<User> _userRetriever;
    private readonly ISiteContext _siteContext;
    private readonly IValidationState _validationState;

    public UserApiPermissionsCrudService(IRepository<UserApiPermission> userApiPermissionRepository, IRetriever<ApiMethodGroup> apiMethodGroupRetriever, IRetriever<User> userRetriever, ISiteContext siteContext, IValidationState validationState)
    {
        _userApiPermissionRepository = userApiPermissionRepository;
        _apiMethodGroupRetriever = apiMethodGroupRetriever;
        _userRetriever = userRetriever;
        _siteContext = siteContext;
        _validationState = validationState;
    }

    public async Task<IDictionary<string, ApiEndpointGroupPermissions>> GetUserApiPermissionsByEndpointGroupNameAsync(Guid userGUID, CancellationToken cancellationToken)
    {
        var user = await GetValidatedUserAsync(userGUID, cancellationToken);

        var apiMethodGroups = await _apiMethodGroupRetriever.RetrieveEntitiesAsync(null, cancellationToken);

        var userApiPermissions = await GetUserApiPermissionsAsync(user, apiMethodGroups, cancellationToken);

        return userApiPermissions;
    }

    public async Task<IDictionary<string, ApiEndpointGroupPermissions>> SetUserApiPermissionsByEndpointGroupNameAsync(Guid userGUID, IDictionary<string, ApiEndpointGroupPermissions> apiPermissionsByEndpointGroupName, CancellationToken cancellationToken)
    {
        var user = await GetValidatedUserAsync(userGUID, cancellationToken);

        var apiMethodGroups = (await _apiMethodGroupRetriever.RetrieveEntitiesAsync(null, cancellationToken))
            .ToArray();

        await SetUserApiPermissionsAsync(user, apiMethodGroups, apiPermissionsByEndpointGroupName, cancellationToken);

        var userApiPermissions = await GetUserApiPermissionsAsync(user, apiMethodGroups, cancellationToken);

        return userApiPermissions;
    }

    private async Task<User> GetValidatedUserAsync(Guid userGUID, CancellationToken cancellationToken)
    {
        var user = await _userRetriever.RetrieveEntityAsync(query =>
        {
            query.Global();

            query.Where(x => x.GUID == userGUID);

        }, cancellationToken);

        if (user is null)
        {
            _validationState.AddError(nameof(userGUID), $"Cannot find user with GUID '{userGUID}'.");

            throw new NotFoundException();
        }

        return user;
    }

    private async Task<IDictionary<string, ApiEndpointGroupPermissions>> GetUserApiPermissionsAsync(User user, IEnumerable<ApiMethodGroup> apiMethodGroups, CancellationToken cancellationToken)
    {
        var orderedApiMethodGroups = apiMethodGroups.OrderBy(x => x.Name);

        var userApiPermissions = await _userApiPermissionRepository.RetrieveEntitiesAsync(query =>
        {
            query.Where(x => x.UserID == user.ID);

        }, cancellationToken);

        var userApiPermissionByApiMethodGroupID = userApiPermissions.ToDictionary(x => x.ApiMethodGroupID, x => x);

        var apiPermissionsByEndpointGroupName = new Dictionary<string, ApiEndpointGroupPermissions>();

        foreach (var apiMethodGroup in orderedApiMethodGroups)
        {
            if (userApiPermissionByApiMethodGroupID.TryGetValue(apiMethodGroup.ID, out var userApiPermission))
            {
                apiPermissionsByEndpointGroupName[apiMethodGroup.Name] = new ApiEndpointGroupPermissions
                {
                    Read = userApiPermission.AllowRead,
                    Manage = userApiPermission.AllowManage,
                };
            }
            else
            {
                apiPermissionsByEndpointGroupName[apiMethodGroup.Name] = new ApiEndpointGroupPermissions
                {
                    Read = false,
                    Manage = false,
                };
            }
        }

        return apiPermissionsByEndpointGroupName;
    }

    private async Task SetUserApiPermissionsAsync(User user, IEnumerable<ApiMethodGroup> apiMethodGroups, IDictionary<string, ApiEndpointGroupPermissions> apiPermissionsByEndpointGroupName, CancellationToken cancellationToken)
    {
        var userApiPermissions = await _userApiPermissionRepository.RetrieveEntitiesAsync(query =>
        {
            query.Where(x => x.UserID == user.ID);

        }, cancellationToken);

        var userApiPermissionByApiMethodGroupID = userApiPermissions.ToDictionary(x => x.ApiMethodGroupID, x => x);

        foreach (var apiMethodGroup in apiMethodGroups)
        {
            var requestedPermissions = apiPermissionsByEndpointGroupName.TryGetValue(apiMethodGroup.Name, out var permissions)
                ? permissions
                : new ApiEndpointGroupPermissions
                {
                    Read = false,
                    Manage = false,
                };

            userApiPermissionByApiMethodGroupID.TryGetValue(apiMethodGroup.ID, out var existingUserApiPermission);

            if (requestedPermissions is { Read: false, Manage: false })
            {
                if (existingUserApiPermission is not null)
                {
                    await _userApiPermissionRepository.DeleteEntityAsync(existingUserApiPermission, cancellationToken);
                }
            }
            else
            {
                if (existingUserApiPermission is null)
                {
                    var userApiPermission = new UserApiPermission
                    {
                        UserID = user.ID,
                        SiteID = _siteContext.CurrentSiteID,
                        ApiMethodGroupID = apiMethodGroup.ID,
                        AllowRead = requestedPermissions.Read,
                        AllowManage = requestedPermissions.Manage,
                    };

                    await _userApiPermissionRepository.InsertEntityAsync(userApiPermission, cancellationToken);
                }
                else
                {
                    existingUserApiPermission.AllowRead = requestedPermissions.Read;
                    existingUserApiPermission.AllowManage = requestedPermissions.Manage;

                    await _userApiPermissionRepository.UpdateEntityAsync(existingUserApiPermission, cancellationToken);
                }
            }
        }
    }
}
