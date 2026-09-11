using BDO;
using BDO.GWT.Core.DTOs.Cookieboot;
using BDO.GWT.Web.Core.LiveSite.Components.ViewComponents;
using CMS.DataEngine;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Components.ViewComponents
{
    public class CookiebotTests
    {
        private Mock<IInfoProvider<GlobalsettingskeyInfo>> _providerMock;
        private Mock<IInfoProvider<WebChannelSnippetInfo>> _siteProviderMock;
        private Cookiebot _component;

        [SetUp]
        public void Setup()
        {
            _providerMock = new Mock<IInfoProvider<GlobalsettingskeyInfo>>();
            _siteProviderMock = new Mock<IInfoProvider<WebChannelSnippetInfo>>();

            _component = new Cookiebot(
                _providerMock.Object,
                _siteProviderMock.Object);
        }

        private static Mock<ObjectQuery<T>> CreateMockQuery<T>(List<T> items)
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

        [Test]
        public async Task InvokeAsync_UsesSnippetValues_WhenNoInheritFlagsSet()
        {
            var snippet = new TestWebChannelSnippetInfo
            {
                EnableCookiebotInheritfromglobal = false,
                EnableCookiebot = true,
                CookieIDInheritfromglobal = false,
                CookieID = "local-cookie",
                CustomCookiebotScriptInheritfromglobal = false,
                CustomCookiebotScript = "local-script"
            };

            _siteProviderMock.Setup(x => x.Get())
                .Returns(CreateMockQuery(new List<WebChannelSnippetInfo> { snippet }).Object);

            var result = await _component.InvokeAsync(CancellationToken.None) as ViewViewComponentResult;
            var model = result?.ViewData?.Model as CookiebotDTO;

            Assert.That(model, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(model!.EnableCookiebot, Is.True);
                Assert.That(model.CookieID, Is.EqualTo("local-cookie"));
                Assert.That(model.CustomCookiebotScript, Is.EqualTo("local-script"));
                _providerMock.Verify(x => x.Get(), Times.Never);
            });
        }

        [Test]
        public async Task InvokeAsync_UsesGlobalValues_WhenAllInheritFlagsSet()
        {
            var snippet = new TestWebChannelSnippetInfo
            {
                EnableCookiebotInheritfromglobal = true,
                EnableCookiebot = false,
                CookieIDInheritfromglobal = true,
                CookieID = "local-cookie",
                CustomCookiebotScriptInheritfromglobal = true,
                CustomCookiebotScript = "local-script"
            };
            var global = new TestGlobalSettingsInfo
            {
                EnableCookiebot = true,
                CookieID = "global-cookie",
                CustomCookiebotScript = "global-script"
            };

            _siteProviderMock.Setup(x => x.Get())
                .Returns(CreateMockQuery(new List<WebChannelSnippetInfo> { snippet }).Object);
            _providerMock.Setup(x => x.Get())
                .Returns(CreateMockQuery(new List<GlobalsettingskeyInfo> { global }).Object);

            var result = await _component.InvokeAsync(CancellationToken.None) as ViewViewComponentResult;
            var model = result?.ViewData?.Model as CookiebotDTO;

            Assert.That(model, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(model!.EnableCookiebot, Is.True);
                Assert.That(model.CookieID, Is.EqualTo("global-cookie"));
                Assert.That(model.CustomCookiebotScript, Is.EqualTo("global-script"));
                _providerMock.Verify(x => x.Get(), Times.Once);
            });
        }

        [Test]
        public async Task InvokeAsync_UsesMixedSources_PerFieldInheritFlags()
        {
            var snippet = new TestWebChannelSnippetInfo
            {
                EnableCookiebotInheritfromglobal = true,
                EnableCookiebot = false,
                CookieIDInheritfromglobal = false,
                CookieID = "local-cookie",
                CustomCookiebotScriptInheritfromglobal = true,
                CustomCookiebotScript = "local-script"
            };
            var global = new TestGlobalSettingsInfo
            {
                EnableCookiebot = true,
                CookieID = "global-cookie",
                CustomCookiebotScript = "global-script"
            };

            _siteProviderMock.Setup(x => x.Get())
                .Returns(CreateMockQuery(new List<WebChannelSnippetInfo> { snippet }).Object);
            _providerMock.Setup(x => x.Get())
                .Returns(CreateMockQuery(new List<GlobalsettingskeyInfo> { global }).Object);

            var result = await _component.InvokeAsync(CancellationToken.None) as ViewViewComponentResult;
            var model = result?.ViewData?.Model as CookiebotDTO;

            Assert.That(model, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(model!.EnableCookiebot, Is.True); // from global
                Assert.That(model.CookieID, Is.EqualTo("local-cookie")); // from snippet
                Assert.That(model.CustomCookiebotScript, Is.EqualTo("global-script")); // from global
            });
        }

        [Test]
        public async Task InvokeAsync_FallsBackToDefaults_WhenInheritingAndGlobalMissing()
        {
            var snippet = new TestWebChannelSnippetInfo
            {
                EnableCookiebotInheritfromglobal = true,
                EnableCookiebot = true,
                CookieIDInheritfromglobal = true,
                CookieID = "local-cookie",
                CustomCookiebotScriptInheritfromglobal = true,
                CustomCookiebotScript = "local-script"
            };

            _siteProviderMock.Setup(x => x.Get())
                .Returns(CreateMockQuery(new List<WebChannelSnippetInfo> { snippet }).Object);
            _providerMock.Setup(x => x.Get())
                .Returns(CreateMockQuery(new List<GlobalsettingskeyInfo>()).Object);

            var result = await _component.InvokeAsync(CancellationToken.None) as ViewViewComponentResult;
            var model = result?.ViewData?.Model as CookiebotDTO;

            Assert.That(model, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(model!.EnableCookiebot, Is.False);
                Assert.That(model.CookieID, Is.Null);
                Assert.That(model.CustomCookiebotScript, Is.Null);
            });
        }

        private sealed class TestWebChannelSnippetInfo : WebChannelSnippetInfo
        {
            public override bool EnableCookiebotInheritfromglobal { get; set; }
            public override bool EnableCookiebot { get; set; }
            public override bool CookieIDInheritfromglobal { get; set; }
            public override string CookieID { get; set; } = string.Empty;
            public override bool CustomCookiebotScriptInheritfromglobal { get; set; }
            public override string CustomCookiebotScript { get; set; } = string.Empty;
        }

        private sealed class TestGlobalSettingsInfo : GlobalsettingskeyInfo
        {
            public override bool EnableCookiebot { get; set; }
            public override string CookieID { get; set; } = string.Empty;
            public override string CustomCookiebotScript { get; set; } = string.Empty;
        }
    }
}