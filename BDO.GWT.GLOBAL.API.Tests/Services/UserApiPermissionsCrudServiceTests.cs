using BDO.GWT.Domain.Core.Constants;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Services;
using CMS.DataEngine;
using CMS.Membership;
using Moq;
using System.Data;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BDO.GWT.GLOBAL.API.Tests.Services;

[TestFixture]
public class UserApiPermissionsCrudServiceTests
{
    // NOTE: IInfoProvider<TInfo>.Get() returns CMS.DataEngine.ObjectQuery<TInfo>, and
    // GetEnumerableTypedResultAsync / SetAsync / DeleteAsync are Kentico extension methods
    // tied to the CMS query/IoC infrastructure. They cannot be stubbed to return working,
    // queryable results in isolation (same limitation documented in CultureValidationServiceTests).
    // These tests therefore cover: the public interface/contract, constructor wiring, provider
    // invocation + exception propagation, and the pure/near-pure private helpers reachable via
    // reflection (DeduplicateByGroupName and the no-duplicate path of DeduplicateAndCleanUpAsync).

    private static UserApiPermissionsCrudService CreateServiceWithoutConstructor() =>
        (UserApiPermissionsCrudService)RuntimeHelpers.GetUninitializedObject(typeof(UserApiPermissionsCrudService));

    private static UserApiPermissionsCrudService CreateServiceWithMockedDependencies(
        out Mock<IInfoProvider<UserInfo>> userProviderMock,
        out Mock<IInfoProvider<Api_permissionsInfo>> apiProviderMock)
    {
        userProviderMock = new Mock<IInfoProvider<UserInfo>>();
        apiProviderMock = new Mock<IInfoProvider<Api_permissionsInfo>>();
        return new UserApiPermissionsCrudService(userProviderMock.Object, apiProviderMock.Object);
    }

    private static FakeApiPermissionsInfo CreatePermission(string methodGroup, int id, bool read = false, bool manage = false)
    {
        var info = (FakeApiPermissionsInfo)RuntimeHelpers.GetUninitializedObject(typeof(FakeApiPermissionsInfo));
        info.MethodGroup = methodGroup;
        info.Api_permissionsID = id;
        info.Read = read;
        info.Manage = manage;
        return info;
    }

    #region Contract

    [Test]
    public void Service_ImplementsInterface()
    {
        var service = CreateServiceWithoutConstructor();

        Assert.That(service, Is.InstanceOf<IUserApiPermissionsCrudService>());
    }

    [Test]
    public void Interface_HasExpectedMethods()
    {
        var interfaceType = typeof(IUserApiPermissionsCrudService);

        Assert.Multiple(() =>
        {
            Assert.That(interfaceType.GetMethod("GetUserApiPermissionsByEndpointGroupNameAsync"), Is.Not.Null);
            Assert.That(interfaceType.GetMethod("SetUserApiPermissionsByEndpointGroupNameAsync"), Is.Not.Null);
        });
    }

    [Test]
    public void Interface_Methods_HaveExpectedReturnTypes()
    {
        var interfaceType = typeof(IUserApiPermissionsCrudService);

        Assert.Multiple(() =>
        {
            Assert.That(interfaceType.GetMethod("GetUserApiPermissionsByEndpointGroupNameAsync")!.ReturnType,
                Is.EqualTo(typeof(Task<IDictionary<string, ApiEndpointGroupPermissions>>)));
            Assert.That(interfaceType.GetMethod("SetUserApiPermissionsByEndpointGroupNameAsync")!.ReturnType,
                Is.EqualTo(typeof(Task<IDictionary<string, ApiEndpointGroupPermissions>>)));
        });
    }

