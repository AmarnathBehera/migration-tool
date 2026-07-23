using System.Reflection;
using XperienceCommunity.Redirects.UIPages;

namespace BDO.GWT.GLOBAL.Tests.Modules.Redirects;

public class RedirectCreateTests
{
    [Test]
    public void CreatePage_InheritsFromModelEditPageOfRedirectEditModel()
    {
        var baseType = typeof(RedirectCreate).BaseType;

        Assert.That(baseType, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(baseType!.IsGenericType, Is.True);
            Assert.That(baseType.GetGenericArguments()[0], Is.EqualTo(typeof(RedirectEditModel)));
        });
    }

    [Test]
    public void CreatePage_ExposesExpectedConstructor()
    {
        var ctor = typeof(RedirectCreate).GetConstructors().Single();

        Assert.That(ctor.GetParameters(), Has.Length.EqualTo(5));
        Assert.Multiple(() =>
        {
            Assert.That(ctor.GetParameters()[0].ParameterType.Name, Is.EqualTo("IFormItemCollectionProvider"));
            Assert.That(ctor.GetParameters()[1].ParameterType.Name, Is.EqualTo("IFormDataBinder"));
            Assert.That(ctor.GetParameters()[2].ParameterType.Name, Is.EqualTo("IPageLinkGenerator"));
            Assert.That(ctor.GetParameters()[3].ParameterType.Name, Is.EqualTo("IInfoProvider`1"));
            Assert.That(ctor.GetParameters()[4].ParameterType.Name, Is.EqualTo("IWebPageUrlRetriever"));
        });
    }

    [Test]
    public void CreatePage_HasProtectedModelPropertyOfExpectedType()
    {
        var prop = typeof(RedirectCreate)
            .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single(p => p.Name == "Model" && p.DeclaringType == typeof(RedirectCreate));

        Assert.That(prop, Is.Not.Null);
        Assert.That(prop.PropertyType, Is.EqualTo(typeof(RedirectEditModel)));
    }
}
