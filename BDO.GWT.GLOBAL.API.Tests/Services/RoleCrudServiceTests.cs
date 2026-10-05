using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Services;
using CMS.ContentEngine;
using CMS.Core;
using CMS.DataEngine;
using CMS.EmailLibrary;
using CMS.Membership;
using CMS.Websites;
using CMS.Websites.Internal;
using CMS.Workspaces;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Data;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace BDO.GWT.GLOBAL.API.Tests.Services;

[TestFixture]
public class RoleCrudServiceTests
{
    private static readonly string[] ViewPermission = { "View" };
    private static readonly string[] FlyPermission = { "Fly" };
    private static readonly string[] ExpectedChannelNames = { "Alpha", "Zeta" };

    private static RoleCrudService CreateUninitializedService() =>
        (RoleCrudService)RuntimeHelpers.GetUninitializedObject(typeof(RoleCrudService));

    private static RoleCrudService CreateService()
    {
        return new RoleCrudService(
            new Mock<IInfoProvider<UserInfo>>().Object,
            new Mock<IInfoProvider<UserRoleInfo>>().Object,
            new Mock<IInfoProvider<RoleInfo>>().Object,
            new Mock<IInfoProvider<ApplicationPermissionInfo>>().Object,
            new Mock<IInfoProvider<WorkspaceDataPermissionInfo>>().Object,
            new Mock<IInfoProvider<WorkspaceInfo>>().Object,
            new Mock<IInfoProvider<EmailChannelInfo>>().Object,
            new Mock<IInfoProvider<WebPageAclRoleInfo>>().Object,
            new Mock<IInfoProvider<WebPageAclRolePermissionInfo>>().Object,
            new Mock<IInfoProvider<WebsiteChannelInfo>>().Object,
            new Mock<IInfoProvider<ChannelInfo>>().Object,
            new Mock<IHttpContextAccessor>().Object,
            new Mock<CMS.Core.IEventLogService>().Object);
    }

    [Test]
    public void ImplementsRoleCrudServiceContract()
    {
        Assert.That(CreateUninitializedService(), Is.InstanceOf<IRoleCrudService>());
    }

    [Test]
    public void ContractContainsAllPublicOperations()
    {
        var contract = typeof(IRoleCrudService);

        Assert.Multiple(() =>
        {
            Assert.That(contract.GetMethod(nameof(IRoleCrudService.GetRolesAsync)), Is.Not.Null);
            Assert.That(contract.GetMethod(nameof(IRoleCrudService.GetRoleAsync)), Is.Not.Null);
            Assert.That(contract.GetMethod(nameof(IRoleCrudService.CreateRoleAsync)), Is.Not.Null);
            Assert.That(contract.GetMethod(nameof(IRoleCrudService.UpdateRoleAsync)), Is.Not.Null);
            Assert.That(contract.GetMethod(nameof(IRoleCrudService.DeleteRoleAsync)), Is.Not.Null);
            Assert.That(contract.GetMethod(nameof(IRoleCrudService.AssignRolePermissionsAsync)), Is.Not.Null);
            Assert.That(contract.GetMethod(nameof(IRoleCrudService.CreateChannelRoleAsync)), Is.Not.Null);
            Assert.That(contract.GetMethod(nameof(IRoleCrudService.CreateChannelRolesForAllChannelsAsync)), Is.Not.Null);
            Assert.That(contract.GetMethod(nameof(IRoleCrudService.GetWebsiteChannelNamesAsync)), Is.Not.Null);
        });
    }

    [Test]
    public void ContractOperationsReturnExpectedTaskTypes()
    {
        Assert.Multiple(() =>
        {
            Assert.That(typeof(IRoleCrudService).GetMethod(nameof(IRoleCrudService.GetRolesAsync))!.ReturnType,
                Is.EqualTo(typeof(Task<IEnumerable<RoleOutput>>)));
            Assert.That(typeof(IRoleCrudService).GetMethod(nameof(IRoleCrudService.GetRoleAsync))!.ReturnType,
                Is.EqualTo(typeof(Task<RoleOutput?>)));
            Assert.That(typeof(IRoleCrudService).GetMethod(nameof(IRoleCrudService.CreateRoleAsync))!.ReturnType,
                Is.EqualTo(typeof(Task<RoleOutput>)));
            Assert.That(typeof(IRoleCrudService).GetMethod(nameof(IRoleCrudService.UpdateRoleAsync))!.ReturnType,
                Is.EqualTo(typeof(Task<RoleOutput>)));
            Assert.That(typeof(IRoleCrudService).GetMethod(nameof(IRoleCrudService.DeleteRoleAsync))!.ReturnType,
                Is.EqualTo(typeof(Task)));
            Assert.That(typeof(IRoleCrudService).GetMethod(nameof(IRoleCrudService.AssignRolePermissionsAsync))!.ReturnType,
                Is.EqualTo(typeof(Task<RoleOutput>)));
            Assert.That(typeof(IRoleCrudService).GetMethod(nameof(IRoleCrudService.CreateChannelRoleAsync))!.ReturnType,
                Is.EqualTo(typeof(Task<RoleOutput>)));
            Assert.That(typeof(IRoleCrudService).GetMethod(nameof(IRoleCrudService.CreateChannelRolesForAllChannelsAsync))!.ReturnType,
                Is.EqualTo(typeof(Task<IEnumerable<RoleOutput>>)));
            Assert.That(typeof(IRoleCrudService).GetMethod(nameof(IRoleCrudService.GetWebsiteChannelNamesAsync))!.ReturnType,
                Is.EqualTo(typeof(Task<IEnumerable<string>>)));
        });
    }

