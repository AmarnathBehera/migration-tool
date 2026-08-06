using System.Reflection;
using CMS.DataEngine;
using CMS.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using XperienceCommunity.Redirects;
using XperienceCommunity.Redirects.Admin;
using XperienceCommunity.Redirects.Services;
using RedirectInfoX = global::BDO.RedirectInfo;

namespace BDO.GWT.GLOBAL.Tests.Modules.Redirects;

public class RedirectConfigurationServiceTests
{
    // ── structural ──────────────────────────────────────────────────────────

    [Test]
    public void Service_ImplementsIRedirectService()
    {
        Assert.That(typeof(IRedirectService).IsAssignableFrom(typeof(RedirectService)), Is.True);
    }

    [Test]
    public void Service_IsPublicNonAbstractClass()
    {
        Assert.Multiple(() =>
        {
            Assert.That(typeof(RedirectService).IsPublic, Is.True);
            Assert.That(typeof(RedirectService).IsAbstract, Is.False);
        });
    }

    [Test]
    public void Service_HasSinglePublicConstructor()
    {
        Assert.That(typeof(RedirectService).GetConstructors(), Has.Length.EqualTo(1));
    }

    [Test]
    public void Service_Constructor_HasTwoParameters()
    {
        Assert.That(typeof(RedirectService).GetConstructors().Single().GetParameters(), Has.Length.EqualTo(2));
    }

    [Test]
    public void Service_Constructor_Parameter0_IsIInfoProvider()
    {
        var param = typeof(RedirectService).GetConstructors().Single().GetParameters()[0];

        Assert.That(param.ParameterType, Is.EqualTo(typeof(IInfoProvider<RedirectInfoX>)));
    }

    [Test]
    public void Service_Constructor_Parameter1_IsIProgressiveCache()
    {
        var param = typeof(RedirectService).GetConstructors().Single().GetParameters()[1];

        Assert.That(param.ParameterType, Is.EqualTo(typeof(IProgressiveCache)));
    }

    // ── interface shape ─────────────────────────────────────────────────────

    [Test]
    public void IRedirectService_ExposesGetRedirects()
    {
        Assert.That(typeof(IRedirectService).GetMethod(nameof(IRedirectService.GetRedirects)), Is.Not.Null);
    }

    [Test]
    public void GetRedirects_ReturnType_IsTaskOfReadOnlyCollectionOfRedirectInfo()
    {
        var method = typeof(RedirectService).GetMethod(nameof(RedirectService.GetRedirects));

        Assert.That(method!.ReturnType, Is.EqualTo(typeof(Task<IReadOnlyCollection<RedirectInfoX>>)));
    }

    [Test]
    public void GetRedirects_HasNoParameters()
    {
        var method = typeof(RedirectService).GetMethod(nameof(RedirectService.GetRedirects));

        Assert.That(method!.GetParameters(), Is.Empty);
    }

    // ── private fields ──────────────────────────────────────────────────────

    [Test]
    public void Service_HasPrivateField_ForInfoProvider()
    {
        var field = typeof(RedirectService)
            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
            .FirstOrDefault(f => f.FieldType == typeof(IInfoProvider<RedirectInfoX>));

        Assert.That(field, Is.Not.Null);
    }

    [Test]
    public void Service_HasPrivateField_ForCache()
    {
        var field = typeof(RedirectService)
            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
            .FirstOrDefault(f => f.FieldType == typeof(IProgressiveCache));

        Assert.That(field, Is.Not.Null);
    }

    // ── IRedirectService shape ───────────────────────────────────────────────

    [Test]
    public void IRedirectService_IsPublicInterface()
    {
        Assert.Multiple(() =>
        {
            Assert.That(typeof(IRedirectService).IsPublic, Is.True);
            Assert.That(typeof(IRedirectService).IsInterface, Is.True);
        });
    }

    [Test]
    public void IRedirectService_HasExactlyOneMethod()
    {
        Assert.That(typeof(IRedirectService).GetMethods(), Has.Length.EqualTo(1));
    }

    [Test]
    public void GetRedirects_IsAsync()
    {
        var method = typeof(RedirectService).GetMethod(nameof(RedirectService.GetRedirects))!;
        // async methods are marked with AsyncStateMachineAttribute
        var asyncAttr = method.GetCustomAttributes()
            .FirstOrDefault(a => a.GetType().Name == "AsyncStateMachineAttribute");
        Assert.That(asyncAttr, Is.Not.Null);
    }

    // ── RedirectServiceCollectionExtensions ──────────────────────────────────

