using BDO.GWT.Common.Core.Exceptions;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Services;
using CMS.Base;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Websites;
using Moq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BDO.GWT.GLOBAL.API.Tests.Services;

[TestFixture]
public class ChannelCrudServiceTests
{
    [Test]
    public void ChannelCrudService_ImplementsIChannelCrudService()
    {
        var service = (ChannelCrudService)RuntimeHelpers.GetUninitializedObject(typeof(ChannelCrudService));

        Assert.That(service, Is.InstanceOf<IChannelCrudService>());
    }

    [Test]
    public void ChannelCrudService_Interface_HasExpectedMethods()
    {
        var interfaceType = typeof(IChannelCrudService);

        Assert.Multiple(() =>
        {
            Assert.That(interfaceType.GetMethod("GetEntitiesAsync"), Is.Not.Null);
            Assert.That(interfaceType.GetMethod("GetEntityAsync"), Is.Not.Null);
            Assert.That(interfaceType.GetMethod("CreateEntityAsync"), Is.Not.Null);
            Assert.That(interfaceType.GetMethod("UpdateEntityAsync"), Is.Not.Null);
            Assert.That(interfaceType.GetMethod("DeleteEntityAsync"), Is.Not.Null);
        });
    }

    [Test]
    public void ChannelsQuery_DefaultValues_AreNull()
    {
        var query = new ChannelsQuery();

        Assert.That(query.AdministrationDomain, Is.Null);
    }

    [Test]
    public void ChannelsQuery_CanSetAdministrationDomain()
    {
        var query = new ChannelsQuery { AdministrationDomain = "test.example.com" };

        Assert.That(query.AdministrationDomain, Is.EqualTo("test.example.com"));
    }

    [Test]
    public void ChannelsQuery_MultipleInstances_AreIndependent()
    {
        var q1 = new ChannelsQuery { AdministrationDomain = "a.example.com" };
        var q2 = new ChannelsQuery { AdministrationDomain = "b.example.com" };

        Assert.Multiple(() =>
        {
            Assert.That(q1.AdministrationDomain, Is.EqualTo("a.example.com"));
            Assert.That(q2.AdministrationDomain, Is.EqualTo("b.example.com"));
        });
    }

    [Test]
    public void ChannelOutput_DefaultCultures_IsEmpty()
    {
        var output = new ChannelOutput
        {
            General = new SiteGeneralOutput
            {
                ID = 1,
                GUID = Guid.NewGuid(),
                DisplayName = "Test",
                CodeName = "test",
                WebsitedomainresentationURL = "test.example.com"
            }
        };

        Assert.That(output.Cultures, Is.Empty);
    }

    [Test]
    public void ChannelOutput_General_StoresValues()
    {
        var guid = Guid.NewGuid();
        var output = new ChannelOutput
        {
            General = new SiteGeneralOutput
            {
                ID = 42,
                GUID = guid,
                DisplayName = "My Channel",
                CodeName = "my-channel",
                WebsitedomainresentationURL = "my-channel.example.com",
                DefaultContentCulture = "en-GB"
            }
        };

        Assert.Multiple(() =>
        {
            Assert.That(output.General.ID, Is.EqualTo(42));
            Assert.That(output.General.GUID, Is.EqualTo(guid));
            Assert.That(output.General.DisplayName, Is.EqualTo("My Channel"));
            Assert.That(output.General.CodeName, Is.EqualTo("my-channel"));
            Assert.That(output.General.WebsitedomainresentationURL, Is.EqualTo("my-channel.example.com"));
            Assert.That(output.General.DefaultContentCulture, Is.EqualTo("en-GB"));
        });
    }

    [Test]
    public void SiteGeneralOutput_DefaultContentCulture_IsNullByDefault()
    {
        var output = new SiteGeneralOutput
        {
            ID = 1,
            GUID = Guid.NewGuid(),
            DisplayName = "Test",
            CodeName = "test",
            WebsitedomainresentationURL = "test.example.com"
        };

        Assert.That(output.DefaultContentCulture, Is.Null);
    }