    [Test]
    public void EveryContractOperationAcceptsCancellationToken()
    {
        foreach (var method in typeof(IRoleCrudService).GetMethods())
        {
            Assert.That(method.GetParameters().Any(parameter => parameter.ParameterType == typeof(CancellationToken)),
                Is.True, $"{method.Name} should accept a CancellationToken");
        }
    }

    [Test]
    public void ConstructorWithAllDependenciesCreatesService()
    {
        Assert.That(CreateService(), Is.Not.Null);
    }

    [Test]
    public void ConstructorAssignsAllInstanceDependencies()
    {
        var service = CreateService();
        var dependencyFields = typeof(RoleCrudService)
            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
            .Where(field => field.Name.StartsWith('_'));

        Assert.That(dependencyFields, Is.Not.Empty);
        Assert.Multiple(() =>
        {
            foreach (var field in dependencyFields)
            {
                Assert.That(field.GetValue(service), Is.Not.Null, $"{field.Name} should be assigned");
            }
        });
    }

    [TestCase("Ind", "Channel 'Ind' was not found.")]
    [TestCase("", "Channel '' was not found.")]
    public void ChannelNotFoundExceptionContainsChannelName(string channelName, string expectedMessage)
    {
        var exception = new ChannelNotFoundException(channelName);

        Assert.That(exception.Message, Is.EqualTo(expectedMessage));
    }

    [Test]
    public void ChannelRoleExistExceptionContainsRoleName()
    {
        var exception = new ChannelRoleExistException("Ind-Editor");

        Assert.That(exception.Message, Is.EqualTo("Role 'Ind-Editor' already exists for this channel."));
    }

    [Test]
    public void RolePersistenceExceptionPreservesMessageAndInnerException()
    {
        var innerException = new InvalidOperationException("database failure");

        var exception = new RolePersistenceException("Could not save role.", innerException);

        Assert.Multiple(() =>
        {
            Assert.That(exception.Message, Is.EqualTo("Could not save role."));
            Assert.That(exception.InnerException, Is.SameAs(innerException));
        });
    }

    #region GetRolesAsync / GetRoleAsync (read paths with mocked queries)

