using BDO.GWT.Core.Services;
using BDO.GWT.GLOBAL.API.ApiClients;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Extensions;
using BDO.GWT.GLOBAL.API.Helpers;
using BDO.GWT.GLOBAL.API.Middleware;
using BDO.GWT.GLOBAL.API.Options;
using BDO.GWT.GLOBAL.API.Services;
using CMS.MacroEngine;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BDO.GWT.GLOBAL.API.Tests.Extensions;

[TestFixture]
public class ServiceCollectionExtensionsTests
{
    private IServiceCollection _services = null!;

    [SetUp]
    public void Setup()
    {
        _services = new ServiceCollection();
        _services.AddApi();
    }

    [Test]
    public void AddApi_RegistersMemoryCache()
    {
        AssertRegistered(typeof(IMemoryCache));
    }

    [Test]
    public void AddApi_RegistersHttpClientFactory()
    {
        AssertRegistered(typeof(IHttpClientFactory));
    }

    [Test]
    public void AddApi_RegistersMemberFirmMiddleware_AsScoped()
    {
        AssertLifetime(typeof(MemberFirmMiddleware), ServiceLifetime.Scoped);
    }

    [Test]
    public void AddApi_ConfiguresMemberFirmServiceOptions()
    {
        AssertRegistered(typeof(IConfigureOptions<MemberFirmServiceOptions>));
    }

    [Test]
    public void AddApi_ConfiguresMasterDataApiOptions()
    {
        AssertRegistered(typeof(IConfigureOptions<MasterDataApiOptions>));
    }

    [TestCaseSource(nameof(ScopedServiceRegistrations))]
    public void AddApi_RegistersScopedService(Type serviceType, Type implementationType)
    {
        AssertScopedMapping(serviceType, implementationType);
    }

    [Test]
    public void AddApi_RegistersMacroResolver_AsScoped()
    {
        AssertLifetime(typeof(IMacroResolver), ServiceLifetime.Scoped);
    }

    [Test]
    public void AddApi_DoesNotThrow_WhenCalledMultipleTimes()
    {
        var services = new ServiceCollection();

        Assert.DoesNotThrow(() =>
        {
            services.AddApi();
            services.AddApi();
        });
    }

    private static IEnumerable<TestCaseData> ScopedServiceRegistrations()
    {
        yield return Case<IMemberFirmService, MemberFirmService>();
        yield return Case<IMasterDataApiClient, MasterDataApiClient>();
        yield return Case<IValidationState, ValidationState>();
        yield return Case<IAuthorizationHelper, AuthorizationHelper>();
        yield return Case<IPageContentValidator, PageContentValidator>();
        yield return Case<IMappingHelper, MappingHelper>();
        yield return Case<ICultureService, CultureService>();
        yield return Case<ICultureValidationService, CultureValidationService>();
        yield return Case<IFormCrudService, FormCrudService>();
        yield return Case<IDealCrudService, DealCrudService>();
        yield return Case<ICareerCrudService, CareerCrudService>();
        yield return Case<IPersonCrudService, PersonCrudService>();
        yield return Case<IInsightCrudService, InsightCrudService>();
        yield return Case<ISectionEventsCrudService, SectionEventsCrudService>();
        yield return Case<ILocationOfficeCrudService, LocationOfficeCrudService>();
        yield return Case<IGlobalSettingCrudService, GlobalSettingCrudService>();
        yield return Case<IChannelSettingCrudService, ChannelSettingCrudService>();
        yield return Case<IChannelCrudService, ChannelCrudService>();
        yield return Case<IChannelInfoService, ChannelInfoService>();
        yield return Case<IUserCrudService, UserCrudService>();
        yield return Case<IUserApiPermissionsCrudService, UserApiPermissionsCrudService>();
        yield return Case<ILicenseKeysCrudService, LicenseKeysCrudService>();
        yield return Case<IScheduledTaskCrudService, ScheduledTaskCrudService>();
        yield return Case<IPageAttachmentService, PageAttachmentService>();
        yield return Case<IPageChannelResolver, PageChannelResolver>();
        yield return Case<IContentItemInfoService, ContentItemInfoService>();
        yield return Case<IPageAttachmentCrudService, PageAttachmentCrudService>();
        yield return Case<IMediaFileCrudService, MediaFileCrudService>();
        yield return Case<IContentCategoryService, ContentCategoryService>();
        yield return Case<IEventLogCrudService, EventLogCrudService>();
        yield return Case<ISettingsCrudHelper, SettingsCrudHelper>();
        yield return Case<ICustomGlobalLocationsService, CustomGlobalLocationsService>();
    }

    private static TestCaseData Case<TService, TImplementation>() =>
        new TestCaseData(typeof(TService), typeof(TImplementation))
            .SetName($"AddApi_RegistersScopedService({typeof(TService).Name} -> {typeof(TImplementation).Name})");

    private void AssertRegistered(Type serviceType)
    {
        Assert.That(
            _services.Any(d => d.ServiceType == serviceType),
            Is.True,
            $"Expected {serviceType.Name} to be registered.");
    }

    private void AssertLifetime(Type serviceType, ServiceLifetime lifetime)
    {
        var descriptor = _services.SingleOrDefault(d => d.ServiceType == serviceType);

        Assert.That(descriptor, Is.Not.Null, $"Expected {serviceType.Name} to be registered.");
        Assert.That(descriptor!.Lifetime, Is.EqualTo(lifetime));
    }

    private void AssertScopedMapping(Type serviceType, Type implementationType)
    {
        var descriptor = _services.SingleOrDefault(d => d.ServiceType == serviceType);

        Assert.That(descriptor, Is.Not.Null, $"Expected {serviceType.Name} to be registered.");
        Assert.Multiple(() =>
        {
            Assert.That(descriptor!.Lifetime, Is.EqualTo(ServiceLifetime.Scoped));
            Assert.That(descriptor.ImplementationType, Is.EqualTo(implementationType));
        });
    }
}