    [Test]
    public void CultureSummaryOutput_StoresValues()
    {
        var guid = Guid.NewGuid();
        var culture = new CultureSummaryOutput
        {
            GUID = guid,
            Name = "English (United Kingdom)",
            Code = "en-GB"
        };

        Assert.Multiple(() =>
        {
            Assert.That(culture.GUID, Is.EqualTo(guid));
            Assert.That(culture.Name, Is.EqualTo("English (United Kingdom)"));
            Assert.That(culture.Code, Is.EqualTo("en-GB"));
        });
    }

    [Test]
    public void NormalizeWebsiteChannelDomain_ReturnsEmpty_WhenInputIsNullOrWhitespace()
    {
        Assert.Multiple(() =>
        {
            Assert.That(InvokeNormalizeWebsiteChannelDomain(null!), Is.EqualTo(string.Empty));
            Assert.That(InvokeNormalizeWebsiteChannelDomain("   "), Is.EqualTo(string.Empty));
        });
    }

    [Test]
    public void NormalizeWebsiteChannelDomain_ReturnsHost_WhenAbsoluteUrlHasDefaultPort()
    {
        var result = InvokeNormalizeWebsiteChannelDomain("https://example.com/path?x=1");

        Assert.That(result, Is.EqualTo("example.com"));
    }

    [Test]
    public void NormalizeWebsiteChannelDomain_ReturnsHostAndPort_WhenAbsoluteUrlHasNonDefaultPort()
    {
        var result = InvokeNormalizeWebsiteChannelDomain("https://example.com:8443/path");

        Assert.That(result, Is.EqualTo("example.com:8443"));
    }

    [Test]
    public void NormalizeWebsiteChannelDomain_ReturnsFirstSegment_WhenRelativePathProvided()
    {
        var result = InvokeNormalizeWebsiteChannelDomain("example.com/some/path");

        Assert.That(result, Is.EqualTo("example.com"));
    }

    [Test]
    public void ResolvePresentationUrl_PrefersLiveSiteAlias_WhenProvided()
    {
        var input = new ChannelInput
        {
            General = new ChannelGeneralInput
            {
                DisplayName = "A",
                CodeName = "a",
                PresentationURL = "https://fallback.example.com"
            },
            DomainAliases = new ChannelDomainAliasesInput
            {
                LiveSite = [new ChannelDomainAliasInput { PresentationURL = "https://preferred.example.com/path" }]
            },
            Cultures = [new ChannelCultureInput { Code = "en-GB" }]
        };

        var result = InvokeResolvePresentationUrl(input);

        Assert.That(result, Is.EqualTo("preferred.example.com"));
    }

    [Test]
    public void ResolvePresentationUrl_FallsBackToGeneralPresentationUrl_WhenLiveSiteIsEmpty()
    {
        var input = new ChannelInput
        {
            General = new ChannelGeneralInput
            {
                DisplayName = "A",
                CodeName = "a",
                PresentationURL = "https://general.example.com/path"
            },
            DomainAliases = new ChannelDomainAliasesInput { LiveSite = [] },
            Cultures = [new ChannelCultureInput { Code = "en-GB" }]
        };

        var result = InvokeResolvePresentationUrl(input);

        Assert.That(result, Is.EqualTo("general.example.com"));
    }

    [Test]
    public void ToChannelOutput_UsesDefaults_WhenValuesAreMissing()
    {
        var container = new Mock<IDataContainer>();
        container.Setup(c => c.GetValue(nameof(ChannelInfo.ChannelID))).Returns((object)null!);
        container.Setup(c => c.GetValue(nameof(ChannelInfo.ChannelGUID))).Returns((object)null!);
        container.Setup(c => c.GetValue(nameof(ChannelInfo.ChannelDisplayName))).Returns((object)null!);
        container.Setup(c => c.GetValue(nameof(ChannelInfo.ChannelName))).Returns((object)null!);
        container.Setup(c => c.GetValue(nameof(WebsiteChannelInfo.WebsiteChannelDomain))).Returns((object)null!);
        container.Setup(c => c.GetValue(nameof(WebsiteChannelInfo.WebsiteChannelPrimaryContentLanguageID))).Returns((object)null!);

        var cultures = new List<ContentLanguageInfo>();
        var result = InvokeToChannelOutput(container.Object, new Dictionary<int, ContentLanguageInfo>(), cultures);

        Assert.Multiple(() =>
        {
            Assert.That(result.General.ID, Is.EqualTo(0));
            Assert.That(result.General.GUID, Is.EqualTo(Guid.Empty));
            Assert.That(result.General.DisplayName, Is.EqualTo(string.Empty));
            Assert.That(result.General.CodeName, Is.EqualTo(string.Empty));
            Assert.That(result.General.WebsitedomainresentationURL, Is.EqualTo(string.Empty));
            Assert.That(result.General.DefaultContentCulture, Is.Null);
            Assert.That(result.Cultures, Is.Empty);
        });
    }

