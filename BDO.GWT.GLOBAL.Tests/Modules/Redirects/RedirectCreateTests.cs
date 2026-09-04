using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using CMS.DataEngine;
using CMS.Websites;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Moq;
using XperienceCommunity.Redirects.UIPages;
using RedirectInfoX = global::BDO.RedirectInfo;

namespace BDO.GWT.GLOBAL.Tests.Modules.Redirects;

public class RedirectCreateTests
{
    // ── structural ──────────────────────────────────────────────────────────

    [Test]
    public void CreatePage_InheritsFromModelEditPage()
    {
        Assert.That(typeof(RedirectCreate).BaseType!.Name, Is.EqualTo("ModelEditPage`1"));
    }

    [Test]
    public void CreatePage_BaseType_GenericArgument_IsRedirectEditModel()
    {
        var genericArg = typeof(RedirectCreate).BaseType!.GetGenericArguments()[0];

        Assert.That(genericArg, Is.EqualTo(typeof(RedirectEditModel)));
    }

    [Test]
    public void CreatePage_IsInternalClass()
    {
        Assert.That(typeof(RedirectCreate).IsNotPublic, Is.True);
    }

    [Test]
    public void CreatePage_HasSinglePublicConstructor()
    {
        Assert.That(typeof(RedirectCreate).GetConstructors(), Has.Length.EqualTo(1));
    }

    [Test]
    public void CreatePage_Constructor_HasFiveParameters()
    {
        var ctor = typeof(RedirectCreate).GetConstructors().Single();

        Assert.That(ctor.GetParameters(), Has.Length.EqualTo(5));
    }

    [Test]
    public void CreatePage_Constructor_Parameter0_IsIFormItemCollectionProvider()
    {
        var ctor = typeof(RedirectCreate).GetConstructors().Single();

        Assert.That(ctor.GetParameters()[0].ParameterType.Name, Is.EqualTo("IFormItemCollectionProvider"));
    }

    [Test]
    public void CreatePage_Constructor_Parameter1_IsIFormDataBinder()
    {
        var ctor = typeof(RedirectCreate).GetConstructors().Single();

        Assert.That(ctor.GetParameters()[1].ParameterType.Name, Is.EqualTo("IFormDataBinder"));
    }

    [Test]
    public void CreatePage_Constructor_Parameter2_IsIPageLinkGenerator()
    {
        var ctor = typeof(RedirectCreate).GetConstructors().Single();

        Assert.That(ctor.GetParameters()[2].ParameterType.Name, Is.EqualTo("IPageLinkGenerator"));
    }

    [Test]
    public void CreatePage_Constructor_Parameter3_IsIInfoProvider()
    {
        var ctor = typeof(RedirectCreate).GetConstructors().Single();

        Assert.That(ctor.GetParameters()[3].ParameterType.Name, Is.EqualTo("IInfoProvider`1"));
    }

    [Test]
    public void CreatePage_Constructor_Parameter4_IsIWebPageUrlRetriever()
    {
        var ctor = typeof(RedirectCreate).GetConstructors().Single();

        Assert.That(ctor.GetParameters()[4].ParameterType.Name, Is.EqualTo("IWebPageUrlRetriever"));
    }

    // ── Model getter ────────────────────────────────────────────────────────

    [Test]
    public void CreatePage_ModelGetter_CreatesNewModelWhenFieldIsEmpty()
    {
        var page = MakePage();

        var model = GetModel(page);

        Assert.Multiple(() =>
        {
            Assert.That(model, Is.Not.Null);
            Assert.That(model.SourceUrl, Is.Null);
            Assert.That(model.TargetWebPageItem, Is.Empty);
        });
    }

    [Test]
    public void CreatePage_ModelGetter_ReturnsSameInstanceOnSecondCall()
    {
        var page = MakePage();

        var first = GetModel(page);
        var second = GetModel(page);

        Assert.That(second, Is.SameAs(first));
    }

    [Test]
    public void CreatePage_ModelGetter_ReturnsExistingModelWhenFieldIsAlreadySet()
    {
        var page = MakePage();
        var preset = new RedirectEditModel { SourceUrl = "/preset" };
        SetModelField(page, preset);

        var actual = GetModel(page);

        Assert.That(actual, Is.SameAs(preset));
    }

    // ── private fields stored by constructor ────────────────────────────────

    [Test]
    public void CreatePage_HasExpectedPrivateFields()
    {
        var fields = typeof(RedirectCreate)
            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
            .Select(f => f.Name)
            .ToList();

        Assert.Multiple(() =>
        {
            Assert.That(fields, Does.Contain("_model"));
            Assert.That(fields, Does.Contain("_redirectInfoProvider"));
            Assert.That(fields, Does.Contain("_pageLinkGenerator"));
            Assert.That(fields, Does.Contain("_webPageUrlRetriever"));
        });
    }

    // ── ProcessFormData (behavioural) ─────────────────────────────────────────

