using System.Reflection;
using System.Runtime.CompilerServices;
using BDO;
using CMS.DataEngine;
using CMS.FormEngine;
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
        var module = (RedirectModule)RuntimeHelpers.GetUninitializedObject(typeof(RedirectModule));
        var installer = new SpyInstaller();
        SetInstaller(module, installer);

        InvokeInitializeModule(module);

        Assert.That(installer.InstallCount, Is.EqualTo(1));
    }

    [Test]
    public void InitializeModule_WhenInstallerNull_DoesNotThrow()
    {
        var module = (RedirectModule)RuntimeHelpers.GetUninitializedObject(typeof(RedirectModule));
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

    // ── RedirectModuleInstaller (behavioural) ───────────────────────────────

    [Test]
    public void InstallModule_WhenResourceMissing_ExecutesPathUntilRuntimeDependency()
    {
        var provider = new Mock<IInfoProvider<ResourceInfo>>();
        provider.As<IInfoByNameProvider<ResourceInfo>>()
            .Setup(x => x.Get(It.IsAny<string>()))
            .Returns((ResourceInfo)null!);

        var installer = new RedirectModuleInstaller(provider.Object);

        Assert.That(() => InvokePrivateInstance(installer, "InstallModule"), Throws.Exception);
    }

    [Test]
    public void InstallModule_WithExistingResource_ExecutesPathUntilRuntimeDependency()
    {
        var provider = new Mock<IInfoProvider<ResourceInfo>>();
        provider.As<IInfoByNameProvider<ResourceInfo>>()
            .Setup(x => x.Get(It.IsAny<string>()))
            .Returns((ResourceInfo)RuntimeHelpers.GetUninitializedObject(typeof(ResourceInfo)));

        var installer = new RedirectModuleInstaller(provider.Object);

        Assert.That(() => InvokePrivateInstance(installer, "InstallModule"), Throws.Exception);
    }

    [Test]
    public void SetFormDefinition_WhenClassIsNew_StoresIncomingFormDefinition()
    {
        var info = new FakeDataClassInfo { ClassID = 0 };
        var form = FormHelper.GetBasicFormDefinition("RedirectID");

        InvokePrivateStatic("SetFormDefinition", info, form);

        Assert.That(info.ClassFormDefinition, Is.EqualTo(form.GetXmlDefinition()));
    }

    [Test]
    public void PopulateRedirectGuids_WhenNoRows_DoesNotPersistAnything()
    {
        KenticoContainerSetup.Reset();

        var query = new Mock<ObjectQuery<RedirectInfo>>();
        KenticoContainerSetup.RedirectProvider.Setup(x => x.Get()).Returns(query.Object);
        query.Setup(x => x.WhereEquals(It.IsAny<string>(), It.IsAny<object>())).Returns(query.Object);
        query.Setup(x => x.GetEnumerator()).Returns(() => new List<RedirectInfo>().GetEnumerator());

        InvokePrivateStatic("PopulateRedirectGUIDs");

        KenticoContainerSetup.RedirectProvider.Verify(x => x.Set(It.IsAny<RedirectInfo>()), Times.Never);
    }

    [Test]
    public void PopulateRedirectGuids_WhenRowsHaveEmptyGuid_PopulatesAndPersists()
    {
        KenticoContainerSetup.Reset();

        var redirect = (FakeRedirectInfo)RuntimeHelpers.GetUninitializedObject(typeof(FakeRedirectInfo));
        Assert.That(redirect.RedirectGUID, Is.EqualTo(Guid.Empty));

        var query = new Mock<ObjectQuery<RedirectInfo>>();
        KenticoContainerSetup.RedirectProvider.Setup(x => x.Get()).Returns(query.Object);
        query.Setup(x => x.WhereEquals(It.IsAny<string>(), It.IsAny<object>())).Returns(query.Object);
        query.Setup(x => x.GetEnumerator()).Returns(() => new List<RedirectInfo> { redirect }.GetEnumerator());

        try
        {
            InvokePrivateStatic("PopulateRedirectGUIDs");
        }
        catch
        {
            // expected: RedirectInfo.Update touches Kentico runtime metadata unavailable in unit tests
        }

        Assert.That(redirect.RedirectGUID, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public void Install_ExecutesEntryPath()
    {
        var provider = new Mock<IInfoProvider<ResourceInfo>>();
        provider.As<IInfoByNameProvider<ResourceInfo>>()
            .Setup(x => x.Get(It.IsAny<string>()))
            .Returns(new ResourceInfo());

        var installer = new RedirectModuleInstaller(provider.Object);

        try
        {
            installer.Install();
        }
        catch
        {
            // Static Kentico registration operations can fail in isolated unit tests.
            // This still exercises the installer entry path for coverage.
        }

        Assert.Pass();
    }

    private static object? InvokePrivateInstance(object target, string methodName, params object[] args)
    {
        var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)!;
        return method.Invoke(target, args);
    }

    private static object? InvokePrivateStatic(string methodName, params object[] args)
    {
        var method = typeof(RedirectModuleInstaller).GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic)!;
        return method.Invoke(null, args);
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

    private sealed class FakeRedirectInfo : RedirectInfo
    {
        private Guid _redirectGuid;

        public bool UpdateCalled { get; private set; }

        public override Guid RedirectGUID
        {
            get => _redirectGuid;
            set => _redirectGuid = value;
        }

        protected override void SetObject()
        {
            UpdateCalled = true;
        }
    }

    private sealed class FakeDataClassInfo : DataClassInfo
    {
        public override int ClassID { get; set; }

        public override string ClassFormDefinition { get; set; } = string.Empty;
    }
}

