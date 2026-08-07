using BDO.GWT.GLOBAL.API.Data;
using CMS.Base;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Membership;
using CMS.Websites;
using CMS.Websites.Internal;
using System.Security.Claims;

namespace BDO.GWT.GLOBAL.API.Services;

public interface IUserCrudService
{
    Task<IEnumerable<UserOutput>> GetUsersAsync(string? userName, CancellationToken cancellationToken);
    Task<UserOutput?> GetUserAsync(Guid userGUID, CancellationToken cancellationToken);
    Task<UserOutput> CreateUserAsync(UserInput userInput, CancellationToken cancellationToken);
    Task<UserOutput> UpdateUserAsync(Guid userGUID, UserInput userInput, CancellationToken cancellationToken);
    Task DeleteUserAsync(Guid userGUID, CancellationToken cancellationToken);
}

public class UserCrudService : IUserCrudService
{
    private readonly IInfoProvider<UserInfo> _userInfoProvider;
    private readonly IInfoProvider<UserRoleInfo> _userRoleInfoProvider;
    private readonly IInfoProvider<RoleInfo> _roleInfoProvider;
    private readonly IInfoProvider<WebPageAclInfo> _webPageAclInfoProvider;
    private readonly IInfoProvider<WebPageAclRoleInfo> _webPageAclRoleInfoProvider;
    private readonly IInfoProvider<WebsiteChannelInfo> _websiteChannelInfoProvider;
    private readonly IInfoProvider<ChannelInfo> _channelInfoProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserCrudService(
        IInfoProvider<UserInfo> userInfoProvider,
        IInfoProvider<UserRoleInfo> userRoleInfoProvider,
        IInfoProvider<RoleInfo> roleInfoProvider,
        IInfoProvider<WebPageAclInfo> webPageAclInfoProvider,
        IInfoProvider<WebPageAclRoleInfo> webPageAclRoleInfoProvider,
        IInfoProvider<WebsiteChannelInfo> websiteChannelInfoProvider,
        IInfoProvider<ChannelInfo> channelInfoProvider,
        IHttpContextAccessor httpContextAccessor)
    {
        _userInfoProvider = userInfoProvider;
        _userRoleInfoProvider = userRoleInfoProvider;
        _roleInfoProvider = roleInfoProvider;
        _webPageAclInfoProvider = webPageAclInfoProvider;
        _webPageAclRoleInfoProvider = webPageAclRoleInfoProvider;
        _websiteChannelInfoProvider = websiteChannelInfoProvider;
        _channelInfoProvider = channelInfoProvider;
        _httpContextAccessor = httpContextAccessor;
    }

    // ---------- READ ----------

    public async Task<IEnumerable<UserOutput>> GetUsersAsync(string? userName, CancellationToken cancellationToken)
    {
        var query = _userInfoProvider.Get();

        if (!string.IsNullOrEmpty(userName))
        {
            query = query.WhereContains(nameof(UserInfo.UserName), userName);
        }

        var users = await query.GetEnumerableTypedResultAsync(cancellationToken: cancellationToken);

        var context = await BuildContextAsync(cancellationToken);

        return users.Select(u => ToUserOutput(u, context));
    }

