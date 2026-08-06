using System.Reflection;
using System.Runtime.CompilerServices;
using BDO.GWT.GLOBAL.Tests;
using CMS.ContentEngine;
using CMS.Core;
using CMS.DataEngine;
using CMS.Websites;
using Kentico.Xperience.Admin.Base;
using Moq;
using XperienceCommunity.Redirects.UIPages;

namespace BDO.GWT.GLOBAL.Tests.Modules.Redirects;

public class RedirectBulkImportTests
{
    [Test]
    public void Page_IsInternalClass()
    {
        Assert.That(typeof(RedirectBulkImport).IsNotPublic, Is.True);
    }

    [Test]
    public void Page_InheritsFromModelEditPageOfRedirectBulkImportModel()
    {
        var baseType = typeof(RedirectBulkImport).BaseType;

        Assert.Multiple(() =>
        {
            Assert.That(baseType?.Name, Is.EqualTo("ModelEditPage`1"));
            Assert.That(baseType?.GetGenericArguments().Single(), Is.EqualTo(typeof(RedirectBulkImportModel)));
        });
    }

    [Test]
    public void Page_HasSinglePublicConstructor()
    {
        Assert.That(typeof(RedirectBulkImport).GetConstructors(), Has.Length.EqualTo(1));
    }

    [Test]
    public void Constructor_HasExpectedParameterTypes()
    {
        var parameters = typeof(RedirectBulkImport).GetConstructors().Single().GetParameters();

        Assert.Multiple(() =>
        {
            Assert.That(parameters, Has.Length.EqualTo(7));
            Assert.That(parameters[0].ParameterType.Name, Is.EqualTo("IFormItemCollectionProvider"));
            Assert.That(parameters[1].ParameterType.Name, Is.EqualTo("IFormDataBinder"));
            Assert.That(parameters[2].ParameterType, Is.EqualTo(typeof(IInfoProvider<RedirectInfo>)));
            Assert.That(parameters[3].ParameterType, Is.EqualTo(typeof(IInfoProvider<WebsiteChannelInfo>)));
            Assert.That(parameters[4].ParameterType, Is.EqualTo(typeof(IContentQueryExecutor)));
            Assert.That(parameters[5].ParameterType, Is.EqualTo(typeof(IWebPageUrlRetriever)));
            Assert.That(parameters[6].ParameterType, Is.EqualTo(typeof(IEventLogService)));
        });
    }

    [Test]
    public void Page_HasUiEvaluatePermissionAttribute()
    {
        var customData = typeof(RedirectBulkImport).CustomAttributes
            .SingleOrDefault(a => a.AttributeType.Name == "UIEvaluatePermissionAttribute");

        Assert.That(customData, Is.Not.Null);
    }

    [Test]
    public void ModelGetter_WhenModelFieldNull_CreatesNewModel()
    {
        var page = MakePage();

        var model = GetModel(page);

        Assert.Multiple(() =>
        {
            Assert.That(model, Is.Not.Null);
            Assert.That(model.WebsiteChannelName, Is.EqualTo(string.Empty));
            Assert.That(model.Delimiter, Is.EqualTo("semicolon"));
        });
    }

    [Test]
    public void ModelGetter_WhenCalledTwice_ReturnsSameInstance()
    {
        var page = MakePage();

        var first = GetModel(page);
        var second = GetModel(page);

        Assert.That(second, Is.SameAs(first));
    }

    [Test]
    public void ModelGetter_WhenFieldPreset_ReturnsPresetModel()
    {
        var page = MakePage();
        var preset = new RedirectBulkImportModel { WebsiteChannelName = "7" };

        SetModelField(page, preset);

        Assert.That(GetModel(page), Is.SameAs(preset));
    }

    [Test]
    public void Page_DefinesExpectedPrivateFields()
    {
        var fields = typeof(RedirectBulkImport)
            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
            .Select(f => f.Name)
            .ToList();

        Assert.Multiple(() =>
        {
            Assert.That(fields, Does.Contain("_redirectInfoProvider"));
            Assert.That(fields, Does.Contain("_websiteChannelInfoProvider"));
            Assert.That(fields, Does.Contain("_executor"));
            Assert.That(fields, Does.Contain("_webPageUrlRetriever"));
            Assert.That(fields, Does.Contain("_eventLogService"));
            Assert.That(fields, Does.Contain("_model"));
        });
    }

    [Test]
    public void NormalizeUrl_WhenInputIsNullOrWhiteSpace_ReturnsEmpty()
    {
        Assert.Multiple(() =>
        {
            Assert.That(NormalizeUrl(null!), Is.EqualTo(string.Empty));
            Assert.That(NormalizeUrl("   "), Is.EqualTo(string.Empty));
        });
    }

    [Test]
    public void NormalizeUrl_AddsLeadingSlash_AndLowerCases()
    {
        Assert.That(NormalizeUrl("Section/Page"), Is.EqualTo("/section/page"));
    }

