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
}