    [Test]
    public void ServiceCollectionExtensions_TypeExists()
    {
        Assert.That(typeof(RedirectServiceCollectionExtensions), Is.Not.Null);
    }

    [Test]
    public void ServiceCollectionExtensions_AddXperienceCommunityRedirects_IsPublicStatic()
    {
        var method = typeof(RedirectServiceCollectionExtensions)
            .GetMethod(nameof(RedirectServiceCollectionExtensions.AddXperienceCommunityRedirects));

        Assert.Multiple(() =>
        {
            Assert.That(method, Is.Not.Null);
            Assert.That(method!.IsPublic, Is.True);
            Assert.That(method.IsStatic, Is.True);
        });
    }

    [Test]
    public void ServiceCollectionExtensions_AddXperienceCommunityRedirects_ReturnsIServiceCollection()
    {
        var method = typeof(RedirectServiceCollectionExtensions)
            .GetMethod(nameof(RedirectServiceCollectionExtensions.AddXperienceCommunityRedirects))!;

        Assert.That(method.ReturnType.Name, Is.EqualTo("IServiceCollection"));
    }

    // ── behavioural: RedirectService ─────────────────────────────────────────

    [Test]
    public void Constructor_StoresDependencies_AndImplementsInterface()
    {
        var provider = new Mock<IInfoProvider<RedirectInfoX>>();
        var cache = new Mock<IProgressiveCache>();

        var service = new RedirectService(provider.Object, cache.Object);

        Assert.That(service, Is.InstanceOf<IRedirectService>());
    }

    [Test]
    public async Task GetRedirects_LoadsRedirectsThroughCache()
    {
        var cache = new Mock<IProgressiveCache>();
        cache.Setup(c => c.LoadAsync(
                It.IsAny<Func<CacheSettings, Task<List<RedirectInfoX>>>>(),
                It.IsAny<CacheSettings>()))
            .Returns<Func<CacheSettings, Task<List<RedirectInfoX>>>, CacheSettings>(
                (factory, settings) => factory(settings));

        var query = new Mock<ObjectQuery<RedirectInfoX>>();
        query.Setup(q => q.GetEnumerableTypedResultAsync())
            .ReturnsAsync(new List<RedirectInfoX> { new(), new() });

        var provider = new Mock<IInfoProvider<RedirectInfoX>>();
        provider.Setup(p => p.Get()).Returns(query.Object);

        var service = new RedirectService(provider.Object, cache.Object);

        var result = await service.GetRedirects();

        Assert.That(result, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task GetRedirects_WhenNoRedirects_ReturnsEmptyCollection()
    {
        var cache = new Mock<IProgressiveCache>();
        cache.Setup(c => c.LoadAsync(
                It.IsAny<Func<CacheSettings, Task<List<RedirectInfoX>>>>(),
                It.IsAny<CacheSettings>()))
            .Returns<Func<CacheSettings, Task<List<RedirectInfoX>>>, CacheSettings>(
                (factory, settings) => factory(settings));

        var query = new Mock<ObjectQuery<RedirectInfoX>>();
        query.Setup(q => q.GetEnumerableTypedResultAsync())
            .ReturnsAsync(new List<RedirectInfoX>());

        var provider = new Mock<IInfoProvider<RedirectInfoX>>();
        provider.Setup(p => p.Get()).Returns(query.Object);

        var service = new RedirectService(provider.Object, cache.Object);

        var result = await service.GetRedirects();

        Assert.That(result, Is.Empty);
    }

    // ── behavioural: RedirectServiceCollectionExtensions ─────────────────────

    [Test]
    public void AddXperienceCommunityRedirects_ReturnsSameServiceCollectionInstance()
    {
        var services = new ServiceCollection();

        var result = services.AddXperienceCommunityRedirects();

        Assert.That(result, Is.SameAs(services));
    }

    [Test]
    public void AddXperienceCommunityRedirects_RegistersInstallerAsSingleton()
    {
        var services = new ServiceCollection();

        services.AddXperienceCommunityRedirects();

        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IRedirectModuleInstaller));

        Assert.That(descriptor, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(descriptor!.ImplementationType, Is.EqualTo(typeof(RedirectModuleInstaller)));
            Assert.That(descriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        });
    }

    [Test]
    public void AddXperienceCommunityRedirects_RegistersRedirectServiceAsSingleton()
    {
        var services = new ServiceCollection();

        services.AddXperienceCommunityRedirects();

        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IRedirectService));

        Assert.That(descriptor, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(descriptor!.ImplementationType, Is.EqualTo(typeof(RedirectService)));
            Assert.That(descriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        });
    }
}