    [Test]
    public void GetEntitiesAsync_WhenDependenciesUnavailable_Throws()
    {
        var service = (ChannelCrudService)RuntimeHelpers.GetUninitializedObject(typeof(ChannelCrudService));

        Assert.ThrowsAsync<NullReferenceException>(async () =>
            await service.GetEntitiesAsync(new ChannelsQuery(), CancellationToken.None));
    }

    [Test]
    public void GetEntityAsync_WhenDependenciesUnavailable_Throws()
    {
        var service = (ChannelCrudService)RuntimeHelpers.GetUninitializedObject(typeof(ChannelCrudService));

        Assert.ThrowsAsync<NullReferenceException>(async () =>
            await service.GetEntityAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Test]
    public void CreateEntityAsync_WhenDependenciesUnavailable_Throws()
    {
        var service = (ChannelCrudService)RuntimeHelpers.GetUninitializedObject(typeof(ChannelCrudService));

        Assert.ThrowsAsync<NullReferenceException>(async () =>
            await service.CreateEntityAsync(BuildChannelInput(), CancellationToken.None));
    }

    [Test]
    public void UpdateEntityAsync_WhenDependenciesUnavailable_Throws()
    {
        var service = (ChannelCrudService)RuntimeHelpers.GetUninitializedObject(typeof(ChannelCrudService));

        Assert.ThrowsAsync<NullReferenceException>(async () =>
            await service.UpdateEntityAsync(Guid.NewGuid(), BuildChannelInput(), CancellationToken.None));
    }

    [Test]
    public void DeleteEntityAsync_WhenDependenciesUnavailable_Throws()
    {
        var service = (ChannelCrudService)RuntimeHelpers.GetUninitializedObject(typeof(ChannelCrudService));

        Assert.ThrowsAsync<NullReferenceException>(async () =>
            await service.DeleteEntityAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Test]
    public void ValidateDomainAvailabilityAsync_ThrowsUnprocessable_WhenDomainIsEmpty()
    {
        var service = CreateServiceWithUnusedProviders();

        var ex = Assert.ThrowsAsync<UnprocessableEntityException>(async () =>
            await InvokeValidateDomainAvailabilityAsync(service, "", null, CancellationToken.None));

        Assert.That(ex!.Message, Is.EqualTo("Presentation URL is required."));
    }

    [Test]
    public void ResolvePrimaryLanguageIdAsync_ThrowsUnprocessable_WhenNoDefaultCultureAndNoCultures()
    {
        var service = CreateServiceWithUnusedProviders();
        var input = new ChannelInput
        {
            General = new ChannelGeneralInput
            {
                DisplayName = "A",
                CodeName = "a",
                PresentationURL = "https://a.example.com",
                DefaultContentCulture = null
            },
            DomainAliases = new ChannelDomainAliasesInput { LiveSite = [] },
            Cultures = []
        };

        var ex = Assert.ThrowsAsync<UnprocessableEntityException>(async () =>
            await InvokeResolvePrimaryLanguageIdAsync(service, input, CancellationToken.None));

        Assert.That(ex!.Message, Is.EqualTo("Default content culture is required."));
    }

    [Test]
    public void ToChannelOutput_DefaultContentCulture_IsNull_WhenPrimaryLanguageIsMissingFromDictionary()
    {
        var container = CreateContainer(
            channelId: 20,
            channelGuid: Guid.NewGuid(),
            displayName: "Channel 20",
            codeName: "channel-20",
            domain: "channel20.example.com",
            primaryLanguageId: 9);

        var result = InvokeToChannelOutput(container, new Dictionary<int, ContentLanguageInfo>(), new List<ContentLanguageInfo>());

        Assert.Multiple(() =>
        {
            Assert.That(result.General.DefaultContentCulture, Is.Null);
            Assert.That(result.General.WebsitedomainresentationURL, Is.EqualTo("channel20.example.com"));
            Assert.That(result.Cultures, Is.Empty);
        });
    }

    private static string InvokeNormalizeWebsiteChannelDomain(string value)
    {
        var method = typeof(ChannelCrudService).GetMethod("NormalizeWebsiteChannelDomain", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("NormalizeWebsiteChannelDomain method not found.");

        return (string)method.Invoke(null, [value])!;
    }

    private static string InvokeResolvePresentationUrl(ChannelInput input)
    {
        var method = typeof(ChannelCrudService).GetMethod("ResolvePresentationUrl", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("ResolvePresentationUrl method not found.");

        return (string)method.Invoke(null, [input])!;
    }

    private static ChannelOutput InvokeToChannelOutput(
        IDataContainer container,
        Dictionary<int, ContentLanguageInfo> languagesById,
        List<ContentLanguageInfo> allCultures)
    {
        var method = typeof(ChannelCrudService).GetMethod("ToChannelOutput", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("ToChannelOutput method not found.");

        return (ChannelOutput)method.Invoke(null, [container, languagesById, allCultures])!;
    }

    private static IDataContainer CreateContainer(
        int channelId,
        Guid channelGuid,
        string? displayName,
        string? codeName,
        string? domain,
        int primaryLanguageId)
    {
        var container = new Mock<IDataContainer>();
        container.Setup(c => c.GetValue(nameof(ChannelInfo.ChannelID))).Returns(channelId);
        container.Setup(c => c.GetValue(nameof(ChannelInfo.ChannelGUID))).Returns(channelGuid);
        container.Setup(c => c.GetValue(nameof(ChannelInfo.ChannelDisplayName))).Returns(displayName!);
        container.Setup(c => c.GetValue(nameof(ChannelInfo.ChannelName))).Returns(codeName!);
        container.Setup(c => c.GetValue(nameof(WebsiteChannelInfo.WebsiteChannelDomain))).Returns(domain!);
        container.Setup(c => c.GetValue(nameof(WebsiteChannelInfo.WebsiteChannelPrimaryContentLanguageID))).Returns(primaryLanguageId);

        return container.Object;
    }

    private static ChannelCrudService CreateServiceWithUnusedProviders()
    {
        var channelProvider = new Mock<IInfoProvider<ChannelInfo>>();
        var websiteChannelProvider = new Mock<IInfoProvider<WebsiteChannelInfo>>();
        var languageProvider = new Mock<IInfoProvider<ContentLanguageInfo>>();

        return new ChannelCrudService(channelProvider.Object, websiteChannelProvider.Object, languageProvider.Object);
    }

    private static ChannelInput BuildChannelInput()
    {
        return new ChannelInput
        {
            General = new ChannelGeneralInput
            {
                DisplayName = "Channel",
                CodeName = "channel",
                PresentationURL = "https://channel.example.com",
                DefaultContentCulture = "en-GB"
            },
            DomainAliases = new ChannelDomainAliasesInput
            {
                LiveSite = [new ChannelDomainAliasInput { PresentationURL = "https://channel.example.com" }]
            },
            Cultures = [new ChannelCultureInput { Code = "en-GB" }]
        };
    }

    private static async Task InvokeValidateDomainAvailabilityAsync(
        ChannelCrudService service,
        string domain,
        int? currentChannelId,
        CancellationToken cancellationToken)
    {
        var method = typeof(ChannelCrudService).GetMethod("ValidateDomainAvailabilityAsync", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("ValidateDomainAvailabilityAsync method not found.");

        var task = (Task)method.Invoke(service, [domain, currentChannelId, cancellationToken])!;
        await task;
    }

    private static async Task<int> InvokeResolvePrimaryLanguageIdAsync(
        ChannelCrudService service,
        ChannelInput input,
        CancellationToken cancellationToken)
    {
        var method = typeof(ChannelCrudService).GetMethod("ResolvePrimaryLanguageIdAsync", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("ResolvePrimaryLanguageIdAsync method not found.");

        var task = (Task<int>)method.Invoke(service, [input, cancellationToken])!;
        return await task;
    }
}