    [Test]
    public async Task GetRolesAsync_WithoutFilter_ReturnsMappedRoles_AndSkipsWhereContains()
    {
        var roleGuid = Guid.NewGuid();
        var harness = new Harness();
        var roleQuery = QueryReturning<RoleInfo>(new RoleInfo[] { CreateRole(1, roleGuid, "Editors", "Editors", "desc") });
        harness.Roles.Setup(x => x.Get()).Returns(roleQuery.Object);
        harness.SetupEmptyContext();

        var service = harness.Build();

        var result = (await service.GetRolesAsync(null, CancellationToken.None)).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].GUID, Is.EqualTo(roleGuid));
            Assert.That(result[0].RoleName, Is.EqualTo("Editors"));
            Assert.That(result[0].Description, Is.EqualTo("desc"));
            Assert.That(result[0].WebsiteChannels, Is.Empty);
            Assert.That(result[0].ApplicationPermissions, Is.Empty);
        });

        roleQuery.Verify(x => x.WhereContains(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task GetRolesAsync_WithRoleNameFilter_CallsWhereContains()
    {
        var harness = new Harness();
        var roleQuery = QueryReturning<RoleInfo>(new RoleInfo[] { CreateRole(1, Guid.NewGuid(), "Editors", "Editors", "desc") });
        harness.Roles.Setup(x => x.Get()).Returns(roleQuery.Object);
        harness.SetupEmptyContext();

        var service = harness.Build();

        var result = (await service.GetRolesAsync("Edit", CancellationToken.None)).ToList();

        Assert.That(result, Has.Count.EqualTo(1));
        roleQuery.Verify(x => x.WhereContains(nameof(RoleInfo.RoleName), "Edit"), Times.Once);
    }

    [Test]
    public async Task GetRolesAsync_WhenNoRoles_ReturnsEmpty()
    {
        var harness = new Harness();
        harness.Roles.Setup(x => x.Get()).Returns(QueryReturning(Array.Empty<RoleInfo>()).Object);
        harness.SetupEmptyContext();

        var service = harness.Build();

        var result = await service.GetRolesAsync(null, CancellationToken.None);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetRoleAsync_WhenRoleExists_ReturnsMappedRole()
    {
        var roleGuid = Guid.NewGuid();
        var harness = new Harness();
        harness.Roles.Setup(x => x.Get())
            .Returns(QueryReturning<RoleInfo>(new RoleInfo[] { CreateRole(1, roleGuid, "Editors", "Editors Display", "desc") }).Object);
        harness.SetupEmptyContext();

        var service = harness.Build();

        var result = await service.GetRoleAsync(roleGuid, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.GUID, Is.EqualTo(roleGuid));
            Assert.That(result.RoleName, Is.EqualTo("Editors"));
            Assert.That(result.DisplayName, Is.EqualTo("Editors Display"));
        });
    }

    [Test]
    public async Task GetRoleAsync_WhenRoleMissing_ReturnsNull()
    {
        var harness = new Harness();
        harness.Roles.Setup(x => x.Get()).Returns(QueryReturning(Array.Empty<RoleInfo>()).Object);

        var service = harness.Build();

        var result = await service.GetRoleAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetRoleAsync_MapsWebsiteEmailWorkspaceAndGenericApplicationPermissions()
    {
        var roleGuid = Guid.NewGuid();
        var channelGuid = Guid.NewGuid();
        var websiteChannelGuid = Guid.NewGuid();
        var emailChannelGuid = Guid.NewGuid();
        var workspaceGuid = Guid.NewGuid();

        var harness = new Harness();
        harness.Roles.Setup(x => x.Get())
            .Returns(QueryReturning<RoleInfo>(new RoleInfo[] { CreateRole(1, roleGuid, "Editors", "Editors", "desc") }).Object);

        harness.Channels.Setup(x => x.Get())
            .Returns(QueryReturning<ChannelInfo>(new ChannelInfo[] { CreateChannel(300, "Global", channelGuid, "Global Site") }).Object);
        harness.WebsiteChannels.Setup(x => x.Get())
            .Returns(QueryReturning<WebsiteChannelInfo>(new WebsiteChannelInfo[] { CreateWebsiteChannel(300, websiteChannelGuid) }).Object);
        harness.EmailChannels.Setup(x => x.Get())
            .Returns(QueryReturning<EmailChannelInfo>(new EmailChannelInfo[] { CreateEmailChannel(300, emailChannelGuid) }).Object);

        harness.ApplicationPermissions.Setup(x => x.Get())
            .Returns(QueryReturning<ApplicationPermissionInfo>(new ApplicationPermissionInfo[]
            {
                CreateApplicationPermission(1, ChannelApplicationPrefix + websiteChannelGuid.ToString("D"), "View"),
                CreateApplicationPermission(1, EmailChannelApplicationPrefix + emailChannelGuid.ToString("D"), "View"),
                CreateApplicationPermission(1, "redirects", "View")
            }).Object);

        harness.Workspaces.Setup(x => x.Get())
            .Returns(QueryReturning<WorkspaceInfo>(new WorkspaceInfo[] { CreateWorkspace(50, workspaceGuid, "KenticoDefault", "Default") }).Object);
        harness.WorkspaceDataPermissions.Setup(x => x.Get())
            .Returns(QueryReturning<WorkspaceDataPermissionInfo>(new WorkspaceDataPermissionInfo[]
            {
                CreateWorkspacePermission(1, 50, ContentHubApplicationName, "View")
            }).Object);

        var service = harness.Build();

        var result = await service.GetRoleAsync(roleGuid, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            var websiteChannel = result!.WebsiteChannels.Single();
            Assert.That(websiteChannel.GUID, Is.EqualTo(channelGuid));
            Assert.That(websiteChannel.ChannelName, Is.EqualTo("Global"));
            Assert.That(websiteChannel.DisplayName, Is.EqualTo("Global Site"));
            Assert.That(websiteChannel.Permissions, Is.EqualTo(ViewPermission));

            var emailChannel = result.EmailChannels.Single();
            Assert.That(emailChannel.GUID, Is.EqualTo(channelGuid));
            Assert.That(emailChannel.Permissions, Is.EqualTo(ViewPermission));

            var workspace = result.ContentHubWorkspaces.Single();
            Assert.That(workspace.GUID, Is.EqualTo(workspaceGuid));
            Assert.That(workspace.WorkspaceName, Is.EqualTo("KenticoDefault"));
            Assert.That(workspace.DisplayName, Is.EqualTo("Default"));
            Assert.That(workspace.Permissions, Is.EqualTo(ViewPermission));

            var application = result.ApplicationPermissions.Single();
            Assert.That(application.ApplicationName, Is.EqualTo("redirects"));
            Assert.That(application.Permissions, Is.EqualTo(ViewPermission));
        });
    }

    #endregion

    #region Caller validation (shared by all write operations)

    [Test]
    public void CreateRoleAsync_WhenNoHttpContext_ThrowsUnauthorized()
    {
        var harness = new Harness();

        var service = harness.Build();

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await service.CreateRoleAsync(new RoleInput { RoleName = "Editors" }, CancellationToken.None));
    }

    [Test]
    public void CreateRoleAsync_WhenCallerNotFound_ThrowsUnauthorized()
    {
        var harness = new Harness();
        harness.Http.Setup(x => x.HttpContext).Returns(AuthenticatedContext("admin"));
        harness.Users.Setup(x => x.Get()).Returns(QueryReturning(Array.Empty<UserInfo>()).Object);

        var service = harness.Build();

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await service.CreateRoleAsync(new RoleInput { RoleName = "Editors" }, CancellationToken.None));
    }

    [Test]
    public void CreateRoleAsync_WhenCallerDisabled_ThrowsUnauthorized()
    {
        var harness = new Harness();
        harness.Http.Setup(x => x.HttpContext).Returns(AuthenticatedContext("admin"));
        harness.Users.Setup(x => x.Get())
            .Returns(QueryReturning<UserInfo>(new UserInfo[] { CreateUser("admin", enabled: false, adminAccess: true) }).Object);

        var service = harness.Build();

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await service.CreateRoleAsync(new RoleInput { RoleName = "Editors" }, CancellationToken.None));
    }

    [Test]
    public void CreateRoleAsync_WhenCallerHasNoAdministrationAccess_ThrowsUnauthorized()
    {
        var harness = new Harness();
        harness.Http.Setup(x => x.HttpContext).Returns(AuthenticatedContext("admin"));
        harness.Users.Setup(x => x.Get())
            .Returns(QueryReturning<UserInfo>(new UserInfo[] { CreateUser("admin", enabled: true, adminAccess: false) }).Object);

        var service = harness.Build();

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await service.CreateRoleAsync(new RoleInput { RoleName = "Editors" }, CancellationToken.None));
    }

    #endregion

    #region CreateRoleAsync / UpdateRoleAsync input validation

    [Test]
    public void CreateRoleAsync_WhenRoleNameMissing_ThrowsInvalidOperation()
    {
        var harness = new Harness();
        harness.SetupValidCaller();
        harness.Roles.Setup(x => x.Get()).Returns(QueryReturning(Array.Empty<RoleInfo>()).Object);

        var service = harness.Build();

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.CreateRoleAsync(new RoleInput { RoleName = "   " }, CancellationToken.None));
    }

    [Test]
    public void CreateRoleAsync_WhenRoleNameAlreadyExists_ThrowsInvalidOperation()
    {
        var harness = new Harness();
        harness.SetupValidCaller();
        harness.Roles.Setup(x => x.Get())
            .Returns(QueryReturning<RoleInfo>(new RoleInfo[] { CreateRole(1, Guid.NewGuid(), "Editors", "Editors", "desc") }).Object);

        var service = harness.Build();

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.CreateRoleAsync(new RoleInput { RoleName = "Editors" }, CancellationToken.None));
    }

    [Test]
    public void UpdateRoleAsync_WhenRoleNotFound_ThrowsKeyNotFound()
    {
        var harness = new Harness();
        harness.SetupValidCaller();
        harness.Roles.Setup(x => x.Get()).Returns(QueryReturning(Array.Empty<RoleInfo>()).Object);

        var service = harness.Build();

        Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await service.UpdateRoleAsync(Guid.NewGuid(), new RoleInput { RoleName = "Editors" }, CancellationToken.None));
    }

    [Test]
    public void UpdateRoleAsync_WhenRoleIsSystemRole_ThrowsInvalidOperation()
    {
        var harness = new Harness();
        harness.SetupValidCaller();
        harness.Roles.Setup(x => x.Get())
            .Returns(QueryReturning<RoleInfo>(new RoleInfo[] { CreateRole(1, Guid.NewGuid(), "Administrator", "Administrator", "desc") }).Object);

        var service = harness.Build();

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.UpdateRoleAsync(Guid.NewGuid(), new RoleInput { RoleName = "Renamed" }, CancellationToken.None));
    }

    #endregion

    #region DeleteRoleAsync

    [Test]
    public void DeleteRoleAsync_WhenRoleNotFound_ThrowsKeyNotFound()
    {
        var harness = new Harness();
        harness.SetupValidCaller();
        harness.Roles.Setup(x => x.Get()).Returns(QueryReturning(Array.Empty<RoleInfo>()).Object);

        var service = harness.Build();

        Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await service.DeleteRoleAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Test]
    public void DeleteRoleAsync_WhenRoleIsSystemRole_ThrowsInvalidOperation()
    {
        var harness = new Harness();
        harness.SetupValidCaller();
        harness.Roles.Setup(x => x.Get())
            .Returns(QueryReturning<RoleInfo>(new RoleInfo[] { CreateRole(1, Guid.NewGuid(), "Administrator", "Administrator", "desc") }).Object);

        var service = harness.Build();

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.DeleteRoleAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Test]
    public void DeleteRoleAsync_WhenRoleAssignedToUsers_ThrowsInvalidOperation()
    {
        var harness = new Harness();
        harness.SetupValidCaller();
        harness.Roles.Setup(x => x.Get())
            .Returns(QueryReturning<RoleInfo>(new RoleInfo[] { CreateRole(5, Guid.NewGuid(), "Editors", "Editors", "desc") }).Object);
        harness.UserRoles.Setup(x => x.Get())
            .Returns(QueryReturning<UserRoleInfo>(new UserRoleInfo[] { CreateUserRole(5) }).Object);

        var service = harness.Build();

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.DeleteRoleAsync(Guid.NewGuid(), CancellationToken.None));
    }

    #endregion

    #region AssignRolePermissionsAsync

    [Test]
    public void AssignRolePermissionsAsync_WhenRoleNameMissing_ThrowsInvalidOperation()
    {
        var harness = new Harness();
        harness.SetupValidCaller();

        var service = harness.Build();

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.AssignRolePermissionsAsync(new AssignRolePermissionsInput { RoleName = "  " }, CancellationToken.None));
    }

    [Test]
    public void AssignRolePermissionsAsync_WhenNothingToAssign_ThrowsInvalidOperation()
    {
        var harness = new Harness();
        harness.SetupValidCaller();

        var service = harness.Build();

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.AssignRolePermissionsAsync(new AssignRolePermissionsInput { RoleName = "Editors" }, CancellationToken.None));
    }

    [Test]
    public void AssignRolePermissionsAsync_WhenRoleNotFound_ThrowsInvalidOperation()
    {
        var harness = new Harness();
        harness.SetupValidCaller();
        harness.Roles.Setup(x => x.Get()).Returns(QueryReturning(Array.Empty<RoleInfo>()).Object);

        var input = new AssignRolePermissionsInput
        {
            RoleName = "Editors",
            Applications = new List<ApplicationPermissionInput>
            {
                new() { ApplicationName = "redirects", Permissions = ViewPermission }
            }
        };

        var service = harness.Build();

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.AssignRolePermissionsAsync(input, CancellationToken.None));
    }

    #endregion

    #region CreateChannelRoleAsync

    [Test]
    public void CreateChannelRoleAsync_WhenWebsiteChannelMissing_ThrowsInvalidOperation()
    {
        var harness = new Harness();
        harness.SetupValidCaller();

        var service = harness.Build();

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.CreateChannelRoleAsync(
                new CreateChannelRoleInput { WebsiteChannel = "   ", RoleType = RolesAllowedDropdown.Editor },
                CancellationToken.None));
    }

    [Test]
    public void CreateChannelRoleAsync_WhenChannelNotFound_ThrowsChannelNotFound()
    {
        var harness = new Harness();
        harness.SetupValidCaller();
        harness.Channels.Setup(x => x.Get()).Returns(QueryReturning(Array.Empty<ChannelInfo>()).Object);

        var service = harness.Build();

        Assert.ThrowsAsync<ChannelNotFoundException>(async () =>
            await service.CreateChannelRoleAsync(
                new CreateChannelRoleInput { WebsiteChannel = "Ind", RoleType = RolesAllowedDropdown.Editor },
                CancellationToken.None));
    }

    [Test]
    public void CreateChannelRoleAsync_WhenRoleAlreadyExists_ThrowsChannelRoleExist()
    {
        var harness = new Harness();
        harness.SetupValidCaller();
        harness.Channels.Setup(x => x.Get())
            .Returns(QueryReturning<ChannelInfo>(new ChannelInfo[] { CreateChannel(1, "Ind") }).Object);
        harness.WebsiteChannels.Setup(x => x.Get())
            .Returns(QueryReturning<WebsiteChannelInfo>(new WebsiteChannelInfo[] { CreateWebsiteChannel(1, Guid.NewGuid()) }).Object);
        harness.Roles.Setup(x => x.Get())
            .Returns(QueryReturning<RoleInfo>(new RoleInfo[] { CreateRole(1, Guid.NewGuid(), "Ind-Editor", "Ind-Editor", "desc") }).Object);

        var service = harness.Build();

        Assert.ThrowsAsync<ChannelRoleExistException>(async () =>
            await service.CreateChannelRoleAsync(
                new CreateChannelRoleInput { WebsiteChannel = "Ind", RoleType = RolesAllowedDropdown.Editor },
                CancellationToken.None));
    }

    #endregion

    #region CreateChannelRolesForAllChannelsAsync

    [Test]
    public void CreateChannelRolesForAllChannelsAsync_WhenCallerInvalid_ThrowsUnauthorized()
    {
        var harness = new Harness();

        var service = harness.Build();

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await service.CreateChannelRolesForAllChannelsAsync(CancellationToken.None));
    }

    [Test]
    public async Task CreateChannelRolesForAllChannelsAsync_WhenNoChannels_ReturnsEmpty()
    {
        var harness = new Harness();
        harness.SetupValidCaller();

        var service = harness.Build();

        var result = await service.CreateChannelRolesForAllChannelsAsync(CancellationToken.None);

        Assert.That(result, Is.Empty);
    }

    #endregion

    #region GetWebsiteChannelNamesAsync

    [Test]
    public async Task GetWebsiteChannelNamesAsync_ReturnsChannelNamesOrderedAlphabetically()
    {
        var harness = new Harness();
        harness.Channels.Setup(x => x.Get())
            .Returns(QueryReturning<ChannelInfo>(new ChannelInfo[] { CreateChannel(1, "Zeta"), CreateChannel(2, "Alpha") }).Object);
        harness.WebsiteChannels.Setup(x => x.Get())
            .Returns(QueryReturning<WebsiteChannelInfo>(new WebsiteChannelInfo[]
            {
                CreateWebsiteChannel(1, Guid.NewGuid()),
                CreateWebsiteChannel(2, Guid.NewGuid())
            }).Object);

        var service = harness.Build();

        var result = (await service.GetWebsiteChannelNamesAsync(CancellationToken.None)).ToList();

        Assert.That(result, Is.EqualTo(ExpectedChannelNames));
    }

    [Test]
    public async Task GetWebsiteChannelNamesAsync_WhenNoChannels_ReturnsEmpty()
    {
        var harness = new Harness();
        harness.Channels.Setup(x => x.Get()).Returns(QueryReturning(Array.Empty<ChannelInfo>()).Object);
        harness.WebsiteChannels.Setup(x => x.Get()).Returns(QueryReturning(Array.Empty<WebsiteChannelInfo>()).Object);

        var service = harness.Build();

        var result = await service.GetWebsiteChannelNamesAsync(CancellationToken.None);

        Assert.That(result, Is.Empty);
    }

    #endregion

    #region AssignRolePermissionsAsync grant resolution / validation

    private static Harness HarnessWithExistingRole(string roleName = "Editors")
    {
        var harness = new Harness();
        harness.SetupValidCaller();
        harness.Roles.Setup(x => x.Get())
            .Returns(QueryReturning<RoleInfo>(new RoleInfo[] { CreateRole(1, Guid.NewGuid(), roleName, roleName, "desc") }).Object);
        return harness;
    }

    [Test]
    public void AssignRolePermissionsAsync_WhenWebsiteChannelNameMissing_ThrowsInvalidOperation()
    {
        var harness = HarnessWithExistingRole();
        var input = new AssignRolePermissionsInput
        {
            RoleName = "Editors",
            WebsiteChannels = new List<ChannelPermissionInput> { new() { WebsiteChannel = "  " } }
        };

        var service = harness.Build();

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.AssignRolePermissionsAsync(input, CancellationToken.None));
    }

    [Test]
    public void AssignRolePermissionsAsync_WhenWebsiteChannelPermissionUnknown_ThrowsInvalidOperation()
    {
        var harness = HarnessWithExistingRole();
        var input = new AssignRolePermissionsInput
        {
            RoleName = "Editors",
            WebsiteChannels = new List<ChannelPermissionInput>
            {
                new() { WebsiteChannel = "Ind", Permissions = FlyPermission }
            }
        };

        var service = harness.Build();

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.AssignRolePermissionsAsync(input, CancellationToken.None));
    }

    [Test]
    public void AssignRolePermissionsAsync_WhenWebsiteChannelNotFound_ThrowsChannelNotFound()
    {
        var harness = HarnessWithExistingRole();
        harness.Channels.Setup(x => x.Get()).Returns(QueryReturning(Array.Empty<ChannelInfo>()).Object);
        var input = new AssignRolePermissionsInput
        {
            RoleName = "Editors",
            WebsiteChannels = new List<ChannelPermissionInput>
            {
                new() { WebsiteChannel = "Ind", Permissions = ViewPermission }
            }
        };

        var service = harness.Build();

        Assert.ThrowsAsync<ChannelNotFoundException>(async () =>
            await service.AssignRolePermissionsAsync(input, CancellationToken.None));
    }

    [Test]
    public void AssignRolePermissionsAsync_WhenEmailChannelNameMissing_ThrowsInvalidOperation()
    {
        var harness = HarnessWithExistingRole();
        var input = new AssignRolePermissionsInput
        {
            RoleName = "Editors",
            EmailChannels = new List<EmailChannelPermissionInput> { new() { EmailChannel = " " } }
        };

        var service = harness.Build();

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.AssignRolePermissionsAsync(input, CancellationToken.None));
    }

    [Test]
    public void AssignRolePermissionsAsync_WhenApplicationNameMissing_ThrowsInvalidOperation()
    {
        var harness = HarnessWithExistingRole();
        var input = new AssignRolePermissionsInput
        {
            RoleName = "Editors",
            Applications = new List<ApplicationPermissionInput> { new() { ApplicationName = null } }
        };

        var service = harness.Build();

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.AssignRolePermissionsAsync(input, CancellationToken.None));
    }

    [Test]
    public void AssignRolePermissionsAsync_WhenApplicationNameLooksLikeChannel_ThrowsInvalidOperation()
    {
        var harness = HarnessWithExistingRole();
        var input = new AssignRolePermissionsInput
        {
            RoleName = "Editors",
            Applications = new List<ApplicationPermissionInput>
            {
                new() { ApplicationName = "Kentico.Xperience.Application.WebPages_abc", Permissions = ViewPermission }
            }
        };

        var service = harness.Build();

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.AssignRolePermissionsAsync(input, CancellationToken.None));
    }

    [Test]
    public void AssignRolePermissionsAsync_WhenWorkspaceNameMissing_ThrowsInvalidOperation()
    {
        var harness = HarnessWithExistingRole();
        var input = new AssignRolePermissionsInput
        {
            RoleName = "Editors",
            Workspaces = new List<WorkspacePermissionInput> { new() { Workspace = "  " } }
        };

        var service = harness.Build();

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.AssignRolePermissionsAsync(input, CancellationToken.None));
    }

    [Test]
    public void AssignRolePermissionsAsync_WhenWorkspaceNotFound_ThrowsInvalidOperation()
    {
        var harness = HarnessWithExistingRole();
        harness.Workspaces.Setup(x => x.Get()).Returns(QueryReturning(Array.Empty<WorkspaceInfo>()).Object);
        var input = new AssignRolePermissionsInput
        {
            RoleName = "Editors",
            Workspaces = new List<WorkspacePermissionInput>
            {
                new() { Workspace = "Default", Permissions = ViewPermission }
            }
        };

        var service = harness.Build();

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.AssignRolePermissionsAsync(input, CancellationToken.None));
    }

    #endregion

    #region Test harness and fakes

    private sealed class Harness
    {
        public readonly Mock<IInfoProvider<UserInfo>> Users = new();
        public readonly Mock<IInfoProvider<UserRoleInfo>> UserRoles = new();
        public readonly Mock<IInfoProvider<RoleInfo>> Roles = new();
        public readonly Mock<IInfoProvider<ApplicationPermissionInfo>> ApplicationPermissions = new();
        public readonly Mock<IInfoProvider<WorkspaceDataPermissionInfo>> WorkspaceDataPermissions = new();
        public readonly Mock<IInfoProvider<WorkspaceInfo>> Workspaces = new();
        public readonly Mock<IInfoProvider<EmailChannelInfo>> EmailChannels = new();
        public readonly Mock<IInfoProvider<WebPageAclRoleInfo>> AclRoles = new();
        public readonly Mock<IInfoProvider<WebPageAclRolePermissionInfo>> AclRolePermissions = new();
        public readonly Mock<IInfoProvider<WebsiteChannelInfo>> WebsiteChannels = new();
        public readonly Mock<IInfoProvider<ChannelInfo>> Channels = new();
        public readonly Mock<IHttpContextAccessor> Http = new();
        public readonly Mock<IEventLogService> EventLog = new();

        public Harness()
        {
            // Default every provider to an empty result so helper queries never hit a null query.
            Users.Setup(x => x.Get()).Returns(QueryReturning(Array.Empty<UserInfo>()).Object);
            UserRoles.Setup(x => x.Get()).Returns(QueryReturning(Array.Empty<UserRoleInfo>()).Object);
            Roles.Setup(x => x.Get()).Returns(QueryReturning(Array.Empty<RoleInfo>()).Object);
            ApplicationPermissions.Setup(x => x.Get()).Returns(QueryReturning(Array.Empty<ApplicationPermissionInfo>()).Object);
            WorkspaceDataPermissions.Setup(x => x.Get()).Returns(QueryReturning(Array.Empty<WorkspaceDataPermissionInfo>()).Object);
            Workspaces.Setup(x => x.Get()).Returns(QueryReturning(Array.Empty<WorkspaceInfo>()).Object);
            EmailChannels.Setup(x => x.Get()).Returns(QueryReturning(Array.Empty<EmailChannelInfo>()).Object);
            AclRoles.Setup(x => x.Get()).Returns(QueryReturning(Array.Empty<WebPageAclRoleInfo>()).Object);
            AclRolePermissions.Setup(x => x.Get()).Returns(QueryReturning(Array.Empty<WebPageAclRolePermissionInfo>()).Object);
            WebsiteChannels.Setup(x => x.Get()).Returns(QueryReturning(Array.Empty<WebsiteChannelInfo>()).Object);
            Channels.Setup(x => x.Get()).Returns(QueryReturning(Array.Empty<ChannelInfo>()).Object);
        }

        public RoleCrudService Build() =>
            new(
                Users.Object,
                UserRoles.Object,
                Roles.Object,
                ApplicationPermissions.Object,
                WorkspaceDataPermissions.Object,
                Workspaces.Object,
                EmailChannels.Object,
                AclRoles.Object,
                AclRolePermissions.Object,
                WebsiteChannels.Object,
                Channels.Object,
                Http.Object,
                EventLog.Object);

        /// <summary>Configures the context-building providers to return empty data for read mapping.</summary>
        public void SetupEmptyContext()
        {
            ApplicationPermissions.Setup(x => x.Get()).Returns(QueryReturning(Array.Empty<ApplicationPermissionInfo>()).Object);
            WorkspaceDataPermissions.Setup(x => x.Get()).Returns(QueryReturning(Array.Empty<WorkspaceDataPermissionInfo>()).Object);
            Workspaces.Setup(x => x.Get()).Returns(QueryReturning(Array.Empty<WorkspaceInfo>()).Object);
            EmailChannels.Setup(x => x.Get()).Returns(QueryReturning(Array.Empty<EmailChannelInfo>()).Object);
            WebsiteChannels.Setup(x => x.Get()).Returns(QueryReturning(Array.Empty<WebsiteChannelInfo>()).Object);
            Channels.Setup(x => x.Get()).Returns(QueryReturning(Array.Empty<ChannelInfo>()).Object);
        }

        /// <summary>Sets up an authenticated caller that is enabled and has administration access.</summary>
        public void SetupValidCaller(string userName = "admin")
        {
            Http.Setup(x => x.HttpContext).Returns(AuthenticatedContext(userName));
            Users.Setup(x => x.Get())
                .Returns(QueryReturning<UserInfo>(new UserInfo[] { CreateUser(userName, enabled: true, adminAccess: true) }).Object);
        }
    }

    private static Mock<ObjectQuery<T>> QueryReturning<T>(IEnumerable<T> rows)
        where T : BaseInfo
    {
        var query = new Mock<ObjectQuery<T>>();

        query.Setup(x => x.WhereContains(It.IsAny<string>(), It.IsAny<string>())).Returns(query.Object);
        query.Setup(x => x.WhereEquals(It.IsAny<string>(), It.IsAny<object>())).Returns(query.Object);
        query.Setup(x => x.WhereNotEquals(It.IsAny<string>(), It.IsAny<object>())).Returns(query.Object);
        query.Setup(x => x.WhereIn(It.IsAny<string>(), It.IsAny<ICollection<int>>())).Returns(query.Object);
        query.Setup(x => x.WhereIn(It.IsAny<string>(), It.IsAny<ICollection<Guid>>())).Returns(query.Object);
        query.Setup(x => x.Columns(It.IsAny<string[]>())).Returns(query.Object);
        query.Setup(x => x.TopN(It.IsAny<int>())).Returns(query.Object);
        query.Setup(x => x.GetEnumerableTypedResultAsync(
                It.IsAny<CommandBehavior>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken?>()))
            .ReturnsAsync(rows);

        return query;
    }

    private static DefaultHttpContext AuthenticatedContext(string? userName)
    {
        var identity = new ClaimsIdentity("TestAuth");
        if (!string.IsNullOrEmpty(userName))
            identity.AddClaim(new Claim(ClaimTypes.Name, userName));

        return new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
    }

    private static FakeUserInfo CreateUser(string userName, bool enabled, bool adminAccess)
    {
        var user = (FakeUserInfo)RuntimeHelpers.GetUninitializedObject(typeof(FakeUserInfo));
        user.UserName = userName;
        user.UserEnabled = enabled;
        user.UserAdministrationAccess = adminAccess;
        return user;
    }

    private static FakeRoleInfo CreateRole(int id, Guid guid, string roleName, string displayName, string description)
    {
        var role = (FakeRoleInfo)RuntimeHelpers.GetUninitializedObject(typeof(FakeRoleInfo));
        role.RoleID = id;
        role.RoleGUID = guid;
        role.RoleName = roleName;
        role.RoleDisplayName = displayName;
        role.RoleDescription = description;
        return role;
    }

    private static FakeUserRoleInfo CreateUserRole(int roleId)
    {
        var link = (FakeUserRoleInfo)RuntimeHelpers.GetUninitializedObject(typeof(FakeUserRoleInfo));
        link.RoleID = roleId;
        return link;
    }

    private static FakeChannelInfo CreateChannel(int id, string channelName)
    {
        var channel = (FakeChannelInfo)RuntimeHelpers.GetUninitializedObject(typeof(FakeChannelInfo));
        channel.ChannelID = id;
        channel.ChannelName = channelName;
        return channel;
    }

    private static FakeWebsiteChannelInfo CreateWebsiteChannel(int channelId, Guid guid)
    {
        var websiteChannel = (FakeWebsiteChannelInfo)RuntimeHelpers.GetUninitializedObject(typeof(FakeWebsiteChannelInfo));
        websiteChannel.WebsiteChannelChannelID = channelId;
        websiteChannel.WebsiteChannelGUID = guid;
        return websiteChannel;
    }

    private const string ChannelApplicationPrefix = "Kentico.Xperience.Application.WebPages_";
    private const string EmailChannelApplicationPrefix = "Kentico.Xperience.Application.EmailChannel_";
    private const string ContentHubApplicationName = "Kentico.Xperience.Application.ContentHub";

    private static FakeChannelInfo CreateChannel(int id, string channelName, Guid guid, string displayName)
    {
        var channel = (FakeChannelInfo)RuntimeHelpers.GetUninitializedObject(typeof(FakeChannelInfo));
        channel.ChannelID = id;
        channel.ChannelName = channelName;
        channel.ChannelGUID = guid;
        channel.ChannelDisplayName = displayName;
        return channel;
    }

    private static FakeEmailChannelInfo CreateEmailChannel(int channelId, Guid guid)
    {
        var emailChannel = (FakeEmailChannelInfo)RuntimeHelpers.GetUninitializedObject(typeof(FakeEmailChannelInfo));
        emailChannel.EmailChannelChannelID = channelId;
        emailChannel.EmailChannelGUID = guid;
        return emailChannel;
    }

    private static FakeApplicationPermissionInfo CreateApplicationPermission(int roleId, string applicationName, string permissionName)
    {
        var permission = (FakeApplicationPermissionInfo)RuntimeHelpers.GetUninitializedObject(typeof(FakeApplicationPermissionInfo));
        permission.RoleID = roleId;
        permission.ApplicationName = applicationName;
        permission.PermissionName = permissionName;
        return permission;
    }

    private static FakeWorkspaceInfo CreateWorkspace(int id, Guid guid, string name, string displayName)
    {
        var workspace = (FakeWorkspaceInfo)RuntimeHelpers.GetUninitializedObject(typeof(FakeWorkspaceInfo));
        workspace.WorkspaceID = id;
        workspace.WorkspaceGUID = guid;
        workspace.WorkspaceName = name;
        workspace.WorkspaceDisplayName = displayName;
        return workspace;
    }

    private static FakeWorkspaceDataPermissionInfo CreateWorkspacePermission(int roleId, int workspaceId, string applicationName, string permissionName)
    {
        var permission = (FakeWorkspaceDataPermissionInfo)RuntimeHelpers.GetUninitializedObject(typeof(FakeWorkspaceDataPermissionInfo));
        permission.WorkspaceDataPermissionRoleID = roleId;
        permission.WorkspaceDataPermissionWorkspaceID = workspaceId;
        permission.WorkspaceDataPermissionApplicationName = applicationName;
        permission.WorkspaceDataPermissionPermissionName = permissionName;
        return permission;
    }

    private sealed class FakeUserInfo : UserInfo
    {
        private string userName = string.Empty;
        private bool userEnabled;
        private bool userAdministrationAccess;

        public override string UserName { get => userName; set => userName = value; }
        public override bool UserEnabled { get => userEnabled; set => userEnabled = value; }
        public override bool UserAdministrationAccess { get => userAdministrationAccess; set => userAdministrationAccess = value; }
    }

    private sealed class FakeRoleInfo : RoleInfo
    {
        private int roleID;
        private Guid roleGUID;
        private string roleName = string.Empty;
        private string roleDisplayName = string.Empty;
        private string roleDescription = string.Empty;

        public override int RoleID { get => roleID; set => roleID = value; }
        public override Guid RoleGUID { get => roleGUID; set => roleGUID = value; }
        public override string RoleName { get => roleName; set => roleName = value; }
        public override string RoleDisplayName { get => roleDisplayName; set => roleDisplayName = value; }
        public override string RoleDescription { get => roleDescription; set => roleDescription = value; }
    }

    private sealed class FakeUserRoleInfo : UserRoleInfo
    {
        private int roleID;

        public override int RoleID { get => roleID; set => roleID = value; }
    }

    private sealed class FakeChannelInfo : ChannelInfo
    {
        private int channelID;
        private Guid channelGUID;
        private string channelName = string.Empty;
        private string channelDisplayName = string.Empty;

        public override int ChannelID { get => channelID; set => channelID = value; }
        public override Guid ChannelGUID { get => channelGUID; set => channelGUID = value; }
        public override string ChannelName { get => channelName; set => channelName = value; }
        public override string ChannelDisplayName { get => channelDisplayName; set => channelDisplayName = value; }
    }

    private sealed class FakeWebsiteChannelInfo : WebsiteChannelInfo
    {
        private int channelID;
        private Guid websiteChannelGUID;

        public override int WebsiteChannelChannelID { get => channelID; set => channelID = value; }
        public override Guid WebsiteChannelGUID { get => websiteChannelGUID; set => websiteChannelGUID = value; }
    }

    private sealed class FakeEmailChannelInfo : EmailChannelInfo
    {
        private int channelID;
        private Guid emailChannelGUID;

        public override int EmailChannelChannelID { get => channelID; set => channelID = value; }
        public override Guid EmailChannelGUID { get => emailChannelGUID; set => emailChannelGUID = value; }
    }

    private sealed class FakeApplicationPermissionInfo : ApplicationPermissionInfo
    {
        private int roleID;
        private string applicationName = string.Empty;
        private string permissionName = string.Empty;

        public override int RoleID { get => roleID; set => roleID = value; }
        public override string ApplicationName { get => applicationName; set => applicationName = value; }
        public override string PermissionName { get => permissionName; set => permissionName = value; }
    }

    private sealed class FakeWorkspaceInfo : WorkspaceInfo
    {
        private int workspaceID;
        private Guid workspaceGUID;
        private string workspaceName = string.Empty;
        private string workspaceDisplayName = string.Empty;

        public override int WorkspaceID { get => workspaceID; set => workspaceID = value; }
        public override Guid WorkspaceGUID { get => workspaceGUID; set => workspaceGUID = value; }
        public override string WorkspaceName { get => workspaceName; set => workspaceName = value; }
        public override string WorkspaceDisplayName { get => workspaceDisplayName; set => workspaceDisplayName = value; }
    }

    private sealed class FakeWorkspaceDataPermissionInfo : WorkspaceDataPermissionInfo
    {
        private int roleID;
        private int workspaceID;
        private string applicationName = string.Empty;
        private string permissionName = string.Empty;

        public override int WorkspaceDataPermissionRoleID { get => roleID; set => roleID = value; }
        public override int WorkspaceDataPermissionWorkspaceID { get => workspaceID; set => workspaceID = value; }
        public override string WorkspaceDataPermissionApplicationName { get => applicationName; set => applicationName = value; }
        public override string WorkspaceDataPermissionPermissionName { get => permissionName; set => permissionName = value; }
    }

    #endregion
}
