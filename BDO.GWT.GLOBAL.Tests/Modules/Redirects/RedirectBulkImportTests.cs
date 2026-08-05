using System.Reflection;
using System.Runtime.CompilerServices;
using CMS.ContentEngine;
using CMS.Core;
using CMS.DataEngine;
using CMS.Websites;
using Kentico.Xperience.Admin.Base;
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
}
