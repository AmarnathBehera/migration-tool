using BDO;
using BDO.GWT.Common;
using BDO.GWT.Xperience;
using CMS.DataEngine;
using CMS.Websites.Routing;
using GWTXbyKWeb.Components.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Components.ViewComponents
{
    public class ScriptsBottomBodyTests
    {
        private Mock<IInfoProvider<GlobalsettingskeyInfo>> _globalProviderMock = null!;
        private Mock<IInfoProvider<WebChannelSnippetInfo>> _siteProviderMock = null!;
        private Mock<IWebsiteContext> _websiteContextMock = null!;
        private Mock<IWebsiteChannelContext> _websiteChannelContextMock = null!;
        private Mock<IInfoProvider<WebChannelSettingsInfo>> _webChannelSettingsProviderMock = null!;
        private ScriptsBottomBody _component = null!;

        [SetUp]
        public void Setup()
        {
            _globalProviderMock = new Mock<IInfoProvider<GlobalsettingskeyInfo>>();
            _siteProviderMock = new Mock<IInfoProvider<WebChannelSnippetInfo>>();
            _websiteContextMock = new Mock<IWebsiteContext>();
            _websiteChannelContextMock = new Mock<IWebsiteChannelContext>();
            _webChannelSettingsProviderMock = new Mock<IInfoProvider<WebChannelSettingsInfo>>();

            _component = new ScriptsBottomBody(
                _globalProviderMock.Object,
                _siteProviderMock.Object,
                _websiteContextMock.Object,
                _websiteChannelContextMock.Object,
                _webChannelSettingsProviderMock.Object);
        }

        private static Mock<ObjectQuery<T>> SetupQueryReturning<T>(IEnumerable<T> items)
            where T : AbstractInfo<T, IInfoProvider<T>>, new()
        {
            var itemList = items.ToList();
            var mockQuery = new Mock<ObjectQuery<T>>();

            mockQuery.Setup(q => q.WhereEquals(It.IsAny<string>(), It.IsAny<object>()))
                     .Returns(mockQuery.Object);
            mockQuery.Setup(q => q.TopN(It.IsAny<int>()))
                     .Returns(mockQuery.Object);

            mockQuery.As<IEnumerable<T>>()
                     .Setup(q => q.GetEnumerator())
                     .Returns(() => itemList.GetEnumerator());

            mockQuery.Setup(q => q.GetEnumerableTypedResult(
                         It.IsAny<System.Data.CommandBehavior>(),
                         It.IsAny<bool>()))
                     .Returns(itemList);

            return mockQuery;
        }

        

        [Test]
        public async Task InvokeAsync_WhenChannelNotFound_ReturnsDefaultView()
        {
            _websiteChannelContextMock.SetupGet(c => c.WebsiteChannelID).Returns(1);

            var channelQuery = SetupQueryReturning<WebChannelSettingsInfo>([]);
            _webChannelSettingsProviderMock.Setup(p => p.Get()).Returns(channelQuery.Object);

            var result = await _component.InvokeAsync(CancellationToken.None) as ViewViewComponentResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.ViewName, Does.Contain("Default.cshtml"));
            Assert.That(result.ViewData?.Model, Is.Null);
        }

       
        [Test]
        public async Task InvokeAsync_AlwaysReturnsViewViewComponentResult()
        {
            _websiteChannelContextMock.SetupGet(c => c.WebsiteChannelID).Returns(1);

            var channelQuery = SetupQueryReturning<WebChannelSettingsInfo>([]);
            _webChannelSettingsProviderMock.Setup(p => p.Get()).Returns(channelQuery.Object);

            var result = await _component.InvokeAsync(CancellationToken.None);

            Assert.That(result, Is.TypeOf<ViewViewComponentResult>());
        }
    }
}