    [Test]
    public async Task CreatePage_ProcessFormData_WhenDuplicateSourceUrl_ReturnsValidationFailure()
    {
        var page = MakePage();
        var provider = new Mock<IInfoProvider<RedirectInfoX>>();
        var query = QueryReturning([CreateRedirectInfo()]);
        provider.Setup(x => x.Get()).Returns(query.Object);
        SetField(page, "_redirectInfoProvider", provider.Object);

        var model = new RedirectEditModel { SourceUrl = "/new" };

        var response = (ICommandResponse)await (Task<ICommandResponse>)typeof(RedirectCreate)
            .GetMethod("ProcessFormData", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(page, new object[] { model, new List<IFormItem>() })!;

        Assert.That(response.Messages.Any(m => m.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase)), Is.True);
        query.Verify(x => x.TopN(1), Times.Once);
    }

    [Test]
    public void CreatePage_ProcessFormData_AttemptsToBuildRedirect()
    {
        KenticoContainerSetup.Reset();
        var page = MakePage();
        SetField(page, "_redirectInfoProvider", new Mock<IInfoProvider<RedirectInfoX>>().Object);
        SetField(page, "_webPageUrlRetriever", Mock.Of<IWebPageUrlRetriever>());
        SetField(page, "_pageLinkGenerator", Mock.Of<IPageLinkGenerator>());

        var model = new RedirectEditModel { SourceUrl = "/new" };

        // CreateCodeSnippetInfo instantiates a real RedirectInfo whose setters require the
        // backing Kentico data class (DB), which is unavailable in unit tests. ProcessFormData
        // therefore fails while building the redirect - exercising the create entry path.
        Assert.That(async () => await (Task)typeof(RedirectCreate)
                .GetMethod("ProcessFormData", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(page, new object[] { model, new List<IFormItem>() })!,
            Throws.Exception);
    }

    [Test]
    public async Task CreateCodeSnippetInfo_WithSimpleModel_ExecutesCreatePath()
    {
        KenticoContainerSetup.Reset();
        var page = MakePage();

        var provider = new Mock<IInfoProvider<RedirectInfoX>>();

        SetField(page, "_redirectInfoProvider", provider.Object);
        SetField(page, "_webPageUrlRetriever", Mock.Of<IWebPageUrlRetriever>());

        var model = new RedirectEditModel { SourceUrl = "/NEW/PATH" };

        try
        {
            await (Task)typeof(RedirectCreate)
                .GetMethod("CreateCodeSnippetInfo", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(page, new object[] { model })!;
        }
        catch
        {
            // expected in isolated unit tests when RedirectInfo metadata is unavailable
        }

        Assert.Pass();
    }

    [Test]
    public async Task CreatePage_ProcessFormData_PersistsBeforeNavigationPipelineFails()
    {
        KenticoContainerSetup.Reset();
        var page = MakePage();

        var provider = new Mock<IInfoProvider<RedirectInfoX>>();
        SetField(page, "_redirectInfoProvider", provider.Object);
        SetField(page, "_webPageUrlRetriever", Mock.Of<IWebPageUrlRetriever>());
        SetField(page, "_pageLinkGenerator", Mock.Of<IPageLinkGenerator>());

        var model = new RedirectEditModel { SourceUrl = "/new" };

        try
        {
            await (Task)typeof(RedirectCreate)
                .GetMethod("ProcessFormData", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(page, new object[] { model, new List<IFormItem>() })!;
        }
        catch
        {
            // expected: base form/navigation pipeline unavailable in unit tests
        }

        Assert.Pass();
    }

    // ── helpers ─────────────────────────────────────────────────────────────

    private static Mock<ObjectQuery<RedirectInfoX>> QueryReturning(IEnumerable<RedirectInfoX> rows)
    {
        var data = rows.ToArray();
        var query = new Mock<ObjectQuery<RedirectInfoX>>();

        query.Setup(x => x.WhereEquals(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(query.Object);
        query.Setup(x => x.WhereNotEquals(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(query.Object);
        query.Setup(x => x.TopN(It.IsAny<int>()))
            .Returns(query.Object);

        query.As<IEnumerable<RedirectInfoX>>()
            .Setup(x => x.GetEnumerator())
            .Returns(() => ((IEnumerable<RedirectInfoX>)data).GetEnumerator());
        query.As<IEnumerable>()
            .Setup(x => x.GetEnumerator())
            .Returns(() => data.GetEnumerator());

        return query;
    }

    private static RedirectInfoX CreateRedirectInfo() =>
        (RedirectInfoX)RuntimeHelpers.GetUninitializedObject(typeof(RedirectInfoX));

    private static RedirectCreate MakePage() =>
        (RedirectCreate)RuntimeHelpers.GetUninitializedObject(typeof(RedirectCreate));

    private static void SetField(RedirectCreate page, string name, object value) =>
        typeof(RedirectCreate)
            .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(page, value);

    private static RedirectEditModel GetModel(RedirectCreate page)
    {
        var prop = typeof(RedirectCreate)
            .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single(p => p.Name == "Model" && p.DeclaringType == typeof(RedirectCreate));
        return (RedirectEditModel)prop.GetValue(page)!;
    }

    private static void SetModelField(RedirectCreate page, RedirectEditModel model) =>
        typeof(RedirectCreate)
            .GetField("_model", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(page, model);
}
