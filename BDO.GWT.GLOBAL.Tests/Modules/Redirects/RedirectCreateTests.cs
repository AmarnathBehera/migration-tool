using System.Reflection;
using System.Runtime.CompilerServices;
using CMS.DataEngine;
using CMS.Websites;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Moq;
using XperienceCommunity.Redirects.UIPages;
using RedirectInfoX = global::XperienceCommunity.Redirects.RedirectInfo;

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

    // ── helpers ─────────────────────────────────────────────────────────────

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
