using System.Reflection;
using System.Runtime.CompilerServices;
using BDO.GWT.GLOBAL.Tests;
using CMS.ContentEngine;
using CMS.Core;
using CMS.DataEngine;
using CMS.Websites;
using CMS.Websites.Internal;
using Kentico.Xperience.Admin.Base;
using Moq;
using XperienceCommunity.Redirects.UIPages;

namespace BDO.GWT.GLOBAL.Tests.Modules.Redirects;

public class RedirectBulkImportTests
{
    private static readonly string[] ExpectedParsedRows = ["/a;/b", "/c;/d"];

    // -------------------------------------------------------------------------
    // Structural / reflection tests
    // -------------------------------------------------------------------------

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

    // -------------------------------------------------------------------------
    // Model property
    // -------------------------------------------------------------------------

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

    // -------------------------------------------------------------------------
    // GetDelimiter
    // -------------------------------------------------------------------------

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

    // -------------------------------------------------------------------------
    // NormalizeUrl
    // -------------------------------------------------------------------------

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
    public void NormalizeUrl_WhenAlreadyHasLeadingSlash_DoesNotDoubleSlash()
    {
        Assert.That(NormalizeUrl("/already/normalized"), Is.EqualTo("/already/normalized"));
    }

    // -------------------------------------------------------------------------
    // ParseCsvRows
    // -------------------------------------------------------------------------

    [Test]
    public void ParseCsvRows_RemovesEmptyLines_AndHeader()
    {
        var rows = RedirectBulkImport.ParseCsvRows("Source;Target\r\n  /a;/b  \r\n\r\n/c;/d", hasHeaderRow: true);

        Assert.That(rows, Is.EqualTo(ExpectedParsedRows));
    }

    [Test]
    public void ParseCsvRows_WhenNoHeader_KeepsFirstRow()
    {
        var rows = RedirectBulkImport.ParseCsvRows("/a;/b\n/c;/d", hasHeaderRow: false);

        Assert.That(rows, Is.EqualTo(ExpectedParsedRows));
    }

    [Test]
    public void ParseCsvRows_WhenInputIsNull_ReturnsEmpty()
    {
        var rows = RedirectBulkImport.ParseCsvRows(null!, hasHeaderRow: false);

        Assert.That(rows, Is.Empty);
    }

    [Test]
    public void ParseCsvRows_WhenInputIsEmpty_ReturnsEmpty()
    {
        var rows = RedirectBulkImport.ParseCsvRows(string.Empty, hasHeaderRow: true);

        Assert.That(rows, Is.Empty);
    }

    [Test]
    public void ParseCsvRows_WhenOnlyHeaderRow_AndHasHeaderRow_ReturnsEmpty()
    {
        var rows = RedirectBulkImport.ParseCsvRows("Source;Target", hasHeaderRow: true);

        Assert.That(rows, Is.Empty);
    }

    // -------------------------------------------------------------------------
    // BuildSummary
    // -------------------------------------------------------------------------

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
    public void BuildSummary_WhenDuplicatesOnly_ContainsDuplicateSegment()
    {
        var summary = RedirectBulkImport.BuildSummary(imported: 0, duplicates: 2, notFound: 0, malformed: 0);

        Assert.Multiple(() =>
        {
            Assert.That(summary, Does.Contain("Imported 0 redirect(s)."));
            Assert.That(summary, Does.Contain("Skipped 2 duplicate(s)."));
            Assert.That(summary, Does.Not.Contain("had a target URL"));
            Assert.That(summary, Does.Not.Contain("malformed"));
        });
    }

    [Test]
    public void BuildSummary_WhenNotFoundOnly_ContainsNotFoundSegment()
    {
        var summary = RedirectBulkImport.BuildSummary(imported: 0, duplicates: 0, notFound: 5, malformed: 0);

        Assert.Multiple(() =>
        {
            Assert.That(summary, Does.Contain("5 row(s) had a target URL that could not be matched to a page."));
            Assert.That(summary, Does.Not.Contain("duplicate"));
            Assert.That(summary, Does.Not.Contain("malformed"));
        });
    }

