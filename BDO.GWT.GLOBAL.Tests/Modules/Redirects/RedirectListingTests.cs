using System.Reflection;
using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;
using Kentico.Xperience.Admin.Base;
using XperienceCommunity.Redirects.UIPages;

namespace BDO.GWT.GLOBAL.Tests.Modules.Redirects;

public class RedirectListingTests
{
    [Test]
    public void ListingPage_InheritsFromListingPage()
    {
        Assert.That(typeof(RedirectListing).BaseType, Is.EqualTo(typeof(ListingPage)));
    }

    [Test]
    public void ListingPage_DeclaresExpectedObjectTypeProperty()
    {
        var prop = typeof(RedirectListing).GetProperty("ObjectType", BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(prop, Is.Not.Null);
        Assert.That(prop!.DeclaringType, Is.EqualTo(typeof(RedirectListing)));
    }

    [Test]
    public void ListingPage_ExposesExpectedConstructor()
    {
        var ctor = typeof(RedirectListing).GetConstructors().Single();

        Assert.That(ctor.GetParameters(), Has.Length.EqualTo(4));
        Assert.Multiple(() =>
        {
            Assert.That(ctor.GetParameters()[0].ParameterType.Name, Is.EqualTo("IContentQueryExecutor"));
            Assert.That(ctor.GetParameters()[1].ParameterType.Name, Is.EqualTo("IWebPageUrlRetriever"));
            Assert.That(ctor.GetParameters()[2].ParameterType.Name, Is.EqualTo("IChannelAccessService"));
            Assert.That(ctor.GetParameters()[3].ParameterType.Name, Is.EqualTo("IInfoProvider`1"));
        });
    }

    [Test]
    public void MassDelete_MethodExists_WithExpectedSignature()
    {
        var method = typeof(RedirectListing).GetMethod(nameof(RedirectListing.MassDelete));

        Assert.That(method, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(method!.ReturnType.Name, Does.StartWith("Task"));
            Assert.That(method.GetParameters(), Has.Length.EqualTo(2));
        });
    }
}