    public async Task<UserOutput?> GetUserAsync(Guid userGUID, CancellationToken cancellationToken)
    {
        var user = (await _userInfoProvider.Get()
            .WhereEquals(nameof(UserInfo.UserGUID), userGUID)
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
            .FirstOrDefault();

        if (user is null)
        {
            return null;
        }

        var context = await BuildContextAsync(cancellationToken);

        return ToUserOutput(user, context);
    }

    // ---------- CREATE ----------

    public async Task<UserOutput> CreateUserAsync(UserInput userInput, CancellationToken cancellationToken)
    {
        await ValidateUserInputAsync(null, userInput, cancellationToken);

        var user = new UserInfo
        {
            UserName = userInput.General.UserName,
            FirstName = userInput.General.FirstName,
            LastName = userInput.General.LastName,
            Email = userInput.General.Email,
            UserEnabled = userInput.General.Enabled,
            UserIsExternal = userInput.General.IsExternalUser,
            UserPreferredLanguage = userInput.General.PreferredLanguage,
            //UserAdministrationAccess = true
        };

        await _userInfoProvider.SetAsync(user, cancellationToken);

        await SyncUserRolesAsync(user.UserID, userInput.Roles, cancellationToken);

        var context = await BuildContextAsync(cancellationToken);

        return ToUserOutput(user, context);
    }

    // ---------- UPDATE ----------

    public async Task<UserOutput> UpdateUserAsync(Guid userGUID, UserInput userInput, CancellationToken cancellationToken)
    {
        var user = await GetValidatedUserAsync(userGUID, cancellationToken);

        await ValidateUserInputAsync(user.UserID, userInput, cancellationToken);

        user.UserName = userInput.General.UserName;
        user.FirstName = userInput.General.FirstName;
        user.LastName = userInput.General.LastName;
        user.Email = userInput.General.Email;
        user.UserEnabled = userInput.General.Enabled;
        user.UserIsExternal = userInput.General.IsExternalUser;
        user.UserPreferredLanguage = userInput.General.PreferredLanguage;

        var contextUser = await GetContextUserAsync(cancellationToken);

        using (new CMSActionContext(contextUser))
        {
            await _userInfoProvider.SetAsync(user, cancellationToken);

            await SyncUserRolesAsync(user.UserID, userInput.Roles, cancellationToken);
        }

        var context = await BuildContextAsync(cancellationToken);

        return ToUserOutput(user, context);
    }

    // ---------- DELETE ----------

    public async Task DeleteUserAsync(Guid userGUID, CancellationToken cancellationToken)
    {
        var user = await GetValidatedUserAsync(userGUID, cancellationToken);

        var contextUser = await GetContextUserAsync(cancellationToken);

        using (new CMSActionContext(contextUser))
        {
            await _userInfoProvider.DeleteAsync(user, cancellationToken);
        }
    }

    // ---------- CALLER CONTEXT ----------

    private async Task<UserInfo> GetContextUserAsync(CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("No HTTP context available.");

        var callerIdentifier = httpContext.User.FindFirst(ClaimTypes.Email)?.Value
            ?? throw new InvalidOperationException("Caller identity claim not found.");

        var callerUser = (await _userInfoProvider.Get()
            .WhereEquals(nameof(UserInfo.UserName), callerIdentifier)
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
            .FirstOrDefault();

        if (callerUser is null || !callerUser.UserAdministrationAccess)
        {
            throw new InvalidOperationException(
                $"Caller '{callerIdentifier}' was not found in Kentico or lacks administration access.");
        }

        return callerUser;
    }

    // ---------- VALIDATION HELPERS ----------

    private async Task<UserInfo> GetValidatedUserAsync(Guid userGUID, CancellationToken cancellationToken)
    {
        var user = (await _userInfoProvider.Get()
            .WhereEquals(nameof(UserInfo.UserGUID), userGUID)
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
            .FirstOrDefault();

        if (user is null)
        {
            throw new InvalidOperationException($"Cannot find user with GUID '{userGUID}'.");
        }

        return user;
    }

    private async Task ValidateUserInputAsync(int? existingUserId, UserInput userInput, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(userInput.General.UserName))
        {
            throw new InvalidOperationException("User name is required.");
        }

        var userNameQuery = _userInfoProvider.Get()
            .WhereEquals(nameof(UserInfo.UserName), userInput.General.UserName);

        if (existingUserId.HasValue)
        {
            userNameQuery = userNameQuery.WhereNotEquals(nameof(UserInfo.UserID), existingUserId.Value);
        }

        var userNameExists = (await userNameQuery.GetEnumerableTypedResultAsync(cancellationToken: cancellationToken)).Any();

        if (userNameExists)
        {
            throw new InvalidOperationException($"User name '{userInput.General.UserName}' already exists.");
        }

        if (!string.IsNullOrEmpty(userInput.General.Email))
        {
            var emailQuery = _userInfoProvider.Get()
                .WhereEquals(nameof(UserInfo.Email), userInput.General.Email);

            if (existingUserId.HasValue)
            {
                emailQuery = emailQuery.WhereNotEquals(nameof(UserInfo.UserID), existingUserId.Value);
            }

            var emailExists = (await emailQuery.GetEnumerableTypedResultAsync(cancellationToken: cancellationToken)).Any();

            if (emailExists)
            {
                throw new InvalidOperationException($"Email '{userInput.General.Email}' already exists.");
            }
        }

        var roleGuids = userInput.Roles.Select(r => r.GUID).ToArray();

        if (roleGuids.Length > 0)
        {
            var existingRoleGuids = (await _roleInfoProvider.Get()
                .WhereIn(nameof(RoleInfo.RoleGUID), roleGuids)
                .Columns(nameof(RoleInfo.RoleGUID))
                .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
                .Select(r => r.RoleGUID)
                .ToHashSet();

            var invalidRoleGuids = roleGuids.Where(g => !existingRoleGuids.Contains(g)).ToArray();

            if (invalidRoleGuids.Length > 0)
            {
                throw new InvalidOperationException($"Roles not found: {string.Join(", ", invalidRoleGuids)}.");
            }
        }
    }

    private async Task SyncUserRolesAsync(int userId, IEnumerable<RoleSummaryInput> requestedRoles, CancellationToken cancellationToken)
    {
        var requestedRoleGuids = requestedRoles.Select(r => r.GUID).ToArray();

        var requestedRoleIds = (await _roleInfoProvider.Get()
            .WhereIn(nameof(RoleInfo.RoleGUID), requestedRoleGuids)
            .Columns(nameof(RoleInfo.RoleID))
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
            .Select(r => r.RoleID)
            .ToHashSet();

        var existingLinks = (await _userRoleInfoProvider.Get()
            .WhereEquals(nameof(UserRoleInfo.UserID), userId)
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
            .ToArray();

        var existingRoleIds = existingLinks.Select(l => l.RoleID).ToHashSet();

        foreach (var link in existingLinks.Where(l => !requestedRoleIds.Contains(l.RoleID)))
        {
            await _userRoleInfoProvider.DeleteAsync(link, cancellationToken);
        }

        foreach (var roleId in requestedRoleIds.Where(id => !existingRoleIds.Contains(id)))
        {
            await _userRoleInfoProvider.SetAsync(new UserRoleInfo { UserID = userId, RoleID = roleId }, cancellationToken);
        }
    }

    // ---------- MAPPING ----------

    private sealed record MappingContext(
        Dictionary<int, RoleInfo> RolesById,
        ILookup<int, int> RoleIdsByUserId,
        ILookup<int, int> WebPageAclIdsByRoleId,
        Dictionary<int, int> WebsiteChannelIdByAclId,
        Dictionary<int, int> ChannelIdByWebsiteChannelId,
        Dictionary<int, ChannelInfo> ChannelsById);

    private async Task<MappingContext> BuildContextAsync(CancellationToken cancellationToken)
    {
        var roles = await _roleInfoProvider.Get()
            .Columns(nameof(RoleInfo.RoleID), nameof(RoleInfo.RoleGUID), nameof(RoleInfo.RoleDisplayName))
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken);
        var rolesById = roles.ToDictionary(r => r.RoleID, r => r);

        var userRoles = await _userRoleInfoProvider.Get()
            .Columns(nameof(UserRoleInfo.UserID), nameof(UserRoleInfo.RoleID))
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken);
        var roleIdsByUserId = userRoles.ToLookup(l => l.UserID, l => l.RoleID);

        var aclRoles = await _webPageAclRoleInfoProvider.Get()
            .Columns(nameof(WebPageAclRoleInfo.WebPageAclRoleRoleID), nameof(WebPageAclRoleInfo.WebPageAclRoleWebPageAclID))
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken);
        var webPageAclIdsByRoleId = aclRoles.ToLookup(a => a.WebPageAclRoleRoleID, a => a.WebPageAclRoleWebPageAclID);

        var acls = await _webPageAclInfoProvider.Get()
            .Columns(nameof(WebPageAclInfo.WebPageAclID), nameof(WebPageAclInfo.WebPageAclWebsiteChannelID))
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken);
        var websiteChannelIdByAclId = acls.ToDictionary(a => a.WebPageAclID, a => a.WebPageAclWebsiteChannelID);

        var websiteChannels = await _websiteChannelInfoProvider.Get()
            .Columns(nameof(WebsiteChannelInfo.WebsiteChannelID), nameof(WebsiteChannelInfo.WebsiteChannelChannelID))
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken);
        var channelIdByWebsiteChannelId = websiteChannels.ToDictionary(w => w.WebsiteChannelID, w => w.WebsiteChannelChannelID);

        var channels = await _channelInfoProvider.Get()
            .Columns(nameof(ChannelInfo.ChannelID), nameof(ChannelInfo.ChannelGUID), nameof(ChannelInfo.ChannelDisplayName))
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken);
        var channelsById = channels.ToDictionary(c => c.ChannelID, c => c);

