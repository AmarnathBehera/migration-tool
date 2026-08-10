using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Queries;
using BDO.GWT.GLOBAL.API.Services;
using System.Runtime.CompilerServices;

namespace BDO.GWT.GLOBAL.API.Tests.Services;

[TestFixture]
public class GlobalSettingCrudServiceTests
{
    [Test]
    public void GlobalSettingCrudService_ImplementsIGlobalSettingCrudService()
    {
        var service = (GlobalSettingCrudService)RuntimeHelpers.GetUninitializedObject(typeof(GlobalSettingCrudService));

        Assert.That(service, Is.InstanceOf<IGlobalSettingCrudService>());
    }

    [Test]
    public void GlobalSettingOutput_DefaultValues()
    {
        var output = new GlobalSettingOutput
        {
            CodeName = "BDO.Cookiebot",
            DisplayName = "Cookiebot"
        };

        Assert.Multiple(() =>
        {
            Assert.That(output.CodeName, Is.EqualTo("BDO.Cookiebot"));
            Assert.That(output.DisplayName, Is.EqualTo("Cookiebot"));
            Assert.That(output.Value, Is.Null);
        });
    }

    [Test]
    public void GlobalSettingOutput_CanStoreValue()
    {
        var value = new Dictionary<string, object> { ["Key"] = "Val" };
        var output = new GlobalSettingOutput
        {
            CodeName = "BDO.Test",
            DisplayName = "Test Setting",
            Value = value
        };

        Assert.That(output.Value, Is.SameAs(value));
    }

    [Test]
    public void SettingsQuery_DefaultDisplayName_IsNull()
    {
        var query = new SettingsQuery();

        Assert.That(query.DisplayName, Is.Null);
    }

    [Test]
    public void SettingsQuery_CanSetDisplayName()
    {
        var query = new SettingsQuery { DisplayName = "Cookiebot" };

        Assert.That(query.DisplayName, Is.EqualTo("Cookiebot"));
    }

    [TestCase("Include Custom Scripts")]
    [TestCase("Cookiebot")]
    [TestCase("Events & Webinar")]
    public void SettingsQuery_AcceptsKnownDisplayNames(string displayName)
    {
        var query = new SettingsQuery { DisplayName = displayName };

        Assert.That(query.DisplayName, Is.EqualTo(displayName));
    }
}
