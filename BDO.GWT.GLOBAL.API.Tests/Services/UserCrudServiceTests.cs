using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Services;
using System.Runtime.CompilerServices;

namespace BDO.GWT.GLOBAL.API.Tests.Services;

[TestFixture]
public class UserCrudServiceTests
{
    [Test]
    public void UserCrudService_ImplementsIUserCrudService()
    {
        var service = (UserCrudService)RuntimeHelpers.GetUninitializedObject(typeof(UserCrudService));

        Assert.That(service, Is.InstanceOf<IUserCrudService>());
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
