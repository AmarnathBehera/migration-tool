using System.Reflection;
using System.Runtime.Serialization;
using XperienceCommunity.Redirects.UIPages;

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

    // ── helpers ─────────────────────────────────────────────────────────────

    private static RedirectEdit MakePage() =>
        (RedirectEdit)FormatterServices.GetUninitializedObject(typeof(RedirectEdit));

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
}
