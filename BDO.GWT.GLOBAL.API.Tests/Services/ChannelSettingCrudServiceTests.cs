using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Queries;
using BDO.GWT.GLOBAL.API.Services;
using ChannelSettingInput = BDO.GWT.Web.Core.API.Data.ChannelSettingInput;
using CMS.DataEngine;
using Moq;
using System.Data;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;

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
    public async Task GetChannelSettings_ReturnsEmpty_WhenNoSnippetFound()
    {
        var service = new ChannelSettingCrudService(BuildProvider([]).Object);

        var result = await service.GetChannelSettings(100, new SettingsQuery(), CancellationToken.None);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetChannelSetting_ReturnsNull_WhenCodeNameNotFound()
    {
        var service = new ChannelSettingCrudService(BuildProvider([]).Object);

        var result = await service.GetChannelSetting(100, "BDO_DoesNotExist", CancellationToken.None);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task UpdateChannelSetting_ReturnsNull_WhenSnippetNotFound()
    {
        var providerMock = BuildProvider([]);
        var service = new ChannelSettingCrudService(providerMock.Object);

        var result = await service.UpdateChannelSetting(
            100,
            "BDO_ScriptHeadKeyValue",
            new ChannelSettingInput { InheritFromGlobalSettings = true, Value = "updated" },
            CancellationToken.None);

        Assert.That(result, Is.Null);
        providerMock.Verify(x => x.SetAsync(It.IsAny<WebChannelSnippetInfo>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void ResolveSettingType_CoversAllBranches()
    {
        Assert.Multiple(() =>
        {
            Assert.That(InvokeResolveSettingType(null), Is.EqualTo("string"));
            Assert.That(InvokeResolveSettingType(DBNull.Value), Is.EqualTo("string"));
            Assert.That(InvokeResolveSettingType("x"), Is.EqualTo("string"));
            Assert.That(InvokeResolveSettingType(true), Is.EqualTo("boolean"));
            Assert.That(InvokeResolveSettingType(1), Is.EqualTo("integer"));
            Assert.That(InvokeResolveSettingType(1.2m), Is.EqualTo("number"));
            Assert.That(InvokeResolveSettingType(DateTime.UtcNow), Is.EqualTo("datetime"));
            Assert.That(InvokeResolveSettingType(Guid.NewGuid()), Is.EqualTo("guid"));
            Assert.That(InvokeResolveSettingType(new Version(1, 0)), Is.EqualTo("Version"));
        });
    }

    [Test]
    public void ConvertJsonElement_CoversFallbackBranches()
    {
        Assert.Multiple(() =>
        {
            Assert.That(InvokeConvertJsonElement(ParseElement("null"), "x"), Is.Null);
            Assert.That(InvokeConvertJsonElement(ParseElement("true"), null), Is.EqualTo(true));
            Assert.That(InvokeConvertJsonElement(ParseElement("\"txt\""), null), Is.EqualTo("txt"));
            Assert.That(InvokeConvertJsonElement(ParseElement("12"), null), Is.EqualTo(12L));
            Assert.That(InvokeConvertJsonElement(ParseElement("12.5"), null), Is.EqualTo(12.5m));
            Assert.That(InvokeConvertJsonElement(ParseElement("[1,2]"), null), Is.EqualTo("[1,2]"));
            Assert.That(InvokeConvertJsonElement(ParseElement("{\"a\":1}"), null), Is.EqualTo("{\"a\":1}"));
        });
    }

    [Test]
    public void ConvertJsonElement_CoversCurrentTypeConversions()
    {
        var guid = Guid.NewGuid();

        Assert.Multiple(() =>
        {
            Assert.That(InvokeConvertJsonElement(ParseElement("\"true\""), false), Is.EqualTo(true));
            Assert.That(InvokeConvertJsonElement(ParseElement("\"abc\""), false), Is.EqualTo(false));
            Assert.That(InvokeConvertJsonElement(ParseElement("\"5\""), 1), Is.EqualTo(5));
            Assert.That(InvokeConvertJsonElement(ParseElement("\"abc\""), 1), Is.EqualTo(1));
            Assert.That(InvokeConvertJsonElement(ParseElement("\"6\""), 1L), Is.EqualTo(6L));
            Assert.That(InvokeConvertJsonElement(ParseElement("\"abc\""), 1L), Is.EqualTo(1L));
            Assert.That(InvokeConvertJsonElement(ParseElement("\"7.5\""), 1.0m), Is.EqualTo(7.5m));
            Assert.That(InvokeConvertJsonElement(ParseElement("\"abc\""), 1.0m), Is.EqualTo(1.0m));
            Assert.That(InvokeConvertJsonElement(ParseElement($"\"{guid}\""), Guid.Empty), Is.EqualTo(guid));
            Assert.That(InvokeConvertJsonElement(ParseElement("\"bad-guid\""), guid), Is.EqualTo(guid));
        });
    }

    [Test]
    public void RemoveInheritSuffix_ReturnsOriginal_WhenNoSuffixFound()
    {
        var service = new ChannelSettingCrudService(BuildProvider([]).Object);
        var result = InvokeRemoveInheritSuffix(service, "ScriptHead");

        Assert.That(result, Is.EqualTo("ScriptHead"));
    }

    [Test]
    public void RemoveInheritSuffix_StripsMatchingSuffix()
    {
        var service = new ChannelSettingCrudService(BuildProvider([]).Object);
        var result = InvokeRemoveInheritSuffix(service, "ScriptHeadInheritfromglobalsettings");

        Assert.That(result, Is.EqualTo("ScriptHead"));
    }

    [Test]
    public void NormalizeColumnNameForInheritMatch_StripsKeyValue()
    {
        Assert.That(InvokeNormalizeColumnNameForInheritMatch("ScriptHeadKeyValue"), Is.EqualTo("ScriptHead"));
        Assert.That(InvokeNormalizeColumnNameForInheritMatch("DisplayName"), Is.EqualTo("DisplayName"));
    }

    [Test]
    public void GetMethods_WhenDependenciesUnavailable_Throw()
    {
        var service = (ChannelSettingCrudService)RuntimeHelpers.GetUninitializedObject(typeof(ChannelSettingCrudService));

        Assert.Multiple(() =>
        {
            Assert.ThrowsAsync<NullReferenceException>(async () => await service.GetChannelSettings(1, new SettingsQuery(), CancellationToken.None));
            Assert.ThrowsAsync<NullReferenceException>(async () => await service.GetChannelSetting(1, "BDO_ScriptHeadKeyValue", CancellationToken.None));
            Assert.ThrowsAsync<NullReferenceException>(async () => await service.UpdateChannelSetting(1, "BDO_ScriptHeadKeyValue", new ChannelSettingInput { InheritFromGlobalSettings = false, Value = "x" }, CancellationToken.None));
        });
    }

    private static Mock<IInfoProvider<WebChannelSnippetInfo>> BuildProvider(IEnumerable<WebChannelSnippetInfo> rows)
    {
        var query = new Mock<ObjectQuery<WebChannelSnippetInfo>>();
        query.Setup(x => x.WhereEquals(It.IsAny<string>(), It.IsAny<object>())).Returns(query.Object);
        query.Setup(x => x.GetEnumerableTypedResult(It.IsAny<CommandBehavior>(), It.IsAny<bool>())).Returns(rows);
        query.Setup(x => x.GetEnumerableTypedResultAsync(It.IsAny<CommandBehavior>(), It.IsAny<bool>(), It.IsAny<CancellationToken?>())).ReturnsAsync(rows);

        var providerMock = new Mock<IInfoProvider<WebChannelSnippetInfo>>();
        providerMock.Setup(x => x.Get()).Returns(query.Object);

        return providerMock;
    }

    private static string InvokeResolveSettingType(object? value)
    {
        var method = typeof(ChannelSettingCrudService).GetMethod("ResolveSettingType", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.That(method, Is.Not.Null);

        return (string)method!.Invoke(null, [value])!;
    }

    private static object? InvokeConvertJsonElement(JsonElement element, object? currentValue)
    {
        var method = typeof(ChannelSettingCrudService).GetMethod("ConvertJsonElement", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.That(method, Is.Not.Null);

        return method!.Invoke(null, [element, currentValue]);
    }

    private static string InvokeRemoveInheritSuffix(ChannelSettingCrudService service, string columnName)
    {
        var method = typeof(ChannelSettingCrudService).GetMethod("RemoveInheritSuffix", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(method, Is.Not.Null);

        return (string)method!.Invoke(service, [columnName])!;
    }

    private static string InvokeNormalizeColumnNameForInheritMatch(string columnName)
    {
        var method = typeof(ChannelSettingCrudService).GetMethod("NormalizeColumnNameForInheritMatch", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.That(method, Is.Not.Null);

        return (string)method!.Invoke(null, [columnName])!;
    }

    private static JsonElement ParseElement(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
