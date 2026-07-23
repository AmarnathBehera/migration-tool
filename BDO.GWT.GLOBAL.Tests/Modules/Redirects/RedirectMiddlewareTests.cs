using System.Data;
using System.Reflection;
using System.Runtime.CompilerServices;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Websites;
using CMS.Websites.Internal;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using XperienceCommunity.Redirects;
using XperienceCommunity.Redirects.Services;
using RedirectInfoX = XperienceCommunity.Redirects.RedirectInfo;

namespace BDO.GWT.GLOBAL.Tests.Modules.Redirects;

public class RedirectMiddlewareTests
{
    // ── structural ──────────────────────────────────────────────────────────

    [Test]
    public void Middleware_IsPublicClass()
    {
        Assert.That(typeof(RedirectMiddleware).IsPublic, Is.True);
    }

    [Test]
    public void Middleware_HasSinglePublicConstructor()
    {
        Assert.That(typeof(RedirectMiddleware).GetConstructors(), Has.Length.EqualTo(1));
    }

    [Test]
    public void Middleware_Constructor_HasThreeParameters()
    {
        Assert.That(typeof(RedirectMiddleware).GetConstructors().Single().GetParameters(), Has.Length.EqualTo(3));
    }

    [Test]
    public void Middleware_Constructor_Parameter0_IsRequestDelegate()
    {
        var param = typeof(RedirectMiddleware).GetConstructors().Single().GetParameters()[0];
        Assert.That(param.ParameterType, Is.EqualTo(typeof(RequestDelegate)));
    }

    [Test]
    public void Middleware_Constructor_Parameter1_IsIRedirectService()
    {
        var param = typeof(RedirectMiddleware).GetConstructors().Single().GetParameters()[1];
        Assert.That(param.ParameterType, Is.EqualTo(typeof(IRedirectService)));
    }

    [Test]
    public void Middleware_Constructor_Parameter2_IsIWebPageUrlRetriever()
    {
        var param = typeof(RedirectMiddleware).GetConstructors().Single().GetParameters()[2];
        Assert.That(param.ParameterType.Name, Is.EqualTo("IWebPageUrlRetriever"));
    }

    [Test]
    public void Middleware_HasInvokeAsyncMethod()
    {
        Assert.That(typeof(RedirectMiddleware).GetMethod(nameof(RedirectMiddleware.InvokeAsync)), Is.Not.Null);
    }

    [Test]
    public void Middleware_InvokeAsync_ReturnsTask()
    {
        var method = typeof(RedirectMiddleware).GetMethod(nameof(RedirectMiddleware.InvokeAsync))!;
        Assert.That(method.ReturnType, Is.EqualTo(typeof(Task)));
    }

    [Test]
    public void Middleware_InvokeAsync_IsPublic()
    {
        var method = typeof(RedirectMiddleware).GetMethod(nameof(RedirectMiddleware.InvokeAsync))!;
        Assert.That(method.IsPublic, Is.True);
    }

    [Test]
    public void Middleware_InvokeAsync_HasOneParameter_HttpContext()
    {
        var method = typeof(RedirectMiddleware).GetMethod(nameof(RedirectMiddleware.InvokeAsync))!;
        Assert.Multiple(() =>
        {
            Assert.That(method.GetParameters(), Has.Length.EqualTo(1));
            Assert.That(method.GetParameters()[0].ParameterType, Is.EqualTo(typeof(HttpContext)));
        });
    }

    // ── private fields ───────────────────────────────────────────────────────

    [Test]
    public void Middleware_HasPrivateField_Next()
    {
        var field = typeof(RedirectMiddleware)
            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
            .FirstOrDefault(f => f.FieldType == typeof(RequestDelegate));

        Assert.That(field, Is.Not.Null);
    }

    [Test]
    public void Middleware_HasPrivateField_RedirectService()
    {
        var field = typeof(RedirectMiddleware)
            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
            .FirstOrDefault(f => f.FieldType == typeof(IRedirectService));

        Assert.That(field, Is.Not.Null);
    }

