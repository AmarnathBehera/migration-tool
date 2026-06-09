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
    public class MarketoMunchkinCodeTests
    {
        private Mock<IInfoProvider<GlobalsettingskeyInfo>> _globalProviderMock = null!;
        private Mock<IInfoProvider<WebChannelSnippetInfo>> _siteProviderMock = null!;
        private Mock<IWebsiteChannelContext> _websiteChannelContextMock = null!;
        private Mock<IInfoProvider<WebChannelSettingsInfo>> _webChannelSettingsProviderMock = null!;
        private MarketoMunchkinCode _component = null!;

        private const string ExpectedViewPath = "~/Views/Shared/Components/Scripts/Default.cshtml";

        [SetUp]
        public void Setup()
        {
            _globalProviderMock = new Mock<IInfoProvider<GlobalsettingskeyInfo>>();
            _siteProviderMock = new Mock<IInfoProvider<WebChannelSnippetInfo>>();
            _websiteChannelContextMock = new Mock<IWebsiteChannelContext>();
            _webChannelSettingsProviderMock = new Mock<IInfoProvider<WebChannelSettingsInfo>>();

            _component = new MarketoMunchkinCode(
                _globalProviderMock.Object,
                _siteProviderMock.Object,
                _websiteChannelContextMock.Object,
                _webChannelSettingsProviderMock.Object);
        }

        // ─── Helper: build a mocked ObjectQuery<T> that enumerates the given items ──
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

        private sealed class StubChannelSettings : WebChannelSettingsInfo
        {
            private readonly int _id;
            private readonly int _channelId;

            public override int WebChannelSettingsID => _id;
            public override int WebChannelSettingsChannelID => _channelId;

            internal StubChannelSettings(int id = 10, int channelId = 1)
            {
                _id = id;
                _channelId = channelId;
            }
        }

        private sealed class StubSiteScript : WebChannelSnippetInfo
        {
            private readonly bool _inherit;
            private readonly string _keyValue;

            public override bool MarketoMunchkinCodeInheritfromglobal => _inherit;
            public override string MarketoMunchkinCodeKeyValue => _keyValue;

            internal StubSiteScript(bool inherit, string keyValue = "")
            {
                _inherit = inherit;
                _keyValue = keyValue;
            }
        }

        private sealed class StubGlobalSettings : GlobalsettingskeyInfo
        {
            private readonly string _keyValue;

            public override string MarketoMunchkinCodeKeyValue => _keyValue;

            internal StubGlobalSettings(string keyValue) => _keyValue = keyValue;
        }

        // ── Factory helpers ─────────────────────────────────────────────────────────
        private static StubChannelSettings MakeChannelSettings(int settingsId, int channelId)
      => new StubChannelSettings(settingsId, channelId);

        private static StubSiteScript MakeSiteScript(
            bool inheritFromGlobal,
            string keyValue,
            int webChannelSettingsId = 10)
            => new StubSiteScript(inheritFromGlobal, keyValue);

        private static StubGlobalSettings MakeGlobalSettings(string keyValue)
            => new StubGlobalSettings(keyValue);

        // ════════════════════════════════════════════════════════════════════════════
        // BRANCH 1 — Channel settings record not found (currentChannelId == null)
        // ════════════════════════════════════════════════════════════════════════════


        [Test]
        public async Task InvokeAsync_WhenChannelNotFound_ReturnsDefaultView()
        {
            _websiteChannelContextMock.SetupGet(c => c.WebsiteChannelID).Returns(1);
            _webChannelSettingsProviderMock.Setup(p => p.Get())
                .Returns(SetupQueryReturning<WebChannelSettingsInfo>([]).Object);

            var result = await _component.InvokeAsync(CancellationToken.None) as ViewViewComponentResult;

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result!.ViewName, Does.Contain("Default.cshtml"));
                Assert.That(result.ViewData?.Model, Is.Null);
            });
        }

        [Test]
        public async Task InvokeAsync_WhenChannelNotFound_ReturnsExactScriptsDefaultViewPath()
        {
            _websiteChannelContextMock.SetupGet(c => c.WebsiteChannelID).Returns(1);
            _webChannelSettingsProviderMock.Setup(p => p.Get())
                .Returns(SetupQueryReturning<WebChannelSettingsInfo>([]).Object);

            var result = await _component.InvokeAsync(CancellationToken.None) as ViewViewComponentResult;

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result!.ViewName, Does.Contain("Scripts"));
                Assert.That(result.ViewName, Does.Contain("Default.cshtml"));
                Assert.That(result.ViewName, Is.EqualTo(ExpectedViewPath));
            });
        }

        [Test]
        public async Task InvokeAsync_WhenChannelNotFound_DoesNotCallSiteOrGlobalProvider()
        {
            _websiteChannelContextMock.SetupGet(c => c.WebsiteChannelID).Returns(1);
            _webChannelSettingsProviderMock.Setup(p => p.Get())
                .Returns(SetupQueryReturning<WebChannelSettingsInfo>([]).Object);

            await _component.InvokeAsync(CancellationToken.None);

            _siteProviderMock.Verify(p => p.Get(), Times.Never);
            _globalProviderMock.Verify(p => p.Get(), Times.Never);
        }

        // ════════════════════════════════════════════════════════════════════════════
        // BRANCH 2 — Channel found, site script not found → null model
        // ════════════════════════════════════════════════════════════════════════════

        [Test]
        public async Task InvokeAsync_WhenSiteScriptNotFound_ReturnsViewWithNullModel()
        {
            const int channelId = 1;
            const int settingsId = 10;

            _websiteChannelContextMock.SetupGet(c => c.WebsiteChannelID).Returns(channelId);

            _webChannelSettingsProviderMock.Setup(p => p.Get())
                .Returns(
                    SetupQueryReturning<WebChannelSettingsInfo>(
                        [MakeChannelSettings(settingsId, channelId)]
                    ).Object);

            _siteProviderMock.Setup(p => p.Get())
                .Returns(SetupQueryReturning<WebChannelSnippetInfo>([]).Object);

            var result = await _component.InvokeAsync(CancellationToken.None) as ViewViewComponentResult;

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result!.ViewName, Is.EqualTo(ExpectedViewPath));
                Assert.That(result.ViewData?.Model, Is.Null);
            });
        }

        [Test]
        public async Task InvokeAsync_WhenSiteScriptNotFound_DoesNotCallGlobalProvider()
        {
            const int channelId = 1;
            const int settingsId = 10;

            _websiteChannelContextMock.SetupGet(c => c.WebsiteChannelID).Returns(channelId);

            _webChannelSettingsProviderMock.Setup(p => p.Get())
                .Returns(
                    SetupQueryReturning<WebChannelSettingsInfo>(
                        [MakeChannelSettings(settingsId, channelId)]
                    ).Object);

            _siteProviderMock.Setup(p => p.Get())
                .Returns(SetupQueryReturning<WebChannelSnippetInfo>([]).Object);

            await _component.InvokeAsync(CancellationToken.None);

            _globalProviderMock.Verify(p => p.Get(), Times.Never);
        }

        // ════════════════════════════════════════════════════════════════════════════
        // BRANCH 3 — Site script found, InheritFromGlobal = false
        //            → value = sitescript.ScriptexecutedwhencookiesareacceptedKeyValue
        // ════════════════════════════════════════════════════════════════════════════

        [Test]
        public async Task InvokeAsync_WhenInheritFromGlobalFalse_ReturnsSiteScriptKeyValue()
        {
            const int channelId = 1;
            const int settingsId = 10;
            const string siteKeyValue = "script-cookies-accepted-Site-value";

            _websiteChannelContextMock.SetupGet(c => c.WebsiteChannelID).Returns(channelId);

            _webChannelSettingsProviderMock.Setup(p => p.Get())
                .Returns(
                    SetupQueryReturning<WebChannelSettingsInfo>(
                        [MakeChannelSettings(settingsId, channelId)]
                    ).Object);

            _siteProviderMock.Setup(p => p.Get())
                .Returns(
                    SetupQueryReturning<WebChannelSnippetInfo>(
                        [MakeSiteScript(false, siteKeyValue, settingsId)]
                    ).Object);

            var result = await _component.InvokeAsync(CancellationToken.None) as ViewViewComponentResult;

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result!.ViewName, Is.EqualTo(ExpectedViewPath));
                Assert.That(result.ViewData?.Model, Is.EqualTo(siteKeyValue));
            });
        }

        [Test]
        public async Task InvokeAsync_WhenInheritFromGlobalFalse_DoesNotCallGlobalProvider()
        {
            const int channelId = 1;
            const int settingsId = 10;

            _websiteChannelContextMock.SetupGet(c => c.WebsiteChannelID).Returns(channelId);

            _webChannelSettingsProviderMock.Setup(p => p.Get())
                .Returns(
                    SetupQueryReturning<WebChannelSettingsInfo>(
                        [MakeChannelSettings(settingsId, channelId)]
                    ).Object);

            _siteProviderMock.Setup(p => p.Get())
                .Returns(
                    SetupQueryReturning<WebChannelSnippetInfo>(
                        [MakeSiteScript(false, "site-value", settingsId)]
                    ).Object);

            await _component.InvokeAsync(CancellationToken.None);

            _globalProviderMock.Verify(p => p.Get(), Times.Never);
        }

        [Test]
        public async Task InvokeAsync_WhenInheritFromGlobalFalse_ViewPathIsCorrect()
        {
            const int channelId = 1;
            const int settingsId = 10;

            _websiteChannelContextMock.SetupGet(c => c.WebsiteChannelID).Returns(channelId);

            _webChannelSettingsProviderMock.Setup(p => p.Get())
                .Returns(
                    SetupQueryReturning<WebChannelSettingsInfo>(
                        [MakeChannelSettings(settingsId, channelId)]
                    ).Object);

            _siteProviderMock.Setup(p => p.Get())
                .Returns(
                    SetupQueryReturning<WebChannelSnippetInfo>(
                        [MakeSiteScript(false, "site-value", settingsId)]
                    ).Object);

            var result = await _component.InvokeAsync(CancellationToken.None) as ViewViewComponentResult;

            Assert.That(result!.ViewName, Is.EqualTo(ExpectedViewPath));
        }

        // ════════════════════════════════════════════════════════════════════════════
        // BRANCH 4 — Site script found, InheritFromGlobal = true
        //            → value = global provider's ScriptexecutedwhencookiesareacceptedKeyValue
        // ════════════════════════════════════════════════════════════════════════════

        [Test]
        public async Task InvokeAsync_WhenInheritFromGlobalTrue_ReturnsGlobalKeyValue()
        {
            const int channelId = 1;
            const int settingsId = 10;
            const string globalKeyValue = "global-script-cookies-accepted-Site-value";

            _websiteChannelContextMock.SetupGet(c => c.WebsiteChannelID).Returns(channelId);

            _webChannelSettingsProviderMock.Setup(p => p.Get())
                .Returns(
                    SetupQueryReturning<WebChannelSettingsInfo>(
                        [MakeChannelSettings(settingsId, channelId)]
                    ).Object);

            // FIX: inheritFromGlobal = true so the global branch is reached
            _siteProviderMock.Setup(p => p.Get())
                .Returns(
                    SetupQueryReturning<WebChannelSnippetInfo>(
                        [MakeSiteScript(true, "site-value", settingsId)]
                    ).Object);

            // FIX: global provider must be set up to return the expected value
            _globalProviderMock.Setup(p => p.Get())
                .Returns(
                    SetupQueryReturning<GlobalsettingskeyInfo>(
                        [MakeGlobalSettings(globalKeyValue)]
                    ).Object);

            var result = await _component.InvokeAsync(CancellationToken.None) as ViewViewComponentResult;

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result!.ViewName, Is.EqualTo(ExpectedViewPath));
                Assert.That(result.ViewData?.Model, Is.EqualTo(globalKeyValue));
            });
        }

        [Test]
        public async Task InvokeAsync_WhenInheritFromGlobalTrue_CallsGlobalProviderExactlyOnce()
        {
            const int channelId = 1;
            const int settingsId = 10;

            _websiteChannelContextMock.SetupGet(c => c.WebsiteChannelID).Returns(channelId);

            _webChannelSettingsProviderMock.Setup(p => p.Get())
                .Returns(
                    SetupQueryReturning<WebChannelSettingsInfo>(
                        [MakeChannelSettings(settingsId, channelId)]
                    ).Object);

            // FIX: inheritFromGlobal = true
            _siteProviderMock.Setup(p => p.Get())
                .Returns(
                    SetupQueryReturning<WebChannelSnippetInfo>(
                        [MakeSiteScript(true, "site-value", settingsId)]
                    ).Object);

            _globalProviderMock.Setup(p => p.Get())
                .Returns(
                    SetupQueryReturning<GlobalsettingskeyInfo>(
                        [MakeGlobalSettings("global-value")]
                    ).Object);

            // FIX: single invoke — Times.Once means called once per invocation
            await _component.InvokeAsync(CancellationToken.None);

            _globalProviderMock.Verify(p => p.Get(), Times.Once);
        }

        [Test]
        public async Task InvokeAsync_WhenInheritFromGlobalTrue_IgnoresSiteScriptKeyValue()
        {
            const int channelId = 1;
            const int settingsId = 10;
            const string siteKeyValue = "site-value-must-not-appear";
            const string globalValue = "global-value";

            _websiteChannelContextMock.SetupGet(c => c.WebsiteChannelID).Returns(channelId);

            _webChannelSettingsProviderMock.Setup(p => p.Get())
                .Returns(
                    SetupQueryReturning<WebChannelSettingsInfo>(
                        [MakeChannelSettings(settingsId, channelId)]
                    ).Object);

            // FIX: inheritFromGlobal = true, site key value must not appear in result
            _siteProviderMock.Setup(p => p.Get())
                .Returns(
                    SetupQueryReturning<WebChannelSnippetInfo>(
                        [MakeSiteScript(true, siteKeyValue, settingsId)]
                    ).Object);

            _globalProviderMock.Setup(p => p.Get())
                .Returns(
                    SetupQueryReturning<GlobalsettingskeyInfo>(
                        [MakeGlobalSettings(globalValue)]
                    ).Object);

            var result = await _component.InvokeAsync(CancellationToken.None) as ViewViewComponentResult;

            Assert.That(result!.ViewData?.Model, Is.Not.EqualTo(siteKeyValue));
        }

        // ════════════════════════════════════════════════════════════════════════════
        // BRANCH 5 — InheritFromGlobal = true, global provider returns no record
        //            → value resolves to null (?.ScriptexecutedwhencookiesareacceptedKeyValue)
        // ════════════════════════════════════════════════════════════════════════════

        [Test]
        public async Task InvokeAsync_WhenInheritFromGlobalTrue_AndGlobalProviderReturnsEmpty_ReturnsNullModel()
        {
            const int channelId = 1;
            const int settingsId = 10;

            _websiteChannelContextMock.SetupGet(c => c.WebsiteChannelID).Returns(channelId);

            _webChannelSettingsProviderMock.Setup(p => p.Get())
                .Returns(
                    SetupQueryReturning<WebChannelSettingsInfo>(
                        [MakeChannelSettings(settingsId, channelId)]
                    ).Object);

            // FIX: inheritFromGlobal = true so the global provider is consulted
            _siteProviderMock.Setup(p => p.Get())
                .Returns(
                    SetupQueryReturning<WebChannelSnippetInfo>(
                        [MakeSiteScript(true, "site-value", settingsId)]
                    ).Object);

            // Global provider returns empty — model should be null
            _globalProviderMock.Setup(p => p.Get())
                .Returns(SetupQueryReturning<GlobalsettingskeyInfo>([]).Object);

            var result = await _component.InvokeAsync(CancellationToken.None) as ViewViewComponentResult;

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result!.ViewName, Is.EqualTo(ExpectedViewPath));
                Assert.That(result.ViewData?.Model, Is.Null);
            });
        }

        // ════════════════════════════════════════════════════════════════════════════
        // QUERY FILTER / TopN verification
        // ════════════════════════════════════════════════════════════════════════════

        [Test]
        public async Task InvokeAsync_FiltersChannelByCurrentWebsiteChannelID()
        {
            _websiteChannelContextMock.SetupGet(c => c.WebsiteChannelID).Returns(42);
            var channelQuery = SetupQueryReturning<WebChannelSettingsInfo>([]);
            _webChannelSettingsProviderMock.Setup(p => p.Get()).Returns(channelQuery.Object);

            await _component.InvokeAsync(CancellationToken.None);

            channelQuery.Verify(q => q.WhereEquals(
                nameof(WebChannelSettingsInfo.WebChannelSettingsChannelID),
                It.IsAny<object>()), Times.AtLeastOnce);
        }

        [Test]
        public async Task InvokeAsync_FiltersSiteScriptByWebChannelSettingsID()
        {
            const int channelId = 1;
            const int settingsId = 10;

            _websiteChannelContextMock.SetupGet(c => c.WebsiteChannelID).Returns(channelId);

            _webChannelSettingsProviderMock.Setup(p => p.Get())
                .Returns(
                    SetupQueryReturning<WebChannelSettingsInfo>(
                        [MakeChannelSettings(settingsId, channelId)]
                    ).Object);

            // FIX: set up siteQuery before invoking, so the verification captures the actual call
            var siteQuery = SetupQueryReturning<WebChannelSnippetInfo>([]);
            _siteProviderMock.Setup(p => p.Get()).Returns(siteQuery.Object);

            await _component.InvokeAsync(CancellationToken.None);

            siteQuery.Verify(q => q.WhereEquals(
                nameof(WebChannelSnippetInfo.WebChannelSnippetWebChannelSettingsID),
                It.IsAny<object>()), Times.AtLeastOnce);
        }

        [Test]
        public async Task InvokeAsync_AppliesTopN1ToSiteScriptQuery()
        {
            const int channelId = 1;
            const int settingsId = 10;

            _websiteChannelContextMock.SetupGet(c => c.WebsiteChannelID).Returns(channelId);

            _webChannelSettingsProviderMock.Setup(p => p.Get())
                .Returns(
                    SetupQueryReturning<WebChannelSettingsInfo>(
                        [MakeChannelSettings(settingsId, channelId)]
                    ).Object);

            var siteQuery = SetupQueryReturning<WebChannelSnippetInfo>([]);
            _siteProviderMock.Setup(p => p.Get()).Returns(siteQuery.Object);

            await _component.InvokeAsync(CancellationToken.None);

            siteQuery.Verify(q => q.TopN(1), Times.AtLeastOnce);
        }

        [Test]
        public async Task InvokeAsync_AppliesTopN1ToGlobalProviderQuery()
        {
            const int channelId = 1;
            const int settingsId = 10;

            _websiteChannelContextMock.SetupGet(c => c.WebsiteChannelID).Returns(channelId);

            _webChannelSettingsProviderMock.Setup(p => p.Get())
                .Returns(
                    SetupQueryReturning<WebChannelSettingsInfo>(
                        [MakeChannelSettings(settingsId, channelId)]
                    ).Object);

            // FIX: inheritFromGlobal = true so the global query is actually executed
            _siteProviderMock.Setup(p => p.Get())
                .Returns(
                    SetupQueryReturning<WebChannelSnippetInfo>(
                        [MakeSiteScript(true, "any", settingsId)]
                    ).Object);

            var globalQuery = SetupQueryReturning<GlobalsettingskeyInfo>(
                [MakeGlobalSettings("global-key")]);

            _globalProviderMock.Setup(p => p.Get())
                .Returns(globalQuery.Object);

            await _component.InvokeAsync(CancellationToken.None);

            globalQuery.Verify(q => q.TopN(1), Times.AtLeastOnce);
        }

        // ════════════════════════════════════════════════════════════════════════════
        // General / parametric
        // ════════════════════════════════════════════════════════════════════════════

        [TestCase(1)]
        [TestCase(5)]
        [TestCase(99)]
        public async Task InvokeAsync_WithDifferentWebsiteChannelIDs_AlwaysReturnsViewViewComponentResult(int channelId)
        {
            _websiteChannelContextMock.SetupGet(c => c.WebsiteChannelID).Returns(channelId);
            _webChannelSettingsProviderMock.Setup(p => p.Get())
                .Returns(SetupQueryReturning<WebChannelSettingsInfo>([]).Object);

            var result = await _component.InvokeAsync(CancellationToken.None);

            Assert.That(result, Is.TypeOf<ViewViewComponentResult>());
        }

        [Test]
        public async Task InvokeAsync_WhenCancellationTokenProvided_StillReturnsResult()
        {
            _websiteChannelContextMock.SetupGet(c => c.WebsiteChannelID).Returns(1);
            _webChannelSettingsProviderMock.Setup(p => p.Get())
                .Returns(SetupQueryReturning<WebChannelSettingsInfo>([]).Object);

            using var cts = new CancellationTokenSource();
            var result = await _component.InvokeAsync(cts.Token);

            Assert.That(result, Is.TypeOf<ViewViewComponentResult>());
        }

        [Test]
        public async Task InvokeAsync_AlwaysReturnsViewViewComponentResult()
        {
            _websiteChannelContextMock.SetupGet(c => c.WebsiteChannelID).Returns(1);
            _webChannelSettingsProviderMock.Setup(p => p.Get())
                .Returns(SetupQueryReturning<WebChannelSettingsInfo>([]).Object);

            var result = await _component.InvokeAsync(CancellationToken.None);

            Assert.That(result, Is.TypeOf<ViewViewComponentResult>());
        }
    }
}