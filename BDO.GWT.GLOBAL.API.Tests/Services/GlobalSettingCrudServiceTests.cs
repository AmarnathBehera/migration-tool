using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Queries;
using BDO.GWT.GLOBAL.API.Services;
using System.Reflection;
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

    [Test]
    public void GetGlobalSettings_WhenDependenciesUnavailable_Throws()
    {
        var service = (GlobalSettingCrudService)RuntimeHelpers.GetUninitializedObject(typeof(GlobalSettingCrudService));
        var query = new SettingsQuery();

        Assert.ThrowsAsync<NullReferenceException>(async () =>
            await service.GetGlobalSettings(query, CancellationToken.None));
    }

    [Test]
    public void GetGlobalSettings_WithDisplayNameFilter_WhenDependenciesUnavailable_Throws()
    {
        var service = (GlobalSettingCrudService)RuntimeHelpers.GetUninitializedObject(typeof(GlobalSettingCrudService));
        var query = new SettingsQuery { DisplayName = "Cookiebot" };

        Assert.ThrowsAsync<NullReferenceException>(async () =>
            await service.GetGlobalSettings(query, CancellationToken.None));
    }

    [Test]
    public void GetGlobalSetting_WhenDependenciesUnavailable_Throws()
    {
        var service = (GlobalSettingCrudService)RuntimeHelpers.GetUninitializedObject(typeof(GlobalSettingCrudService));

        Assert.ThrowsAsync<NullReferenceException>(async () =>
            await service.GetGlobalSetting("BDO.Cookiebot", CancellationToken.None));
    }

    [Test]
    public void GlobalSettingCrudService_Interface_HasExpectedMethods()
    {
        var interfaceType = typeof(IGlobalSettingCrudService);

        Assert.Multiple(() =>
        {
            Assert.That(interfaceType.GetMethod("GetGlobalSettings"), Is.Not.Null);
            Assert.That(interfaceType.GetMethod("GetGlobalSetting"), Is.Not.Null);
        });
    }

    [Test]
    public void GlobalSettingCrudService_ImplementsInterface()
    {
        Assert.That(typeof(IGlobalSettingCrudService).IsAssignableFrom(typeof(GlobalSettingCrudService)), Is.True);
    }

    [Test]
    public void GlobalSettingInput_DefaultValue_IsNull()
    {
        var input = new GlobalSettingInput();

        Assert.That(input.Value, Is.Null);
    }

    [Test]
    public void GlobalSettingInput_Value_CanBeSet()
    {
        var value = new Dictionary<string, object> { ["key"] = "val" };
        var input = new GlobalSettingInput { Value = value };

        Assert.That(input.Value, Is.SameAs(value));
    }

    [Test]
    public void GlobalSettingInput_Value_CanBeString()
    {
        var input = new GlobalSettingInput { Value = "test-value" };

        Assert.That(input.Value, Is.EqualTo("test-value"));
    }

    [Test]
    public void GlobalSettingInput_Value_CanBeInteger()
    {
        var input = new GlobalSettingInput { Value = 42 };

        Assert.That(input.Value, Is.EqualTo(42));
    }
}