    // ── InvokeAsync – non-html requests pass through ─────────────────────────

    [Test]
    public async Task InvokeAsync_NonHtmlRequest_CallsNext()
    {
        bool nextCalled = false;
        var middleware = MakeMiddleware(
            next: _ => { nextCalled = true; return Task.CompletedTask; },
            redirectService: EmptyRedirectService());

        var context = MakeContext("/some/path", accept: "application/json");

        await middleware.InvokeAsync(context);

        Assert.That(nextCalled, Is.True);
    }

    [Test]
    public async Task InvokeAsync_NonHtmlRequest_DoesNotSetRedirectResponse()
    {
        var middleware = MakeMiddleware(
            next: _ => Task.CompletedTask,
            redirectService: EmptyRedirectService());

        var context = MakeContext("/some/path", accept: "application/json");

        await middleware.InvokeAsync(context);

        Assert.That(context.Response.StatusCode, Is.Not.EqualTo(301));
    }

    // ── InvokeAsync – excluded paths pass through ────────────────────────────

    [Test]
    public async Task InvokeAsync_AdminPath_CallsNext()
    {
        bool nextCalled = false;
        var middleware = MakeMiddleware(
            next: _ => { nextCalled = true; return Task.CompletedTask; },
            redirectService: EmptyRedirectService());

        var context = MakeContext("/admin/dashboard");

        await middleware.InvokeAsync(context);

        Assert.That(nextCalled, Is.True);
    }

    [Test]
    public async Task InvokeAsync_CmsCtxPath_CallsNext()
    {
        bool nextCalled = false;
        var middleware = MakeMiddleware(
            next: _ => { nextCalled = true; return Task.CompletedTask; },
            redirectService: EmptyRedirectService());

        var context = MakeContext("/cmsctx/something");

        await middleware.InvokeAsync(context);

        Assert.That(nextCalled, Is.True);
    }

    [Test]
    public async Task InvokeAsync_GetMediaPath_CallsNext()
    {
        bool nextCalled = false;
        var middleware = MakeMiddleware(
            next: _ => { nextCalled = true; return Task.CompletedTask; },
            redirectService: EmptyRedirectService());

        var context = MakeContext("/getmedia/abc");

        await middleware.InvokeAsync(context);

        Assert.That(nextCalled, Is.True);
    }

    [Test]
    public async Task InvokeAsync_GetContentAssetPath_CallsNext()
    {
        bool nextCalled = false;
        var middleware = MakeMiddleware(
            next: _ => { nextCalled = true; return Task.CompletedTask; },
            redirectService: EmptyRedirectService());

        var context = MakeContext("/getcontentasset/123");

        await middleware.InvokeAsync(context);

        Assert.That(nextCalled, Is.True);
    }

    [Test]
    public async Task InvokeAsync_KenticoPath_CallsNext()
    {
        bool nextCalled = false;
        var middleware = MakeMiddleware(
            next: _ => { nextCalled = true; return Task.CompletedTask; },
            redirectService: EmptyRedirectService());

        var context = MakeContext("/kentico.something");

        await middleware.InvokeAsync(context);

        Assert.That(nextCalled, Is.True);
    }

    // ── InvokeAsync – no redirects configured ────────────────────────────────

    [Test]
    public async Task InvokeAsync_NoRedirectsConfigured_CallsNext()
    {
        bool nextCalled = false;
        var middleware = MakeMiddleware(
            next: _ => { nextCalled = true; return Task.CompletedTask; },
            redirectService: EmptyRedirectService());

        var context = MakeContext("/some/page");

        await middleware.InvokeAsync(context);

        Assert.That(nextCalled, Is.True);
    }

