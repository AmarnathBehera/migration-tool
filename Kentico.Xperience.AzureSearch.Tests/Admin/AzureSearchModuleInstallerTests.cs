using CMS.DataEngine;
using CMS.Modules;

using Kentico.Xperience.AzureSearch.Admin;

namespace Kentico.Xperience.AzureSearch.Tests.Admin;

/// <summary>
/// Tests for <see cref="AzureSearchModuleInstaller"/>.
/// </summary>
/// <remarks>
/// This installer cannot be fully unit tested. Its methods call static Kentico CMS APIs
/// (<c>DataClassInfoProvider</c>, <c>DataClassInfo.New</c>, <c>FormHelper</c>) and mutate
/// <see cref="ResourceInfo"/>, whose setters route through <c>AbstractInfoBase&lt;T&gt;.SetValue</c> and require
/// an initialized Xperience IoC container (<c>CMS.Core.Service.InitializeContainer</c>) plus a database
/// connection. Deeper coverage belongs in an Xperience integration test that boots the CMS application.
/// </remarks>
[TestFixture]
[Category("Unit")]
internal class AzureSearchModuleInstallerTests
{
    private const string EXPECTED_RESOURCE_NAME = "CMS.Integration.AzureSearch";


    /// <summary>
    /// <see cref="AzureSearchModuleInstaller.Install"/> calls the <c>InfoProviderExtensions.Get(string)</c>
    /// extension method, which requires the provider to also implement <see cref="IInfoByNameProvider{TInfo}"/>.
    /// </summary>
    private static IInfoProvider<ResourceInfo> CreateResourceProvider() =>
        Substitute.For<IInfoProvider<ResourceInfo>, IInfoByNameProvider<ResourceInfo>>();


    [Test]
    public void Constructor_DoesNotResolveAnyCmsServices()
    {
        // The constructor only stores the provider reference; it must not eagerly touch CMS APIs,
        // otherwise module registration would fail during application startup.
        Assert.That(() => new AzureSearchModuleInstaller(CreateResourceProvider()), Throws.Nothing);
    }


    [Test]
    public void Install_LooksUpTheAzureSearchResourceByExpectedCodeName()
    {
        var mockResourceProvider = CreateResourceProvider();
        var installer = new AzureSearchModuleInstaller(mockResourceProvider);

        try
        {
            installer.Install();
        }
        catch (Exception)
        {
            // Expected outside of a running Xperience application - subsequent lines require
            // an initialized CMS IoC container and database.
        }

        ((IInfoByNameProvider<ResourceInfo>)mockResourceProvider)
            .Received(1)
            .Get(EXPECTED_RESOURCE_NAME);
    }
}