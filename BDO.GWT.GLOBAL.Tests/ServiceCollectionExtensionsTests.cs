using BDO.GWT.Common;
using BDO.GWT.Common.Options;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.Services.Header;
using BDO.GWT.GLOBAL.Components.FormComponents.CustomDropdown;
using BDO.GWT.GLOBAL.Services;
using BDO.GWT.Web.Core.LiveSite.Services;
using BDO.GWT.Web.Core.Services;
using BDO.GWT.Web.Services;
using BDO.GWT.Xperience;
using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BDO.GWT.GLOBAL.Tests;

public class ServiceCollectionExtensionsTests
{
    private IServiceCollection _services = null!;

    [SetUp]
    public void Setup()
    {
        _services = new ServiceCollection();

        // Add a minimal configuration so BindConfiguration calls don't fail
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Caching:Enabled"] = "true",
                ["Caching:React:Enabled"] = "true",
                ["React:BaseUrl"] = "http://localhost"
            })
            .Build();

        _services.AddSingleton<IConfiguration>(configuration);
    }

    [Test]
    public void AddXperience_RegistersOptionsConfigurations()
    {
        _services.AddXperience();

        Assert.Multiple(() =>
        {
            Assert.That(_services.Any(sd => sd.ServiceType.IsGenericType &&
                sd.ServiceType.GetGenericArguments().FirstOrDefault() == typeof(CachingOptions)),
                Is.True, "CachingOptions should be configured");

            Assert.That(_services.Any(sd => sd.ServiceType.IsGenericType &&
                sd.ServiceType.GetGenericArguments().FirstOrDefault() == typeof(ReactCachingOptions)),
                Is.True, "ReactCachingOptions should be configured");

            Assert.That(_services.Any(sd => sd.ServiceType.IsGenericType &&
                sd.ServiceType.GetGenericArguments().FirstOrDefault() == typeof(ReactOptions)),
                Is.True, "ReactOptions should be configured");
        });
    }

    [Test]
    public void AddXperience_RegistersHttpClient()
    {
        _services.AddXperience();

        Assert.That(_services.Any(sd => sd.ServiceType == typeof(IHttpClientFactory)), Is.True);
    }

    [Test]
    [TestCase(typeof(IPageContext), typeof(XperiencePageContext))]
    [TestCase(typeof(IWebsiteContext), typeof(WebsiteContext))]
    [TestCase(typeof(IPageBuilderSectionService), typeof(PageBuilderSectionService))]
    [TestCase(typeof(IChannelAccessService), typeof(ChannelAccessService))]
    [TestCase(typeof(IMicrositeMasterPageService), typeof(MicrositeMasterPageService))]
    [TestCase(typeof(IMicrositePageService), typeof(MicrositePageService))]
    [TestCase(typeof(IBreadcrumbsService), typeof(BreadcrumbsService))]
    [TestCase(typeof(IMicrositeHeaderService), typeof(MicrositeHeaderService))]
    public void AddXperience_RegistersScopedService(Type serviceType, Type implementationType)
    {
        _services.AddXperience();

        var descriptor = _services.FirstOrDefault(sd => sd.ServiceType == serviceType);

        Assert.That(descriptor, Is.Not.Null, $"{serviceType.Name} should be registered");
        Assert.Multiple(() =>
        {
            Assert.That(descriptor!.ImplementationType, Is.EqualTo(implementationType),
                $"{serviceType.Name} should map to {implementationType.Name}");
            Assert.That(descriptor.Lifetime, Is.EqualTo(ServiceLifetime.Scoped),
                $"{serviceType.Name} should be registered as Scoped");
        });
    }

    [Test]
    public void AddXperience_RegistersDropdownScriptExecutorAsTransient()
    {
        _services.AddXperience();

        var descriptor = _services.FirstOrDefault(sd => sd.ServiceType == typeof(DropdownScriptExecutor));

        Assert.That(descriptor, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(descriptor!.ImplementationType, Is.EqualTo(typeof(DropdownScriptExecutor)));
            Assert.That(descriptor.Lifetime, Is.EqualTo(ServiceLifetime.Transient));
        });
    }

    [Test]
    public void AddXperience_RegistersXperiencePageContextDirectly()
    {
        _services.AddXperience();

        var descriptor = _services.FirstOrDefault(sd =>
            sd.ServiceType == typeof(XperiencePageContext) &&
            sd.ImplementationType == typeof(XperiencePageContext));

        Assert.That(descriptor, Is.Not.Null);
        Assert.That(descriptor!.Lifetime, Is.EqualTo(ServiceLifetime.Scoped));
    }

    [Test]
    public void AddXperience_ConfiguresSEOOptionsSetup()
    {
        _services.AddXperience();

        // ConfigureOptions<SEOOptionsSetup> registers it as IConfigureOptions<>.
        Assert.That(
            _services.Any(sd =>
                sd.ServiceType.IsGenericType &&
                sd.ServiceType.GetGenericTypeDefinition() == typeof(IConfigureOptions<>)),
            Is.True);
    }

    [Test]
    public void AddXperience_CanBeCalledWithoutThrowingException()
    {
        Assert.DoesNotThrow(() => _services.AddXperience());
    }
}