    [Test]
    public void NormalizeUrl_RemovesTilde_AndTrailingSlash()
    {
        Assert.That(NormalizeUrl("~/Section/Page/"), Is.EqualTo("/section/page"));
    }

    [Test]
    public void NormalizeUrl_LeavesRootSlashIntact()
    {
        Assert.That(NormalizeUrl("/"), Is.EqualTo("/"));
    }

    [Test]
    public void PrivateConstants_HaveExpectedValues()
    {
        var pageSize = (int)typeof(RedirectBulkImport)
            .GetField("PageSize", BindingFlags.Static | BindingFlags.NonPublic)!
            .GetValue(null)!;

        var maxRows = (int)typeof(RedirectBulkImport)
            .GetField("MaxRows", BindingFlags.Static | BindingFlags.NonPublic)!
            .GetValue(null)!;

        Assert.Multiple(() =>
        {
            Assert.That(pageSize, Is.EqualTo(500));
            Assert.That(maxRows, Is.EqualTo(200));
        });
    }

    [Test]
    public void ProcessFormData_MethodSignature_IsExpected()
    {
        var method = typeof(RedirectBulkImport)
            .GetMethod("ProcessFormData", BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(method, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(method!.GetParameters(), Has.Length.EqualTo(2));
            Assert.That(method.ReturnType, Is.EqualTo(typeof(Task<ICommandResponse>)));
        });
    }

    [Test]
    public void GetDefaultLanguageName_MethodExists_AndReturnsString()
    {
        var method = typeof(RedirectBulkImport)
            .GetMethod("GetDefaultLanguageName", BindingFlags.Static | BindingFlags.NonPublic);

        Assert.That(method, Is.Not.Null);
        Assert.That(method!.ReturnType, Is.EqualTo(typeof(string)));
    }

    [Test]
    public void GetDelimiter_CommaAndDefaultCases_AreHandled()
    {
        Assert.Multiple(() =>
        {
            Assert.That(RedirectBulkImport.GetDelimiter("comma"), Is.EqualTo(','));
            Assert.That(RedirectBulkImport.GetDelimiter("semicolon"), Is.EqualTo(';'));
            Assert.That(RedirectBulkImport.GetDelimiter("anything"), Is.EqualTo(';'));
        });
    }

    [Test]
    public void ParseCsvRows_RemovesEmptyLines_AndHeader()
    {
        var rows = RedirectBulkImport.ParseCsvRows("Source;Target\r\n  /a;/b  \r\n\r\n/c;/d", hasHeaderRow: true);

        Assert.That(rows, Is.EqualTo(new[] { "/a;/b", "/c;/d" }));
    }

    [Test]
    public void ParseCsvRows_WhenNoHeader_KeepsFirstRow()
    {
        var rows = RedirectBulkImport.ParseCsvRows("/a;/b\n/c;/d", hasHeaderRow: false);

        Assert.That(rows, Is.EqualTo(new[] { "/a;/b", "/c;/d" }));
    }

    [Test]
    public void BuildSummary_WhenAllCountersPresent_ContainsAllSegments()
    {
        var summary = RedirectBulkImport.BuildSummary(imported: 2, duplicates: 1, notFound: 3, malformed: 4);

        Assert.Multiple(() =>
        {
            Assert.That(summary, Does.Contain("Imported 2 redirect(s)."));
            Assert.That(summary, Does.Contain("Skipped 1 duplicate(s)."));
            Assert.That(summary, Does.Contain("3 row(s) had a target URL that could not be matched to a page."));
            Assert.That(summary, Does.Contain("4 row(s) were malformed."));
        });
    }

    [Test]
    public void BuildSummary_WhenOnlyImported_ContainsImportedSegmentOnly()
    {
        var summary = RedirectBulkImport.BuildSummary(imported: 1, duplicates: 0, notFound: 0, malformed: 0);

        Assert.That(summary, Is.EqualTo("Imported 1 redirect(s)."));
    }

    [Test]
    public void TryParseImportRow_WhenMalformed_ReturnsFalse()
    {
        var parsed = RedirectBulkImport.TryParseImportRow("single-column", ';', out var source, out var target);

        Assert.Multiple(() =>
        {
            Assert.That(parsed, Is.False);
            Assert.That(source, Is.EqualTo(string.Empty));
            Assert.That(target, Is.EqualTo(string.Empty));
        });
    }

    [Test]
    public void TryParseImportRow_WhenValid_NormalizesValues()
    {
        var parsed = RedirectBulkImport.TryParseImportRow("\"/My-Source\";\"~/Target/Page/\"", ';', out var source, out var target);

        Assert.Multiple(() =>
        {
            Assert.That(parsed, Is.True);
            Assert.That(source, Is.EqualTo("/my-source"));
            Assert.That(target, Is.EqualTo("/target/page"));
        });
    }

    [Test]
    public void TryParseImportRow_WhenSourceOrTargetEmpty_ReturnsFalse()
    {
        var parsed = RedirectBulkImport.TryParseImportRow(" ; ", ';', out var source, out var target);

        Assert.Multiple(() =>
        {
            Assert.That(parsed, Is.False);
            Assert.That(source, Is.EqualTo(string.Empty));
            Assert.That(target, Is.EqualTo(string.Empty));
        });
    }

