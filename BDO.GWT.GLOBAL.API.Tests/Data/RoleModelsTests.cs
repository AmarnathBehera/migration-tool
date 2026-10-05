using BDO.GWT.GLOBAL.API.Data;

namespace BDO.GWT.GLOBAL.API.Tests.Data;

[TestFixture]
public class RoleModelsTests
{
    [Test]
    public void RoleInput_Defaults_AreEmptyOrNull()
    {
        var input = new RoleInput();

        Assert.Multiple(() =>
        {
            Assert.That(input.RoleName, Is.EqualTo(string.Empty));
            Assert.That(input.DisplayName, Is.Null);
            Assert.That(input.Description, Is.Null);
        });
    }

    [Test]
    public void RoleInput_StoresValues()
    {
        var input = new RoleInput { RoleName = "Editors", DisplayName = "Editors Display", Description = "A role" };

        Assert.Multiple(() =>
        {
            Assert.That(input.RoleName, Is.EqualTo("Editors"));
            Assert.That(input.DisplayName, Is.EqualTo("Editors Display"));
            Assert.That(input.Description, Is.EqualTo("A role"));
        });
    }

    [Test]
    public void RoleChannelOutput_Defaults_AreEmpty()
    {
        var output = new RoleChannelOutput();

        Assert.Multiple(() =>
        {
            Assert.That(output.GUID, Is.EqualTo(Guid.Empty));
            Assert.That(output.ChannelName, Is.EqualTo(string.Empty));
            Assert.That(output.DisplayName, Is.EqualTo(string.Empty));
            Assert.That(output.Permissions, Is.Empty);
        });
    }

    [Test]
    public void RoleChannelOutput_StoresValues()
    {
        var guid = Guid.NewGuid();
        var output = new RoleChannelOutput
        {
            GUID = guid,
            ChannelName = "Ind",
            DisplayName = "India",
            Permissions = new[] { "View", "ManagePermissions" }
        };

        Assert.Multiple(() =>
        {
            Assert.That(output.GUID, Is.EqualTo(guid));
            Assert.That(output.ChannelName, Is.EqualTo("Ind"));
            Assert.That(output.DisplayName, Is.EqualTo("India"));
            Assert.That(output.Permissions, Is.EqualTo(new[] { "View", "ManagePermissions" }));
        });
    }

    [Test]
    public void RoleApplicationOutput_StoresValues()
    {
        var output = new RoleApplicationOutput
        {
            ApplicationName = "redirects",
            Permissions = new[] { "View", "Update" }
        };

        Assert.Multiple(() =>
        {
            Assert.That(output.ApplicationName, Is.EqualTo("redirects"));
            Assert.That(output.Permissions, Is.EqualTo(new[] { "View", "Update" }));
        });
    }

    [Test]
    public void RoleApplicationOutput_Defaults_AreEmpty()
    {
        var output = new RoleApplicationOutput();

        Assert.Multiple(() =>
        {
            Assert.That(output.ApplicationName, Is.EqualTo(string.Empty));
            Assert.That(output.Permissions, Is.Empty);
        });
    }

    [Test]
    public void RoleEmailChannelOutput_StoresValues()
    {
        var guid = Guid.NewGuid();
        var output = new RoleEmailChannelOutput
        {
            GUID = guid,
            ChannelName = "Email",
            DisplayName = "Email Channel",
            Permissions = new[] { "View" }
        };

        Assert.Multiple(() =>
        {
            Assert.That(output.GUID, Is.EqualTo(guid));
            Assert.That(output.ChannelName, Is.EqualTo("Email"));
            Assert.That(output.DisplayName, Is.EqualTo("Email Channel"));
            Assert.That(output.Permissions, Is.EqualTo(new[] { "View" }));
        });
    }

    [Test]
    public void RoleWorkspaceOutput_StoresValues()
    {
        var guid = Guid.NewGuid();
        var output = new RoleWorkspaceOutput
        {
            GUID = guid,
            WorkspaceName = "KenticoDefault",
            DisplayName = "Default",
            Permissions = new[] { "View", "Create" }
        };

        Assert.Multiple(() =>
        {
            Assert.That(output.GUID, Is.EqualTo(guid));
            Assert.That(output.WorkspaceName, Is.EqualTo("KenticoDefault"));
            Assert.That(output.DisplayName, Is.EqualTo("Default"));
            Assert.That(output.Permissions, Is.EqualTo(new[] { "View", "Create" }));
        });
    }

