using System.Reflection;
using CMS.DataEngine;
using CMS.Helpers;
using Microsoft.Extensions.DependencyInjection;
using XperienceCommunity.Redirects;
using XperienceCommunity.Redirects.Services;
using RedirectInfoX = global::XperienceCommunity.Redirects.RedirectInfo;

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

    [Test]
    public void AddXperienceCommunityRedirects_RegistersIRedirectService()
    {
        var services = new ServiceCollection();
        services.AddXperienceCommunityRedirects();

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IRedirectService));

        Assert.That(descriptor, Is.Not.Null);
    }

    [Test]
    public void AddXperienceCommunityRedirects_RegistersIRedirectModuleInstaller()
    {
        var services = new ServiceCollection();
        services.AddXperienceCommunityRedirects();

        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType.Name == "IRedirectModuleInstaller");

        Assert.That(descriptor, Is.Not.Null);
    }

    [Test]
    public void AddXperienceCommunityRedirects_IRedirectService_RegisteredAsSingleton()
    {
        var services = new ServiceCollection();
        services.AddXperienceCommunityRedirects();

        var descriptor = services.First(d => d.ServiceType == typeof(IRedirectService));

        Assert.That(descriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
    }

    [Test]
    public void AddXperienceCommunityRedirects_IRedirectModuleInstaller_RegisteredAsSingleton()
    {
        var services = new ServiceCollection();
        services.AddXperienceCommunityRedirects();

        var descriptor = services.First(d => d.ServiceType.Name == "IRedirectModuleInstaller");

        Assert.That(descriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
    }

    [Test]
    public void AddXperienceCommunityRedirects_IRedirectService_ImplementationType_IsRedirectService()
    {
        var services = new ServiceCollection();
        services.AddXperienceCommunityRedirects();

        var descriptor = services.First(d => d.ServiceType == typeof(IRedirectService));

        Assert.That(descriptor.ImplementationType, Is.EqualTo(typeof(RedirectService)));
    }

    [Test]
    public void AddXperienceCommunityRedirects_ReturnsSameServiceCollection()
    {
        var services = new ServiceCollection();
        var result = services.AddXperienceCommunityRedirects();

        Assert.That(result, Is.SameAs(services));
    }

    // ── RedirectService structural ──────────────────────────────────────────

    [Test]
    public void RedirectService_HasSinglePublicConstructor()
    {
        Assert.That(typeof(RedirectService).GetConstructors(), Has.Length.EqualTo(1));
    }

    [Test]
    public void RedirectService_Constructor_HasTwoParameters()
    {
        Assert.That(typeof(RedirectService).GetConstructors().Single().GetParameters(), Has.Length.EqualTo(2));
    }

    [Test]
    public void RedirectService_Constructor_Parameter0_IsIInfoProviderOfRedirectInfo()
    {
        var param = typeof(RedirectService).GetConstructors().Single().GetParameters()[0];

        Assert.That(param.ParameterType, Is.EqualTo(typeof(IInfoProvider<RedirectInfoX>)));
    }

    [Test]
    public void RedirectService_Constructor_Parameter1_IsIProgressiveCache()
    {
        var param = typeof(RedirectService).GetConstructors().Single().GetParameters()[1];

        Assert.That(param.ParameterType.Name, Is.EqualTo("IProgressiveCache"));
    }

    // ── RedirectInfo shape ───────────────────────────────────────────────────

    [Test]
    public void RedirectInfo_ObjectType_IsNotNullOrEmpty()
    {
        Assert.That(RedirectInfoX.OBJECT_TYPE, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public void RedirectInfo_HasRedirectSourceUrlProperty()
    {
        var prop = typeof(RedirectInfoX).GetProperty(nameof(RedirectInfoX.RedirectSourceUrl));

        Assert.That(prop, Is.Not.Null);
    }

    [Test]
    public void RedirectInfo_HasRedirectTargetWebPageItemGUIDProperty()
    {
        var prop = typeof(RedirectInfoX).GetProperty(nameof(RedirectInfoX.RedirectTargetWebPageItemGUID));

        Assert.That(prop, Is.Not.Null);
    }

    [Test]
    public void RedirectInfo_HasRedirectTargetPageURLProperty()
    {
        var prop = typeof(RedirectInfoX).GetProperty(nameof(RedirectInfoX.RedirectTargetPageURL));

        Assert.That(prop, Is.Not.Null);
    }

    [Test]
    public void RedirectInfo_HasRedirectChannelIDProperty()
    {
        var prop = typeof(RedirectInfoX).GetProperty(nameof(RedirectInfoX.RedirectChannelID));

        Assert.That(prop, Is.Not.Null);
    }

    [Test]
    public void RedirectInfo_HasRedirectChannelNameProperty()
    {
        var prop = typeof(RedirectInfoX).GetProperty(nameof(RedirectInfoX.RedirectChannelName));

        Assert.That(prop, Is.Not.Null);
    }

    [Test]
    public void RedirectInfo_HasRedirectGUIDProperty()
    {
        var prop = typeof(RedirectInfoX).GetProperty(nameof(RedirectInfoX.RedirectGUID));

        Assert.That(prop, Is.Not.Null);
    }

    [Test]
    public void RedirectInfo_HasRedirectIDProperty()
    {
        var prop = typeof(RedirectInfoX).GetProperty(nameof(RedirectInfoX.RedirectID));

        Assert.That(prop, Is.Not.Null);
    }

    [Test]
    public void RedirectInfo_TYPEINFO_ObjectClassName_IsNotNullOrEmpty()
    {
        Assert.That(RedirectInfoX.TYPEINFO.ObjectClassName, Is.Not.Null.And.Not.Empty);
    }

    // ── RedirectServiceCollectionExtensions – additional ────────────────────

    [Test]
    public void ServiceCollectionExtensions_AddXperienceCommunityRedirects_HasOneParameter()
    {
        var method = typeof(RedirectServiceCollectionExtensions)
            .GetMethod(nameof(RedirectServiceCollectionExtensions.AddXperienceCommunityRedirects))!;

        Assert.That(method.GetParameters(), Has.Length.EqualTo(1));
    }

    [Test]
    public void ServiceCollectionExtensions_AddXperienceCommunityRedirects_Parameter0_IsIServiceCollection()
    {
        var param = typeof(RedirectServiceCollectionExtensions)
            .GetMethod(nameof(RedirectServiceCollectionExtensions.AddXperienceCommunityRedirects))!
            .GetParameters()[0];

        Assert.That(param.ParameterType.Name, Is.EqualTo("IServiceCollection"));
    }

    [Test]
    public void AddXperienceCommunityRedirects_IsExtensionMethod()
    {
        var method = typeof(RedirectServiceCollectionExtensions)
            .GetMethod(nameof(RedirectServiceCollectionExtensions.AddXperienceCommunityRedirects))!;

        Assert.That(method.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false), Is.True);
    }

    [Test]
    public void ServiceCollectionExtensions_HasAtLeastOneStaticMethod()
    {
        var staticMethods = typeof(RedirectServiceCollectionExtensions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static);

        Assert.That(staticMethods, Is.Not.Empty);
    }

    [Test]
    public void AddXperienceCommunityRedirects_CanBeCalledTwice_WithoutThrowingOnServiceDescriptors()
    {
        var services = new ServiceCollection();
        services.AddXperienceCommunityRedirects();

        Assert.DoesNotThrow(() => services.AddXperienceCommunityRedirects());
    }

    // ── IRedirectService – additional shape ─────────────────────────────────

    [Test]
    public void IRedirectService_GetRedirects_HasNoParameters()
    {
        var method = typeof(IRedirectService).GetMethod(nameof(IRedirectService.GetRedirects))!;

        Assert.That(method.GetParameters(), Is.Empty);
    }

    [Test]
    public void IRedirectService_GetRedirects_ReturnType_IsTaskOfIReadOnlyCollection()
    {
        var method = typeof(IRedirectService).GetMethod(nameof(IRedirectService.GetRedirects))!;

        Assert.That(method.ReturnType.Name, Is.EqualTo("Task`1"));
    }

    // ── RedirectModuleInstaller shape ────────────────────────────────────────

    [Test]
    public void RedirectModuleInstaller_TypeExists()
    {
        var type = typeof(RedirectServiceCollectionExtensions).Assembly
            .GetTypes()
            .FirstOrDefault(t => t.Name == "RedirectModuleInstaller");

        Assert.That(type, Is.Not.Null);
    }

    [Test]
    public void IRedirectModuleInstaller_TypeExists()
    {
        var type = typeof(RedirectServiceCollectionExtensions).Assembly
            .GetTypes()
            .FirstOrDefault(t => t.Name == "IRedirectModuleInstaller");

        Assert.That(type, Is.Not.Null);
    }

    [Test]
    public void IRedirectModuleInstaller_HasInstallMethod()
    {
        var type = typeof(RedirectServiceCollectionExtensions).Assembly
            .GetTypes()
            .Single(t => t.Name == "IRedirectModuleInstaller");

        Assert.That(type.GetMethod("Install"), Is.Not.Null);
    }

    [Test]
    public void RedirectModuleInstaller_Install_IsVoid()
    {
        var type = typeof(RedirectServiceCollectionExtensions).Assembly
            .GetTypes()
            .Single(t => t.Name == "RedirectModuleInstaller");

        var method = type.GetMethod("Install");

        Assert.That(method!.ReturnType, Is.EqualTo(typeof(void)));
    }

    [Test]
    public void RedirectModuleInstaller_HasSinglePublicConstructor()
    {
        var type = typeof(RedirectServiceCollectionExtensions).Assembly
            .GetTypes()
            .Single(t => t.Name == "RedirectModuleInstaller");

        Assert.That(type.GetConstructors(), Has.Length.EqualTo(1));
    }

    [Test]
    public void RedirectModuleInstaller_Constructor_HasOneParameter_IInfoProvider()
    {
        var type = typeof(RedirectServiceCollectionExtensions).Assembly
            .GetTypes()
            .Single(t => t.Name == "RedirectModuleInstaller");

        var param = type.GetConstructors().Single().GetParameters()[0];

        Assert.That(param.ParameterType.Name, Is.EqualTo("IInfoProvider`1"));
    }

    [Test]
    public void RedirectModuleInstaller_ImplementsIRedirectModuleInstaller()
    {
        var assembly = typeof(RedirectServiceCollectionExtensions).Assembly;
        var installerType = assembly.GetTypes().Single(t => t.Name == "RedirectModuleInstaller");
        var interfaceType = assembly.GetTypes().Single(t => t.Name == "IRedirectModuleInstaller");

        Assert.That(interfaceType.IsAssignableFrom(installerType), Is.True);
    }
}
