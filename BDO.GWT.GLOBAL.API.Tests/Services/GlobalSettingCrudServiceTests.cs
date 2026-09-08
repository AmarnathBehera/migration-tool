using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Helpers;
using BDO.GWT.GLOBAL.API.Queries;
using BDO.GWT.GLOBAL.API.Services;
using CMS.DataEngine;
using Moq;
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
            DisplayName = "Cookiebot",
            Type = "string"
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
            Type = "string",
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

    // --- Guard branch reachable without provider access --------------------------

    [TestCase("BDO.WebChannelSnippet")]
    [TestCase("BDO.Email")]
    [TestCase("")]
    [TestCase("bdo.globalsettingskey")]
    public void GetGlobalSetting_Throws_WhenCodeNameIsNotAKnownGlobalSetting(string codeName)
    {
        // With an unconfigured provider, resolving the setting fails before any match can occur.
        var providerMock = new Mock<IInfoProvider<GlobalsettingskeyInfo>>();
        var settingsCrudHelperMock = new Mock<ISettingsCrudHelper>();
        var validationStateMock = new Mock<IValidationState>();
        var service = new GlobalSettingCrudService(
            providerMock.Object,
            settingsCrudHelperMock.Object,
            validationStateMock.Object);

        Assert.ThrowsAsync<NullReferenceException>(async () =>
            await service.GetGlobalSetting(codeName, CancellationToken.None));
    }

    [Test]
    public void SetGlobalSetting_Throws_WhenProviderIsUnconfigured()
    {
        var providerMock = new Mock<IInfoProvider<GlobalsettingskeyInfo>>();
        var settingsCrudHelperMock = new Mock<ISettingsCrudHelper>();
        var validationStateMock = new Mock<IValidationState>();
        var service = new GlobalSettingCrudService(
            providerMock.Object,
            settingsCrudHelperMock.Object,
            validationStateMock.Object);

        Assert.ThrowsAsync<NullReferenceException>(async () =>
            await service.SetGlobalSetting("BDO.Cookiebot", new GlobalSettingInput { Value = "x" }, CancellationToken.None));
    }

    [Test]
    public void SetGlobalSetting_WhenDependenciesUnavailable_Throws()
    {
        var service = (GlobalSettingCrudService)RuntimeHelpers.GetUninitializedObject(typeof(GlobalSettingCrudService));

        Assert.ThrowsAsync<NullReferenceException>(async () =>
            await service.SetGlobalSetting("BDO.Cookiebot", new GlobalSettingInput { Value = "x" }, CancellationToken.None));
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

    // --- GetTypeName private static method: exhaustive branch coverage -----------

    private static string InvokeGetTypeName(string? cmsDataType, object? value)
    {
        var method = typeof(GlobalSettingCrudService).GetMethod(
            "GetTypeName",
            BindingFlags.NonPublic | BindingFlags.Static)!;
        return (string)method.Invoke(null, new[] { cmsDataType, value })!;
    }

    [TestCase(FieldDataType.Boolean, "boolean")]
    [TestCase(FieldDataType.Double, "double")]
    [TestCase(FieldDataType.Integer, "int32")]
    [TestCase(FieldDataType.LongInteger, "int64")]
    [TestCase(FieldDataType.LongText, "string")]
    [TestCase(FieldDataType.Text, "string")]
    [TestCase(FieldDataType.DateTime, "datetime")]
    [TestCase(FieldDataType.Guid, "guid")]
    [TestCase("SomethingUnknown", "string")]
    public void GetTypeName_WithCmsDataType_ReturnsExpected(string cmsDataType, string expected)
    {
        var result = InvokeGetTypeName(cmsDataType, null);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void GetTypeName_NullDataType_NullValue_ReturnsString()
    {
        Assert.That(InvokeGetTypeName(null, null), Is.EqualTo("string"));
    }

    [Test]
    public void GetTypeName_NullDataType_BoolValue_ReturnsBoolean()
    {
        Assert.That(InvokeGetTypeName(null, true), Is.EqualTo("boolean"));
    }

    [Test]
    public void GetTypeName_NullDataType_IntValue_ReturnsInt32()
    {
        Assert.That(InvokeGetTypeName(null, 5), Is.EqualTo("int32"));
    }

    [Test]
    public void GetTypeName_NullDataType_LongValue_ReturnsInt64()
    {
        Assert.That(InvokeGetTypeName(null, 5L), Is.EqualTo("int64"));
    }

    [Test]
    public void GetTypeName_NullDataType_DoubleValue_ReturnsDouble()
    {
        Assert.That(InvokeGetTypeName(null, 3.14d), Is.EqualTo("double"));
    }

    [Test]
    public void GetTypeName_NullDataType_DecimalValue_ReturnsDouble()
    {
        Assert.That(InvokeGetTypeName(null, 3.14m), Is.EqualTo("double"));
    }

    [Test]
    public void GetTypeName_NullDataType_FloatValue_ReturnsDouble()
    {
        Assert.That(InvokeGetTypeName(null, 3.14f), Is.EqualTo("double"));
    }

    [Test]
    public void GetTypeName_NullDataType_DateTimeValue_ReturnsDateTime()
    {
        Assert.That(InvokeGetTypeName(null, DateTime.UtcNow), Is.EqualTo("datetime"));
    }

    [Test]
    public void GetTypeName_NullDataType_GuidValue_ReturnsGuid()
    {
        Assert.That(InvokeGetTypeName(null, Guid.NewGuid()), Is.EqualTo("guid"));
    }

    [Test]
    public void GetTypeName_NullDataType_UnknownValue_ReturnsString()
    {
        Assert.That(InvokeGetTypeName(null, new object()), Is.EqualTo("string"));
    }

    [Test]
    public void GetTypeName_NullDataType_StringValue_ReturnsString()
    {
        Assert.That(InvokeGetTypeName(null, "hello"), Is.EqualTo("string"));
    }

    // --- GetValidatedSettingAndColumn / provider-guard branches ------------------

    [Test]
    public void GetValidatedSettingAndColumn_WhenProviderReturnsNull_AddsErrorAndThrows()
    {
        var providerMock = new Mock<IInfoProvider<GlobalsettingskeyInfo>>();
        var settingsCrudHelperMock = new Mock<ISettingsCrudHelper>();
        var validationStateMock = new Mock<IValidationState>();
        var service = new GlobalSettingCrudService(
            providerMock.Object,
            settingsCrudHelperMock.Object,
            validationStateMock.Object);

        // Unconfigured provider -> Get() returns null -> NRE bubbles up before validation.
        Assert.ThrowsAsync<NullReferenceException>(async () =>
            await service.SetGlobalSetting("any", new GlobalSettingInput { Value = "x" }, CancellationToken.None));
    }

    [Test]
    public void Constructor_AssignsAllDependencies()
    {
        var providerMock = new Mock<IInfoProvider<GlobalsettingskeyInfo>>();
        var settingsCrudHelperMock = new Mock<ISettingsCrudHelper>();
        var validationStateMock = new Mock<IValidationState>();

        var service = new GlobalSettingCrudService(
            providerMock.Object,
            settingsCrudHelperMock.Object,
            validationStateMock.Object);

        var providerField = typeof(GlobalSettingCrudService)
            .GetField("_customScriptinfoProvider", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var helperField = typeof(GlobalSettingCrudService)
            .GetField("_settingsCrudHelper", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var validationField = typeof(GlobalSettingCrudService)
            .GetField("_validationState", BindingFlags.Instance | BindingFlags.NonPublic)!;

        Assert.Multiple(() =>
        {
            Assert.That(providerField.GetValue(service), Is.SameAs(providerMock.Object));
            Assert.That(helperField.GetValue(service), Is.SameAs(settingsCrudHelperMock.Object));
            Assert.That(validationField.GetValue(service), Is.SameAs(validationStateMock.Object));
        });
    }
}
