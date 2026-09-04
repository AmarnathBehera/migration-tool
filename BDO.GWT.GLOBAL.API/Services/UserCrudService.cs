using BDO.GWT.GLOBAL.API.Data;
using CMS.Base;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Membership;
using CMS.Websites;
using CMS.Websites.Internal;

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

#pragma warning disable S107
    public UserCrudService(
        IInfoProvider<UserInfo> userInfoProvider,
        IInfoProvider<UserRoleInfo> userRoleInfoProvider,
        IInfoProvider<RoleInfo> roleInfoProvider,
        IInfoProvider<WebPageAclInfo> webPageAclInfoProvider,
        IInfoProvider<WebPageAclRoleInfo> webPageAclRoleInfoProvider,
        IInfoProvider<WebsiteChannelInfo> websiteChannelInfoProvider,
        IInfoProvider<ChannelInfo> channelInfoProvider,
        IHttpContextAccessor httpContextAccessor)
#pragma warning restore S107
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
            query = query.WhereContains(nameof(UserInfo.UserName), userName);

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
            return null;

        var context = await BuildContextAsync(cancellationToken);
        return ToUserOutput(user, context);
    }

    // ---------- CREATE ----------

    public async Task<UserOutput> CreateUserAsync(UserInput userInput, CancellationToken cancellationToken)
    {
        var caller = await GetAndValidateCallerAsync(cancellationToken);

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
            UserAdministrationAccess = true
        };

        using (var actionContext = new CMSActionContext(caller))
        {
            actionContext.UseGlobalAdminContext = true;
            _userInfoProvider.Set(user);
             await SyncUserRolesAsync(user.UserID, userInput.Roles, cancellationToken);
        }


        var context = await BuildContextAsync(cancellationToken);
        return ToUserOutput(user, context);
    }

    // ---------- UPDATE ----------

    public async Task<UserOutput> UpdateUserAsync(Guid userGUID, UserInput userInput, CancellationToken cancellationToken)
    {
        var caller = await GetAndValidateCallerAsync(cancellationToken);

        var user = await GetValidatedUserAsync(userGUID, cancellationToken);

        await ValidateUserInputAsync(user.UserID, userInput, cancellationToken);

        user.UserName = userInput.General.UserName;
        user.FirstName = userInput.General.FirstName;
        user.LastName = userInput.General.LastName;
        user.Email = userInput.General.Email;
        user.UserEnabled = userInput.General.Enabled;
        user.UserIsExternal = userInput.General.IsExternalUser;
        user.UserPreferredLanguage = userInput.General.PreferredLanguage;

        using (var actionContext = new CMSActionContext(caller))
        {
            actionContext.UseGlobalAdminContext = true;
            _userInfoProvider.Set(user);
        await SyncUserRolesAsync(user.UserID, userInput.Roles, cancellationToken);
        }


        var context = await BuildContextAsync(cancellationToken);
        return ToUserOutput(user, context);
    }

    // ---------- DELETE ----------

    public async Task DeleteUserAsync(Guid userGUID, CancellationToken cancellationToken)
    {
        var caller = await GetAndValidateCallerAsync(cancellationToken);

        var user = await GetValidatedUserAsync(userGUID, cancellationToken);

        using (var actionContext = new CMSActionContext(caller))
        {
            actionContext.UseGlobalAdminContext = true;
            _userInfoProvider.Delete(user);
        }
    }

    // ---------- CALLER VALIDATION ----------

    /// <summary>
    /// Resolves the authenticated HTTP caller to a Kentico UserInfo and verifies
    /// they are enabled and have administration access. Throws UnauthorizedAccessException
    /// on any failure so the controller returns 401 rather than 500.
    /// </summary>
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

    private async Task<UserInfo> GetValidatedUserAsync(Guid userGUID, CancellationToken cancellationToken)
    {
        var user = (await _userInfoProvider.Get()
            .WhereEquals(nameof(UserInfo.UserGUID), userGUID)
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
            .FirstOrDefault();

        return user ?? throw new InvalidOperationException($"Cannot find user with GUID '{userGUID}'.");
    }

    private async Task ValidateUserInputAsync(int? existingUserId, UserInput userInput, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(userInput.General.UserName))
            throw new InvalidOperationException("User name is required.");

        var userNameQuery = _userInfoProvider.Get()
            .WhereEquals(nameof(UserInfo.UserName), userInput.General.UserName);

        if (existingUserId.HasValue)
            userNameQuery = userNameQuery.WhereNotEquals(nameof(UserInfo.UserID), existingUserId.Value);

        if ((await userNameQuery.GetEnumerableTypedResultAsync(cancellationToken: cancellationToken)).Any())
            throw new InvalidOperationException($"User name '{userInput.General.UserName}' already exists.");

        if (!string.IsNullOrEmpty(userInput.General.Email))
        {
            var emailQuery = _userInfoProvider.Get()
                .WhereEquals(nameof(UserInfo.Email), userInput.General.Email);

            if (existingUserId.HasValue)
                emailQuery = emailQuery.WhereNotEquals(nameof(UserInfo.UserID), existingUserId.Value);

            if ((await emailQuery.GetEnumerableTypedResultAsync(cancellationToken: cancellationToken)).Any())
                throw new InvalidOperationException($"Email '{userInput.General.Email}' already exists.");
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
                throw new InvalidOperationException($"Roles not found: {string.Join(", ", invalidRoleGuids)}.");
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
            await _userRoleInfoProvider.DeleteAsync(link, cancellationToken);

        foreach (var roleId in requestedRoleIds.Where(id => !existingRoleIds.Contains(id)))
            await _userRoleInfoProvider.SetAsync(new UserRoleInfo { UserID = userId, RoleID = roleId }, cancellationToken);
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

        var userRoles = await _userRoleInfoProvider.Get()
            .Columns(nameof(UserRoleInfo.UserID), nameof(UserRoleInfo.RoleID))
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken);

        var aclRoles = await _webPageAclRoleInfoProvider.Get()
            .Columns(nameof(WebPageAclRoleInfo.WebPageAclRoleRoleID), nameof(WebPageAclRoleInfo.WebPageAclRoleWebPageAclID))
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken);

        var acls = await _webPageAclInfoProvider.Get()
            .Columns(nameof(WebPageAclInfo.WebPageAclID), nameof(WebPageAclInfo.WebPageAclWebsiteChannelID))
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken);

        var websiteChannels = await _websiteChannelInfoProvider.Get()
            .Columns(nameof(WebsiteChannelInfo.WebsiteChannelID), nameof(WebsiteChannelInfo.WebsiteChannelChannelID))
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken);

        var channels = await _channelInfoProvider.Get()
            .Columns(nameof(ChannelInfo.ChannelID), nameof(ChannelInfo.ChannelGUID), nameof(ChannelInfo.ChannelDisplayName))
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken);

        return new MappingContext(
            roles.ToDictionary(r => r.RoleID, r => r),
            userRoles.ToLookup(l => l.UserID, l => l.RoleID),
            aclRoles.ToLookup(a => a.WebPageAclRoleRoleID, a => a.WebPageAclRoleWebPageAclID),
            acls.ToDictionary(a => a.WebPageAclID, a => a.WebPageAclWebsiteChannelID),
            websiteChannels.ToDictionary(w => w.WebsiteChannelID, w => w.WebsiteChannelChannelID),
            channels.ToDictionary(c => c.ChannelID, c => c));
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
            .Select(id => ctx.RolesById[id])
            .Select(r => new RoleSummaryOutput { GUID = r.RoleGUID, DisplayName = r.RoleDisplayName })
            .ToArray();

       

        return new UserOutput { General = general, Roles = roles};
    }
}