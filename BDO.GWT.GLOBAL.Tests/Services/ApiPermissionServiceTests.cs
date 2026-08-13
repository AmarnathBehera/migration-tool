using BDO;
using BDO.GWT.GLOBAL.Services;
using CMS.DataEngine;
using CMS.Membership;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Services;

public class ApiPermissionServiceTests
{
    private Mock<IInfoProvider<UserInfo>> userProvider = null!;
    private Mock<IInfoProvider<Api_permissionsInfo>> permissionProvider = null!;
    private ApiPermissionService service = null!;

    [SetUp]
    public void SetUp()
    {
        userProvider = new Mock<IInfoProvider<UserInfo>>();
        permissionProvider = new Mock<IInfoProvider<Api_permissionsInfo>>();
        service = new ApiPermissionService(userProvider.Object, permissionProvider.Object);
    }

    [Test]
    public void GetMethodGroups_ReturnsAllKnownGroupsInIdOrder()
    {
        var groups = service.GetMethodGroups();

        Assert.Multiple(() =>
        {
            Assert.That(groups, Has.Count.EqualTo(18));
            Assert.That(groups.Select(group => group.Id), Is.EqualTo(Enumerable.Range(1, 18)));
            Assert.That(groups.Select(group => group.DisplayName), Does.Contain("Users"));
            Assert.That(groups.Select(group => group.DisplayName), Does.Contain("Media File"));
        });
    }

    [Test]
    public void GetUsers_ReturnsUsersOrderedByUserNameWithDisplayName()
    {
        var users = new[]
        {
            CreateUser(2, "zcarter", "Zoe", "Carter"),
            CreateUser(1, "asmith", "Ada", "Smith")
        };
        userProvider.Setup(provider => provider.Get()).Returns(users.AsQueryable());

        var result = service.GetUsers();

        Assert.That(result.Select(user => user.Id), Is.EqualTo(new[] { 1, 2 }));
        Assert.That(result.Select(user => user.DisplayName), Is.EqualTo(new[]
        {
            "Ada Smith (asmith)",
            "Zoe Carter (zcarter)"
        }));
    }

    [Test]
    public void GetMatrixByUser_ReturnsOnlyMatchingChannelAndIgnoresMalformedRecords()
    {
        var permissions = new[]
        {
            CreatePermission(7, "Users", 10, true, false),
            CreatePermission(7, "Users", 20, false, true),
            CreatePermission(7, "Unknown", 10, true, true),
            CreatePermission(7, "Events", 10, false, false)
        };
        permissionProvider.Setup(provider => provider.Get()).Returns(permissions.AsQueryable());

        var result = service.GetMatrixByUser(7, 10);

        var usersRow = result.Single(row => row.MethodGroupId == 1);
        var eventsRow = result.Single(row => row.MethodGroupId == 2);
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(18));
            Assert.That(usersRow.AllowRead, Is.True);
            Assert.That(usersRow.AllowManage, Is.False);
            Assert.That(eventsRow.AllowRead, Is.False);
            Assert.That(eventsRow.AllowManage, Is.False);
        });
    }

    [Test]
    public void GetMatrixByMethodGroup_ReturnsAllUsersWithPermissionState()
    {
        var users = new[]
        {
            CreateUser(2, "zcarter", "Zoe", "Carter"),
            CreateUser(1, "asmith", "Ada", "Smith")
        };
        userProvider.Setup(provider => provider.Get()).Returns(users.AsQueryable());
        permissionProvider.Setup(provider => provider.Get()).Returns(new[]
        {
            CreatePermission(1, "Events", 10, true, true)
        }.AsQueryable());

        var result = service.GetMatrixByMethodGroup(2, 10);

        Assert.Multiple(() =>
        {
            Assert.That(result.Select(row => row.UserId), Is.EqualTo(new[] { 1, 2 }));
            Assert.That(result.All(row => row.MethodGroupName == "Events"), Is.True);
            Assert.That(result[0].AllowRead, Is.True);
            Assert.That(result[0].AllowManage, Is.True);
            Assert.That(result[1].AllowRead, Is.False);
            Assert.That(result[1].AllowManage, Is.False);
        });
    }

    [Test]
    public void SetPermission_UpdatesExistingRecordForSelectedChannel()
    {
        var record = CreatePermission(7, "Users", 10, false, false);
        permissionProvider.Setup(provider => provider.Get()).Returns(new[] { record }.AsQueryable());

        service.SetPermission(7, 1, true, true, 10);

        permissionProvider.Verify(provider => provider.Set(record), Times.Once);
        Assert.Multiple(() =>
        {
            Assert.That(record.Read, Is.True);
            Assert.That(record.Manage, Is.True);
            Assert.That(record.MethodGroup, Is.EqualTo("Users"));
        });
    }

    [Test]
    public void SetPermission_CreatesRecordWithChannelAndPermissionFlags()
    {
        Api_permissionsInfo? savedRecord = null;
        permissionProvider.Setup(provider => provider.Get()).Returns(Array.Empty<Api_permissionsInfo>().AsQueryable());
        permissionProvider
            .Setup(provider => provider.Set(It.IsAny<Api_permissionsInfo>()))
            .Callback<Api_permissionsInfo>(record => savedRecord = record);

        service.SetPermission(7, 2, false, true, 20);

        permissionProvider.Verify(provider => provider.Set(It.IsAny<Api_permissionsInfo>()), Times.Once);
        Assert.That(savedRecord, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(savedRecord!.User, Is.EqualTo("7"));
            Assert.That(savedRecord.MethodGroup, Is.EqualTo("Events"));
            Assert.That(savedRecord.APIPermissionWebChannelSettingID, Is.EqualTo("20"));
            Assert.That(savedRecord.Read, Is.False);
            Assert.That(savedRecord.Manage, Is.True);
        });
    }

    private static UserInfo CreateUser(int id, string userName, string firstName, string lastName)
    {
        var user = new UserInfo
        {
            UserID = id,
            UserName = userName
        };
        user.SetValue("FirstName", firstName);
        user.SetValue("LastName", lastName);
        return user;
    }

    private static Api_permissionsInfo CreatePermission(
        int userId,
        string methodGroup,
        int channelId,
        bool read,
        bool manage) => new()
        {
            User = userId.ToString(),
            MethodGroup = methodGroup,
            APIPermissionWebChannelSettingID = channelId.ToString(),
            Read = read,
            Manage = manage
        };
}