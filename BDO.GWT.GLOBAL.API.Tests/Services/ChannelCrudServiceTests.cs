using BDO.GWT.Common.Core.Exceptions;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Services;
using CMS.Base;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Websites;
using Moq;
using System.Data;
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

    [Test]
    public async Task GetEntityAsync_ReturnsNull_WhenNoChannelFound()
    {
        var channelProvider = new Mock<IInfoProvider<ChannelInfo>>();
        var websiteProvider = new Mock<IInfoProvider<WebsiteChannelInfo>>();
        var languageProvider = new Mock<IInfoProvider<ContentLanguageInfo>>();

        channelProvider.Setup(x => x.Get()).Returns(QueryReturningDataContainers<ChannelInfo>([]).Object);

        var service = new ChannelCrudService(channelProvider.Object, websiteProvider.Object, languageProvider.Object);

        var result = await service.GetEntityAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task DeleteEntityAsync_DeletesWebsiteChannels_AndChannel()
    {
        var channelGuid = Guid.NewGuid();
        var channel = CreateChannelInfo(7, channelGuid, "Channel", "channel");
        var wc1 = CreateWebsiteChannel(7, "a.example.com", 4);
        var wc2 = CreateWebsiteChannel(7, "b.example.com", 4);

        var channelProvider = new Mock<IInfoProvider<ChannelInfo>>();
        var websiteProvider = new Mock<IInfoProvider<WebsiteChannelInfo>>();
        var languageProvider = new Mock<IInfoProvider<ContentLanguageInfo>>();

        channelProvider.Setup(x => x.Get()).Returns(QueryReturning<ChannelInfo>(new ChannelInfo[] { channel }).Object);
        websiteProvider.Setup(x => x.Get()).Returns(QueryReturning<WebsiteChannelInfo>(new WebsiteChannelInfo[] { wc1, wc2 }).Object);

        var service = new ChannelCrudService(channelProvider.Object, websiteProvider.Object, languageProvider.Object);

        await service.DeleteEntityAsync(channelGuid, CancellationToken.None);

        websiteProvider.Verify(x => x.DeleteAsync(It.IsAny<WebsiteChannelInfo>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        channelProvider.Verify(x => x.DeleteAsync(It.IsAny<ChannelInfo>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void DeleteEntityAsync_ThrowsValidationException_WhenGdprSettingsClassIsMissing()
    {
        var channelGuid = Guid.NewGuid();
        var channel = CreateChannelInfo(9, channelGuid, "Channel", "channel");

        var channelProvider = new Mock<IInfoProvider<ChannelInfo>>();
        var websiteProvider = new Mock<IInfoProvider<WebsiteChannelInfo>>();
        var languageProvider = new Mock<IInfoProvider<ContentLanguageInfo>>();

        channelProvider.Setup(x => x.Get()).Returns(QueryReturning<ChannelInfo>(new ChannelInfo[] { channel }).Object);
        websiteProvider.Setup(x => x.Get()).Returns(QueryReturning<WebsiteChannelInfo>(Array.Empty<WebsiteChannelInfo>()).Object);
        channelProvider.Setup(x => x.Delete(It.IsAny<ChannelInfo>()))
            .Throws(CreateMissingGdprSettingsException());
        channelProvider.Setup(x => x.DeleteAsync(It.IsAny<ChannelInfo>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(CreateMissingGdprSettingsException());

        var service = new ChannelCrudService(channelProvider.Object, websiteProvider.Object, languageProvider.Object);

        var ex = Assert.ThrowsAsync<ValidationException>(async () =>
            await service.DeleteEntityAsync(channelGuid, CancellationToken.None));

        Assert.That(ex!.Message, Is.EqualTo("Channel delete failed because CMS class 'BDO.GDPRSettings' is not available in this environment."));
    }

    [Test]
    public void CreateEntityAsync_ThrowsUnprocessable_WhenDomainAlreadyUsed()
    {
        var channelProvider = new Mock<IInfoProvider<ChannelInfo>>();
        var websiteProvider = new Mock<IInfoProvider<WebsiteChannelInfo>>();
        var languageProvider = new Mock<IInfoProvider<ContentLanguageInfo>>();

        var language = CreateLanguageInfo(4, "en-GB", "English");
        var conflict = CreateWebsiteChannel(99, "channel.example.com", 4);

        languageProvider.Setup(x => x.Get()).Returns(QueryReturning<ContentLanguageInfo>(new ContentLanguageInfo[] { language }).Object);
        websiteProvider.Setup(x => x.Get()).Returns(QueryReturning<WebsiteChannelInfo>(new WebsiteChannelInfo[] { conflict }).Object);

        var service = new ChannelCrudService(channelProvider.Object, websiteProvider.Object, languageProvider.Object);

        var ex = Assert.ThrowsAsync<UnprocessableEntityException>(async () =>
            await service.CreateEntityAsync(BuildChannelInput(), CancellationToken.None));

        Assert.That(ex!.Message, Is.EqualTo("Website channel domain 'channel.example.com' is already used by another channel."));
    }

    [Test]
    public async Task UpdateEntityAsync_ReturnsUpdatedChannel_WhenDependenciesProvideData()
    {
        var channelGuid = Guid.NewGuid();
        var channel = CreateChannelInfo(11, channelGuid, "Old", "old");
        var language = CreateLanguageInfo(4, "en-GB", "English");
        var sameChannel = CreateWebsiteChannel(11, "channel.example.com", 4);
        var existingChannel = CreateWebsiteChannel(11, "channel.example.com", 4);
        var updatedContainer = CreateDataContainer(11, channelGuid, "Channel", "channel", "channel.example.com", 4);

        var channelProvider = new Mock<IInfoProvider<ChannelInfo>>();
        var websiteProvider = new Mock<IInfoProvider<WebsiteChannelInfo>>();
        var languageProvider = new Mock<IInfoProvider<ContentLanguageInfo>>();

        channelProvider.SetupSequence(x => x.Get())
            .Returns(QueryReturning<ChannelInfo>(new ChannelInfo[] { channel }).Object)
            .Returns(QueryReturningDataContainers<ChannelInfo>(new[] { updatedContainer }).Object);

        languageProvider.Setup(x => x.Get()).Returns(QueryReturning<ContentLanguageInfo>(new ContentLanguageInfo[] { language }).Object);

        websiteProvider.SetupSequence(x => x.Get())
            .Returns(QueryReturning<WebsiteChannelInfo>(new WebsiteChannelInfo[] { sameChannel }).Object)
            .Returns(QueryReturning<WebsiteChannelInfo>(new WebsiteChannelInfo[] { existingChannel }).Object);

        var service = new ChannelCrudService(channelProvider.Object, websiteProvider.Object, languageProvider.Object);

        var result = await service.UpdateEntityAsync(channelGuid, BuildChannelInput(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.General.ID, Is.EqualTo(11));
            Assert.That(result.General.GUID, Is.EqualTo(channelGuid));
            Assert.That(result.General.DisplayName, Is.EqualTo("Channel"));
            Assert.That(result.General.CodeName, Is.EqualTo("channel"));
            Assert.That(result.General.WebsitedomainresentationURL, Is.EqualTo("channel.example.com"));
            Assert.That(result.General.DefaultContentCulture, Is.EqualTo("en-GB"));
        });
    }

    [Test]
    public async Task GetEntitiesAsync_ReturnsMappedOutputs_WhenRowsExist()
    {
        var container = CreateDataContainer(33, Guid.NewGuid(), "Channel 33", "channel-33", "c33.example.com", 0);

        var channelProvider = new Mock<IInfoProvider<ChannelInfo>>();
        var websiteProvider = new Mock<IInfoProvider<WebsiteChannelInfo>>();
        var languageProvider = new Mock<IInfoProvider<ContentLanguageInfo>>();

        channelProvider.Setup(x => x.Get()).Returns(QueryReturningDataContainers<ChannelInfo>([container]).Object);
        languageProvider.Setup(x => x.Get()).Returns(QueryReturning<ContentLanguageInfo>(Array.Empty<ContentLanguageInfo>()).Object);

        var service = new ChannelCrudService(channelProvider.Object, websiteProvider.Object, languageProvider.Object);

        var result = (await service.GetEntitiesAsync(new ChannelsQuery(), CancellationToken.None)).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].General.ID, Is.EqualTo(33));
            Assert.That(result[0].General.DisplayName, Is.EqualTo("Channel 33"));
            Assert.That(result[0].General.CodeName, Is.EqualTo("channel-33"));
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

    private static Mock<ObjectQuery<T>> QueryReturning<T>(IEnumerable<T> rows)
        where T : BaseInfo
    {
        var query = new Mock<ObjectQuery<T>>();

        query.Setup(x => x.WhereEquals(It.IsAny<string>(), It.IsAny<object>())).Returns(query.Object);
        query.Setup(x => x.TopN(It.IsAny<int>())).Returns(query.Object);
        query.Setup(x => x.Columns(It.IsAny<string[]>())).Returns(query.Object);
        query.Setup(x => x.GetEnumerableTypedResultAsync(
                It.IsAny<CommandBehavior>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken?>()))
            .ReturnsAsync(rows);

        return query;
    }

    private static Mock<ObjectQuery<T>> QueryReturningDataContainers<T>(IEnumerable<IDataContainer> rows)
        where T : BaseInfo
    {
        var query = new Mock<ObjectQuery<T>>();

        query.Setup(x => x.WhereEquals(It.IsAny<string>(), It.IsAny<object>())).Returns(query.Object);
        query.Setup(x => x.TopN(It.IsAny<int>())).Returns(query.Object);
        query.Setup(x => x.Source(It.IsAny<Action<QuerySource>>())).Returns(query.Object);
        query.Setup(x => x.Columns(It.IsAny<string[]>())).Returns(query.Object);
        query.Setup(x => x.GetDataContainerResultAsync(
                It.IsAny<CommandBehavior>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken?>()))
            .ReturnsAsync(rows);

        return query;
    }

    private static IDataContainer CreateDataContainer(
        int channelId,
        Guid channelGuid,
        string displayName,
        string codeName,
        string domain,
        int primaryLanguageId)
    {
        var container = new Mock<IDataContainer>();
        container.Setup(c => c.GetValue(nameof(ChannelInfo.ChannelID))).Returns(channelId);
        container.Setup(c => c.GetValue(nameof(ChannelInfo.ChannelGUID))).Returns(channelGuid);
        container.Setup(c => c.GetValue(nameof(ChannelInfo.ChannelDisplayName))).Returns(displayName);
        container.Setup(c => c.GetValue(nameof(ChannelInfo.ChannelName))).Returns(codeName);
        container.Setup(c => c.GetValue(nameof(WebsiteChannelInfo.WebsiteChannelDomain))).Returns(domain);
        container.Setup(c => c.GetValue(nameof(WebsiteChannelInfo.WebsiteChannelPrimaryContentLanguageID))).Returns(primaryLanguageId);
        return container.Object;
    }

    private static FakeChannelInfo CreateChannelInfo(int channelId, Guid channelGuid, string displayName, string codeName)
    {
        var channel = (FakeChannelInfo)RuntimeHelpers.GetUninitializedObject(typeof(FakeChannelInfo));
        channel.ChannelID = channelId;
        channel.ChannelGUID = channelGuid;
        channel.ChannelDisplayName = displayName;
        channel.ChannelName = codeName;
        return channel;
    }

    private static FakeWebsiteChannelInfo CreateWebsiteChannel(int channelId, string domain, int primaryLanguageId)
    {
        var websiteChannel = (FakeWebsiteChannelInfo)RuntimeHelpers.GetUninitializedObject(typeof(FakeWebsiteChannelInfo));
        websiteChannel.WebsiteChannelChannelID = channelId;
        websiteChannel.WebsiteChannelDomain = domain;
        websiteChannel.WebsiteChannelPrimaryContentLanguageID = primaryLanguageId;
        return websiteChannel;
    }

    private static FakeContentLanguageInfo CreateLanguageInfo(int languageId, string code, string displayName)
    {
        var language = (FakeContentLanguageInfo)RuntimeHelpers.GetUninitializedObject(typeof(FakeContentLanguageInfo));
        language.ContentLanguageID = languageId;
        language.ContentLanguageGUID = Guid.NewGuid();
        language.ContentLanguageName = code;
        language.ContentLanguageDisplayName = displayName;
        return language;
    }

    private static DataClassNotFoundException CreateMissingGdprSettingsException()
    {
        var ex = (DataClassNotFoundException)RuntimeHelpers.GetUninitializedObject(typeof(DataClassNotFoundException));
        var messageField = typeof(Exception).GetField("_message", BindingFlags.Instance | BindingFlags.NonPublic);
        messageField?.SetValue(ex, "Class with name 'BDO.GDPRSettings' doesn't exist.");
        return ex;
    }

    private sealed class FakeChannelInfo : ChannelInfo
    {
        private int channelId;
        private Guid channelGuid;
        private string displayName = string.Empty;
        private string codeName = string.Empty;

        public override int ChannelID { get => channelId; set => channelId = value; }
        public override Guid ChannelGUID { get => channelGuid; set => channelGuid = value; }
        public override string ChannelDisplayName { get => displayName; set => displayName = value; }
        public override string ChannelName { get => codeName; set => codeName = value; }
    }

    private sealed class FakeWebsiteChannelInfo : WebsiteChannelInfo
    {
        private int channelId;
        private string domain = string.Empty;
        private int primaryLanguageId;

        public override int WebsiteChannelChannelID { get => channelId; set => channelId = value; }
        public override string WebsiteChannelDomain { get => domain; set => domain = value; }
        public override int WebsiteChannelPrimaryContentLanguageID { get => primaryLanguageId; set => primaryLanguageId = value; }
    }

    private sealed class FakeContentLanguageInfo : ContentLanguageInfo
    {
        private int languageId;
        private Guid languageGuid;
        private string languageName = string.Empty;
        private string displayName = string.Empty;

        public override int ContentLanguageID { get => languageId; set => languageId = value; }
        public override Guid ContentLanguageGUID { get => languageGuid; set => languageGuid = value; }
        public override string ContentLanguageName { get => languageName; set => languageName = value; }
        public override string ContentLanguageDisplayName { get => displayName; set => displayName = value; }
    }
}
