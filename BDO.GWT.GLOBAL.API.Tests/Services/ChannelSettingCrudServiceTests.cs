using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Queries;
using BDO.GWT.GLOBAL.API.Services;
using ChannelSettingInput = BDO.GWT.Web.Core.API.Data.ChannelSettingInput;
using CMS.DataEngine;
using Moq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BDO.GWT.GLOBAL.API.Tests.Services;

[TestFixture]
public class ChannelSettingCrudServiceTests
{
    [Test]
    public void ChannelSettingCrudService_ImplementsIChannelSettingCrudService()
    {
        var service = (ChannelSettingCrudService)RuntimeHelpers.GetUninitializedObject(typeof(ChannelSettingCrudService));

        Assert.That(service, Is.InstanceOf<IChannelSettingCrudService>());
    }

    [Test]
    public void ChannelSettingCrudService_Interface_HasExpectedMethods()
    {
        var interfaceType = typeof(IChannelSettingCrudService);

        Assert.Multiple(() =>
        {
            Assert.That(interfaceType.GetMethod("GetChannelSettings"), Is.Not.Null);
            Assert.That(interfaceType.GetMethod("GetChannelSetting"), Is.Not.Null);
            Assert.That(interfaceType.GetMethod("UpdateChannelSetting"), Is.Not.Null);
        });
    }

    [Test]
    public void ChannelSettingOutput_DefaultValues()
    {
        var output = new ChannelSettingOutput
        {
            CodeName = "BDO_ScriptHeadKeyValue",
            DisplayName = "ScriptHeadKeyValue",
            InheritFromGlobalSettings = false,
            Type = "string"
        };

        Assert.Multiple(() =>
        {
            Assert.That(output.CodeName, Is.EqualTo("BDO_ScriptHeadKeyValue"));
            Assert.That(output.DisplayName, Is.EqualTo("ScriptHeadKeyValue"));
            Assert.That(output.InheritFromGlobalSettings, Is.False);
            Assert.That(output.Type, Is.EqualTo("string"));
            Assert.That(output.Value, Is.Null);
        });
    }

    [Test]
    public void ChannelSettingInput_DefaultValues_AreInitialized()
    {
        var input = new ChannelSettingInput
        {
            InheritFromGlobalSettings = false
        };

        Assert.Multiple(() =>
        {
            Assert.That(input.InheritFromGlobalSettings, Is.False);
            Assert.That(input.Value, Is.Null);
        });
    }

    [Test]
    public void GetChannelSettings_WhenDependenciesUnavailable_Throws()
    {
        var service = (ChannelSettingCrudService)RuntimeHelpers.GetUninitializedObject(typeof(ChannelSettingCrudService));

        Assert.ThrowsAsync<NullReferenceException>(async () =>
            await service.GetChannelSettings(1, new SettingsQuery(), CancellationToken.None));
    }

    [Test]
    public void GetChannelSetting_WhenDependenciesUnavailable_Throws()
    {
        var service = (ChannelSettingCrudService)RuntimeHelpers.GetUninitializedObject(typeof(ChannelSettingCrudService));

        Assert.ThrowsAsync<NullReferenceException>(async () =>
            await service.GetChannelSetting(1, "BDO_ScriptHeadKeyValue", CancellationToken.None));
    }

    [Test]
    public void UpdateChannelSetting_WhenDependenciesUnavailable_Throws()
    {
        var service = (ChannelSettingCrudService)RuntimeHelpers.GetUninitializedObject(typeof(ChannelSettingCrudService));
        var request = new ChannelSettingInput
        {
            InheritFromGlobalSettings = true,
            Value = "value"
        };

        Assert.ThrowsAsync<NullReferenceException>(async () =>
            await service.UpdateChannelSetting(1, "BDO_ScriptHeadKeyValue", request, CancellationToken.None));
    }

    [Test]
    public void ResolveSettingType_ReturnsBoolean_ForBool()
    {
        var service = new ChannelSettingCrudService(new Mock<IInfoProvider<WebChannelSnippetInfo>>().Object);

        var result = InvokeResolveSettingType(service, true);

        Assert.That(result, Is.EqualTo("boolean"));
    }

    [Test]
    public void ResolveSettingType_ReturnsInteger_ForInt()
    {
        var service = new ChannelSettingCrudService(new Mock<IInfoProvider<WebChannelSnippetInfo>>().Object);

        var result = InvokeResolveSettingType(service, 5);

        Assert.That(result, Is.EqualTo("integer"));
    }

    [Test]
    public void ResolveSettingType_ReturnsString_ForNull()
    {
        var service = new ChannelSettingCrudService(new Mock<IInfoProvider<WebChannelSnippetInfo>>().Object);

        var result = InvokeResolveSettingType(service, null);

        Assert.That(result, Is.EqualTo("string"));
    }

    private static string InvokeResolveSettingType(ChannelSettingCrudService service, object? value)
    {
        var method = typeof(ChannelSettingCrudService).GetMethod("ResolveSettingType", BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(method, Is.Not.Null);

        return (string)method!.Invoke(service, [value])!;
    }
}
