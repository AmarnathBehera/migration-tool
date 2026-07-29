using System.Reflection;
using System.Runtime.CompilerServices;
using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;
using CMS.DataEngine;
using Moq;
using XperienceCommunity.Redirects.UIPages;
using RedirectInfoX = global::XperienceCommunity.Redirects.RedirectInfo;

namespace BDO.GWT.GLOBAL.Tests.Modules.Redirects;

public class RedirectListingTests
{
    private static readonly int[] SingleIdentifier = { 1 };
    private static readonly int[] TwoIdentifiers = { 1, 2 };

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
    public void ListingPage_Constructor_HasTwoParameters()
    {
        Assert.That(typeof(RedirectListing).GetConstructors().Single().GetParameters(), Has.Length.EqualTo(2));
    }

    [Test]
    public void ListingPage_Constructor_Parameter0_IsIChannelAccessService()
    {
        Assert.That(
            typeof(RedirectListing).GetConstructors().Single().GetParameters()[0].ParameterType.Name,
            Is.EqualTo("IChannelAccessService"));
    }

    [Test]
    public void ListingPage_Constructor_Parameter1_IsIInfoProvider()
    {
        Assert.That(
            typeof(RedirectListing).GetConstructors().Single().GetParameters()[1].ParameterType.Name,
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
        var page = (RedirectListing)RuntimeHelpers.GetUninitializedObject(typeof(RedirectListing));
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
            Assert.That(fields, Does.Contain("_channelAccessService"));
            Assert.That(fields, Does.Contain("_redirectInfoProvider"));
        });
    }

    [Test]
    public void ListingPage_PrivateField_ChannelAccessServiceType_IsIChannelAccessService()
    {
        var field = typeof(RedirectListing)
            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single(f => f.Name == "_channelAccessService");

        Assert.That(field.FieldType, Is.EqualTo(typeof(IChannelAccessService)));
    }

    // ── MassDelete (behavioural) ──────────────────────────────────────────────

    [Test]
    public void ListingPage_Constructor_StoresInjectedDependencies()
    {
        var channelAccess = new Mock<IChannelAccessService>().Object;
        var provider = new Mock<IInfoProvider<RedirectInfoX>>().Object;

        var listing = new RedirectListing(channelAccess, provider);

        Assert.Multiple(() =>
        {
            Assert.That(GetField(listing, "_channelAccessService"), Is.SameAs(channelAccess));
            Assert.That(GetField(listing, "_redirectInfoProvider"), Is.SameAs(provider));
        });
    }

    [Test]
    public async Task MassDelete_WithNoIdentifiers_ReturnsResponse()
    {
        var listing = MakeListing(accessibleIds: null, provider: new Mock<IInfoProvider<RedirectInfoX>>().Object);

        var response = await listing.MassDelete(Array.Empty<int>(), CancellationToken.None);

        Assert.That(response, Is.Not.Null);
    }

    [Test]
    public async Task MassDelete_WhenRedirectNotFound_SkipsWithoutDeleting()
    {
        var provider = new Mock<IInfoProvider<RedirectInfoX>>();
        provider.As<IInfoByIdProvider<RedirectInfoX>>()
            .Setup(x => x.Get(It.IsAny<int>()))
            .Returns((RedirectInfoX)null!);

        var listing = MakeListing(accessibleIds: null, provider: provider.Object);

        var response = await listing.MassDelete(TwoIdentifiers, CancellationToken.None);

        Assert.That(response, Is.Not.Null);
    }

    [Test]
    public async Task MassDelete_WhenRedirectNotInAccessibleChannel_SkipsIt()
    {
        var redirect = MakeRedirect(channelId: 99);
        var provider = new Mock<IInfoProvider<RedirectInfoX>>();
        provider.As<IInfoByIdProvider<RedirectInfoX>>()
            .Setup(x => x.Get(It.IsAny<int>()))
            .Returns(redirect);

        var listing = MakeListing(accessibleIds: new List<int> { 5 }, provider: provider.Object);

        var response = await listing.MassDelete(SingleIdentifier, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response, Is.Not.Null);
            Assert.That(redirect.DeleteCalled, Is.False);
        });
    }

    [Test]
    public async Task MassDelete_WhenRedirectInAccessibleChannel_DeletesIt()
    {
        var redirect = MakeRedirect(channelId: 5);
        var provider = new Mock<IInfoProvider<RedirectInfoX>>();
        provider.As<IInfoByIdProvider<RedirectInfoX>>()
            .Setup(x => x.Get(It.IsAny<int>()))
            .Returns(redirect);

        var listing = MakeListing(accessibleIds: new List<int> { 5 }, provider: provider.Object);

        var response = await listing.MassDelete(SingleIdentifier, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response, Is.Not.Null);
            Assert.That(redirect.DeleteCalled, Is.True);
        });
    }

    [Test]
    public async Task MassDelete_WhenGlobalAdmin_DeletesEveryRedirect()
    {
        // A null accessible-id collection represents a global administrator: every redirect
        // is deletable regardless of its channel.
        var redirect = MakeRedirect(channelId: 42);
        var provider = new Mock<IInfoProvider<RedirectInfoX>>();
        provider.As<IInfoByIdProvider<RedirectInfoX>>()
            .Setup(x => x.Get(It.IsAny<int>()))
            .Returns(redirect);

        var listing = MakeListing(accessibleIds: null, provider: provider.Object);

        await listing.MassDelete(SingleIdentifier, CancellationToken.None);

        Assert.That(redirect.DeleteCalled, Is.True);
    }

    // ── GetWebPageUrl (behavioural) ──────────────────────────────────────────

    [Test]
    public void GetWebPageUrl_WhenUrlIsEmpty_ReturnsPageDeleted()
    {
        var listing = MakeListingViaConstructor();

        Assert.That(InvokeGetWebPageUrl(listing, string.Empty), Is.EqualTo("Page deleted"));
    }

    [Test]
    public void GetWebPageUrl_WhenUrlIsProvided_ReturnsUrl()
    {
        var listing = MakeListingViaConstructor();

        Assert.That(InvokeGetWebPageUrl(listing, "/some/url"), Is.EqualTo("/some/url"));
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    private static RedirectListing MakeListingViaConstructor() =>
        new(Mock.Of<IChannelAccessService>(), Mock.Of<IInfoProvider<RedirectInfoX>>());

    private static string InvokeGetWebPageUrl(RedirectListing listing, string url)
    {
        var method = typeof(RedirectListing)
            .GetMethod("GetWebPageUrl", BindingFlags.Instance | BindingFlags.NonPublic)!;
        return (string)method.Invoke(listing, new object?[] { url, null })!;
    }

    private static RedirectListing MakeListing(ICollection<int>? accessibleIds, IInfoProvider<RedirectInfoX> provider)
    {
        var listing = (RedirectListing)RuntimeHelpers.GetUninitializedObject(typeof(RedirectListing));

        var channelAccess = new Mock<IChannelAccessService>();
        channelAccess.Setup(c => c.GetAccessibleWebChannelSettingsIdsAsync())
            .Returns(Task.FromResult<ICollection<int>>(accessibleIds!));

        SetField(listing, "_channelAccessService", channelAccess.Object);
        SetField(listing, "_redirectInfoProvider", provider);
        return listing;
    }

    private static void SetField(object target, string name, object value) =>
        typeof(RedirectListing).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(target, value);

    private static object? GetField(object target, string name) =>
        typeof(RedirectListing).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(target);

    private static FakeRedirectInfo MakeRedirect(int channelId)
    {
        var info = (FakeRedirectInfo)RuntimeHelpers.GetUninitializedObject(typeof(FakeRedirectInfo));
        info.RedirectChannelID = channelId;
        return info;
    }

    // Overrides the virtual data field and Delete() so the object can be exercised without
    // an initialised CMS IoC container (which the real getters/setters and provider require).
    private sealed class FakeRedirectInfo : RedirectInfoX
    {
        private int _channelId;

        public bool DeleteCalled { get; private set; }

        public override int RedirectChannelID
        {
            get => _channelId;
            set => _channelId = value;
        }

        public override bool Delete()
        {
            DeleteCalled = true;
            return true;
        }
    }
}
