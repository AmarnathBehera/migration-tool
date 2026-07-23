using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
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

    // ── ExcludedStartingPaths field ──────────────────────────────────────────

    [Test]
    public void Middleware_HasExcludedStartingPathsField()
    {
        var field = typeof(RedirectMiddleware)
            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .FirstOrDefault(f => f.FieldType == typeof(string[]));

        Assert.That(field, Is.Not.Null);
    }

    [Test]
    public void Middleware_ExcludedStartingPaths_ContainsAdmin()
    {
        var middleware = MakeMiddleware(
            next: _ => Task.CompletedTask,
            redirectService: EmptyRedirectService());

        var field = typeof(RedirectMiddleware)
            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .Single(f => f.FieldType == typeof(string[]));
        var paths = (string[])field.GetValue(middleware)!;

        Assert.That(paths, Does.Contain("/admin"));
    }

    [Test]
    public void Middleware_ExcludedStartingPaths_ContainsCmsctx()
    {
        var middleware = MakeMiddleware(
            next: _ => Task.CompletedTask,
            redirectService: EmptyRedirectService());

        var field = typeof(RedirectMiddleware)
            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .Single(f => f.FieldType == typeof(string[]));
        var paths = (string[])field.GetValue(middleware)!;

        Assert.That(paths, Does.Contain("/cmsctx"));
    }

    [Test]
    public void Middleware_ExcludedStartingPaths_ContainsGetmedia()
    {
        var middleware = MakeMiddleware(
            next: _ => Task.CompletedTask,
            redirectService: EmptyRedirectService());

        var field = typeof(RedirectMiddleware)
            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .Single(f => f.FieldType == typeof(string[]));
        var paths = (string[])field.GetValue(middleware)!;

        Assert.That(paths, Does.Contain("/getmedia"));
    }

    [Test]
    public void Middleware_ExcludedStartingPaths_ContainsGetcontentasset()
    {
        var middleware = MakeMiddleware(
            next: _ => Task.CompletedTask,
            redirectService: EmptyRedirectService());

        var field = typeof(RedirectMiddleware)
            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .Single(f => f.FieldType == typeof(string[]));
        var paths = (string[])field.GetValue(middleware)!;

        Assert.That(paths, Does.Contain("/getcontentasset"));
    }

    // ── InvokeAsync – matched redirect but same source/target ─────────────────

    [Test]
    public async Task InvokeAsync_HtmlRequest_WithNoMatchingRedirect_CallsNext()
    {
        bool nextCalled = false;
        var redirect = MakeRedirectInfo("/source", Guid.NewGuid());
        var redirectService = SingleRedirectService(redirect);

        var middleware = MakeMiddleware(
            next: _ => { nextCalled = true; return Task.CompletedTask; },
            redirectService: redirectService);

        var context = MakeContext("/unrelated/path");

        await middleware.InvokeAsync(context);

        Assert.That(nextCalled, Is.True);
    }

    [Test]
    public async Task InvokeAsync_HtmlRequest_PathCaseInsensitiveNoMatch_CallsNext()
    {
        bool nextCalled = false;
        var redirect = MakeRedirectInfo("/different/path", Guid.NewGuid());
        var redirectService = SingleRedirectService(redirect);

        var middleware = MakeMiddleware(
            next: _ => { nextCalled = true; return Task.CompletedTask; },
            redirectService: redirectService);

        var context = MakeContext("/my/path");

        await middleware.InvokeAsync(context);

        Assert.That(nextCalled, Is.True);
    }

    // ── InvokeAsync – accept header variations ───────────────────────────────

    [Test]
    public async Task InvokeAsync_TextHtmlAccept_IsProcessed()
    {
        bool nextCalled = false;

        var middleware = MakeMiddleware(
            next: _ => { nextCalled = true; return Task.CompletedTask; },
            redirectService: EmptyRedirectService());

        var context = MakeContext("/some/page", accept: "text/html");

        await middleware.InvokeAsync(context);

        Assert.That(nextCalled, Is.True);
    }

    [Test]
    public async Task InvokeAsync_ApplicationXmlAccept_CallsNext()
    {
        bool nextCalled = false;

        var middleware = MakeMiddleware(
            next: _ => { nextCalled = true; return Task.CompletedTask; },
            redirectService: EmptyRedirectService());

        var context = MakeContext("/some/page", accept: "application/xml");

        await middleware.InvokeAsync(context);

        Assert.That(nextCalled, Is.True);
    }

    [Test]
    public async Task InvokeAsync_EmptyAcceptHeader_CallsNext()
    {
        bool nextCalled = false;

        var middleware = MakeMiddleware(
            next: _ => { nextCalled = true; return Task.CompletedTask; },
            redirectService: EmptyRedirectService());

        var context = MakeContext("/some/page", accept: "");

        await middleware.InvokeAsync(context);

        Assert.That(nextCalled, Is.True);
    }

    // ── RedirectMiddlewareExtensions – additional ────────────────────────────

    [Test]
    public void Extensions_UseXperienceCommunityRedirects_IsExtensionMethod()
    {
        var method = typeof(RedirectMiddlewareExtensions).GetMethod(
            nameof(RedirectMiddlewareExtensions.UseXperienceCommunityRedirects))!;

        Assert.That(method.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false), Is.True);
    }

    [Test]
    public void Extensions_TypeIsPublicStaticClass()
    {
        Assert.Multiple(() =>
        {
            Assert.That(typeof(RedirectMiddlewareExtensions).IsPublic, Is.True);
            Assert.That(typeof(RedirectMiddlewareExtensions).IsAbstract, Is.True); // static classes are abstract+sealed
            Assert.That(typeof(RedirectMiddlewareExtensions).IsSealed, Is.True);
        });
    }

    // ── helpers (private) ────────────────────────────────────────────────────

    private static RedirectInfoX MakeRedirectInfo(string sourceUrl, Guid targetGuid)
    {
        var info = (FakeRedirectInfoX)FormatterServices.GetUninitializedObject(typeof(FakeRedirectInfoX));
        info.RedirectSourceUrl = sourceUrl;
        info.RedirectTargetWebPageItemGUID = targetGuid;
        return info;
    }

    private static IRedirectService SingleRedirectService(RedirectInfoX redirect)
    {
        var mock = new Mock<IRedirectService>();
        mock.Setup(s => s.GetRedirects())
            .Returns(Task.FromResult<IReadOnlyCollection<RedirectInfoX>>(new[] { redirect }));
        return mock.Object;
    }

    private sealed class FakeRedirectInfoX : RedirectInfoX
    {
        private string? _sourceUrl;
        private Guid _targetGuid;

        public override string RedirectSourceUrl
        {
            get => _sourceUrl ?? string.Empty;
            set => _sourceUrl = value;
        }

        public override Guid RedirectTargetWebPageItemGUID
        {
            get => _targetGuid;
            set => _targetGuid = value;
        }
    }

    // ── helpers ─────────────────────────────────────────────────────────────

    private static RedirectMiddleware MakeMiddleware(
        RequestDelegate next,
        IRedirectService redirectService) =>
        new(next, redirectService, Mock.Of<CMS.Websites.IWebPageUrlRetriever>());

    private static IRedirectService EmptyRedirectService()
    {
        var mock = new Mock<IRedirectService>();
        mock.Setup(s => s.GetRedirects())
            .Returns(Task.FromResult<IReadOnlyCollection<RedirectInfoX>>(Array.Empty<RedirectInfoX>()));
        return mock.Object;
    }

    private static HttpContext MakeContext(string path, string accept = "text/html")
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Request.Headers.Accept = accept;
        return context;
    }
}
