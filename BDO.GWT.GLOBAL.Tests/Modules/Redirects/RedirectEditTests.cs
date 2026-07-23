using CMS.DataEngine;
using CMS.Websites;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Moq;
using RedirectInfoX = global::XperienceCommunity.Redirects.RedirectInfo;
using XperienceCommunity.Redirects.UIPages;
using IFormItemCollectionProvider = Kentico.Xperience.Admin.Base.Forms.Internal.IFormItemCollectionProvider;

namespace BDO.GWT.GLOBAL.Tests.Modules.Redirects;

public class RedirectEditTests
{
    [Test]
    public void EditPage_InheritsFromModelEditPageOfRedirectEditModel()
    {
        var baseType = typeof(RedirectEdit).BaseType;

        Assert.That(baseType, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(baseType!.IsGenericType, Is.True);
            Assert.That(baseType.GetGenericArguments()[0], Is.EqualTo(typeof(RedirectEditModel)));
        });
    }

    [Test]
    public void EditPage_ExposesExpectedConstructor()
    {
        var ctor = typeof(RedirectEdit).GetConstructors().Single();

        Assert.That(ctor.GetParameters(), Has.Length.EqualTo(4));
        Assert.Multiple(() =>
        {
            Assert.That(ctor.GetParameters()[0].ParameterType.Name, Is.EqualTo("IFormItemCollectionProvider"));
            Assert.That(ctor.GetParameters()[1].ParameterType, Is.EqualTo(typeof(Kentico.Xperience.Admin.Base.Forms.IFormDataBinder)));
            Assert.That(ctor.GetParameters()[2].ParameterType.Name, Is.EqualTo("IInfoProvider`1"));
            Assert.That(ctor.GetParameters()[3].ParameterType.Name, Is.EqualTo("IWebPageUrlRetriever"));
        });
    }

    [Test]
    public void EditPage_DeclaresObjectIDPageParameter()
    {
        var prop = typeof(RedirectEdit).GetProperty(nameof(RedirectEdit.ObjectID));

        Assert.That(prop, Is.Not.Null);
        Assert.That(prop!.GetCustomAttributes(typeof(PageParameterAttribute), inherit: true), Is.Not.Empty);
    }
}