    [Test]
    public async Task InvokeAsync_NoRedirectsConfigured_DoesNotRedirect()
    {
        var middleware = MakeMiddleware(
            next: _ => Task.CompletedTask,
            redirectService: EmptyRedirectService());

        var context = MakeContext("/some/page");

        await middleware.InvokeAsync(context);

        Assert.That(context.Response.StatusCode, Is.Not.EqualTo(301));
    }

    // ── InvokeAsync – redirects configured but none match ────────────────────

    [Test]
    public async Task InvokeAsync_RedirectsConfiguredButNoneMatch_CallsNext()
    {
        bool nextCalled = false;
        var redirect = MakeRedirect(sourceUrl: "/a-different-path", targetGuid: Guid.NewGuid());
        var middleware = MakeMiddleware(
            next: _ => { nextCalled = true; return Task.CompletedTask; },
            redirectService: RedirectServiceWith(redirect));

        var context = MakeContext("/some/page");

        await middleware.InvokeAsync(context);

        Assert.That(nextCalled, Is.True);
    }

    [Test]
    public async Task InvokeAsync_RedirectsConfiguredButNoneMatch_DoesNotRedirect()
    {
        var redirect = MakeRedirect(sourceUrl: "/a-different-path", targetGuid: Guid.NewGuid());
        var middleware = MakeMiddleware(
            next: _ => Task.CompletedTask,
            redirectService: RedirectServiceWith(redirect));

        var context = MakeContext("/some/page");

        await middleware.InvokeAsync(context);

        Assert.That(context.Response.StatusCode, Is.Not.EqualTo(301));
    }

    // ── InvokeAsync – matching redirect resolves the target web page ──────────

    [Test]
    public async Task InvokeAsync_MatchingRedirect_IssuesPermanentRedirectToResolvedUrl()
    {
        KenticoContainerSetup.Reset();
        SetupWebPageItemId(42);
        SetupContentLanguages(MakeLanguage("en", "en-US", isDefault: true));

        var retriever = new Mock<IWebPageUrlRetriever>();
        retriever.Setup(r => r.Retrieve(42, "en"))
            .ReturnsAsync(new WebPageUrl("~/new-path", "https://example.org/new-path"));

        var redirect = MakeRedirect(sourceUrl: "/old-path", targetGuid: Guid.NewGuid());
        var context = MakeContext("/old-path");

        await MakeMiddleware(_ => Task.CompletedTask, RedirectServiceWith(redirect), retriever.Object)
            .InvokeAsync(context);

        Assert.Multiple(() =>
        {
            Assert.That(context.Response.StatusCode, Is.EqualTo(301));
            Assert.That(context.Response.Headers.Location.ToString(), Is.EqualTo("/new-path"));
        });
    }

    [Test]
    public async Task InvokeAsync_MatchingRedirect_WhenFirstSegmentMatchesLanguage_ResolvesInThatLanguage()
    {
        KenticoContainerSetup.Reset();
        SetupWebPageItemId(7);
        SetupContentLanguages(
            MakeLanguage("en", "en-US", isDefault: true),
            MakeLanguage("fr", "fr-FR", isDefault: false));

        var retriever = new Mock<IWebPageUrlRetriever>();
        retriever.Setup(r => r.Retrieve(7, "fr"))
            .ReturnsAsync(new WebPageUrl("/fr/target", "https://example.org/fr/target"));

        var redirect = MakeRedirect(sourceUrl: "/fr/source", targetGuid: Guid.NewGuid());
        var context = MakeContext("/fr/source");

        await MakeMiddleware(_ => Task.CompletedTask, RedirectServiceWith(redirect), retriever.Object)
            .InvokeAsync(context);

        Assert.Multiple(() =>
        {
            Assert.That(context.Response.StatusCode, Is.EqualTo(301));
        });
        retriever.Verify(r => r.Retrieve(7, "fr"), Times.Once);
    }