    [Test]
    public async Task BuildChannelUrlLookupAsync_WhenExecutorReturnsNoPages_ReturnsEmptyLookup()
    {
        var page = MakePage();

        var executor = DispatchProxy.Create<IContentQueryExecutor, ContentQueryExecutorProxy>();
        ContentQueryExecutorProxy.GetProxy(executor).OnInvoke = method =>
        {
            var returnType = method.ReturnType;
            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var payloadType = returnType.GetGenericArguments()[0];
                if (payloadType.IsGenericType && payloadType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    var itemType = payloadType.GetGenericArguments()[0];
                    var emptySequence = typeof(Enumerable)
                        .GetMethod(nameof(Enumerable.Empty))!
                        .MakeGenericMethod(itemType)
                        .Invoke(null, null)!;

                    return typeof(Task)
                        .GetMethod(nameof(Task.FromResult))!
                        .MakeGenericMethod(payloadType)
                        .Invoke(null, [emptySequence])!;
                }
            }

            return GetDefaultReturnValue(returnType);
        };

        SetField(page, "_executor", executor);
        SetField(page, "_webPageUrlRetriever", Mock.Of<IWebPageUrlRetriever>());
        SetField(page, "_eventLogService", Mock.Of<IEventLogService>());

        var lookup = await InvokePrivateAsync<Dictionary<string, (Guid Guid, string Url)>>(
            page,
            "BuildChannelUrlLookupAsync",
            "global-channel",
            "en");

        Assert.That(lookup, Is.Empty);
    }

    [Test]
    public void GetDefaultLanguageName_WhenDefaultLanguageExists_ReturnsLanguageName()
    {
        var query = new Mock<ObjectQuery<ContentLanguageInfo>>();
        query.Setup(x => x.TopN(1)).Returns(query.Object);
        query.Setup(x => x.Column(nameof(ContentLanguageInfo.ContentLanguageName))).Returns(query.Object);
        query.Setup(x => x.WhereTrue(nameof(ContentLanguageInfo.ContentLanguageIsDefault))).Returns(query.Object);
        query.Setup(x => x.GetScalarResult<string>()).Returns("en");

        KenticoContainerSetup.ContentLanguageProvider
            .Setup(x => x.Get())
            .Returns(query.Object);

        var method = typeof(RedirectBulkImport)
            .GetMethod("GetDefaultLanguageName", BindingFlags.Static | BindingFlags.NonPublic)!;

        var result = (string)method.Invoke(null, null)!;

        Assert.That(result, Is.EqualTo("en"));
    }

    private static RedirectBulkImport MakePage() =>
        (RedirectBulkImport)RuntimeHelpers.GetUninitializedObject(typeof(RedirectBulkImport));

    private static RedirectBulkImportModel GetModel(RedirectBulkImport page)
    {
        var property = typeof(RedirectBulkImport)
            .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single(p => p.Name == "Model" && p.DeclaringType == typeof(RedirectBulkImport));

        return (RedirectBulkImportModel)property.GetValue(page)!;
    }

    private static void SetModelField(RedirectBulkImport page, RedirectBulkImportModel model) =>
        typeof(RedirectBulkImport)
            .GetField("_model", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(page, model);

    private static string NormalizeUrl(string value)
    {
        var method = typeof(RedirectBulkImport)
            .GetMethod("NormalizeUrl", BindingFlags.Static | BindingFlags.NonPublic)!;

        return (string)method.Invoke(null, [value])!;
    }

    private static async Task<T> InvokePrivateAsync<T>(object target, string methodName, params object[] args)
    {
        var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)!;
        var task = (Task)method.Invoke(target, args)!;
        await task;

        return (T)task.GetType().GetProperty("Result")!.GetValue(task)!;
    }

    private static object? GetDefaultReturnValue(Type returnType)
    {
        if (returnType == typeof(void))
        {
            return null;
        }

        if (returnType == typeof(Task))
        {
            return Task.CompletedTask;
        }

        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var payloadType = returnType.GetGenericArguments()[0];
            object? payload = payloadType.IsValueType ? Activator.CreateInstance(payloadType) : null;
            return typeof(Task)
                .GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(payloadType)
                .Invoke(null, [payload]);
        }

        return returnType.IsValueType ? Activator.CreateInstance(returnType) : null;
    }

    private static void SetField(object target, string fieldName, object value) =>
        typeof(RedirectBulkImport)
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(target, value);

    private class ContentQueryExecutorProxy : DispatchProxy
    {
        public Func<MethodInfo, object?>? OnInvoke { get; set; }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod == null)
            {
                return null;
            }

            return OnInvoke?.Invoke(targetMethod) ?? GetDefaultReturnValue(targetMethod.ReturnType);
        }

        public static ContentQueryExecutorProxy GetProxy(IContentQueryExecutor proxy) =>
            (ContentQueryExecutorProxy)(object)proxy;
    }
}
