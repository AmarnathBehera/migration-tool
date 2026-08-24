using BDO.GWT.GLOBAL.Sync.Filters;
using BDO.GWT.GLOBAL.Sync.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Moq;

namespace BDO.GWT.GLOBAL.Sync.Tests.Filters;

[TestFixture]
public class ApiKeyAuthorizationFilterTests
{
    private const string ValidApiKey = "valid-api-key";

    private ApiKeyAuthorizationFilter _filter = null!;

    [SetUp]
    public void SetUp()
    {
        var options = global::Microsoft.Extensions.Options.Options.Create(new SyncAppOptions
        {
            SyncApiKey = ValidApiKey,
            Regions = Array.Empty<Region>(),
            GlobalSiteGuid = Guid.NewGuid(),
            RegionSitesCacheDuration = TimeSpan.FromHours(1),
        });

        _filter = new ApiKeyAuthorizationFilter(options);
    }

    [Test]
    public async Task OnAuthorizationAsync_WhenApiKeyHeaderMissing_ReturnsUnauthorized()
    {
        var context = CreateContext(apiKey: null);

        await _filter.OnAuthorizationAsync(context);

        Assert.That(context.Result, Is.InstanceOf<UnauthorizedResult>());
    }

    [Test]
    public async Task OnAuthorizationAsync_WhenApiKeyHeaderInvalid_ReturnsForbidden()
    {
        var context = CreateContext(apiKey: "wrong-key");

        await _filter.OnAuthorizationAsync(context);

        Assert.That(context.Result, Is.InstanceOf<StatusCodeResult>());
        Assert.That(((StatusCodeResult)context.Result!).StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
    }

    [Test]
    public async Task OnAuthorizationAsync_WhenApiKeyHeaderValid_DoesNotSetResult()
    {
        var context = CreateContext(apiKey: ValidApiKey);

        await _filter.OnAuthorizationAsync(context);

        Assert.That(context.Result, Is.Null);
    }

    [Test]
    public async Task OnAuthorizationAsync_WhenApiKeyHeaderEmpty_ReturnsForbidden()
    {
        var context = CreateContext(apiKey: string.Empty);

        await _filter.OnAuthorizationAsync(context);

        Assert.That(context.Result, Is.InstanceOf<StatusCodeResult>());
        Assert.That(((StatusCodeResult)context.Result!).StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
    }

    [Test]
    public void HeaderName_ApiKey_HasExpectedValue()
    {
        Assert.That(ApiKeyAuthorizationFilter.HeaderName.ApiKey, Is.EqualTo("X-API-Key"));
    }

    private static AuthorizationFilterContext CreateContext(string? apiKey)
    {
        var httpContext = new DefaultHttpContext();

        if (apiKey is not null)
        {
            httpContext.Request.Headers[ApiKeyAuthorizationFilter.HeaderName.ApiKey] = apiKey;
        }

        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor());

        return new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());
    }
}
