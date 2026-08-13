using BDO;
using BDO.GWT.GLOBAL.Services;
using CMS.Membership;

namespace BDO.GWT.GLOBAL.Tests.Services;

public class ApiPermissionServiceTests
{
    private List<UserInfo> users = null!;
    private List<Api_permissionsInfo> permissions = null!;
    private ApiPermissionService service = null!;

    [SetUp]
    public void SetUp()
    {
        users = new List<UserInfo>();
        permissions = new List<Api_permissionsInfo>();
        service = new ApiPermissionService(() => users, () => permissions, record =>
        {
            if (!permissions.Contains(record))
            {
                permissions.Add(record);
            }
        });
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
    public void GetUsers_WithNoUsers_ReturnsEmptyList()
    {
        Assert.That(service.GetUsers(), Is.Empty);
    }

    [Test]
    public void GetMatrixByUser_WithNoPermissions_ReturnsAllGroupsDenied()
    {
        var result = service.GetMatrixByUser(7, 10);

        Assert.That(result, Has.Count.EqualTo(18));
        Assert.That(result.All(row => !row.AllowRead && !row.AllowManage), Is.True);
    }

    [Test]
    public void GetMatrixByMethodGroup_WithNoUsers_ReturnsEmptyList()
    {
        Assert.That(service.GetMatrixByMethodGroup(2, 10), Is.Empty);
    }
}