using System.Reflection;
using BDO.GWT.GLOBAL.Sync.Data;
using BDO.GWT.GLOBAL.Sync.Services;
using CMS.Base;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Websites;
using Moq;

namespace BDO.GWT.GLOBAL.Sync.Tests.Services;

[TestFixture]
public class ChannelDirectoryServiceTests
{
    private Mock<IInfoProvider<ChannelInfo>> _channelProviderMock = null!;
    private ChannelDirectoryService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _channelProviderMock = new Mock<IInfoProvider<ChannelInfo>>();
        _service = new ChannelDirectoryService(_channelProviderMock.Object);
    }

    [Test]
    public void Constructor_WithValidDependencies_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => new ChannelDirectoryService(_channelProviderMock.Object));
    }

    [Test]
    public void Service_ImplementsIChannelDirectoryService()
    {
        Assert.That(_service, Is.InstanceOf<IChannelDirectoryService>());
    }

    [Test]
    public void GetChannelAsync_WhenProviderGetThrows_PropagatesException()
    {
        _channelProviderMock.Setup(p => p.Get()).Throws<InvalidOperationException>();

        Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetChannelAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Test]
    public void GetAllChannelsExceptAsync_WhenProviderGetThrows_PropagatesException()
    {
        _channelProviderMock.Setup(p => p.Get()).Throws<InvalidOperationException>();

        Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetAllChannelsExceptAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Test]
    public void GetChannelAsync_WithCancelledToken_CanBeInvoked()
    {
        _channelProviderMock.Setup(p => p.Get()).Throws<OperationCanceledException>();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.ThrowsAsync<OperationCanceledException>(
            () => _service.GetChannelAsync(Guid.NewGuid(), cts.Token));
    }

    [Test]
    public void GetAllChannelsExceptAsync_WithCancelledToken_CanBeInvoked()
    {
        _channelProviderMock.Setup(p => p.Get()).Throws<OperationCanceledException>();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.ThrowsAsync<OperationCanceledException>(
            () => _service.GetAllChannelsExceptAsync(Guid.NewGuid(), cts.Token));
    }

    // ---------------------------------------------------------------------
    // ToChannelOutput (private static) — covered via reflection.
    // ---------------------------------------------------------------------

    [Test]
    public void ToChannelOutput_WithAllValuesPresent_MapsEveryField()
    {
        var channelGuid = Guid.NewGuid();
        var container = CreateContainer(
            websiteChannelId: 42,
            channelGuid: channelGuid,
            channelDisplayName: "Global",
            channelName: "global",
            websiteChannelDomain: "https://global.example.com");

        var result = InvokeToChannelOutput(container);

        Assert.Multiple(() =>
        {
            Assert.That(result.General.ID, Is.EqualTo(42));
            Assert.That(result.General.GUID, Is.EqualTo(channelGuid));
            Assert.That(result.General.DisplayName, Is.EqualTo("Global"));
            Assert.That(result.General.CodeName, Is.EqualTo("global"));
            Assert.That(result.General.WebsitedomainresentationURL, Is.EqualTo("https://global.example.com"));
        });
    }

    [Test]
    public void ToChannelOutput_WhenDomainIsNull_UsesEmptyString()
    {
        var container = CreateContainer(
            websiteChannelId: 1,
            channelGuid: Guid.NewGuid(),
            channelDisplayName: "X",
            channelName: "x",
            websiteChannelDomain: null);

        var result = InvokeToChannelOutput(container);

        Assert.That(result.General.WebsitedomainresentationURL, Is.EqualTo(string.Empty));
    }

    [Test]
    public void ToChannelOutput_WhenDisplayNameAndCodeNameAreNull_UsesEmptyStrings()
    {
        var container = CreateContainer(
            websiteChannelId: 5,
            channelGuid: Guid.NewGuid(),
            channelDisplayName: null,
            channelName: null,
            websiteChannelDomain: "https://x.example.com");

        var result = InvokeToChannelOutput(container);

        Assert.Multiple(() =>
        {
            Assert.That(result.General.DisplayName, Is.EqualTo(string.Empty));
            Assert.That(result.General.CodeName, Is.EqualTo(string.Empty));
        });
    }

    [Test]
    public void ToChannelOutput_WhenIdAndGuidAreNull_FallsBackToDefaults()
    {
        var container = new Mock<IDataContainer>();
        container.Setup(c => c.GetValue(nameof(WebsiteChannelInfo.WebsiteChannelID))).Returns((object)null!);
        container.Setup(c => c.GetValue(nameof(ChannelInfo.ChannelGUID))).Returns((object)null!);
        container.Setup(c => c.GetValue(nameof(ChannelInfo.ChannelDisplayName))).Returns("A");
        container.Setup(c => c.GetValue(nameof(ChannelInfo.ChannelName))).Returns("a");
        container.Setup(c => c.GetValue(nameof(WebsiteChannelInfo.WebsiteChannelDomain))).Returns("d");

        var result = InvokeToChannelOutput(container.Object);

        Assert.Multiple(() =>
        {
            Assert.That(result.General.ID, Is.EqualTo(0));
            Assert.That(result.General.GUID, Is.EqualTo(Guid.Empty));
        });
    }

    private static ChannelOutput InvokeToChannelOutput(IDataContainer container)
    {
        var method = typeof(ChannelDirectoryService).GetMethod(
            "ToChannelOutput",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("ToChannelOutput not found.");

        return (ChannelOutput)method.Invoke(null, new object[] { container })!;
    }

    private static IDataContainer CreateContainer(
        int websiteChannelId,
        Guid channelGuid,
        string? channelDisplayName,
        string? channelName,
        string? websiteChannelDomain)
    {
        var mock = new Mock<IDataContainer>();
        mock.Setup(c => c.GetValue(nameof(WebsiteChannelInfo.WebsiteChannelID))).Returns(websiteChannelId);
        mock.Setup(c => c.GetValue(nameof(ChannelInfo.ChannelGUID))).Returns(channelGuid);
        mock.Setup(c => c.GetValue(nameof(ChannelInfo.ChannelDisplayName))).Returns(channelDisplayName!);
        mock.Setup(c => c.GetValue(nameof(ChannelInfo.ChannelName))).Returns(channelName!);
        mock.Setup(c => c.GetValue(nameof(WebsiteChannelInfo.WebsiteChannelDomain))).Returns(websiteChannelDomain!);
        return mock.Object;
    }
}