    [Test]
    public void Interface_Methods_AcceptCancellationToken()
    {
        foreach (var method in typeof(IUserApiPermissionsCrudService).GetMethods())
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
    public void Constructor_WithDependencies_CreatesInstance()
    {
        var service = CreateServiceWithMockedDependencies(out _, out _);

        Assert.That(service, Is.Not.Null);
    }

    [Test]
    public void Constructor_AssignsAllPrivateDependencyFields()
    {
        var service = CreateServiceWithMockedDependencies(out var userProviderMock, out var apiProviderMock);

        var userField = typeof(UserApiPermissionsCrudService)
            .GetField("_userInfoProvider", BindingFlags.Instance | BindingFlags.NonPublic);
        var apiField = typeof(UserApiPermissionsCrudService)
            .GetField("_apiPermissionInfoProvider", BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.Multiple(() =>
        {
            Assert.That(userField!.GetValue(service), Is.SameAs(userProviderMock.Object));
            Assert.That(apiField!.GetValue(service), Is.SameAs(apiProviderMock.Object));
        });
    }

    [Test]
    public void ApiMethodGroupNames_StaticField_IsPopulatedFromConstants()
    {
        var field = typeof(UserApiPermissionsCrudService)
            .GetField("ApiMethodGroupNames", BindingFlags.Static | BindingFlags.NonPublic);
        var names = (string[])field!.GetValue(null)!;

        Assert.Multiple(() =>
        {
            Assert.That(names, Is.Not.Empty);
            Assert.That(names, Does.Contain(ApiMethodGroupName.Users));
            Assert.That(names, Does.Contain(ApiMethodGroupName.Content));
            Assert.That(names, Has.No.Member(string.Empty));
            Assert.That(names, Is.Unique);
        });
    }

    #endregion

    #region Public methods - provider invocation & exception propagation

    [Test]
    public void GetUserApiPermissions_WhenUserProviderThrows_PropagatesException()
    {
        var service = CreateServiceWithMockedDependencies(out var userProviderMock, out _);
        userProviderMock.Setup(x => x.Get()).Throws(new InvalidOperationException("query failed"));

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.GetUserApiPermissionsByEndpointGroupNameAsync(1, Guid.NewGuid(), CancellationToken.None));

        userProviderMock.Verify(x => x.Get(), Times.Once);
    }

    [Test]
    public void SetUserApiPermissions_WhenUserProviderThrows_PropagatesException()
    {
        var service = CreateServiceWithMockedDependencies(out var userProviderMock, out _);
        userProviderMock.Setup(x => x.Get()).Throws(new InvalidOperationException("query failed"));

        var request = new Dictionary<string, ApiEndpointGroupPermissions>();

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.SetUserApiPermissionsByEndpointGroupNameAsync(1, Guid.NewGuid(), request, CancellationToken.None));

        userProviderMock.Verify(x => x.Get(), Times.Once);
    }

    [Test]
    public void GetUserApiPermissions_WhenDependenciesUnavailable_Throws()
    {
        var service = CreateServiceWithoutConstructor();

        Assert.ThrowsAsync<NullReferenceException>(async () =>
            await service.GetUserApiPermissionsByEndpointGroupNameAsync(1, Guid.NewGuid(), CancellationToken.None));
    }

    [Test]
    public void SetUserApiPermissions_WhenDependenciesUnavailable_Throws()
    {
        var service = CreateServiceWithoutConstructor();

        Assert.ThrowsAsync<NullReferenceException>(async () =>
            await service.SetUserApiPermissionsByEndpointGroupNameAsync(
                1, Guid.NewGuid(), new Dictionary<string, ApiEndpointGroupPermissions>(), CancellationToken.None));
    }

    [Test]
    public void GetUserApiPermissions_WithMockedProvider_InvokesUserProviderGet()
    {
        var service = CreateServiceWithMockedDependencies(out var userProviderMock, out _);
        // Unstubbed Get() returns null -> NRE inside GetValidatedUserAsync, but Get() is still invoked.
        Assert.ThrowsAsync<NullReferenceException>(async () =>
            await service.GetUserApiPermissionsByEndpointGroupNameAsync(5, Guid.NewGuid(), CancellationToken.None));

        userProviderMock.Verify(x => x.Get(), Times.Once);
    }

    #endregion

    #region DeduplicateByGroupName (pure static helper)

    private static IDictionary<string, Api_permissionsInfo> InvokeDeduplicateByGroupName(IEnumerable<Api_permissionsInfo> input)
    {
        var method = typeof(UserApiPermissionsCrudService)
            .GetMethod("DeduplicateByGroupName", BindingFlags.Static | BindingFlags.NonPublic)!;
        return (IDictionary<string, Api_permissionsInfo>)method.Invoke(null, [input])!;
    }