    [Test]
    public void BuildSummary_WhenMalformedOnly_ContainsMalformedSegment()
    {
        var summary = RedirectBulkImport.BuildSummary(imported: 0, duplicates: 0, notFound: 0, malformed: 3);

        Assert.Multiple(() =>
        {
            Assert.That(summary, Does.Contain("3 row(s) were malformed."));
            Assert.That(summary, Does.Not.Contain("duplicate"));
            Assert.That(summary, Does.Not.Contain("had a target URL"));
        });
    }

    // -------------------------------------------------------------------------
    // TryParseImportRow
    // -------------------------------------------------------------------------

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
    public void TryParseImportRow_WithCommaDelimiter_SplitsCorrectly()
    {
        var parsed = RedirectBulkImport.TryParseImportRow("/old-page,/new-page", ',', out var source, out var target);

        Assert.Multiple(() =>
        {
            Assert.That(parsed, Is.True);
            Assert.That(source, Is.EqualTo("/old-page"));
            Assert.That(target, Is.EqualTo("/new-page"));
        });
    }

    // -------------------------------------------------------------------------
    // GetRowsValidationError
    // -------------------------------------------------------------------------

    [Test]
    public void GetRowsValidationError_WhenZeroRows_ReturnsNoRowsError()
    {
        var error = InvokeGetRowsValidationError(0);

        Assert.That(error, Is.EqualTo("No CSV rows were found to import."));
    }

    [Test]
    public void GetRowsValidationError_WhenRowCountExceedsMax_ReturnsExceedsLimitError()
    {
        var error = InvokeGetRowsValidationError(201);

        Assert.That(error, Does.Contain("201").And.Contain("200-row limit"));
    }

    [Test]
    public void GetRowsValidationError_WhenRowCountIsExactlyMax_ReturnsNull()
    {
        var error = InvokeGetRowsValidationError(200);

        Assert.That(error, Is.Null);
    }

    [Test]
    public void GetRowsValidationError_WhenRowCountIsValid_ReturnsNull()
    {
        var error = InvokeGetRowsValidationError(10);

        Assert.That(error, Is.Null);
    }

    // -------------------------------------------------------------------------
    // TryResolveChannel
    // -------------------------------------------------------------------------