    [Test]
    public async Task InvokeAsync_MatchingRedirect_WhenFirstSegmentEmpty_ResolvesInDefaultLanguage()
    {
        KenticoContainerSetup.Reset();
        SetupWebPageItemId(9);
        SetupContentLanguages(MakeLanguage("en", "en-US", isDefault: true));

        var retriever = new Mock<IWebPageUrlRetriever>();
        retriever.Setup(r => r.Retrieve(9, "en"))
            .ReturnsAsync(new WebPageUrl("/home", "https://example.org/home"));

        var redirect = MakeRedirect(sourceUrl: "/", targetGuid: Guid.NewGuid());
        var context = MakeContext("/");

        await MakeMiddleware(_ => Task.CompletedTask, RedirectServiceWith(redirect), retriever.Object)
            .InvokeAsync(context);

        Assert.That(context.Response.StatusCode, Is.EqualTo(301));
    }

    [Test]
    public async Task InvokeAsync_MatchingRedirect_WhenResolvedUrlEqualsRequest_CallsNextWithoutRedirect()
    {
        KenticoContainerSetup.Reset();
        SetupWebPageItemId(5);
        SetupContentLanguages(MakeLanguage("en", "en-US", isDefault: true));

        var retriever = new Mock<IWebPageUrlRetriever>();
        retriever.Setup(r => r.Retrieve(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(new WebPageUrl("/old-path", "https://example.org/old-path"));

        bool nextCalled = false;
        var redirect = MakeRedirect(sourceUrl: "/old-path", targetGuid: Guid.NewGuid());
        var context = MakeContext("/old-path");

        await MakeMiddleware(_ => { nextCalled = true; return Task.CompletedTask; },
                RedirectServiceWith(redirect), retriever.Object)
            .InvokeAsync(context);

        Assert.Multiple(() =>
        {
            Assert.That(nextCalled, Is.True);
            Assert.That(context.Response.StatusCode, Is.Not.EqualTo(301));
        });
    }

    // ── extensions class ────────────────────────────────────────────────────

    [Test]
    public void Extensions_UseXperienceCommunityRedirects_MethodExists()
    {
        var method = typeof(RedirectMiddlewareExtensions).GetMethod(
            nameof(RedirectMiddlewareExtensions.UseXperienceCommunityRedirects));

        Assert.That(method, Is.Not.Null);
    }

    [Test]
    public void Extensions_UseXperienceCommunityRedirects_IsPublicStatic()
    {
        var method = typeof(RedirectMiddlewareExtensions).GetMethod(
            nameof(RedirectMiddlewareExtensions.UseXperienceCommunityRedirects))!;

        Assert.Multiple(() =>
        {
            Assert.That(method.IsPublic, Is.True);
            Assert.That(method.IsStatic, Is.True);
        });
    }

    [Test]
    public void Extensions_UseXperienceCommunityRedirects_AcceptsIApplicationBuilder()
    {
        var method = typeof(RedirectMiddlewareExtensions).GetMethod(
            nameof(RedirectMiddlewareExtensions.UseXperienceCommunityRedirects))!;

        Assert.That(method.GetParameters()[0].ParameterType, Is.EqualTo(typeof(IApplicationBuilder)));
    }

    [Test]
    public void Extensions_UseXperienceCommunityRedirects_ReturnsIApplicationBuilder()
    {
        var method = typeof(RedirectMiddlewareExtensions).GetMethod(
            nameof(RedirectMiddlewareExtensions.UseXperienceCommunityRedirects))!;

        Assert.That(method.ReturnType, Is.EqualTo(typeof(IApplicationBuilder)));
    }

    [Test]
    public void UseXperienceCommunityRedirects_AddsMiddlewareAndReturnsSameBuilder()
    {
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var appBuilder = new ApplicationBuilder(serviceProvider);

        var result = appBuilder.UseXperienceCommunityRedirects();

        Assert.That(result, Is.SameAs(appBuilder));
    }

    // ── helpers ─────────────────────────────────────────────────────────────

    private static RedirectMiddleware MakeMiddleware(
        RequestDelegate next,
        IRedirectService redirectService) =>
        new(next, redirectService, Mock.Of<CMS.Websites.IWebPageUrlRetriever>());

    private static RedirectMiddleware MakeMiddleware(
        RequestDelegate next,
        IRedirectService redirectService,
        IWebPageUrlRetriever webPageUrlRetriever) =>
        new(next, redirectService, webPageUrlRetriever);

    private static void SetupWebPageItemId(int id)
    {
        var query = new Mock<ObjectQuery<WebPageItemInfo>>();
        KenticoContainerSetup.WebPageItemProvider.Setup(p => p.Get()).Returns(query.Object);
        query.Setup(q => q.TopN(It.IsAny<int>())).Returns(query.Object);
        query.Setup(q => q.Column(It.IsAny<string>())).Returns(query.Object);
        query.Setup(q => q.WhereEquals(It.IsAny<string>(), It.IsAny<object>())).Returns(query.Object);
        query.Setup(q => q.GetScalarResult<int>(It.IsAny<int>())).Returns(id);
    }

    private static void SetupContentLanguages(params ContentLanguageInfo[] languages)
    {
        var query = new Mock<ObjectQuery<ContentLanguageInfo>>();
        KenticoContainerSetup.ContentLanguageProvider.Setup(p => p.Get()).Returns(query.Object);
        query.Setup(q => q.Columns(It.IsAny<string[]>())).Returns(query.Object);
        query.Setup(q => q.GetEnumerator())
            .Returns(() => ((IEnumerable<ContentLanguageInfo>)languages).GetEnumerator());
    }

    private static TestContentLanguageInfo MakeLanguage(string name, string culture, bool isDefault)
    {
        var lang = (TestContentLanguageInfo)RuntimeHelpers.GetUninitializedObject(typeof(TestContentLanguageInfo));
        lang.ContentLanguageName = name;
        lang.ContentLanguageCultureFormat = culture;
        lang.ContentLanguageIsDefault = isDefault;
        return lang;
    }

    private sealed class TestContentLanguageInfo : ContentLanguageInfo
    {
        public override string ContentLanguageName { get; set; } = string.Empty;
        public override string ContentLanguageCultureFormat { get; set; } = string.Empty;
        public override bool ContentLanguageIsDefault { get; set; }
    }

    private static IRedirectService EmptyRedirectService()
    {
        var mock = new Mock<IRedirectService>();
        mock.Setup(s => s.GetRedirects())
            .Returns(Task.FromResult<IReadOnlyCollection<RedirectInfoX>>(Array.Empty<RedirectInfoX>()));
        return mock.Object;
    }

    private static DefaultHttpContext MakeContext(string path, string accept = "text/html")
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Request.Headers.Accept = accept;
        return context;
    }

    private static IRedirectService RedirectServiceWith(params RedirectInfoX[] redirects)
    {
        var mock = new Mock<IRedirectService>();
        mock.Setup(s => s.GetRedirects())
            .Returns(Task.FromResult<IReadOnlyCollection<RedirectInfoX>>(redirects));
        return mock.Object;
    }

    private static FakeRedirectInfo MakeRedirect(string sourceUrl, Guid targetGuid)
    {
        var info = (FakeRedirectInfo)RuntimeHelpers.GetUninitializedObject(typeof(FakeRedirectInfo));
        info.RedirectSourceUrl = sourceUrl;
        info.RedirectTargetWebPageItemGUID = targetGuid;
        return info;
    }

    // Overrides the virtual data fields so reads bypass Kentico's AbstractInfoBase.GetValue,
    // which requires an initialised CMS IoC container.
    private sealed class FakeRedirectInfo : RedirectInfoX
    {
        private string? _sourceUrl;
        private Guid _targetGuid;

        public override string RedirectSourceUrl
        {
            get => _sourceUrl!;
            set => _sourceUrl = value;
        }

        public override Guid RedirectTargetWebPageItemGUID
        {
            get => _targetGuid;
            set => _targetGuid = value;
        }
    }
}
