using BDO.GWT.Core.DTOs.Cookieboot;
using BDO.GWT.Web.Core.LiveSite.Components.ViewComponents;
using CMS.DataEngine;
using CMS.Websites.Routing;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Components.ViewComponents
{
    public class CookiebotTests
    {
        private Mock<IInfoProvider<WebChannelSettingsInfo>> _webChannelSettingsInfoProviderMock;
        private Mock<IWebsiteChannelContext> _websiteChannelContextMock;
        private Mock<IInfoProvider<GlobalsettingskeyInfo>> _providerMock;
        private Mock<IInfoProvider<WebChannelSnippetInfo>> _siteProviderMock;
        private Cookiebot _component;

        [SetUp]
        public void Setup()
        {
            _webChannelSettingsInfoProviderMock = new Mock<IInfoProvider<WebChannelSettingsInfo>>();
            _websiteChannelContextMock = new Mock<IWebsiteChannelContext>();
            _providerMock = new Mock<IInfoProvider<GlobalsettingskeyInfo>>();
            _siteProviderMock = new Mock<IInfoProvider<WebChannelSnippetInfo>>();

            _component = new Cookiebot(
                _webChannelSettingsInfoProviderMock.Object,
                _websiteChannelContextMock.Object,
                _providerMock.Object,
                _siteProviderMock.Object);
        }

        private static Mock<ObjectQuery<T>> CreateMockQuery<T>(IEnumerable<T> items)
            where T : BaseInfo
        {
            var mockQuery = new Mock<ObjectQuery<T>>();

            mockQuery.Setup(x => x.TopN(It.IsAny<int>()))
                     .Returns(mockQuery.Object);

            mockQuery.As<IEnumerable<T>>()
                     .Setup(x => x.GetEnumerator())
                     .Returns(() => items.GetEnumerator());

            return mockQuery;
        }

        [Test]
        public async Task InvokeAsync_ReturnsDefaultDto_WhenNoData()
        {
            var snippetQuery = CreateMockQuery(new List<WebChannelSnippetInfo>());

            _siteProviderMock.Setup(x => x.Get())
                             .Returns(snippetQuery.Object);

            var result = await _component.InvokeAsync(CancellationToken.None) as ViewViewComponentResult;

            Assert.That(result, Is.Not.Null);

            var model = result?.ViewData?.Model as CookiebotDTO;

            Assert.That(model, Is.Not.Null);
            Assert.That(model.EnableCookiebot, Is.False);
            Assert.That(model.CookieID, Is.Null);
            Assert.That(model.CustomCookiebotScript, Is.Null);
        }
    }
}