        return new MappingContext(
            rolesById,
            roleIdsByUserId,
            webPageAclIdsByRoleId,
            websiteChannelIdByAclId,
            channelIdByWebsiteChannelId,
            channelsById);
    }

    private static UserOutput ToUserOutput(UserInfo user, MappingContext ctx)
    {
        var general = new UserGeneralOutput
        {
            GUID = user.UserGUID,
            UserName = user.UserName,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Enabled = user.UserEnabled,
            IsExternalUser = user.UserIsExternal,
            Created = user.UserCreated,
            LastSignIn = user.LastLogon,
            InvalidSignInAttempts = user.UserFailedLoginAttempts,
            PreferredLanguage = user.UserPreferredLanguage
        };

        var roleIds = ctx.RoleIdsByUserId[user.UserID].ToArray();

        var roles = roleIds
            .Where(ctx.RolesById.ContainsKey)
            .Select(roleId => ctx.RolesById[roleId])
            .Select(r => new RoleSummaryOutput { GUID = r.RoleGUID, DisplayName = r.RoleDisplayName })
            .ToArray();

        var channels = roleIds
            .SelectMany(roleId => ctx.WebPageAclIdsByRoleId[roleId])
            .Distinct()
            .Select(aclId => ctx.WebsiteChannelIdByAclId.TryGetValue(aclId, out var wcId) ? wcId : (int?)null)
            .Where(wcId => wcId.HasValue)
            .Select(wcId => ctx.ChannelIdByWebsiteChannelId.TryGetValue(wcId!.Value, out var chId) ? chId : (int?)null)
            .Where(chId => chId.HasValue)
            .Distinct()
            .Select(chId => ctx.ChannelsById.TryGetValue(chId!.Value, out var channel) ? channel : null)
            .Where(channel => channel is not null)
            .Select(channel => new ChannelSummaryOutput { GUID = channel!.ChannelGUID, DisplayName = channel.ChannelDisplayName })
            .ToArray();

        return new UserOutput
        {
            General = general,
            Roles = roles,
            Channels = channels
        };
    }
}