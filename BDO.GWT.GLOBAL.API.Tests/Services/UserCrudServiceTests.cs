using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Services;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Membership;
using CMS.Websites;
using CMS.Websites.Internal;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Data;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace BDO.GWT.GLOBAL.API.Tests.Services;

[TestFixture]
public class UserCrudServiceTests
{
    private static UserCrudService CreateServiceWithoutConstructor() =>
        (UserCrudService)RuntimeHelpers.GetUninitializedObject(typeof(UserCrudService));

    private static UserCrudService CreateServiceWithMockedDependencies(
        out Mock<IHttpContextAccessor> httpContextAccessorMock)
    {
        httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        return new UserCrudService(
            new Mock<IInfoProvider<UserInfo>>().Object,
            new Mock<IInfoProvider<UserRoleInfo>>().Object,
            new Mock<IInfoProvider<RoleInfo>>().Object,
            new Mock<IInfoProvider<WebPageAclInfo>>().Object,
            new Mock<IInfoProvider<WebPageAclRoleInfo>>().Object,
            new Mock<IInfoProvider<WebsiteChannelInfo>>().Object,
            new Mock<IInfoProvider<ChannelInfo>>().Object,
            httpContextAccessorMock.Object);
    }

    #region Contract

    [Test]
    public void UserCrudService_ImplementsIUserCrudService()
    {
        var service = CreateServiceWithoutConstructor();

        Assert.That(service, Is.InstanceOf<IUserCrudService>());
    }

    [Test]
    public void IUserCrudService_HasExpectedMethods()
    {
        var interfaceType = typeof(IUserCrudService);

        Assert.Multiple(() =>
        {
            Assert.That(interfaceType.GetMethod("GetUsersAsync"), Is.Not.Null);
            Assert.That(interfaceType.GetMethod("GetUserAsync"), Is.Not.Null);
            Assert.That(interfaceType.GetMethod("CreateUserAsync"), Is.Not.Null);
            Assert.That(interfaceType.GetMethod("UpdateUserAsync"), Is.Not.Null);
            Assert.That(interfaceType.GetMethod("DeleteUserAsync"), Is.Not.Null);
        });
    }

    [Test]
    public void IUserCrudService_Methods_HaveExpectedReturnTypes()
    {
        var interfaceType = typeof(IUserCrudService);

        Assert.Multiple(() =>
        {
            Assert.That(interfaceType.GetMethod("GetUsersAsync")!.ReturnType, Is.EqualTo(typeof(Task<IEnumerable<UserOutput>>)));
            Assert.That(interfaceType.GetMethod("GetUserAsync")!.ReturnType, Is.EqualTo(typeof(Task<UserOutput?>)));
            Assert.That(interfaceType.GetMethod("CreateUserAsync")!.ReturnType, Is.EqualTo(typeof(Task<UserOutput>)));
            Assert.That(interfaceType.GetMethod("UpdateUserAsync")!.ReturnType, Is.EqualTo(typeof(Task<UserOutput>)));
            Assert.That(interfaceType.GetMethod("DeleteUserAsync")!.ReturnType, Is.EqualTo(typeof(Task)));
        });
    }

    [Test]
    public void IUserCrudService_Methods_AcceptCancellationToken()
    {
        var interfaceType = typeof(IUserCrudService);

        foreach (var method in interfaceType.GetMethods())
        {
            Assert.That(
                method.GetParameters().Any(p => p.ParameterType == typeof(CancellationToken)),
                Is.True,
                $"{method.Name} should accept a CancellationToken");
        }
    }

    #endregion

    #region Constructor

    [Test]
    public void Constructor_WithAllDependencies_CreatesInstance()
    {
        var service = CreateServiceWithMockedDependencies(out _);

        Assert.That(service, Is.Not.Null);
    }

    [Test]
    public void Constructor_AssignsAllPrivateDependencyFields()
    {
        var service = CreateServiceWithMockedDependencies(out _);

        var dependencyFields = typeof(UserCrudService)
            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
            .Where(f => f.Name.StartsWith('_'));

        Assert.Multiple(() =>
        {
            foreach (var field in dependencyFields)
            {
                Assert.That(field.GetValue(service), Is.Not.Null, $"{field.Name} should be assigned");
            }
        });
    }

