using System.Reflection;
using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;
using CMS.DataEngine;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Modules.Redirects;

public class ChannelDropdownProviderTests
{
    [Test]
    public void Provider_IsPublicClass()
    {
        Assert.That(typeof(ChannelDropdownProvider).IsPublic, Is.True);
    }

    [Test]
    public void Provider_ImplementsIDropDownOptionsProvider()
    {
        Assert.That(typeof(ChannelDropdownProvider).GetInterfaces().Any(i => i.Name == "IDropDownOptionsProvider"), Is.True);
    }

    [Test]
    public void Provider_HasSinglePublicConstructor()
    {
        Assert.That(typeof(ChannelDropdownProvider).GetConstructors(), Has.Length.EqualTo(1));
    }

    [Test]
    public void Provider_Constructor_HasExpectedParameters()
    {
        var parameters = typeof(ChannelDropdownProvider).GetConstructors().Single().GetParameters();

        Assert.Multiple(() =>
        {
            Assert.That(parameters, Has.Length.EqualTo(2));
            Assert.That(parameters[0].ParameterType, Is.EqualTo(typeof(IInfoProvider<CMS.ContentEngine.ChannelInfo>)));
            Assert.That(parameters[1].ParameterType, Is.EqualTo(typeof(IChannelAccessService)));
        });
    }

    [Test]
    public void Provider_Constructor_StoresDependencies()
    {
        var infoProvider = new Mock<IInfoProvider<CMS.ContentEngine.ChannelInfo>>().Object;
        var accessService = new Mock<IChannelAccessService>().Object;

        var provider = new ChannelDropdownProvider(infoProvider, accessService);

        Assert.Multiple(() =>
        {
            Assert.That(GetField(provider, "channelInfoProvider"), Is.SameAs(infoProvider));
            Assert.That(GetField(provider, "channelAccessService"), Is.SameAs(accessService));
        });
    }

    [Test]
    public void GetOptionItems_MethodExists_AndReturnsTask()
    {
        var method = typeof(ChannelDropdownProvider).GetMethod(nameof(ChannelDropdownProvider.GetOptionItems));

        Assert.That(method, Is.Not.Null);
        Assert.That(method!.ReturnType.Name, Is.EqualTo("Task`1"));
    }

    [Test]
    public void GetOptionItems_WhenChannelAccessThrows_PropagatesException()
    {
        var accessService = new Mock<IChannelAccessService>();
        accessService.Setup(x => x.GetAccessibleWebChannelSettingsIdsAsync())
            .ThrowsAsync(new InvalidOperationException("boom"));

        var provider = new ChannelDropdownProvider(Mock.Of<IInfoProvider<CMS.ContentEngine.ChannelInfo>>(), accessService.Object);

        Assert.That(async () => await provider.GetOptionItems(), Throws.TypeOf<InvalidOperationException>());
    }

    [Test]
    public void GetOptionItems_WhenInfoProviderThrows_PropagatesException()
    {
        var accessService = new Mock<IChannelAccessService>();
        accessService.Setup(x => x.GetAccessibleWebChannelSettingsIdsAsync())
            .Returns(Task.FromResult<ICollection<int>?>(null));

        var infoProvider = new Mock<IInfoProvider<CMS.ContentEngine.ChannelInfo>>();
        infoProvider.Setup(x => x.Get())
            .Throws(new InvalidOperationException("query failed"));

        var provider = new ChannelDropdownProvider(infoProvider.Object, accessService.Object);

        Assert.That(async () => await provider.GetOptionItems(), Throws.TypeOf<InvalidOperationException>());
    }

    private static object? GetField(object target, string fieldName) =>
        typeof(ChannelDropdownProvider)
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(target);
}
