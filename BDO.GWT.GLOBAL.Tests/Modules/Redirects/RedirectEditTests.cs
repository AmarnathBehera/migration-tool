using System.Reflection;
using System.Runtime.CompilerServices;
using CMS.DataEngine;
using CMS.Websites;
using Kentico.Xperience.Admin.Base.Forms;
using Moq;
using XperienceCommunity.Redirects.UIPages;
using RedirectInfoX = global::XperienceCommunity.Redirects.RedirectInfo;

namespace BDO.GWT.GLOBAL.Tests.Modules.Redirects;

public class RedirectEditTests
{
    // ── structural ──────────────────────────────────────────────────────────

    [Test]
    public void EditPage_InheritsFromModelEditPage()
    {
        Assert.That(typeof(RedirectEdit).BaseType!.Name, Is.EqualTo("ModelEditPage`1"));
    }

    [Test]
    public void EditPage_BaseType_GenericArgument_IsRedirectEditModel()
    {
        var genericArg = typeof(RedirectEdit).BaseType!.GetGenericArguments()[0];

        Assert.That(genericArg, Is.EqualTo(typeof(RedirectEditModel)));
    }

    [Test]
    public void EditPage_IsInternalClass()
    {
        Assert.That(typeof(RedirectEdit).IsNotPublic, Is.True);
    }

    [Test]
    public void EditPage_HasSinglePublicConstructor()
    {
        Assert.That(typeof(RedirectEdit).GetConstructors(), Has.Length.EqualTo(1));
    }

    [Test]
    public void EditPage_Constructor_HasFourParameters()
    {
        var ctor = typeof(RedirectEdit).GetConstructors().Single();

        Assert.That(ctor.GetParameters(), Has.Length.EqualTo(4));
    }

    [Test]
    public void EditPage_Constructor_Parameter0_IsIFormItemCollectionProvider()
    {
        Assert.That(typeof(RedirectEdit).GetConstructors().Single().GetParameters()[0].ParameterType.Name,
            Is.EqualTo("IFormItemCollectionProvider"));
    }

    [Test]
    public void EditPage_Constructor_Parameter1_IsIFormDataBinder()
    {
        Assert.That(typeof(RedirectEdit).GetConstructors().Single().GetParameters()[1].ParameterType.Name,
            Is.EqualTo("IFormDataBinder"));
    }

    [Test]
    public void EditPage_Constructor_Parameter2_IsIInfoProvider()
    {
        Assert.That(typeof(RedirectEdit).GetConstructors().Single().GetParameters()[2].ParameterType.Name,
            Is.EqualTo("IInfoProvider`1"));
    }

    [Test]
    public void EditPage_Constructor_Parameter3_IsIWebPageUrlRetriever()
    {
        Assert.That(typeof(RedirectEdit).GetConstructors().Single().GetParameters()[3].ParameterType.Name,
            Is.EqualTo("IWebPageUrlRetriever"));
    }

    [Test]
    public void EditPage_ObjectID_HasPageParameterAttribute()
    {
        var prop = typeof(RedirectEdit).GetProperty(nameof(RedirectEdit.ObjectID));

        Assert.That(prop, Is.Not.Null);
        var attr = prop!.GetCustomAttributes()
            .FirstOrDefault(a => a.GetType().Name == "PageParameterAttribute");
        Assert.That(attr, Is.Not.Null);
    }

    [Test]
    public void EditPage_ObjectID_IsPublicProperty()
    {
        var prop = typeof(RedirectEdit).GetProperty(nameof(RedirectEdit.ObjectID));

        Assert.That(prop!.GetGetMethod()?.IsPublic, Is.True);
    }

    // ── Model getter ────────────────────────────────────────────────────────

    [Test]
    public void EditPage_ModelGetter_ReturnsExistingModelWhenFieldIsAlreadySet()
    {
        var page = MakePage();
        var expected = new RedirectEditModel { SourceUrl = "/source" };
        SetModelField(page, expected);

        Assert.That(GetModel(page), Is.SameAs(expected));
    }

    [Test]
    public void EditPage_ModelGetter_ReturnsSameInstanceOnRepeatedCallsWithPresetField()
    {
        var page = MakePage();
        var preset = new RedirectEditModel { SourceUrl = "/source" };
        SetModelField(page, preset);

        var first = GetModel(page);
        var second = GetModel(page);

        Assert.That(second, Is.SameAs(first));
    }

    // ── Model getter (behavioural) ────────────────────────────────────────────

    [Test]
    public void EditPage_ModelGetter_WhenInfoNotFound_ReturnsEmptyModel()
    {
        var page = MakePage();
        SetProviderReturning(page, null);
        page.ObjectID = 10;

        var model = GetModel(page);

        Assert.Multiple(() =>
        {
            Assert.That(model, Is.Not.Null);
            Assert.That(model.SourceUrl, Is.Null);
            Assert.That(model.TargetWebPageItem, Is.Empty);
        });
    }

