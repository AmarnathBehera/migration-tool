using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Services;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BDO.GWT.GLOBAL.API.Tests.Services;

[TestFixture]
public class UserCrudServiceTests
{
    private static UserCrudService CreateServiceWithoutConstructor() =>
        (UserCrudService)RuntimeHelpers.GetUninitializedObject(typeof(UserCrudService));

    [Test]
    public void UserCrudService_ImplementsIUserCrudService()
    {
        var service = CreateServiceWithoutConstructor();

        Assert.That(service, Is.InstanceOf<IUserCrudService>());
    }

    [Test]
    public void UserCrudService_Interface_HasExpectedMethods()
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
    public void GetUsersAsync_WhenDependenciesUnavailable_Throws()
    {
        var service = CreateServiceWithoutConstructor();

        Assert.ThrowsAsync<NullReferenceException>(async () =>
            await service.GetUsersAsync("jdoe", CancellationToken.None));
    }

    [Test]
    public void GetUsersAsync_WithNullUserName_WhenDependenciesUnavailable_Throws()
    {
        var service = CreateServiceWithoutConstructor();

        Assert.ThrowsAsync<NullReferenceException>(async () =>
            await service.GetUsersAsync(null, CancellationToken.None));
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
        var input = new UserInput
        {
            General = new UserGeneralInput { UserName = "newuser" }
        };

        Assert.ThrowsAsync<NullReferenceException>(async () =>
            await service.CreateUserAsync(input, CancellationToken.None));
    }

    [Test]
    public void UpdateUserAsync_WhenDependenciesUnavailable_Throws()
    {
        var service = CreateServiceWithoutConstructor();
        var input = new UserInput
        {
            General = new UserGeneralInput { UserName = "updateduser" }
        };

        Assert.ThrowsAsync<NullReferenceException>(async () =>
            await service.UpdateUserAsync(Guid.NewGuid(), input, CancellationToken.None));
    }

    [Test]
    public void DeleteUserAsync_WhenDependenciesUnavailable_Throws()
    {
        var service = CreateServiceWithoutConstructor();

        Assert.ThrowsAsync<NullReferenceException>(async () =>
            await service.DeleteUserAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [TestCase("jdoe")]
    [TestCase("admin")]
    [TestCase("")]
    public void GetUsersAsync_WithVariousUserNames_WhenDependenciesUnavailable_Throws(string userName)
    {
        var service = CreateServiceWithoutConstructor();

        Assert.ThrowsAsync<NullReferenceException>(async () =>
            await service.GetUsersAsync(userName, CancellationToken.None));
    }

    [Test]
    public void UserOutput_DefaultArrays_AreEmpty()
    {
        var output = new UserOutput
        {
            General = new UserGeneralOutput { UserName = "testuser" }
        };

        Assert.Multiple(() =>
        {
            Assert.That(output.Channels, Is.Empty);
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
        });
    }

    [Test]
    public void ChannelSummaryOutput_StoresValues()
    {
        var guid = Guid.NewGuid();
        var summary = new ChannelSummaryOutput { GUID = guid, DisplayName = "Main Channel" };

        Assert.Multiple(() =>
        {
            Assert.That(summary.GUID, Is.EqualTo(guid));
            Assert.That(summary.DisplayName, Is.EqualTo("Main Channel"));
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
            Channels = [new ChannelSummaryOutput { GUID = Guid.NewGuid(), DisplayName = "Channel A" }],
            Roles = [new RoleSummaryOutput { GUID = Guid.NewGuid(), DisplayName = "Editor" }]
        };

        Assert.Multiple(() =>
        {
            Assert.That(output.Channels, Has.Length.EqualTo(1));
            Assert.That(output.Roles, Has.Length.EqualTo(1));
        });
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
}
