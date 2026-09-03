using BDO;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.LiveSite.Components.ViewComponents;
using BDO.GWT.Web.Core.LiveSite.Services;
using CMS.DataEngine;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Components.ViewComponents
{
    [TestFixture]
    public class CookiebannerTests
    {
        private Mock<ICookieBannerService> _cookieBannerServiceMock = null!;
        private Mock<IInfoProvider<GlobalsettingskeyInfo>> _providerMock = null!;
        private Mock<IInfoProvider<WebChannelSnippetInfo>> _siteProviderMock = null!;
        private Cookiebanner _component = null!;

        [SetUp]
        public void Setup()
        {
            _cookieBannerServiceMock = new Mock<ICookieBannerService>();
            _providerMock = new Mock<IInfoProvider<GlobalsettingskeyInfo>>();
            _siteProviderMock = new Mock<IInfoProvider<WebChannelSnippetInfo>>();

            _component = new Cookiebanner(
                _cookieBannerServiceMock.Object,
                _providerMock.Object,
                _siteProviderMock.Object);
        }

        private static Mock<ObjectQuery<T>> BuildQuery<T>(List<T> items)
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
        public async Task InvokeAsync_ReturnsBannerView_WhenNoChannelSetting()
        {
            // channelSetting == null ? enableCookiebot false ? banner view is rendered.
            _siteProviderMock.Setup(x => x.Get())
                             .Returns(BuildQuery(new List<WebChannelSnippetInfo>()).Object);

            var dto = new CookieBannerDto { DefaultCookieValue = "All" };
            _cookieBannerServiceMock
                .Setup(x => x.GetAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            var result = await _component.InvokeAsync(CancellationToken.None) as ViewViewComponentResult;

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result!.ViewName, Is.EqualTo("~/Views/Shared/Components/Cookiebanner/Default.cshtml"));
                Assert.That(result.ViewData?.Model, Is.SameAs(dto));
            });
        }

        [Test]
        public async Task InvokeAsync_CallsCookieBannerService_WhenCookiebotDisabled()
        {
            _siteProviderMock.Setup(x => x.Get())
                             .Returns(BuildQuery(new List<WebChannelSnippetInfo>()).Object);

            _cookieBannerServiceMock
                .Setup(x => x.GetAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CookieBannerDto());

            await _component.InvokeAsync(CancellationToken.None);

            _cookieBannerServiceMock.Verify(x => x.GetAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task InvokeAsync_PassesCancellationToken_ToCookieBannerService()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            _siteProviderMock.Setup(x => x.Get())
                             .Returns(BuildQuery(new List<WebChannelSnippetInfo>()).Object);
            _cookieBannerServiceMock
                .Setup(x => x.GetAsync(token))
                .ReturnsAsync(new CookieBannerDto());

            await _component.InvokeAsync(token);

            _cookieBannerServiceMock.Verify(x => x.GetAsync(token), Times.Once);
        }

        [Test]
        public void InvokeAsync_PropagatesException_FromSiteProvider()
        {
            _siteProviderMock.Setup(x => x.Get())
                             .Throws(new InvalidOperationException("db error"));

            Assert.ThrowsAsync<InvalidOperationException>(
                () => _component.InvokeAsync(CancellationToken.None));
        }
    }
}