    [Test]
    public void TryResolveChannel_WhenChannelNameIsNonNumeric_ReturnsFalse_WithInvalidMessage()
    {
        var page = MakePage();
        SetField(page, "_websiteChannelInfoProvider", Mock.Of<IInfoProvider<WebsiteChannelInfo>>());

        bool result = InvokeTryResolveChannel(page, "not-a-number",
            out _, out _, out string error);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.False);
            Assert.That(error, Is.EqualTo("The selected site is invalid."));
        });
    }


    // -------------------------------------------------------------------------
    // GetDefaultLanguageName
    // -------------------------------------------------------------------------

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

    // -------------------------------------------------------------------------
    // TryAddPageUrlToLookupAsync
    // -------------------------------------------------------------------------

    [Test]
    public async Task TryAddPageUrlToLookupAsync_WhenUrlRetrieverThrows_LogsResolveUrlException_AndLookupRemainsEmpty()
    {
        var page = MakePage();
        var pageGuid = Guid.NewGuid();
        var lookup = new Dictionary<string, (Guid Guid, string Url)>(StringComparer.OrdinalIgnoreCase);

        var retriever = new Mock<IWebPageUrlRetriever>();
        retriever
            .Setup(x => x.Retrieve(42, "en"))
            .Throws(new InvalidOperationException("boom"));

        var eventLog = new Mock<IEventLogService>();

        SetField(page, "_webPageUrlRetriever", retriever.Object);
        SetField(page, "_eventLogService", eventLog.Object);

        await InvokePrivateTaskAsync(
            page,
            "TryAddPageUrlToLookupAsync",
            lookup,
            CreatePageUrlLookupRow(42, pageGuid),
            "global-channel",
            "en");

        Assert.Multiple(() =>
        {
            Assert.That(eventLog.Invocations, Is.Not.Empty);
            Assert.That(lookup, Is.Empty);
        });
    }

    [Test]
    public async Task TryAddPageUrlToLookupAsync_WhenRelativePathIsEmpty_DoesNotAddToLookup()
    {
        var page = MakePage();
        var pageGuid = Guid.NewGuid();
        var lookup = new Dictionary<string, (Guid Guid, string Url)>(StringComparer.OrdinalIgnoreCase);

        var urlResult = CreateWebPageUrlResult(string.Empty);
        var retriever = new Mock<IWebPageUrlRetriever>();
        retriever
            .Setup(x => x.Retrieve(10, "en"))
            .ReturnsAsync(urlResult);

        SetField(page, "_webPageUrlRetriever", retriever.Object);
        SetField(page, "_eventLogService", Mock.Of<IEventLogService>());

        await InvokePrivateTaskAsync(
            page,
            "TryAddPageUrlToLookupAsync",
            lookup,
            CreatePageUrlLookupRow(10, pageGuid),
            "my-channel",
            "en");

        Assert.That(lookup, Is.Empty);
    }

    [Test]
    public async Task TryAddPageUrlToLookupAsync_WhenSuccessful_AddsNormalizedUrlToLookup()
    {
        var page = MakePage();
        var pageGuid = Guid.NewGuid();
        var lookup = new Dictionary<string, (Guid Guid, string Url)>(StringComparer.OrdinalIgnoreCase);

        var urlResult = CreateWebPageUrlResult("~/Section/Page/");
        var retriever = new Mock<IWebPageUrlRetriever>();
        retriever
            .Setup(x => x.Retrieve(20, "en"))
            .ReturnsAsync(urlResult);

        SetField(page, "_webPageUrlRetriever", retriever.Object);
        SetField(page, "_eventLogService", Mock.Of<IEventLogService>());

        await InvokePrivateTaskAsync(
            page,
            "TryAddPageUrlToLookupAsync",
            lookup,
            CreatePageUrlLookupRow(20, pageGuid),
            "my-channel",
            "en");

        Assert.That(lookup, Contains.Key("/section/page"));
        Assert.That(lookup["/section/page"].Guid, Is.EqualTo(pageGuid));
    }

    [Test]
    public async Task TryAddPageUrlToLookupAsync_WhenUrlCollision_LogsWarning_AndFirstEntryWins()
    {
        var page = MakePage();
        var firstGuid = Guid.NewGuid();
        var secondGuid = Guid.NewGuid();

        var lookup = new Dictionary<string, (Guid Guid, string Url)>(StringComparer.OrdinalIgnoreCase)
        {
            ["/collision"] = (firstGuid, "/collision")
        };

        var urlResult = CreateWebPageUrlResult("/collision");
        var retriever = new Mock<IWebPageUrlRetriever>();
        retriever
            .Setup(x => x.Retrieve(30, "en"))
            .ReturnsAsync(urlResult);

        var eventLog = new Mock<IEventLogService>();

        SetField(page, "_webPageUrlRetriever", retriever.Object);
        SetField(page, "_eventLogService", eventLog.Object);

        await InvokePrivateTaskAsync(
            page,
            "TryAddPageUrlToLookupAsync",
            lookup,
            CreatePageUrlLookupRow(30, secondGuid),
            "my-channel",
            "en");

        // The original first entry must not be overwritten.
        Assert.Multiple(() =>
        {
            Assert.That(lookup["/collision"].Guid, Is.EqualTo(firstGuid));
            Assert.That(eventLog.Invocations, Is.Not.Empty);
        });
    }

    // -------------------------------------------------------------------------
    // BuildChannelUrlLookupAsync
    // -------------------------------------------------------------------------

    [Test]
    public async Task BuildChannelUrlLookupAsync_WhenExecutorReturnsNoPages_ReturnsEmptyLookup()
    {
        var page = MakePage();

        var executor = DispatchProxy.Create<IContentQueryExecutor, ContentQueryExecutorProxy>();
        ContentQueryExecutorProxy.GetProxy(executor).OnInvoke = method =>
            BuildEmptyPageSequenceResult(method.ReturnType);

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
    public async Task BuildChannelUrlLookupAsync_WhenExecutorReturnsPages_BuildsLookupFromUrls()
    {
        var page = MakePage();
        var pageGuid = Guid.NewGuid();
        const int pageId = 55;

        // Build a typed list containing one PageUrlLookupRow so the executor proxy can return it.
        var pageRowType = typeof(RedirectBulkImport).GetNestedType("PageUrlLookupRow", BindingFlags.NonPublic)!;
        var pageRow = Activator.CreateInstance(pageRowType, [pageId, pageGuid])!;
        var singleItemList = (System.Collections.IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(pageRowType))!;
        singleItemList.Add(pageRow);

        bool firstCall = true;
        var executor = DispatchProxy.Create<IContentQueryExecutor, ContentQueryExecutorProxy>();
        ContentQueryExecutorProxy.GetProxy(executor).OnInvoke = method =>
        {
            var returnType = method.ReturnType;
            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var payloadType = returnType.GetGenericArguments()[0];
                if (payloadType.IsGenericType && payloadType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    // First call returns one page so the loop body executes; subsequent calls return empty.
                    object sequence = firstCall
                        ? singleItemList
                        : (object)typeof(Enumerable)
                            .GetMethod(nameof(Enumerable.Empty))!
                            .MakeGenericMethod(payloadType.GetGenericArguments()[0])
                            .Invoke(null, null)!;

                    firstCall = false;

                    return typeof(Task)
                        .GetMethod(nameof(Task.FromResult))!
                        .MakeGenericMethod(payloadType)
                        .Invoke(null, [sequence])!;
                }
            }

            return GetDefaultReturnValue(returnType);
        };

        var urlResult = CreateWebPageUrlResult("/known-page/");
        var retriever = new Mock<IWebPageUrlRetriever>();
        retriever.Setup(x => x.Retrieve(pageId, "en")).ReturnsAsync(urlResult);

        SetField(page, "_executor", executor);
        SetField(page, "_webPageUrlRetriever", retriever.Object);
        SetField(page, "_eventLogService", Mock.Of<IEventLogService>());

        var lookup = await InvokePrivateAsync<Dictionary<string, (Guid Guid, string Url)>>(
            page,
            "BuildChannelUrlLookupAsync",
            "global-channel",
            "en");

        Assert.That(lookup, Contains.Key("/known-page"));
        Assert.That(lookup["/known-page"].Guid, Is.EqualTo(pageGuid));
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

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

    private static string? InvokeGetRowsValidationError(int rowCount)
    {
        var method = typeof(RedirectBulkImport)
            .GetMethod("GetRowsValidationError", BindingFlags.Static | BindingFlags.NonPublic)!;

        return (string?)method.Invoke(null, [rowCount]);
    }

    private static bool InvokeTryResolveChannel(
        RedirectBulkImport page,
        string channelName,
        out WebsiteChannelInfo? websiteChannel,
        out ChannelInfo? channel,
        out string errorMessage)
    {
        var method = typeof(RedirectBulkImport)
            .GetMethod("TryResolveChannel", BindingFlags.Instance | BindingFlags.NonPublic)!;

        object?[] args = [channelName, null, null, string.Empty];
        bool result = (bool)method.Invoke(page, args)!;

        websiteChannel = (WebsiteChannelInfo?)args[1];
        channel = (ChannelInfo?)args[2];
        errorMessage = (string)args[3]!;

        return result;
    }

    private static (int Imported, int Duplicates, int NotFound, int Malformed) InvokeImportRows(
        RedirectBulkImport page,
        IEnumerable<string> rows,
        char delimiter,
        IEnumerable<string> existingSourceUrls,
        Dictionary<string, (Guid Guid, string Url)> urlLookup,
        WebsiteChannelInfo websiteChannel,
        ChannelInfo channel)
    {
        var method = typeof(RedirectBulkImport)
            .GetMethod("ImportRows", BindingFlags.Instance | BindingFlags.NonPublic)!;

        var existingSet = new HashSet<string>(existingSourceUrls, StringComparer.OrdinalIgnoreCase);
        var result = method.Invoke(page, [rows, delimiter, existingSet, urlLookup, websiteChannel, channel])!;

        var resultType = result.GetType();
        return (
            (int)resultType.GetField("Item1")!.GetValue(result)!,
            (int)resultType.GetField("Item2")!.GetValue(result)!,
            (int)resultType.GetField("Item3")!.GetValue(result)!,
            (int)resultType.GetField("Item4")!.GetValue(result)!
        );
    }

    private static WebsiteChannelInfo MakeWebsiteChannel(int channelId)
    {
        var info = (WebsiteChannelInfo)RuntimeHelpers.GetUninitializedObject(typeof(WebsiteChannelInfo));
        typeof(WebsiteChannelInfo)
            .GetProperty(nameof(WebsiteChannelInfo.WebsiteChannelChannelID))
            ?.SetValue(info, channelId);
        return info;
    }

    private static ChannelInfo MakeChannel(string displayName)
    {
        var info = (ChannelInfo)RuntimeHelpers.GetUninitializedObject(typeof(ChannelInfo));
        typeof(ChannelInfo)
            .GetProperty(nameof(ChannelInfo.ChannelDisplayName))
            ?.SetValue(info, displayName);
        return info;
    }

    private static WebPageUrl CreateWebPageUrlResult(string relativePath)
    {
        var result = (WebPageUrl)RuntimeHelpers.GetUninitializedObject(typeof(WebPageUrl));

        var prop = typeof(WebPageUrl).GetProperty(
            nameof(WebPageUrl.RelativePath),
            BindingFlags.Public | BindingFlags.Instance);

        if (prop?.CanWrite == true)
        {
            prop.SetValue(result, relativePath);
        }
        else
        {
            var field = typeof(WebPageUrl)
                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(f => f.Name.Contains("RelativePath", StringComparison.OrdinalIgnoreCase)
                                     || f.Name.Contains("relativePath", StringComparison.OrdinalIgnoreCase));

            field?.SetValue(result, relativePath);
        }

        return result;
    }

    private static object CreatePageUrlLookupRow(int id, Guid guid)
    {
        var type = typeof(RedirectBulkImport).GetNestedType("PageUrlLookupRow", BindingFlags.NonPublic)!;
        return Activator.CreateInstance(type, [id, guid])!;
    }

    private static async Task InvokePrivateTaskAsync(object target, string methodName, params object[] args)
    {
        var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)!;
        var task = (Task)method.Invoke(target, args)!;
        await task;
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
        if (returnType == typeof(void)) return null;
        if (returnType == typeof(Task)) return Task.CompletedTask;

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

    private static object? BuildEmptyPageSequenceResult(Type returnType)
    {
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
            if (targetMethod == null) return null;
            return OnInvoke?.Invoke(targetMethod) ?? GetDefaultReturnValue(targetMethod.ReturnType);
        }

        public static ContentQueryExecutorProxy GetProxy(IContentQueryExecutor proxy) =>
            (ContentQueryExecutorProxy)(object)proxy;
    }
}
