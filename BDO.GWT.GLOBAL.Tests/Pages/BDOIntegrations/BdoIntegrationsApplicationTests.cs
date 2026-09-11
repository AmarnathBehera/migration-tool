using BDO.GWT.Xperience;
using CMS.Membership;
using Kentico.Xperience.Admin.Base;

namespace BDO.GWT.GLOBAL.Tests.Pages.BDOIntegrations;

[TestFixture]
public class BdoIntegrationsApplicationTests
{
    [Test]
    public void InheritsFromApplicationPage()
    {
        Assert.That(typeof(BdoIntegrationsApplication).BaseType, Is.EqualTo(typeof(ApplicationPage)));
    }

    [Test]
    public void Identifier_IsExpectedValue()
    {
        Assert.That(BdoIntegrationsApplication.IDENTIFIER, Is.EqualTo("BDO.BDOIntegrations"));
    }

    [Test]
    public void HasExpectedUiPermissions()
    {
        var permissions = typeof(BdoIntegrationsApplication)
            .GetCustomAttributesData()
            .Where(a => a.AttributeType.Name == "UIPermissionAttribute")
            .Select(a => a.ConstructorArguments.FirstOrDefault().Value?.ToString())
            .ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(permissions, Does.Contain(SystemPermissions.VIEW));
            Assert.That(permissions, Does.Contain(SystemPermissions.CREATE));
            Assert.That(permissions, Does.Contain(SystemPermissions.UPDATE));
            Assert.That(permissions, Does.Contain(SystemPermissions.DELETE));
        });
    }
}
