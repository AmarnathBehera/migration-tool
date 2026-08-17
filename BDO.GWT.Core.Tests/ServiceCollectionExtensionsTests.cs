using BDO.GWT.Core.Services;
using BDO.GWT.Core.LiveSite.Services;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace BDO.GWT.Core.Tests;

[TestFixture]
public class ServiceCollectionExtensionsTests
{
    [Test]
    public void AddContentServices_RegistersInsightCarouselStateStore_AsScoped()
    {
        var services = new ServiceCollection();

        services.AddContentServices();

        var descriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IInsightCarouselStateStore));

        Assert.Multiple(() =>
        {
            Assert.That(descriptor, Is.Not.Null);
            Assert.That(descriptor!.ImplementationType, Is.EqualTo(typeof(InsightCarouselStateStore)));
            Assert.That(descriptor.Lifetime, Is.EqualTo(ServiceLifetime.Scoped));
        });
    }

    [Test]
    public void AddContentServices_RegistersInsightCarouselService_AsScoped()
    {
        var services = new ServiceCollection();

        services.AddContentServices();

        var descriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IInsightCarouselService));

        Assert.Multiple(() =>
        {
            Assert.That(descriptor, Is.Not.Null);
            Assert.That(descriptor!.ImplementationType, Is.EqualTo(typeof(InsightCarouselService)));
            Assert.That(descriptor.Lifetime, Is.EqualTo(ServiceLifetime.Scoped));
        });
    }
}
