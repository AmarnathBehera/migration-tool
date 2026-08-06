using System.Reflection;
using System.Runtime.CompilerServices;
using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;
using CMS.ContentEngine;
using CMS.DataEngine;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Modules.Redirects;

public class ChannelDropdownProviderTests
{
    [Test]
    public void Provider_Constructor_StoresDependencies()
    {
        var infoProvider = new Mock<IInfoProvider<ChannelInfo>>().Object;
        var accessService = new Mock<IChannelAccessService>().Object;

        var provider = new ChannelDropdownProvider(infoProvider, accessService);

        Assert.Multiple(() =>
        {
            Assert.That(GetField(provider, "channelInfoProvider"), Is.SameAs(infoProvider));
            Assert.That(GetField(provider, "channelAccessService"), Is.SameAs(accessService));
        });
    }

    [Test]
    public async Task GetOptionItems_WhenAccessibleChannelIdsIsNull_ReturnsAllWebsiteChannels()
    {
        var channelRows = new[]
        {
            CreateChannel(10, "Global"),
            CreateChannel(20, "US")
        };

        var query = BuildQuery(channelRows);

        var accessService = new Mock<IChannelAccessService>();
        accessService.Setup(x => x.GetAccessibleWebChannelSettingsIdsAsync())
            .ReturnsAsync((ICollection<int>?)null);

        var infoProvider = new Mock<IInfoProvider<ChannelInfo>>();
        infoProvider.Setup(x => x.Get()).Returns(query.Object);

        var provider = new ChannelDropdownProvider(infoProvider.Object, accessService.Object);

        var result = (await provider.GetOptionItems()).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(result.Select(x => x.Value), Is.EqualTo(new[] { "10", "20" }));
            Assert.That(result.Select(x => x.Text), Is.EqualTo(new[] { "Global", "US" }));
        });

        query.Verify(x => x.WhereEquals(nameof(ChannelInfo.ChannelType), "Website"), Times.Once);
        query.Verify(x => x.WhereEquals(nameof(ChannelInfo.ChannelID), -1), Times.Never);
        query.Verify(
            x => x.WhereIn(nameof(ChannelInfo.ChannelID), It.IsAny<List<int>>()),
            Times.Never);
    }

    [Test]
    public async Task GetOptionItems_WhenAccessibleChannelIdsIsEmpty_AddsImpossibleFilter()
    {
        var query = BuildQuery(Array.Empty<ChannelInfo>());

        var accessService = new Mock<IChannelAccessService>();
        accessService.Setup(x => x.GetAccessibleWebChannelSettingsIdsAsync())
            .ReturnsAsync(Array.Empty<int>());

        var infoProvider = new Mock<IInfoProvider<ChannelInfo>>();
        infoProvider.Setup(x => x.Get()).Returns(query.Object);

        var provider = new ChannelDropdownProvider(infoProvider.Object, accessService.Object);

        var result = (await provider.GetOptionItems()).ToList();

        Assert.That(result, Is.Empty);
        query.Verify(x => x.WhereEquals(nameof(ChannelInfo.ChannelType), "Website"), Times.Once);
        query.Verify(x => x.WhereEquals(nameof(ChannelInfo.ChannelID), -1), Times.Once);
        query.Verify(
            x => x.WhereIn(nameof(ChannelInfo.ChannelID), It.IsAny<List<int>>()),
            Times.Never);
    }

    [Test]
    public async Task GetOptionItems_WhenAccessibleChannelIdsProvided_FiltersByIds()
    {
        var channelRows = new[] { CreateChannel(5, "Canada") };
        var query = BuildQuery(channelRows);

        var accessService = new Mock<IChannelAccessService>();
        accessService.Setup(x => x.GetAccessibleWebChannelSettingsIdsAsync())
            .ReturnsAsync(new List<int> { 5, 8 });

        var infoProvider = new Mock<IInfoProvider<ChannelInfo>>();
        infoProvider.Setup(x => x.Get()).Returns(query.Object);

        var provider = new ChannelDropdownProvider(infoProvider.Object, accessService.Object);

        var result = (await provider.GetOptionItems()).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Value, Is.EqualTo("5"));
            Assert.That(result[0].Text, Is.EqualTo("Canada"));
        });

        query.Verify(x => x.WhereEquals(nameof(ChannelInfo.ChannelType), "Website"), Times.Once);
        query.Verify(x => x.WhereEquals(nameof(ChannelInfo.ChannelID), -1), Times.Never);
        query.Verify(
            x => x.WhereIn(nameof(ChannelInfo.ChannelID), It.Is<List<int>>(ids => ids.SequenceEqual(new[] { 5, 8 }))),
            Times.Once);
    }

    [Test]
    public void GetOptionItems_WhenChannelAccessThrows_PropagatesException()
    {
        var accessService = new Mock<IChannelAccessService>();
        accessService.Setup(x => x.GetAccessibleWebChannelSettingsIdsAsync())
            .ThrowsAsync(new InvalidOperationException("boom"));

        var provider = new ChannelDropdownProvider(Mock.Of<IInfoProvider<ChannelInfo>>(), accessService.Object);

        Assert.That(async () => await provider.GetOptionItems(), Throws.TypeOf<InvalidOperationException>());
    }

    [Test]
    public void GetOptionItems_WhenInfoProviderThrows_PropagatesException()
    {
        var accessService = new Mock<IChannelAccessService>();
        accessService.Setup(x => x.GetAccessibleWebChannelSettingsIdsAsync())
            .ReturnsAsync((ICollection<int>?)null);

        var infoProvider = new Mock<IInfoProvider<ChannelInfo>>();
        infoProvider.Setup(x => x.Get())
            .Throws(new InvalidOperationException("query failed"));

        var provider = new ChannelDropdownProvider(infoProvider.Object, accessService.Object);

        Assert.That(async () => await provider.GetOptionItems(), Throws.TypeOf<InvalidOperationException>());
    }

    private static Mock<ObjectQuery<ChannelInfo>> BuildQuery(IEnumerable<ChannelInfo> rows)
    {
        var query = new Mock<ObjectQuery<ChannelInfo>>();

        query.Setup(x => x.WhereEquals(nameof(ChannelInfo.ChannelType), "Website"))
            .Returns(query.Object);

        query.Setup(x => x.WhereEquals(nameof(ChannelInfo.ChannelID), -1))
            .Returns(query.Object);

        query.Setup(x => x.WhereIn(nameof(ChannelInfo.ChannelID), It.IsAny<List<int>>()))
            .Returns(query.Object);

        query.Setup(x => x.GetEnumerableTypedResultAsync())
            .ReturnsAsync(rows);

        return query;
    }

    private static ChannelInfo CreateChannel(int id, string displayName)
    {
        var channel = (FakeChannelInfo)RuntimeHelpers.GetUninitializedObject(typeof(FakeChannelInfo));
        channel.ChannelID = id;
        channel.ChannelDisplayName = displayName;
        return channel;
    }

    private static object? GetField(object target, string fieldName) =>
        typeof(ChannelDropdownProvider)
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(target);

    private sealed class FakeChannelInfo : ChannelInfo
    {
        private int channelID;
        private string channelDisplayName = string.Empty;

        public override int ChannelID
        {
            get => channelID;
            set => channelID = value;
        }

        public override string ChannelDisplayName
        {
            get => channelDisplayName;
            set => channelDisplayName = value;
        }
    }
}
