using CMS.DataEngine;
using CMS.Websites;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Components.ViewComponents;

/// <summary>
/// Shared test scaffolding for <c>ScriptViewComponentBase</c> derivatives.
/// Keeps a single copy of the <see cref="ObjectQuery{T}"/> mock setup so that a
/// change to the component's query pipeline only needs one edit.
/// </summary>
internal static class ScriptQueryMockFactory
{
    /// <summary>
    /// Builds a mocked <see cref="ObjectQuery{T}"/> that returns <paramref name="items"/>
    /// from both the sync and async materialisation methods.
    /// </summary>
    internal static Mock<ObjectQuery<T>> SetupQueryReturning<T>(IEnumerable<T> items)
        where T : AbstractInfo<T, IInfoProvider<T>>, new()
    {
        var itemList = items.ToList();
        var mockQuery = new Mock<ObjectQuery<T>>();

        // ── fluent builders: return the same mock so chaining works ──────────────
        mockQuery.Setup(q => q.WhereEquals(It.IsAny<string>(), It.IsAny<object>()))
                 .Returns(mockQuery.Object);
        mockQuery.Setup(q => q.TopN(It.IsAny<int>()))
                 .Returns(mockQuery.Object);
        mockQuery.Setup(q => q.Columns(It.IsAny<string[]>()))
                 .Returns(mockQuery.Object);

        // ── enumeration ──────────────────────────────────────────────────────────
        mockQuery.As<IEnumerable<T>>()
                 .Setup(q => q.GetEnumerator())
                 .Returns(() => itemList.GetEnumerator());

        // sync materialisation (site-script and global-settings queries)
        mockQuery.Setup(q => q.GetEnumerableTypedResult(
                     It.IsAny<System.Data.CommandBehavior>(),
                     It.IsAny<bool>()))
                 .Returns(itemList);

        // async materialisation (website-channel lookup and settings query)
        mockQuery.Setup(q => q.GetEnumerableTypedResultAsync(
                     It.IsAny<System.Data.CommandBehavior>(),
                     It.IsAny<bool>(),
                     It.IsAny<CancellationToken>()))
                 .ReturnsAsync(itemList);

        return mockQuery;
    }

    /// <summary>
    /// Stubs the <c>IInfoProvider&lt;WebsiteChannelInfo&gt;</c> lookup that translates
    /// <c>CMS_WebsiteChannel.WebsiteChannelID</c> into <c>CMS_Channel.ChannelID</c>.
    /// </summary>
    internal static void SetupWebsiteChannel(
        this Mock<IInfoProvider<WebsiteChannelInfo>> providerMock,
        int websiteChannelId,
        int channelId) =>
        providerMock
            .Setup(p => p.Get())
            .Returns(SetupQueryReturning<WebsiteChannelInfo>(
                [new StubWebsiteChannel(websiteChannelId, channelId)]).Object);

    /// <summary>
    /// Overrides the virtual ID properties so no CMS IoC initialisation is triggered.
    /// </summary>
    internal sealed class StubWebsiteChannel : WebsiteChannelInfo
    {
        private readonly int _websiteChannelId;
        private readonly int _channelId;

        public override int WebsiteChannelID => _websiteChannelId;
        public override int WebsiteChannelChannelID => _channelId;

        internal StubWebsiteChannel(int websiteChannelId, int channelId)
        {
            _websiteChannelId = websiteChannelId;
            _channelId = channelId;
        }
    }
}