    [Test]
    public void RoleOutput_Defaults_AreEmptyCollections()
    {
        var output = new RoleOutput();

        Assert.Multiple(() =>
        {
            Assert.That(output.GUID, Is.EqualTo(Guid.Empty));
            Assert.That(output.RoleName, Is.EqualTo(string.Empty));
            Assert.That(output.DisplayName, Is.EqualTo(string.Empty));
            Assert.That(output.Description, Is.Null);
            Assert.That(output.WebsiteChannels, Is.Empty);
            Assert.That(output.ContentHubWorkspaces, Is.Empty);
            Assert.That(output.EmailChannels, Is.Empty);
            Assert.That(output.ApplicationPermissions, Is.Empty);
        });
    }

    [Test]
    public void RoleOutput_StoresNestedCollections()
    {
        var output = new RoleOutput
        {
            GUID = Guid.NewGuid(),
            RoleName = "Editors",
            DisplayName = "Editors",
            Description = "desc",
            WebsiteChannels = new[] { new RoleChannelOutput { ChannelName = "Ind" } },
            ContentHubWorkspaces = new[] { new RoleWorkspaceOutput { WorkspaceName = "KenticoDefault" } },
            EmailChannels = new[] { new RoleEmailChannelOutput { ChannelName = "Email" } },
            ApplicationPermissions = new[] { new RoleApplicationOutput { ApplicationName = "redirects" } }
        };

        Assert.Multiple(() =>
        {
            Assert.That(output.WebsiteChannels, Has.Exactly(1).Items);
            Assert.That(output.ContentHubWorkspaces, Has.Exactly(1).Items);
            Assert.That(output.EmailChannels, Has.Exactly(1).Items);
            Assert.That(output.ApplicationPermissions, Has.Exactly(1).Items);
        });
    }

    [Test]
    public void ChannelPermissionInput_StoresValues()
    {
        var input = new ChannelPermissionInput { WebsiteChannel = "Ind", Permissions = new[] { "View" } };

        Assert.Multiple(() =>
        {
            Assert.That(input.WebsiteChannel, Is.EqualTo("Ind"));
            Assert.That(input.Permissions, Is.EqualTo(new[] { "View" }));
        });
    }

    [Test]
    public void ApplicationPermissionInput_Defaults_AreNull()
    {
        var input = new ApplicationPermissionInput();

        Assert.Multiple(() =>
        {
            Assert.That(input.ApplicationName, Is.Null);
            Assert.That(input.Permissions, Is.Null);
        });
    }

    [Test]
    public void EmailChannelPermissionInput_StoresValues()
    {
        var input = new EmailChannelPermissionInput { EmailChannel = "Email", Permissions = new[] { "View", "Update" } };

        Assert.Multiple(() =>
        {
            Assert.That(input.EmailChannel, Is.EqualTo("Email"));
            Assert.That(input.Permissions, Is.EqualTo(new[] { "View", "Update" }));
        });
    }

    [Test]
    public void WorkspacePermissionInput_StoresValues()
    {
        var input = new WorkspacePermissionInput { Workspace = "KenticoDefault", Permissions = new[] { "View" } };

        Assert.Multiple(() =>
        {
            Assert.That(input.Workspace, Is.EqualTo("KenticoDefault"));
            Assert.That(input.Permissions, Is.EqualTo(new[] { "View" }));
        });
    }

    [Test]
    public void AssignRolePermissionsInput_Defaults_AreNull()
    {
        var input = new AssignRolePermissionsInput();

        Assert.Multiple(() =>
        {
            Assert.That(input.RoleName, Is.Null);
            Assert.That(input.WebsiteChannels, Is.Null);
            Assert.That(input.Workspaces, Is.Null);
            Assert.That(input.EmailChannels, Is.Null);
            Assert.That(input.Applications, Is.Null);
        });
    }

    [Test]
    public void CreateChannelRoleInput_StoresValues()
    {
        var input = new CreateChannelRoleInput { WebsiteChannel = "Ind", RoleType = RolesAllowedDropdown.Administrator };

        Assert.Multiple(() =>
        {
            Assert.That(input.WebsiteChannel, Is.EqualTo("Ind"));
            Assert.That(input.RoleType, Is.EqualTo(RolesAllowedDropdown.Administrator));
        });
    }

    [Test]
    public void RolesAllowedDropdown_HasEditorAndAdministrator()
    {
        Assert.That(Enum.GetNames(typeof(RolesAllowedDropdown)),
            Is.EquivalentTo(new[] { nameof(RolesAllowedDropdown.Editor), nameof(RolesAllowedDropdown.Administrator) }));
    }
}
