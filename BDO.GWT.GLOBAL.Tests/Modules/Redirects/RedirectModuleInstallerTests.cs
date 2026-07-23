using System.Reflection;
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
}
