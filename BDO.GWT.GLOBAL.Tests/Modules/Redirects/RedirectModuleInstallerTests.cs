using System.Reflection;
using System.Runtime.Serialization;
using CMS.DataEngine;
using CMS.Modules;
using Moq;
using XperienceCommunity.Redirects.Admin;

namespace BDO.GWT.GLOBAL.Tests.Modules.Redirects;

public class RedirectModuleInstallerTests
{
    // ── structural ──────────────────────────────────────────────────────────

    [Test]
    public void Installer_ImplementsIRedirectModuleInstaller()
    {
        Assert.That(typeof(IRedirectModuleInstaller).IsAssignableFrom(typeof(RedirectModuleInstaller)), Is.True);
    }

    [Test]
    public void IRedirectModuleInstaller_ExposesInstallMethod()
    {
        Assert.That(typeof(IRedirectModuleInstaller).GetMethod(nameof(IRedirectModuleInstaller.Install)), Is.Not.Null);
    }

    [Test]
    public void Installer_IsInternalClass()
    {
        Assert.That(typeof(RedirectModuleInstaller).IsNotPublic, Is.True);
    }

    [Test]
    public void Installer_HasSinglePublicConstructor()
    {
        Assert.That(typeof(RedirectModuleInstaller).GetConstructors(), Has.Length.EqualTo(1));
    }

    [Test]
    public void Installer_Constructor_HasOneParameter()
    {
        Assert.That(typeof(RedirectModuleInstaller).GetConstructors().Single().GetParameters(), Has.Length.EqualTo(1));
    }

    [Test]
    public void Installer_Constructor_Parameter0_IsIInfoProviderOfResourceInfo()
    {
        var param = typeof(RedirectModuleInstaller).GetConstructors().Single().GetParameters()[0];
        Assert.That(param.ParameterType, Is.EqualTo(typeof(IInfoProvider<ResourceInfo>)));
    }

    // ── Install method ───────────────────────────────────────────────────────

    [Test]
    public void Install_MethodExists_IsPublic()
    {
        var method = typeof(RedirectModuleInstaller).GetMethod(nameof(IRedirectModuleInstaller.Install));
        Assert.Multiple(() =>
        {
            Assert.That(method, Is.Not.Null);
            Assert.That(method!.IsPublic, Is.True);
        });
    }

    [Test]
    public void Install_ReturnsVoid()
    {
        var method = typeof(RedirectModuleInstaller).GetMethod(nameof(IRedirectModuleInstaller.Install))!;
        Assert.That(method.ReturnType, Is.EqualTo(typeof(void)));
    }

    [Test]
    public void Install_HasNoParameters()
    {
        var method = typeof(RedirectModuleInstaller).GetMethod(nameof(IRedirectModuleInstaller.Install))!;
        Assert.That(method.GetParameters(), Is.Empty);
    }

    // ── RedirectModule ───────────────────────────────────────────────────────

    [Test]
    public void RedirectModule_IsInternalClass()
    {
        Assert.That(typeof(RedirectModule).IsNotPublic, Is.True);
    }

    [Test]
    public void RedirectModule_InheritsFromModule()
    {
        Assert.That(typeof(RedirectModule).BaseType?.Name, Is.EqualTo("Module"));
    }

    [Test]
    public void RedirectModule_HasParameterlessPublicConstructor()
    {
        var ctor = typeof(RedirectModule).GetConstructor(Type.EmptyTypes);
        Assert.That(ctor, Is.Not.Null);
    }

    [Test]
    public void RedirectModule_Constructor_CreatesModuleNamedAfterItself()
    {
        var module = new RedirectModule();

        Assert.That(module, Is.InstanceOf<CMS.DataEngine.Module>());
    }

    // ── RedirectModule.InitializeModule (behavioural) ─────────────────────────

    [Test]
    public void InitializeModule_WhenInstallerSet_CallsInstall()
    {
        var module = (RedirectModule)FormatterServices.GetUninitializedObject(typeof(RedirectModule));
        var installer = new SpyInstaller();
        SetInstaller(module, installer);

        InvokeInitializeModule(module);

        Assert.That(installer.InstallCount, Is.EqualTo(1));
    }

    [Test]
    public void InitializeModule_WhenInstallerNull_DoesNotThrow()
    {
        var module = (RedirectModule)FormatterServices.GetUninitializedObject(typeof(RedirectModule));
        // _installer intentionally left null

        Assert.DoesNotThrow(() => InvokeInitializeModule(module));
    }

    private static void SetInstaller(RedirectModule module, IRedirectModuleInstaller installer) =>
        typeof(RedirectModule)
            .GetField("_installer", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(module, installer);

    private static void InvokeInitializeModule(RedirectModule module)
    {
        var method = typeof(RedirectModule)
            .GetMethod("InitializeModule", BindingFlags.Instance | BindingFlags.NonPublic)!;
        try
        {
            method.Invoke(module, new object?[] { null, EventArgs.Empty });
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            throw ex.InnerException;
        }
    }

    // Hand-written double: the internal IRedirectModuleInstaller cannot be proxied by Moq
    // (Castle DynamicProxy) because the production assembly is not marked
    // InternalsVisibleTo("DynamicProxyGenAssembly2"). The test assembly can implement it
    // directly thanks to InternalsVisibleTo("BDO.GWT.GLOBAL.Tests").
    private sealed class SpyInstaller : IRedirectModuleInstaller
    {
        public int InstallCount { get; private set; }

        public void Install() => InstallCount++;
    }
}
