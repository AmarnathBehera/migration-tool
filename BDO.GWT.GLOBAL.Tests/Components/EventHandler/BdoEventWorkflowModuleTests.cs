using System.Reflection;
using System.Runtime.CompilerServices;
using BDO.GWT.GLOBAL.Components.EventHandler;
using CMS;
using CMS.DataEngine;
using CMS.Websites;
using Microsoft.Extensions.DependencyInjection;
using Module = CMS.DataEngine.Module;

namespace BDO.GWT.GLOBAL.Tests.Components.EventHandler;

public class BdoEventWorkflowModuleTests
{
    [Test]
    public void Module_InheritsFromModule()
    {
        Assert.That(typeof(Module).IsAssignableFrom(typeof(BdoEventWorkflowModule)), Is.True);
    }

    [Test]
    public void Module_ExposesParameterlessConstructor()
    {
        var ctor = typeof(BdoEventWorkflowModule).GetConstructor(Type.EmptyTypes);

        Assert.That(ctor, Is.Not.Null);
    }

    [Test]
    public void Module_HasExpectedName()
    {
        var instance = new BdoEventWorkflowModule();

        Assert.That(instance.Name, Is.EqualTo(nameof(BdoEventWorkflowModule)));
    }

    [Test]
    public void Module_OverridesOnPreInit()
    {
        var method = typeof(BdoEventWorkflowModule).GetMethod(
            "OnPreInit",
            BindingFlags.Instance |
            BindingFlags.NonPublic |
            BindingFlags.Public |
            BindingFlags.DeclaredOnly);

        Assert.That(method, Is.Not.Null);
        Assert.That(method!.DeclaringType, Is.EqualTo(typeof(BdoEventWorkflowModule)));
    }

    [Test]
    public void Module_IsRegisteredViaAssemblyAttribute()
    {
        var assembly = typeof(BdoEventWorkflowModule).Assembly;

        var registered = assembly
            .GetCustomAttributes<RegisterModuleAttribute>()
            .Any(a => GetRegisteredModuleType(a) == typeof(BdoEventWorkflowModule));

        Assert.That(registered, Is.True);
    }

    [Test]
    public void OnPreInit_RegistersBeforeCreateAndAfterPublishHandlers()
    {
        var module = new BdoEventWorkflowModule();
        var services = new ServiceCollection();
        var parameters = CreateModulePreInitParameters(services);

        InvokeOnPreInit(module, parameters);

        var createDescriptor = services.SingleOrDefault(d =>
            d.ServiceType.Name == "IAsyncEventHandler`1" &&
            d.ServiceType.GenericTypeArguments.SingleOrDefault() == typeof(BeforeCreateWebPageEvent) &&
            d.ImplementationType == typeof(NonAsciiUrlSlugHandler));

        var publishDescriptor = services.SingleOrDefault(d =>
            d.ServiceType.Name == "IAsyncEventHandler`1" &&
            d.ServiceType.GenericTypeArguments.SingleOrDefault() == typeof(AfterPublishWebPageEvent) &&
            d.ImplementationType == typeof(BdoEventPublishHandler));

        Assert.Multiple(() =>
        {
            Assert.That(createDescriptor, Is.Not.Null);
            Assert.That(publishDescriptor, Is.Not.Null);
        });
    }

    private static Type? GetRegisteredModuleType(RegisterModuleAttribute attribute)
    {
        var property = attribute.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .FirstOrDefault(p => p.PropertyType == typeof(Type));

        return property?.GetValue(attribute) as Type;
    }

    private static object CreateModulePreInitParameters(IServiceCollection services)
    {
        var type = Type.GetType("CMS.Core.ModulePreInitParameters, CMS.Core", throwOnError: true)!;
        var instance = RuntimeHelpers.GetUninitializedObject(type);

        var property = type.GetProperty("Services", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (property?.CanWrite == true)
        {
            property.SetValue(instance, services);
            return instance;
        }

        var field = type.GetField("<Services>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? type.GetField("Services", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        field!.SetValue(instance, services);
        return instance;
    }

    private static void InvokeOnPreInit(BdoEventWorkflowModule module, object parameters)
    {
        var method = typeof(BdoEventWorkflowModule).GetMethod(
            "OnPreInit",
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            types: [parameters.GetType()],
            modifiers: null)!;

        method.Invoke(module, [parameters]);
    }
}
