using System.Reflection;
using System.Runtime.Serialization;
using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Websites;
using Moq;
using XperienceCommunity.Redirects.UIPages;

namespace BDO.GWT.GLOBAL.Tests.Modules.Redirects;

public class RedirectListingTests
{
    // ── structural ──────────────────────────────────────────────────────────

    [Test]
    public void ListingPage_IsPublicClass()
    {
        Assert.That(typeof(RedirectListing).IsPublic, Is.True);
    }

    [Test]
    public void ListingPage_InheritsFromListingPage()
    {
        Assert.That(typeof(RedirectListing).BaseType?.Name, Is.EqualTo("ListingPage"));
    }

    [Test]
    public void ListingPage_HasSinglePublicConstructor()
    {
        Assert.That(typeof(RedirectListing).GetConstructors(), Has.Length.EqualTo(1));
    }

    [Test]
    public void ListingPage_Constructor_HasFourParameters()
    {
        Assert.That(typeof(RedirectListing).GetConstructors().Single().GetParameters(), Has.Length.EqualTo(4));
    }

    [Test]
    public void ListingPage_Constructor_Parameter0_IsIContentQueryExecutor()
    {
        Assert.That(
            typeof(RedirectListing).GetConstructors().Single().GetParameters()[0].ParameterType.Name,
            Is.EqualTo("IContentQueryExecutor"));
    }

    [Test]
    public void ListingPage_Constructor_Parameter1_IsIWebPageUrlRetriever()
    {
        Assert.That(
            typeof(RedirectListing).GetConstructors().Single().GetParameters()[1].ParameterType.Name,
            Is.EqualTo("IWebPageUrlRetriever"));
    }

    [Test]
    public void ListingPage_Constructor_Parameter2_IsIChannelAccessService()
    {
        Assert.That(
            typeof(RedirectListing).GetConstructors().Single().GetParameters()[2].ParameterType.Name,
            Is.EqualTo("IChannelAccessService"));
    }

    [Test]
    public void ListingPage_Constructor_Parameter3_IsIInfoProvider()
    {
        Assert.That(
            typeof(RedirectListing).GetConstructors().Single().GetParameters()[3].ParameterType.Name,
            Is.EqualTo("IInfoProvider`1"));
    }

 

    // ── methods ─────────────────────────────────────────────────────────────

    [Test]
    public void ListingPage_HasConfigurePageMethod()
    {
        Assert.That(typeof(RedirectListing).GetMethod(nameof(RedirectListing.ConfigurePage)), Is.Not.Null);
    }

    [Test]
    public void ListingPage_ConfigurePage_ReturnsTask()
    {
        var method = typeof(RedirectListing).GetMethod(nameof(RedirectListing.ConfigurePage));
        Assert.That(method!.ReturnType, Is.EqualTo(typeof(Task)));
    }

    [Test]
    public void ListingPage_ConfigurePage_IsPublicOverride()
    {
        var method = typeof(RedirectListing).GetMethod(nameof(RedirectListing.ConfigurePage));
        Assert.Multiple(() =>
        {
            Assert.That(method!.IsPublic, Is.True);
            Assert.That(method.IsVirtual, Is.True);
        });
    }

    [Test]
    public void ListingPage_HasDeleteMethod()
    {
        Assert.That(typeof(RedirectListing).GetMethod(nameof(RedirectListing.Delete)), Is.Not.Null);
    }

    [Test]
    public void ListingPage_Delete_HasPageCommandAttribute()
    {
        var method = typeof(RedirectListing).GetMethod(nameof(RedirectListing.Delete))!;
        var attr = method.GetCustomAttributes().FirstOrDefault(a => a.GetType().Name == "PageCommandAttribute");
        Assert.That(attr, Is.Not.Null);
    }

    [Test]
    public void ListingPage_HasMassDeleteMethod()
    {
        Assert.That(typeof(RedirectListing).GetMethod(nameof(RedirectListing.MassDelete)), Is.Not.Null);
    }

    [Test]
    public void ListingPage_MassDelete_HasPageCommandAttribute()
    {
        var method = typeof(RedirectListing).GetMethod(nameof(RedirectListing.MassDelete))!;
        var attr = method.GetCustomAttributes().FirstOrDefault(a => a.GetType().Name == "PageCommandAttribute");
        Assert.That(attr, Is.Not.Null);
    }

    [Test]
    public void ListingPage_MassDelete_HasTwoParameters()
    {
        var method = typeof(RedirectListing).GetMethod(nameof(RedirectListing.MassDelete))!;
        Assert.That(method.GetParameters(), Has.Length.EqualTo(2));
    }

    [Test]
    public void ListingPage_MassDelete_Parameter0_IsEnumerableOfInt()
    {
        var method = typeof(RedirectListing).GetMethod(nameof(RedirectListing.MassDelete))!;
        Assert.That(method.GetParameters()[0].ParameterType, Is.EqualTo(typeof(IEnumerable<int>)));
    }

    [Test]
    public void ListingPage_MassDelete_Parameter1_IsCancellationToken()
    {
        var method = typeof(RedirectListing).GetMethod(nameof(RedirectListing.MassDelete))!;
        Assert.That(method.GetParameters()[1].ParameterType, Is.EqualTo(typeof(CancellationToken)));
    }

    [Test]
    public void ListingPage_MassDelete_ReturnsTaskOfCommandResponse()
    {
        var method = typeof(RedirectListing).GetMethod(nameof(RedirectListing.MassDelete))!;
        Assert.That(method.ReturnType.Name, Is.EqualTo("Task`1"));
    }

    // ── ObjectType ──────────────────────────────────────────────────────────

    [Test]
    public void ListingPage_ObjectType_ReturnsRedirectObjectType()
    {
        var page = (RedirectListing)FormatterServices.GetUninitializedObject(typeof(RedirectListing));
        var prop = typeof(RedirectListing).GetProperty(
            "ObjectType", BindingFlags.Instance | BindingFlags.NonPublic);

        var value = prop!.GetValue(page);

        Assert.That(value, Is.EqualTo(RedirectInfo.OBJECT_TYPE));
    }

    // ── private fields ───────────────────────────────────────────────────────

    [Test]
    public void ListingPage_HasExpectedPrivateFields()
    {
        var fields = typeof(RedirectListing)
            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
            .Select(f => f.Name)
            .ToList();

        Assert.Multiple(() =>
        {
            Assert.That(fields, Does.Contain("_executor"));
            Assert.That(fields, Does.Contain("_webPageUrlRetriever"));
            Assert.That(fields, Does.Contain("_channelAccessService"));
            Assert.That(fields, Does.Contain("_redirectInfoProvider"));
        });
    }

    [Test]
    public void ListingPage_PrivateField_ExecutorType_IsIContentQueryExecutor()
    {
        var field = typeof(RedirectListing)
            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single(f => f.Name == "_executor");

        Assert.That(field.FieldType.Name, Is.EqualTo("IContentQueryExecutor"));
    }

    [Test]
    public void ListingPage_PrivateField_ChannelAccessServiceType_IsIChannelAccessService()
    {
        var field = typeof(RedirectListing)
            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single(f => f.Name == "_channelAccessService");

        Assert.That(field.FieldType, Is.EqualTo(typeof(IChannelAccessService)));
    }

   
}
