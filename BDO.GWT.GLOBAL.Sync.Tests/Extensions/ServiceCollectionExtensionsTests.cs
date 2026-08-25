using BDO.GWT.Core.Services;
using BDO.GWT.GLOBAL.Sync.Options;
using BDO.GWT.GLOBAL.Sync.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace BDO.GWT.GLOBAL.Sync.Tests.Extensions;

[TestFixture]
public class ServiceCollectionExtensionsTests
{
    private IServiceCollection _services = null!;
    private IConfiguration _configuration = null!;

    [SetUp]
    public void SetUp()
    {
        _services = new ServiceCollection();
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SyncApiKey"] = "test-api-key",
                ["GlobalSiteGuid"] = Guid.NewGuid().ToString(),
                ["RegionSitesCacheDuration"] = "01:00:00",
                ["Regions:0:GUID"] = Guid.NewGuid().ToString(),
                ["Regions:0:Name"] = "EMEA",
                ["Regions:0:SyncServiceUrl"] = "https://example.com",
            })
            .Build();
    }

    [Test]
    public void AddApiService_RegistersGlobalDealsSynchronizer_AsScoped()
    {
        _services.AddApiService(_configuration);

        AssertRegistered<IGlobalDealsSynchronizer, GlobalDealsSynchronizer>(ServiceLifetime.Scoped);
    }

    [Test]
    public void AddApiService_RegistersGlobalSiteDealsService_AsScoped()
    {
        _services.AddApiService(_configuration);

        AssertRegistered<IGlobalSiteDealsService, GlobalSiteDealsService>(ServiceLifetime.Scoped);
    }

    [Test]
    public void AddApiService_RegistersChannelDirectoryService_AsScoped()
    {
        _services.AddApiService(_configuration);

        AssertRegistered<IChannelDirectoryService, ChannelDirectoryService>(ServiceLifetime.Scoped);
    }

    [Test]
    public void AddApiService_RegistersCultureService_AsScoped()
    {
        _services.AddApiService(_configuration);

        AssertRegistered<ICultureService, CultureService>(ServiceLifetime.Scoped);
    }

    [Test]
    public void AddApiService_RegistersHttpClientFactory()
    {
        _services.AddApiService(_configuration);

        Assert.That(
            _services.Any(sd => sd.ServiceType == typeof(IHttpClientFactory)),
            Is.True);
    }

    [Test]
    public void AddApiService_RegistersMemoryCache()
    {
        _services.AddApiService(_configuration);

        Assert.That(
            _services.Any(sd => sd.ServiceType == typeof(Microsoft.Extensions.Caching.Memory.IMemoryCache)),
            Is.True);
    }

    [Test]
    public void AddApiService_ConfiguresSyncAppOptions_FromConfiguration()
    {
        _services.AddApiService(_configuration);

        using var provider = _services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<SyncAppOptions>>().Value;

        Assert.Multiple(() =>
        {
            Assert.That(options.SyncApiKey, Is.EqualTo("test-api-key"));
            Assert.That(options.RegionSitesCacheDuration, Is.EqualTo(TimeSpan.FromHours(1)));
            Assert.That(options.Regions, Has.Length.EqualTo(1));
            Assert.That(options.Regions[0].Name, Is.EqualTo("EMEA"));
            Assert.That(options.Regions[0].SyncServiceUrl, Is.EqualTo("https://example.com"));
        });
    }

    [Test]
    public void AddApiService_CanBeCalledMultipleTimes_WithoutThrowing()
    {
        Assert.DoesNotThrow(() =>
        {
            _services.AddApiService(_configuration);
            _services.AddApiService(_configuration);
        });
    }

    private void AssertRegistered<TService, TImplementation>(ServiceLifetime expectedLifetime)
    {
        var descriptor = _services.FirstOrDefault(sd =>
            sd.ServiceType == typeof(TService) &&
            sd.ImplementationType == typeof(TImplementation));

        Assert.That(descriptor, Is.Not.Null,
            $"Expected {typeof(TService).Name} -> {typeof(TImplementation).Name} to be registered.");
        Assert.That(descriptor!.Lifetime, Is.EqualTo(expectedLifetime));
    }
}