    #endregion

    #region Guard behaviour (no dependencies)

    [Test]
    public void GetUsersAsync_WhenDependenciesUnavailable_Throws()
    {
        var service = CreateServiceWithoutConstructor();

        Assert.ThrowsAsync<NullReferenceException>(async () =>
            await service.GetUsersAsync("jdoe", CancellationToken.None));
    }

    [TestCase("jdoe")]
    [TestCase("admin")]
    [TestCase("")]
    [TestCase(null)]
    public void GetUsersAsync_WithVariousUserNames_WhenDependenciesUnavailable_Throws(string? userName)
    {
        var service = CreateServiceWithoutConstructor();

        Assert.ThrowsAsync<NullReferenceException>(async () =>
            await service.GetUsersAsync(userName, CancellationToken.None));
    }

    [Test]
    public void GetUserAsync_WhenDependenciesUnavailable_Throws()
    {
        var service = CreateServiceWithoutConstructor();

        Assert.ThrowsAsync<NullReferenceException>(async () =>
            await service.GetUserAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Test]
    public void GetUserAsync_WithEmptyGuid_WhenDependenciesUnavailable_Throws()
    {
        var service = CreateServiceWithoutConstructor();

        Assert.ThrowsAsync<NullReferenceException>(async () =>
            await service.GetUserAsync(Guid.Empty, CancellationToken.None));
    }

    [Test]
    public void CreateUserAsync_WhenDependenciesUnavailable_Throws()
    {
        var service = CreateServiceWithoutConstructor();
        var input = new UserInput { General = new UserGeneralInput { UserName = "newuser" } };

        Assert.ThrowsAsync<NullReferenceException>(async () =>
            await service.CreateUserAsync(input, CancellationToken.None));
    }

    [Test]
    public void CreateUserAsync_WithNullInput_WhenDependenciesUnavailable_Throws()
    {
        var service = CreateServiceWithoutConstructor();

        Assert.ThrowsAsync<NullReferenceException>(async () =>
            await service.CreateUserAsync(null!, CancellationToken.None));
    }

    [Test]
    public void UpdateUserAsync_WhenDependenciesUnavailable_Throws()
    {
        var service = CreateServiceWithoutConstructor();
        var input = new UserInput { General = new UserGeneralInput { UserName = "updateduser" } };

        Assert.ThrowsAsync<NullReferenceException>(async () =>
            await service.UpdateUserAsync(Guid.NewGuid(), input, CancellationToken.None));
    }

    [Test]
    public void UpdateUserAsync_WithNullInput_WhenDependenciesUnavailable_Throws()
    {
        var service = CreateServiceWithoutConstructor();

        Assert.ThrowsAsync<NullReferenceException>(async () =>
            await service.UpdateUserAsync(Guid.NewGuid(), null!, CancellationToken.None));
    }

    [Test]
    public void DeleteUserAsync_WhenDependenciesUnavailable_Throws()
    {
        var service = CreateServiceWithoutConstructor();

        Assert.ThrowsAsync<NullReferenceException>(async () =>
            await service.DeleteUserAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Test]
    public void DeleteUserAsync_WithEmptyGuid_WhenDependenciesUnavailable_Throws()
    {
        var service = CreateServiceWithoutConstructor();

        Assert.ThrowsAsync<NullReferenceException>(async () =>
            await service.DeleteUserAsync(Guid.Empty, CancellationToken.None));
    }

    #endregion

    #region Caller validation (no HttpContext)

    [Test]
    public void CreateUserAsync_WithMockedProviders_ButNoHttpContext_ThrowsUnauthorized()
    {
        var service = CreateServiceWithMockedDependencies(out var httpContextAccessorMock);
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var input = new UserInput { General = new UserGeneralInput { UserName = "newuser" } };

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await service.CreateUserAsync(input, CancellationToken.None));
    }

    [Test]
    public void UpdateUserAsync_WithMockedProviders_ButNoHttpContext_ThrowsUnauthorized()
    {
        var service = CreateServiceWithMockedDependencies(out var httpContextAccessorMock);
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var input = new UserInput { General = new UserGeneralInput { UserName = "updateduser" } };

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await service.UpdateUserAsync(Guid.NewGuid(), input, CancellationToken.None));
    }