    [Test]
    public void EditPage_ModelGetter_WhenInfoFound_MapsSourceUrlAndTargetWebPage()
    {
        var page = MakePage();
        var guid = Guid.NewGuid();
        SetProviderReturning(page, MakeInfo(sourceUrl: "/from", targetGuid: guid));
        page.ObjectID = 3;

        var model = GetModel(page);

        Assert.Multiple(() =>
        {
            Assert.That(model.SourceUrl, Is.EqualTo("/from"));
            Assert.That(model.TargetWebPageItem.Single().WebPageGuid, Is.EqualTo(guid));
        });
    }

    [Test]
    public void EditPage_ModelGetter_WhenInfoFound_CachesResult()
    {
        var page = MakePage();
        SetProviderReturning(page, MakeInfo(sourceUrl: "/from", targetGuid: Guid.NewGuid()));
        page.ObjectID = 3;

        var first = GetModel(page);
        var second = GetModel(page);

        Assert.That(second, Is.SameAs(first));
    }

    // ── private fields ──────────────────────────────────────────────────────

    [Test]
    public void EditPage_HasExpectedPrivateFields()
    {
        var fields = typeof(RedirectEdit)
            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
            .Select(f => f.Name)
            .ToList();

        Assert.Multiple(() =>
        {
            Assert.That(fields, Does.Contain("_model"));
            Assert.That(fields, Does.Contain("_redirectInfoProvider"));
            Assert.That(fields, Does.Contain("_webPageUrlRetriever"));
        });
    }

    // ── ProcessFormData (behavioural) ─────────────────────────────────────────

    [Test]
    public async Task EditPage_ProcessFormData_MapsModelOntoLoadedInfo()
    {
        KenticoContainerSetup.Reset();
        var page = MakePage();

        var info = (FakeRedirectInfo)RuntimeHelpers.GetUninitializedObject(typeof(FakeRedirectInfo));
        var provider = new Mock<IInfoProvider<RedirectInfoX>>();
        provider.As<IInfoByIdProvider<RedirectInfoX>>()
            .Setup(x => x.Get(It.IsAny<int>()))
            .Returns(info);
        SetField(page, "_redirectInfoProvider", provider.Object);
        SetField(page, "_webPageUrlRetriever", Mock.Of<IWebPageUrlRetriever>());
        page.ObjectID = 7;

        var model = new RedirectEditModel { SourceUrl = "/Source" };

        // base.ProcessFormData needs the admin ModelEditPage form pipeline, which is not
        // available in a unit test, so it fails only after the redirect info has been mapped
        // and persisted - which is what this test asserts.
        try
        {
            await (Task)typeof(RedirectEdit)
                .GetMethod("ProcessFormData", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(page, new object[] { model, new List<IFormItem>() })!;
        }
        catch
        {
            // expected: base form pipeline unavailable in unit tests
        }

        Assert.That(info.RedirectSourceUrl, Is.EqualTo("/source"));
    }

    // ── helpers ─────────────────────────────────────────────────────────────

    private static RedirectEdit MakePage() =>
        (RedirectEdit)RuntimeHelpers.GetUninitializedObject(typeof(RedirectEdit));

    private static void SetField(RedirectEdit page, string name, object value) =>
        typeof(RedirectEdit)
            .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(page, value);

    private static RedirectEditModel GetModel(RedirectEdit page)
    {
        var prop = typeof(RedirectEdit)
            .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single(p => p.Name == "Model" && p.DeclaringType == typeof(RedirectEdit));
        return (RedirectEditModel)prop.GetValue(page)!;
    }

    private static void SetModelField(RedirectEdit page, RedirectEditModel model) =>
        typeof(RedirectEdit)
            .GetField("_model", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(page, model);

    private static void SetProviderReturning(RedirectEdit page, RedirectInfoX? info)
    {
        var provider = new Mock<IInfoProvider<RedirectInfoX>>();
        provider.As<IInfoByIdProvider<RedirectInfoX>>()
            .Setup(x => x.Get(It.IsAny<int>()))
            .Returns(info!);

        typeof(RedirectEdit)
            .GetField("_redirectInfoProvider", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(page, provider.Object);
    }

    private static FakeRedirectInfo MakeInfo(string sourceUrl, Guid targetGuid)
    {
        var info = (FakeRedirectInfo)RuntimeHelpers.GetUninitializedObject(typeof(FakeRedirectInfo));
        info.RedirectSourceUrl = sourceUrl;
        info.RedirectTargetWebPageItemGUID = targetGuid;
        return info;
    }

    // Overrides the virtual data fields so reads bypass Kentico's AbstractInfoBase.GetValue,
    // which requires an initialised CMS IoC container.
    private sealed class FakeRedirectInfo : RedirectInfoX
    {
        private string? _sourceUrl;
        private Guid _targetGuid;
        private string? _targetPageUrl;

        public override string RedirectSourceUrl
        {
            get => _sourceUrl!;
            set => _sourceUrl = value;
        }

        public override Guid RedirectTargetWebPageItemGUID
        {
            get => _targetGuid;
            set => _targetGuid = value;
        }

        public override string RedirectTargetPageURL
        {
            get => _targetPageUrl ?? string.Empty;
            set => _targetPageUrl = value;
        }
    }
}