    [Test]
    public void DeduplicateByGroupName_WithEmptyInput_ReturnsEmptyDictionary()
    {
        var result = InvokeDeduplicateByGroupName([]);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void DeduplicateByGroupName_WithUniqueGroups_KeepsAllRows()
    {
        var input = new Api_permissionsInfo[]
        {
            CreatePermission(ApiMethodGroupName.Users, 1, read: true),
            CreatePermission(ApiMethodGroupName.Content, 2, manage: true)
        };

        var result = InvokeDeduplicateByGroupName(input);

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[ApiMethodGroupName.Users].Read, Is.True);
            Assert.That(result[ApiMethodGroupName.Content].Manage, Is.True);
        });
    }

    [Test]
    public void DeduplicateByGroupName_WithDuplicateGroups_KeepsHighestId()
    {
        var input = new Api_permissionsInfo[]
        {
            CreatePermission(ApiMethodGroupName.Users, 1, read: false),
            CreatePermission(ApiMethodGroupName.Users, 5, read: true),
            CreatePermission(ApiMethodGroupName.Users, 3, read: false)
        };

        var result = InvokeDeduplicateByGroupName(input);

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[ApiMethodGroupName.Users].Api_permissionsID, Is.EqualTo(5));
            Assert.That(result[ApiMethodGroupName.Users].Read, Is.True);
        });
    }

    #endregion

    #region DeduplicateAndCleanUpAsync (no-duplicate path via reflection)

    private static Task<IDictionary<string, Api_permissionsInfo>> InvokeDeduplicateAndCleanUpAsync(
        UserApiPermissionsCrudService service, IEnumerable<Api_permissionsInfo> input)
    {
        var method = typeof(UserApiPermissionsCrudService)
            .GetMethod("DeduplicateAndCleanUpAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;
        return (Task<IDictionary<string, Api_permissionsInfo>>)method.Invoke(service, [input, CancellationToken.None])!;
    }

    [Test]
    public async Task DeduplicateAndCleanUpAsync_WithEmptyInput_ReturnsEmptyDictionary()
    {
        var service = CreateServiceWithMockedDependencies(out _, out _);

        var result = await InvokeDeduplicateAndCleanUpAsync(service, []);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task DeduplicateAndCleanUpAsync_WithUniqueGroups_ReturnsRows_AndDeletesNothing()
    {
        var service = CreateServiceWithMockedDependencies(out _, out var apiProviderMock);

        var input = new Api_permissionsInfo[]
        {
            CreatePermission(ApiMethodGroupName.Users, 10),
            CreatePermission(ApiMethodGroupName.Content, 11)
        };

        var result = await InvokeDeduplicateAndCleanUpAsync(service, input);

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[ApiMethodGroupName.Users].Api_permissionsID, Is.EqualTo(10));
            Assert.That(result[ApiMethodGroupName.Content].Api_permissionsID, Is.EqualTo(11));
        });
        // No duplicates -> no delete calls (Delete is the only sync interface member available).
        apiProviderMock.Verify(x => x.Delete(It.IsAny<Api_permissionsInfo>()), Times.Never);
    }

    [Test]
    public async Task DeduplicateAndCleanUpAsync_WithDuplicateGroups_DeletesLowerIdsAndKeepsHighest()
    {
        var service = CreateServiceWithMockedDependencies(out _, out var apiProviderMock);

        var keep = CreatePermission(ApiMethodGroupName.Users, 20, read: true, manage: true);
        var duplicate1 = CreatePermission(ApiMethodGroupName.Users, 10, read: false, manage: true);
        var duplicate2 = CreatePermission(ApiMethodGroupName.Users, 5, read: false, manage: false);

        var result = await InvokeDeduplicateAndCleanUpAsync(service, [keep, duplicate1, duplicate2]);

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[ApiMethodGroupName.Users].Api_permissionsID, Is.EqualTo(20));

        apiProviderMock.Verify(x => x.DeleteAsync(It.Is<Api_permissionsInfo>(p => p.Api_permissionsID == 10), It.IsAny<CancellationToken>()), Times.Once);
        apiProviderMock.Verify(x => x.DeleteAsync(It.Is<Api_permissionsInfo>(p => p.Api_permissionsID == 5), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetUserApiPermissionsAsync_WithNoRows_ReturnsAllKnownGroupsDefaultingToFalse()
    {
        var service = CreateServiceWithMockedDependencies(out _, out var apiProviderMock);
        apiProviderMock.Setup(x => x.Get()).Returns(QueryReturning<Api_permissionsInfo>([]).Object);

        var result = await InvokeGetUserApiPermissionsAsync(service, 42, 1001);

        var groupNames = (string[])typeof(UserApiPermissionsCrudService)
            .GetField("ApiMethodGroupNames", BindingFlags.Static | BindingFlags.NonPublic)!
            .GetValue(null)!;

        Assert.That(result.Keys, Is.SupersetOf(groupNames));
        Assert.That(result.Values.All(v => v.Read == false && v.Manage == false), Is.True);
    }

    [Test]
    public async Task GetUserApiPermissionsAsync_WithRows_UsesHighestIdPerGroup()
    {
        var service = CreateServiceWithMockedDependencies(out _, out var apiProviderMock);
        var rows = new Api_permissionsInfo[]
        {
            CreatePermission(ApiMethodGroupName.Users, 3, read: false, manage: false),
            CreatePermission(ApiMethodGroupName.Users, 9, read: true, manage: false),
            CreatePermission(ApiMethodGroupName.Content, 5, read: true, manage: true),
        };

        apiProviderMock.Setup(x => x.Get()).Returns(QueryReturning(rows).Object);

        var result = await InvokeGetUserApiPermissionsAsync(service, 7, 222);

        Assert.Multiple(() =>
        {
            Assert.That(result[ApiMethodGroupName.Users].Read, Is.True);
            Assert.That(result[ApiMethodGroupName.Users].Manage, Is.False);
            Assert.That(result[ApiMethodGroupName.Content].Read, Is.True);
            Assert.That(result[ApiMethodGroupName.Content].Manage, Is.True);
        });
    }

    [Test]
    public async Task SetUserApiPermissionsAsync_WithNoRequestAndNoExisting_CompletesWithoutThrowing()
    {
        var service = CreateServiceWithMockedDependencies(out _, out var apiProviderMock);
        apiProviderMock.Setup(x => x.Get()).Returns(QueryReturning<Api_permissionsInfo>([]).Object);

        await InvokeSetUserApiPermissionsAsync(service, 5, 111, new Dictionary<string, ApiEndpointGroupPermissions>());

        apiProviderMock.Verify(x => x.Get(), Times.Once);
    }

    #endregion

    #region ApiEndpointGroupPermissions DTO

    [Test]
    public void ApiEndpointGroupPermissions_DefaultsAreFalse()
    {
        var permissions = new ApiEndpointGroupPermissions();

        Assert.Multiple(() =>
        {
            Assert.That(permissions.Read, Is.False);
            Assert.That(permissions.Manage, Is.False);
        });
    }

    [TestCase(true, true)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(false, false)]
    public void ApiEndpointGroupPermissions_StoresValues(bool read, bool manage)
    {
        var permissions = new ApiEndpointGroupPermissions { Read = read, Manage = manage };

        Assert.Multiple(() =>
        {
            Assert.That(permissions.Read, Is.EqualTo(read));
            Assert.That(permissions.Manage, Is.EqualTo(manage));
        });
    }

    #endregion

    private static async Task<IDictionary<string, ApiEndpointGroupPermissions>> InvokeGetUserApiPermissionsAsync(
        UserApiPermissionsCrudService service, int websiteChannelId, int userId)
    {
        var method = typeof(UserApiPermissionsCrudService)
            .GetMethod("GetUserApiPermissionsAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var task = (Task<IDictionary<string, ApiEndpointGroupPermissions>>)method.Invoke(service, [websiteChannelId, userId, CancellationToken.None])!;
        return await task;
    }

    private static async Task InvokeSetUserApiPermissionsAsync(
        UserApiPermissionsCrudService service,
        int websiteChannelId,
        int userId,
        IDictionary<string, ApiEndpointGroupPermissions> request)
    {
        var method = typeof(UserApiPermissionsCrudService)
            .GetMethod("SetUserApiPermissionsAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var task = (Task)method.Invoke(service, [websiteChannelId, userId, request, CancellationToken.None])!;
        await task;
    }

    private static Mock<ObjectQuery<T>> QueryReturning<T>(IEnumerable<T> rows)
        where T : BaseInfo
    {
        var query = new Mock<ObjectQuery<T>>();

        query.Setup(x => x.WhereEquals(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(query.Object);
        query.Setup(x => x.WhereNotEquals(It.IsAny<string>(), It.IsAny<object>()))
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

    private sealed class FakeApiPermissionsInfo : Api_permissionsInfo
    {
        private int apiPermissionsId;
        private string methodGroup = string.Empty;
        private bool read;
        private bool manage;

        public override int Api_permissionsID
        {
            get => apiPermissionsId;
            set => apiPermissionsId = value;
        }

        public override string MethodGroup
        {
            get => methodGroup;
            set => methodGroup = value;
        }

        public override bool Read
        {
            get => read;
            set => read = value;
        }

        public override bool Manage
        {
            get => manage;
            set => manage = value;
        }
    }
}