    [Test]
    public void DeleteUserAsync_WithMockedProviders_ButNoHttpContext_ThrowsUnauthorized()
    {
        var service = CreateServiceWithMockedDependencies(out var httpContextAccessorMock);
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await service.DeleteUserAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Test]
    public void CreateUserAsync_WithUnauthenticatedUser_ThrowsUnauthorized()
    {
        var service = CreateServiceWithMockedDependencies(out var httpContextAccessorMock);
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext());

        var input = new UserInput { General = new UserGeneralInput { UserName = "newuser" } };

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await service.CreateUserAsync(input, CancellationToken.None));
    }

    #endregion

    #region UserInput / RoleSummaryInput models

    [Test]
    public void UserInput_Roles_DefaultToEmptyArray()
    {
        var input = new UserInput { General = new UserGeneralInput { UserName = "u" } };

        Assert.That(input.Roles, Is.Empty);
    }

    [Test]
    public void UserInput_General_CanBeSet()
    {
        var input = new UserInput
        {
            General = new UserGeneralInput
            {
                UserName = "newuser",
                FirstName = "New",
                LastName = "User",
                Email = "newuser@example.com",
                Enabled = true,
                IsExternalUser = false,
                PreferredLanguage = "en-GB"
            }
        };

        Assert.Multiple(() =>
        {
            Assert.That(input.General.UserName, Is.EqualTo("newuser"));
            Assert.That(input.General.FirstName, Is.EqualTo("New"));
            Assert.That(input.General.LastName, Is.EqualTo("User"));
            Assert.That(input.General.Email, Is.EqualTo("newuser@example.com"));
            Assert.That(input.General.Enabled, Is.True);
            Assert.That(input.General.IsExternalUser, Is.False);
            Assert.That(input.General.PreferredLanguage, Is.EqualTo("en-GB"));
        });
    }

    [Test]
    public void UserGeneralInput_OptionalProperties_AreNullByDefault()
    {
        var general = new UserGeneralInput { UserName = "u" };

        Assert.Multiple(() =>
        {
            Assert.That(general.FirstName, Is.Null);
            Assert.That(general.LastName, Is.Null);
            Assert.That(general.Email, Is.Null);
            Assert.That(general.PreferredLanguage, Is.Null);
            Assert.That(general.Enabled, Is.False);
            Assert.That(general.IsExternalUser, Is.False);
        });
    }

    [Test]
    public void RoleSummaryInput_StoresGuid()
    {
        var guid = Guid.NewGuid();
        var role = new RoleSummaryInput { GUID = guid };

        Assert.That(role.GUID, Is.EqualTo(guid));
    }

    [Test]
    public void UserInput_CanContainMultipleRoles()
    {
        var input = new UserInput
        {
            General = new UserGeneralInput { UserName = "u" },
            Roles = [new RoleSummaryInput { GUID = Guid.NewGuid() }, new RoleSummaryInput { GUID = Guid.NewGuid() }]
        };

        Assert.That(input.Roles, Has.Length.EqualTo(2));
    }

    #endregion

    #region UserOutput models

    [Test]
    public void UserOutput_DefaultArrays_AreEmpty()
    {
        var output = new UserOutput { General = new UserGeneralOutput { UserName = "testuser" } };

        Assert.Multiple(() =>
        {
           
            Assert.That(output.Roles, Is.Empty);
        });
    }

    [Test]
    public void UserGeneralOutput_StoresAllProperties()
    {
        var guid = Guid.NewGuid();
        var created = DateTime.UtcNow;
        var lastSignIn = DateTime.UtcNow.AddDays(-1);

        var general = new UserGeneralOutput
        {
            GUID = guid,
            UserName = "jdoe",
            FirstName = "John",
            LastName = "Doe",
            Email = "jdoe@example.com",
            Enabled = true,
            IsExternalUser = false,
            Created = created,
            LastSignIn = lastSignIn,
            InvalidSignInAttempts = 2,
            PreferredLanguage = "en"
        };

        Assert.Multiple(() =>
        {
            Assert.That(general.GUID, Is.EqualTo(guid));
            Assert.That(general.UserName, Is.EqualTo("jdoe"));
            Assert.That(general.FirstName, Is.EqualTo("John"));
            Assert.That(general.LastName, Is.EqualTo("Doe"));
            Assert.That(general.Email, Is.EqualTo("jdoe@example.com"));
            Assert.That(general.Enabled, Is.True);
            Assert.That(general.IsExternalUser, Is.False);
            Assert.That(general.Created, Is.EqualTo(created));
            Assert.That(general.LastSignIn, Is.EqualTo(lastSignIn));
            Assert.That(general.InvalidSignInAttempts, Is.EqualTo(2));
            Assert.That(general.PreferredLanguage, Is.EqualTo("en"));
        });
    }

    [Test]
    public void UserGeneralOutput_NullableProperties_AreNullByDefault()
    {
        var general = new UserGeneralOutput { UserName = "testuser" };

        Assert.Multiple(() =>
        {
            Assert.That(general.FirstName, Is.Null);
            Assert.That(general.LastName, Is.Null);
            Assert.That(general.Email, Is.Null);
            Assert.That(general.Created, Is.Null);
            Assert.That(general.LastSignIn, Is.Null);
            Assert.That(general.PreferredLanguage, Is.Null);
            Assert.That(general.InvalidSignInAttempts, Is.EqualTo(0));
        });
    }

    

    [Test]
    public void RoleSummaryOutput_StoresValues()
    {
        var guid = Guid.NewGuid();
        var role = new RoleSummaryOutput { GUID = guid, DisplayName = "Admin" };

        Assert.Multiple(() =>
        {
            Assert.That(role.GUID, Is.EqualTo(guid));
            Assert.That(role.DisplayName, Is.EqualTo("Admin"));
        });
    }

    [Test]
    public void UserOutput_CanSetChannelsAndRoles()
    {
        var output = new UserOutput
        {
            General = new UserGeneralOutput { UserName = "jdoe" },
            Roles = [new RoleSummaryOutput { GUID = Guid.NewGuid(), DisplayName = "Editor" }]
        };

        Assert.Multiple(() =>
        {
           
            Assert.That(output.Roles, Has.Length.EqualTo(1));
        });
    }

    #endregion

    #region Read paths with mocked Kentico queries

    [Test]
    public async Task GetUsersAsync_WithoutFilter_DoesNotCallWhereContains_AndReturnsMappedUsers()
    {
        var harness = new Harness();
        var userQuery = QueryReturning<UserInfo>(new UserInfo[] { CreateUser(1, Guid.NewGuid(), "jdoe", true, true) });
        harness.Users.Setup(x => x.Get()).Returns(userQuery.Object);
        harness.SetupEmptyMapping();

        var service = harness.Build();

        var result = (await service.GetUsersAsync(null, CancellationToken.None)).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].General.UserName, Is.EqualTo("jdoe"));
            Assert.That(result[0].Roles, Is.Empty);
            
        });

        userQuery.Verify(x => x.WhereContains(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task GetUsersAsync_WithUserNameFilter_CallsWhereContains()
    {
        var harness = new Harness();
        var userQuery = QueryReturning<UserInfo>(new UserInfo[] { CreateUser(1, Guid.NewGuid(), "jdoe", true, true) });
        harness.Users.Setup(x => x.Get()).Returns(userQuery.Object);
        harness.SetupEmptyMapping();

        var service = harness.Build();

        var result = (await service.GetUsersAsync("jdoe", CancellationToken.None)).ToList();

        Assert.That(result, Has.Count.EqualTo(1));
        userQuery.Verify(
            x => x.WhereContains(nameof(UserInfo.UserName), "jdoe"),
            Times.Once);
    }

    [Test]
    public async Task GetUsersAsync_MapsRolesAndChannelsThroughAclChain()
    {
        var userGuid = Guid.NewGuid();
        var roleGuid = Guid.NewGuid();
        var channelGuid = Guid.NewGuid();

        var harness = new Harness();
        harness.Users.Setup(x => x.Get())
            .Returns(QueryReturning<UserInfo>(new UserInfo[] { CreateUser(1, userGuid, "jdoe", true, true) }).Object);

        harness.SetupMapping(
            roles: new[] { CreateRole(10, roleGuid, "Editor") },
            userRoles: new[] { CreateUserRole(1, 10) },
            aclRoles: new[] { CreateAclRole(10, 100) },
            acls: new[] { CreateAcl(100, 200) },
            websiteChannels: new[] { CreateWebsiteChannel(200, 300) },
            channels: new[] { CreateChannel(300, channelGuid, "Global") });

        var service = harness.Build();

        var result = (await service.GetUsersAsync(null, CancellationToken.None)).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Roles, Has.Length.EqualTo(1));
            Assert.That(result[0].Roles[0].GUID, Is.EqualTo(roleGuid));
            Assert.That(result[0].Roles[0].DisplayName, Is.EqualTo("Editor"));
            
        });
    }

    [Test]
    public async Task GetUsersAsync_WhenNoUsers_ReturnsEmpty()
    {
        var harness = new Harness();
        harness.Users.Setup(x => x.Get())
            .Returns(QueryReturning(Array.Empty<UserInfo>()).Object);
        harness.SetupEmptyMapping();

        var service = harness.Build();

        var result = await service.GetUsersAsync(null, CancellationToken.None);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetUserAsync_WhenUserExists_ReturnsMappedUser()
    {
        var userGuid = Guid.NewGuid();
        var harness = new Harness();
        harness.Users.Setup(x => x.Get())
            .Returns(QueryReturning<UserInfo>(new UserInfo[] { CreateUser(1, userGuid, "jdoe", true, true) }).Object);
        harness.SetupEmptyMapping();

        var service = harness.Build();

        var result = await service.GetUserAsync(userGuid, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.General.GUID, Is.EqualTo(userGuid));
            Assert.That(result.General.UserName, Is.EqualTo("jdoe"));
        });
    }

    [Test]
    public async Task GetUserAsync_WhenUserMissing_ReturnsNull()
    {
        var harness = new Harness();
        harness.Users.Setup(x => x.Get())
            .Returns(QueryReturning(Array.Empty<UserInfo>()).Object);
        harness.SetupEmptyMapping();

        var service = harness.Build();

        var result = await service.GetUserAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.That(result, Is.Null);
    }

    #endregion

    #region Caller validation with mocked queries

    [Test]
    public void DeleteUserAsync_WhenCallerNotFound_ThrowsUnauthorized()
    {
        var harness = new Harness();
        harness.Http.Setup(x => x.HttpContext).Returns(AuthenticatedContext("admin"));
        harness.Users.Setup(x => x.Get())
            .Returns(QueryReturning(Array.Empty<UserInfo>()).Object);

        var service = harness.Build();

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await service.DeleteUserAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Test]
    public void DeleteUserAsync_WhenCallerDisabled_ThrowsUnauthorized()
    {
        var harness = new Harness();
        harness.Http.Setup(x => x.HttpContext).Returns(AuthenticatedContext("admin"));
        harness.Users.Setup(x => x.Get())
            .Returns(QueryReturning<UserInfo>(new UserInfo[] { CreateUser(1, Guid.NewGuid(), "admin", enabled: false, adminAccess: true) }).Object);

        var service = harness.Build();

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await service.DeleteUserAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Test]
    public void DeleteUserAsync_WhenCallerHasNoAdminAccess_ThrowsUnauthorized()
    {
        var harness = new Harness();
        harness.Http.Setup(x => x.HttpContext).Returns(AuthenticatedContext("admin"));
        harness.Users.Setup(x => x.Get())
            .Returns(QueryReturning<UserInfo>(new UserInfo[] { CreateUser(1, Guid.NewGuid(), "admin", enabled: true, adminAccess: false) }).Object);

        var service = harness.Build();

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await service.DeleteUserAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Test]
    public void DeleteUserAsync_WhenAuthenticatedButNoUserName_ThrowsUnauthorized()
    {
        var harness = new Harness();
        harness.Http.Setup(x => x.HttpContext).Returns(AuthenticatedContext(null));

        var service = harness.Build();

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await service.DeleteUserAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Test]
    public void DeleteUserAsync_WhenCallerValidButTargetUserMissing_ThrowsInvalidOperation()
    {
        var harness = new Harness();
        harness.Http.Setup(x => x.HttpContext).Returns(AuthenticatedContext("admin"));
        harness.Users.SetupSequence(x => x.Get())
            .Returns(QueryReturning<UserInfo>(new UserInfo[] { CreateUser(1, Guid.NewGuid(), "admin", true, true) }).Object)
            .Returns(QueryReturning(Array.Empty<UserInfo>()).Object);

        var service = harness.Build();

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.DeleteUserAsync(Guid.NewGuid(), CancellationToken.None));
    }

    #endregion

    #region Role synchronization


    [Test]
    public async Task SyncUserRolesAsync_WhenRequestedRolesMatchExisting_DoesNotMutateLinks()
    {
        var sameRoleGuid = Guid.NewGuid();

        var harness = new Harness();
        var roleQuery = QueryReturning<RoleInfo>(new RoleInfo[] { CreateRole(2, sameRoleGuid, "Same") });
        roleQuery.Setup(x => x.WhereIn(It.IsAny<string>(), It.IsAny<ICollection<Guid>>()))
            .Returns(roleQuery.Object);

        var userRoleQuery = QueryReturning<UserRoleInfo>(new UserRoleInfo[] { CreateUserRole(55, 2) });

        harness.Roles.Setup(x => x.Get()).Returns(roleQuery.Object);
        harness.UserRoles.Setup(x => x.Get()).Returns(userRoleQuery.Object);
        harness.UserRoles.Setup(x => x.DeleteAsync(It.IsAny<UserRoleInfo>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        harness.UserRoles.Setup(x => x.SetAsync(It.IsAny<UserRoleInfo>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = harness.Build();

        await InvokeSyncUserRolesAsync(
            service,
            55,
            new[] { new RoleSummaryInput { GUID = sameRoleGuid } },
            CancellationToken.None);

        harness.UserRoles.Verify(x => x.DeleteAsync(It.IsAny<UserRoleInfo>(), It.IsAny<CancellationToken>()), Times.Never);
        harness.UserRoles.Verify(x => x.SetAsync(It.IsAny<UserRoleInfo>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Test harness and fakes

    private sealed class Harness
    {
        public readonly Mock<IInfoProvider<UserInfo>> Users = new();
        public readonly Mock<IInfoProvider<UserRoleInfo>> UserRoles = new();
        public readonly Mock<IInfoProvider<RoleInfo>> Roles = new();
        public readonly Mock<IInfoProvider<WebPageAclInfo>> Acls = new();
        public readonly Mock<IInfoProvider<WebPageAclRoleInfo>> AclRoles = new();
        public readonly Mock<IInfoProvider<WebsiteChannelInfo>> WebsiteChannels = new();
        public readonly Mock<IInfoProvider<ChannelInfo>> Channels = new();
        public readonly Mock<IHttpContextAccessor> Http = new();

        public UserCrudService Build() =>
            new(
                Users.Object,
                UserRoles.Object,
                Roles.Object,
                Acls.Object,
                AclRoles.Object,
                WebsiteChannels.Object,
                Channels.Object,
                Http.Object);

        public void SetupEmptyMapping() =>
            SetupMapping(
                Array.Empty<RoleInfo>(),
                Array.Empty<UserRoleInfo>(),
                Array.Empty<WebPageAclRoleInfo>(),
                Array.Empty<WebPageAclInfo>(),
                Array.Empty<WebsiteChannelInfo>(),
                Array.Empty<ChannelInfo>());

        public void SetupMapping(
            IEnumerable<RoleInfo> roles,
            IEnumerable<UserRoleInfo> userRoles,
            IEnumerable<WebPageAclRoleInfo> aclRoles,
            IEnumerable<WebPageAclInfo> acls,
            IEnumerable<WebsiteChannelInfo> websiteChannels,
            IEnumerable<ChannelInfo> channels)
        {
            Roles.Setup(x => x.Get()).Returns(QueryReturning(roles).Object);
            UserRoles.Setup(x => x.Get()).Returns(QueryReturning(userRoles).Object);
            AclRoles.Setup(x => x.Get()).Returns(QueryReturning(aclRoles).Object);
            Acls.Setup(x => x.Get()).Returns(QueryReturning(acls).Object);
            WebsiteChannels.Setup(x => x.Get()).Returns(QueryReturning(websiteChannels).Object);
            Channels.Setup(x => x.Get()).Returns(QueryReturning(channels).Object);
        }
    }

    private static Mock<ObjectQuery<T>> QueryReturning<T>(IEnumerable<T> rows)
        where T : BaseInfo
    {
        var query = new Mock<ObjectQuery<T>>();

        query.Setup(x => x.WhereContains(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(query.Object);
        query.Setup(x => x.WhereEquals(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(query.Object);
        query.Setup(x => x.WhereNotEquals(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(query.Object);
        query.Setup(x => x.WhereIn(It.IsAny<string>(), It.IsAny<ICollection<int>>()))
            .Returns(query.Object);
        query.Setup(x => x.WhereIn(It.IsAny<string>(), It.IsAny<ICollection<Guid>>()))
            .Returns(query.Object);
        query.Setup(x => x.Columns(It.IsAny<string[]>()))
            .Returns(query.Object);
        query.Setup(x => x.TopN(It.IsAny<int>()))
            .Returns(query.Object);
        query.Setup(x => x.GetEnumerableTypedResultAsync(
                It.IsAny<CommandBehavior>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken?>()))
            .ReturnsAsync(rows);

        return query;
    }

    private static async Task InvokeSyncUserRolesAsync(
        UserCrudService service,
        int userId,
        IEnumerable<RoleSummaryInput> requestedRoles,
        CancellationToken cancellationToken)
    {
        var method = typeof(UserCrudService).GetMethod("SyncUserRolesAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var task = (Task)method.Invoke(service, new object[] { userId, requestedRoles, cancellationToken })!;
        await task;
    }

    private static DefaultHttpContext AuthenticatedContext(string? userName)
    {
        var identity = new ClaimsIdentity("TestAuth");
        if (!string.IsNullOrEmpty(userName))
            identity.AddClaim(new Claim(ClaimTypes.Name, userName));

        return new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
    }

    private static FakeUserInfo CreateUser(int id, Guid guid, string userName, bool enabled, bool adminAccess)
    {
        var user = (FakeUserInfo)RuntimeHelpers.GetUninitializedObject(typeof(FakeUserInfo));
        user.UserID = id;
        user.UserGUID = guid;
        user.UserName = userName;
        user.UserEnabled = enabled;
        user.UserAdministrationAccess = adminAccess;
        return user;
    }

    private static FakeRoleInfo CreateRole(int id, Guid guid, string displayName)
    {
        var role = (FakeRoleInfo)RuntimeHelpers.GetUninitializedObject(typeof(FakeRoleInfo));
        role.RoleID = id;
        role.RoleGUID = guid;
        role.RoleDisplayName = displayName;
        return role;
    }

    private static FakeUserRoleInfo CreateUserRole(int userId, int roleId)
    {
        var link = (FakeUserRoleInfo)RuntimeHelpers.GetUninitializedObject(typeof(FakeUserRoleInfo));
        link.UserID = userId;
        link.RoleID = roleId;
        return link;
    }

    private static FakeWebPageAclRoleInfo CreateAclRole(int roleId, int aclId)
    {
        var aclRole = (FakeWebPageAclRoleInfo)RuntimeHelpers.GetUninitializedObject(typeof(FakeWebPageAclRoleInfo));
        aclRole.WebPageAclRoleRoleID = roleId;
        aclRole.WebPageAclRoleWebPageAclID = aclId;
        return aclRole;
    }

    private static FakeWebPageAclInfo CreateAcl(int aclId, int websiteChannelId)
    {
        var acl = (FakeWebPageAclInfo)RuntimeHelpers.GetUninitializedObject(typeof(FakeWebPageAclInfo));
        acl.WebPageAclID = aclId;
        acl.WebPageAclWebsiteChannelID = websiteChannelId;
        return acl;
    }

    private static FakeWebsiteChannelInfo CreateWebsiteChannel(int websiteChannelId, int channelId)
    {
        var wc = (FakeWebsiteChannelInfo)RuntimeHelpers.GetUninitializedObject(typeof(FakeWebsiteChannelInfo));
        wc.WebsiteChannelID = websiteChannelId;
        wc.WebsiteChannelChannelID = channelId;
        return wc;
    }

    private static FakeChannelInfo CreateChannel(int id, Guid guid, string displayName)
    {
        var channel = (FakeChannelInfo)RuntimeHelpers.GetUninitializedObject(typeof(FakeChannelInfo));
        channel.ChannelID = id;
        channel.ChannelGUID = guid;
        channel.ChannelDisplayName = displayName;
        return channel;
    }

    private sealed class FakeUserInfo : UserInfo
    {
        private int userID;
        private Guid userGUID;
        private string userName = string.Empty;
        private string firstName = string.Empty;
        private string lastName = string.Empty;
        private string email = string.Empty;
        private bool userEnabled;
        private bool userIsExternal;
        private bool userAdministrationAccess;
        private string userPreferredLanguage = string.Empty;
        private DateTime userCreated;
        private DateTime lastLogon;
        private int userFailedLoginAttempts;

        public override int UserID { get => userID; set => userID = value; }
        public override Guid UserGUID { get => userGUID; set => userGUID = value; }
        public override string UserName { get => userName; set => userName = value; }
        public override string FirstName { get => firstName; set => firstName = value; }
        public override string LastName { get => lastName; set => lastName = value; }
        public override string Email { get => email; set => email = value; }
        public override bool UserEnabled { get => userEnabled; set => userEnabled = value; }
        public override bool UserIsExternal { get => userIsExternal; set => userIsExternal = value; }
        public override bool UserAdministrationAccess { get => userAdministrationAccess; set => userAdministrationAccess = value; }
        public override string UserPreferredLanguage { get => userPreferredLanguage; set => userPreferredLanguage = value; }
        public override DateTime UserCreated { get => userCreated; set => userCreated = value; }
        public override DateTime LastLogon { get => lastLogon; set => lastLogon = value; }
        public override int UserFailedLoginAttempts { get => userFailedLoginAttempts; set => userFailedLoginAttempts = value; }
    }

    private sealed class FakeRoleInfo : RoleInfo
    {
        private int roleID;
        private Guid roleGUID;
        private string roleDisplayName = string.Empty;

        public override int RoleID { get => roleID; set => roleID = value; }
        public override Guid RoleGUID { get => roleGUID; set => roleGUID = value; }
        public override string RoleDisplayName { get => roleDisplayName; set => roleDisplayName = value; }
    }

    private sealed class FakeUserRoleInfo : UserRoleInfo
    {
        private int userID;
        private int roleID;

        public override int UserID { get => userID; set => userID = value; }
        public override int RoleID { get => roleID; set => roleID = value; }
    }

    private sealed class FakeWebPageAclRoleInfo : WebPageAclRoleInfo
    {
        private int roleID;
        private int aclID;

        public override int WebPageAclRoleRoleID { get => roleID; set => roleID = value; }
        public override int WebPageAclRoleWebPageAclID { get => aclID; set => aclID = value; }
    }

    private sealed class FakeWebPageAclInfo : WebPageAclInfo
    {
        private int aclID;
        private int websiteChannelID;

        public override int WebPageAclID { get => aclID; set => aclID = value; }
        public override int WebPageAclWebsiteChannelID { get => websiteChannelID; set => websiteChannelID = value; }
    }

    private sealed class FakeWebsiteChannelInfo : WebsiteChannelInfo
    {
        private int websiteChannelID;
        private int channelID;

        public override int WebsiteChannelID { get => websiteChannelID; set => websiteChannelID = value; }
        public override int WebsiteChannelChannelID { get => channelID; set => channelID = value; }
    }

    private sealed class FakeChannelInfo : ChannelInfo
    {
        private int channelID;
        private Guid channelGUID;
        private string channelDisplayName = string.Empty;

        public override int ChannelID { get => channelID; set => channelID = value; }
        public override Guid ChannelGUID { get => channelGUID; set => channelGUID = value; }
        public override string ChannelDisplayName { get => channelDisplayName; set => channelDisplayName = value; }
    }

    #endregion
}
