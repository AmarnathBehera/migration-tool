using System.Reflection;
using BDO.GWT.GLOBAL.Sync.Data;
using BDO.GWT.GLOBAL.Sync.Services;
using CMS.ContentEngine;
using CMS.Membership;
using CMS.Websites;
using Microsoft.Extensions.Logging;
using Moq;

namespace BDO.GWT.GLOBAL.Sync.Tests.Services;

[TestFixture]
public class GlobalSiteDealsServiceTests
{
    private static readonly Type ServiceType = typeof(GlobalSiteDealsService);
    private static readonly Type DealCardReferenceMapsType =
        ServiceType.GetNestedType("DealCardReferenceMaps", BindingFlags.NonPublic)
        ?? throw new InvalidOperationException("DealCardReferenceMaps not found.");

    private Mock<IContentQueryExecutor> _contentQueryExecutorMock = null!;
    private Mock<IWebPageManagerFactory> _webPageManagerFactoryMock = null!;
    private Mock<IUserInfoProvider> _userInfoProviderMock = null!;
    private Mock<ILogger<GlobalSiteDealsService>> _loggerMock = null!;
    private GlobalSiteDealsService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _contentQueryExecutorMock = new Mock<IContentQueryExecutor>();
        _webPageManagerFactoryMock = new Mock<IWebPageManagerFactory>();
        _userInfoProviderMock = new Mock<IUserInfoProvider>();
        _loggerMock = new Mock<ILogger<GlobalSiteDealsService>>();

        _service = new GlobalSiteDealsService(
            _contentQueryExecutorMock.Object,
            _webPageManagerFactoryMock.Object,
            _userInfoProviderMock.Object,
            _loggerMock.Object);
    }

    // =====================================================================
    // SynchronizeChannelGlobalDealsAsync (public entry-point)
    // =====================================================================

    [Test]
    public async Task SynchronizeChannelGlobalDealsAsync_WhenSourceChannelsEmpty_ReturnsWithoutAnyWork()
    {
        var globalChannel = CreateChannel(Guid.NewGuid(), "global");

        await _service.SynchronizeChannelGlobalDealsAsync(
            globalChannel,
            Array.Empty<ChannelOutput>(),
            forceUpdate: false,
            CancellationToken.None);

        _webPageManagerFactoryMock.VerifyNoOtherCalls();
        _contentQueryExecutorMock.VerifyNoOtherCalls();
        _userInfoProviderMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task SynchronizeChannelGlobalDealsAsync_WhenSourceChannelsEmpty_LogsInformation()
    {
        var globalChannel = CreateChannel(Guid.NewGuid(), "global");

        await _service.SynchronizeChannelGlobalDealsAsync(
            globalChannel,
            Array.Empty<ChannelOutput>(),
            forceUpdate: false,
            CancellationToken.None);

        VerifyLog(LogLevel.Information, Times.Once());
    }

    [Test]
    public async Task SynchronizeChannelGlobalDealsAsync_WhenAdminUserNotFound_ReturnsWithoutCreatingManager()
    {
        var globalChannel = CreateChannel(Guid.NewGuid(), "global");
        var sourceChannel = CreateChannel(Guid.NewGuid(), "source");

        _userInfoProviderMock
            .Setup(p => p.Get(It.IsAny<string>()))
            .Returns((UserInfo)null!);

        await _service.SynchronizeChannelGlobalDealsAsync(
            globalChannel,
            new[] { sourceChannel },
            forceUpdate: false,
            CancellationToken.None);

        _userInfoProviderMock.Verify(p => p.Get("administrator"), Times.Once);
        _webPageManagerFactoryMock.Verify(
            f => f.Create(It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);
        _contentQueryExecutorMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task SynchronizeChannelGlobalDealsAsync_WhenAdminUserNotFound_LogsError()
    {
        var globalChannel = CreateChannel(Guid.NewGuid(), "global");
        var sourceChannel = CreateChannel(Guid.NewGuid(), "source");

        _userInfoProviderMock
            .Setup(p => p.Get(It.IsAny<string>()))
            .Returns((UserInfo)null!);

        await _service.SynchronizeChannelGlobalDealsAsync(
            globalChannel,
            new[] { sourceChannel },
            forceUpdate: false,
            CancellationToken.None);

        VerifyLog(LogLevel.Error, Times.Once());
    }

    [Test]
    public async Task SynchronizeChannelGlobalDealsAsync_WhenCancellationRequested_StillHandlesEmptySourceChannels()
    {
        var globalChannel = CreateChannel(Guid.NewGuid(), "global");
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await _service.SynchronizeChannelGlobalDealsAsync(
            globalChannel,
            Array.Empty<ChannelOutput>(),
            forceUpdate: false,
            cts.Token);

        _webPageManagerFactoryMock.VerifyNoOtherCalls();
        _userInfoProviderMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task SynchronizeChannelGlobalDealsAsync_WithForceUpdateTrue_AndEmptySources_StillReturnsEarly()
    {
        var globalChannel = CreateChannel(Guid.NewGuid(), "global");

        await _service.SynchronizeChannelGlobalDealsAsync(
            globalChannel,
            Array.Empty<ChannelOutput>(),
            forceUpdate: true,
            CancellationToken.None);

        _webPageManagerFactoryMock.VerifyNoOtherCalls();
        _userInfoProviderMock.VerifyNoOtherCalls();
    }

    [Test]
    public void Service_ImplementsIGlobalSiteDealsService()
    {
        Assert.That(_service, Is.InstanceOf<IGlobalSiteDealsService>());
    }

    [Test]
    public void Constructor_WithValidDependencies_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => new GlobalSiteDealsService(
            _contentQueryExecutorMock.Object,
            _webPageManagerFactoryMock.Object,
            _userInfoProviderMock.Object,
            _loggerMock.Object));
    }

    [Test]
    public async Task SynchronizeChannelGlobalDealsAsync_WhenSourceChannelsEmpty_DoesNotAskForAdminUser()
    {
        var globalChannel = CreateChannel(Guid.NewGuid(), "global");

        await _service.SynchronizeChannelGlobalDealsAsync(
            globalChannel,
            Array.Empty<ChannelOutput>(),
            forceUpdate: false,
            CancellationToken.None);

        _userInfoProviderMock.Verify(p => p.Get(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task SynchronizeChannelGlobalDealsAsync_WhenSourceChannelsEmpty_DoesNotLogError()
    {
        var globalChannel = CreateChannel(Guid.NewGuid(), "global");

        await _service.SynchronizeChannelGlobalDealsAsync(
            globalChannel,
            Array.Empty<ChannelOutput>(),
            forceUpdate: false,
            CancellationToken.None);

        VerifyLog(LogLevel.Error, Times.Never());
    }

    [Test]
    public async Task SynchronizeChannelGlobalDealsAsync_WhenAdminUserNotFound_LooksUpAdministratorUserByName()
    {
        var globalChannel = CreateChannel(Guid.NewGuid(), "global");
        var sourceChannel = CreateChannel(Guid.NewGuid(), "source");

        _userInfoProviderMock
            .Setup(p => p.Get(It.IsAny<string>()))
            .Returns((UserInfo)null!);

        await _service.SynchronizeChannelGlobalDealsAsync(
            globalChannel,
            new[] { sourceChannel },
            forceUpdate: false,
            CancellationToken.None);

        _userInfoProviderMock.Verify(p => p.Get("administrator"), Times.Once);
        _userInfoProviderMock.Verify(p => p.Get(It.IsNotIn("administrator")), Times.Never);
    }

    [Test]
    public async Task SynchronizeChannelGlobalDealsAsync_WhenAdminUserNotFound_DoesNotLogInformation()
    {
        var globalChannel = CreateChannel(Guid.NewGuid(), "global");
        var sourceChannel = CreateChannel(Guid.NewGuid(), "source");

        _userInfoProviderMock
            .Setup(p => p.Get(It.IsAny<string>()))
            .Returns((UserInfo)null!);

        await _service.SynchronizeChannelGlobalDealsAsync(
            globalChannel,
            new[] { sourceChannel },
            forceUpdate: false,
            CancellationToken.None);

        VerifyLog(LogLevel.Information, Times.Never());
    }

    [Test]
    public async Task SynchronizeChannelGlobalDealsAsync_WhenAdminUserNotFound_WithMultipleSourceChannels_StillReturnsEarly()
    {
        var globalChannel = CreateChannel(Guid.NewGuid(), "global");
        var source1 = CreateChannel(Guid.NewGuid(), "source-1");
        var source2 = CreateChannel(Guid.NewGuid(), "source-2");
        var source3 = CreateChannel(Guid.NewGuid(), "source-3");

        _userInfoProviderMock
            .Setup(p => p.Get(It.IsAny<string>()))
            .Returns((UserInfo)null!);

        await _service.SynchronizeChannelGlobalDealsAsync(
            globalChannel,
            new[] { source1, source2, source3 },
            forceUpdate: true,
            CancellationToken.None);

        _userInfoProviderMock.Verify(p => p.Get("administrator"), Times.Once);
        _webPageManagerFactoryMock.Verify(
            f => f.Create(It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);
        _contentQueryExecutorMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task SynchronizeChannelGlobalDealsAsync_WhenAdminUserNotFound_DoesNotCreateWebPageManager()
    {
        var globalChannel = CreateChannel(Guid.NewGuid(), "global");
        var sourceChannel = CreateChannel(Guid.NewGuid(), "source");

        _userInfoProviderMock
            .Setup(p => p.Get(It.IsAny<string>()))
            .Returns((UserInfo)null!);

        await _service.SynchronizeChannelGlobalDealsAsync(
            globalChannel,
            new[] { sourceChannel },
            forceUpdate: false,
            CancellationToken.None);

        _webPageManagerFactoryMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task SynchronizeChannelGlobalDealsAsync_WhenAdminUserNotFound_DoesNotExecuteContentQueries()
    {
        var globalChannel = CreateChannel(Guid.NewGuid(), "global");
        var sourceChannel = CreateChannel(Guid.NewGuid(), "source");

        _userInfoProviderMock
            .Setup(p => p.Get(It.IsAny<string>()))
            .Returns((UserInfo)null!);

        await _service.SynchronizeChannelGlobalDealsAsync(
            globalChannel,
            new[] { sourceChannel },
            forceUpdate: false,
            CancellationToken.None);

        _contentQueryExecutorMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task SynchronizeChannelGlobalDealsAsync_WhenSourceChannelsEmpty_WithCancelledToken_ReturnsEarlyWithoutThrowing()
    {
        var globalChannel = CreateChannel(Guid.NewGuid(), "global");
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.DoesNotThrowAsync(async () =>
            await _service.SynchronizeChannelGlobalDealsAsync(
                globalChannel,
                Array.Empty<ChannelOutput>(),
                forceUpdate: false,
                cts.Token));

        await Task.CompletedTask;
    }

    [Test]
    public async Task SynchronizeChannelGlobalDealsAsync_WhenAdminUserNotFound_WithCancelledToken_ReturnsEarlyWithoutThrowing()
    {
        var globalChannel = CreateChannel(Guid.NewGuid(), "global");
        var sourceChannel = CreateChannel(Guid.NewGuid(), "source");
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        _userInfoProviderMock
            .Setup(p => p.Get(It.IsAny<string>()))
            .Returns((UserInfo)null!);

        Assert.DoesNotThrowAsync(async () =>
            await _service.SynchronizeChannelGlobalDealsAsync(
                globalChannel,
                new[] { sourceChannel },
                forceUpdate: false,
                cts.Token));

        await Task.CompletedTask;
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task SynchronizeChannelGlobalDealsAsync_WhenSourceChannelsEmpty_ForBothForceUpdateFlags_LogsInformation(bool forceUpdate)
    {
        var globalChannel = CreateChannel(Guid.NewGuid(), "global");

        await _service.SynchronizeChannelGlobalDealsAsync(
            globalChannel,
            Array.Empty<ChannelOutput>(),
            forceUpdate: forceUpdate,
            CancellationToken.None);

        VerifyLog(LogLevel.Information, Times.Once());
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task SynchronizeChannelGlobalDealsAsync_WhenAdminUserNotFound_ForBothForceUpdateFlags_LogsError(bool forceUpdate)
    {
        var globalChannel = CreateChannel(Guid.NewGuid(), "global");
        var sourceChannel = CreateChannel(Guid.NewGuid(), "source");

        _userInfoProviderMock
            .Setup(p => p.Get(It.IsAny<string>()))
            .Returns((UserInfo)null!);

        await _service.SynchronizeChannelGlobalDealsAsync(
            globalChannel,
            new[] { sourceChannel },
            forceUpdate: forceUpdate,
            CancellationToken.None);

        VerifyLog(LogLevel.Error, Times.Once());
    }

    // =====================================================================
    // BuildDealsDiff (private static)
    // =====================================================================

    [Test]
    public void BuildDealsDiff_WhenAllInputsEmpty_ReturnsAllEmpty()
    {
        var (toCreate, toUpdate, toRemove) = InvokeBuildDealsDiff(
            remoteDeals: new List<RemoteDeal>(),
            sourceDeals: new List<(Deal Deal, Guid ChannelGuid)>(),
            forceUpdate: false);

        Assert.Multiple(() =>
        {
            Assert.That(toCreate, Is.Empty);
            Assert.That(toUpdate, Is.Empty);
            Assert.That(toRemove, Is.Empty);
        });
    }

    [Test]
    public void BuildDealsDiff_WhenSourceHasNoMatchingRemote_AllGoToCreate()
    {
        var sourceDeal = CreateDeal(Guid.NewGuid());
        var source = new List<(Deal Deal, Guid ChannelGuid)> { (sourceDeal, Guid.NewGuid()) };

        var (toCreate, toUpdate, toRemove) = InvokeBuildDealsDiff(
            new List<RemoteDeal>(), source, forceUpdate: false);

        Assert.Multiple(() =>
        {
            Assert.That(toCreate, Has.Count.EqualTo(1));
            Assert.That(toUpdate, Is.Empty);
            Assert.That(toRemove, Is.Empty);
        });
    }

    [Test]
    public void BuildDealsDiff_WhenRemoteHasNoMatchingSource_AllGoToRemove()
    {
        var remote = new List<RemoteDeal> { CreateRemoteDeal(Guid.NewGuid()) };

        var (toCreate, toUpdate, toRemove) = InvokeBuildDealsDiff(
            remote, new List<(Deal Deal, Guid ChannelGuid)>(), forceUpdate: false);

        Assert.Multiple(() =>
        {
            Assert.That(toCreate, Is.Empty);
            Assert.That(toUpdate, Is.Empty);
            Assert.That(toRemove, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void BuildDealsDiff_WhenMatchingAndNoUpdateSignals_ProducesNoUpdate()
    {
        var pageGuid = Guid.NewGuid();
        var source = new List<(Deal Deal, Guid ChannelGuid)>
        {
            (CreateDeal(pageGuid, lastPublishedWhen: new DateTime(2024, 1, 1)), Guid.NewGuid())
        };
        var remote = new List<RemoteDeal>
        {
            CreateRemoteDeal(pageGuid,
                originalModifiedWhen: new DateTime(2024, 6, 1),
                forceUpdateOnNextSync: false)
        };

        var (toCreate, toUpdate, toRemove) = InvokeBuildDealsDiff(remote, source, forceUpdate: false);

        Assert.Multiple(() =>
        {
            Assert.That(toCreate, Is.Empty);
            Assert.That(toUpdate, Is.Empty);
            Assert.That(toRemove, Is.Empty);
        });
    }

    [Test]
    public void BuildDealsDiff_WhenForceUpdateTrue_MatchingProducesUpdate()
    {
        var pageGuid = Guid.NewGuid();
        var source = new List<(Deal Deal, Guid ChannelGuid)>
        {
            (CreateDeal(pageGuid, lastPublishedWhen: new DateTime(2024, 1, 1)), Guid.NewGuid())
        };
        var remote = new List<RemoteDeal>
        {
            CreateRemoteDeal(pageGuid,
                originalModifiedWhen: new DateTime(2024, 6, 1),
                forceUpdateOnNextSync: false)
        };

        var (toCreate, toUpdate, toRemove) = InvokeBuildDealsDiff(remote, source, forceUpdate: true);

        Assert.Multiple(() =>
        {
            Assert.That(toCreate, Is.Empty);
            Assert.That(toUpdate, Has.Count.EqualTo(1));
            Assert.That(toRemove, Is.Empty);
        });
    }

    [Test]
    public void BuildDealsDiff_WhenExistingHasForceUpdateOnNextSync_ProducesUpdate()
    {
        var pageGuid = Guid.NewGuid();
        var source = new List<(Deal Deal, Guid ChannelGuid)>
        {
            (CreateDeal(pageGuid, lastPublishedWhen: new DateTime(2024, 1, 1)), Guid.NewGuid())
        };
        var remote = new List<RemoteDeal>
        {
            CreateRemoteDeal(pageGuid,
                originalModifiedWhen: new DateTime(2024, 6, 1),
                forceUpdateOnNextSync: true)
        };

        var (_, toUpdate, _) = InvokeBuildDealsDiff(remote, source, forceUpdate: false);

        Assert.That(toUpdate, Has.Count.EqualTo(1));
    }

    [Test]
    public void BuildDealsDiff_WhenSourceLastPublishedNewerThanRemote_ProducesUpdate()
    {
        var pageGuid = Guid.NewGuid();
        var source = new List<(Deal Deal, Guid ChannelGuid)>
        {
            (CreateDeal(pageGuid, lastPublishedWhen: new DateTime(2024, 12, 1)), Guid.NewGuid())
        };
        var remote = new List<RemoteDeal>
        {
            CreateRemoteDeal(pageGuid,
                originalModifiedWhen: new DateTime(2024, 6, 1),
                forceUpdateOnNextSync: false)
        };

        var (_, toUpdate, _) = InvokeBuildDealsDiff(remote, source, forceUpdate: false);

        Assert.That(toUpdate, Has.Count.EqualTo(1));
    }

    [Test]
    public void BuildDealsDiff_MixedInputs_PartitionsCorrectly()
    {
        var matchingGuid = Guid.NewGuid();
        var sourceOnlyGuid = Guid.NewGuid();
        var remoteOnlyGuid = Guid.NewGuid();

        var source = new List<(Deal Deal, Guid ChannelGuid)>
        {
            (CreateDeal(matchingGuid, lastPublishedWhen: new DateTime(2024, 12, 1)), Guid.NewGuid()),
            (CreateDeal(sourceOnlyGuid), Guid.NewGuid()),
        };
        var remote = new List<RemoteDeal>
        {
            CreateRemoteDeal(matchingGuid,
                originalModifiedWhen: new DateTime(2024, 6, 1)),
            CreateRemoteDeal(remoteOnlyGuid),
        };

        var (toCreate, toUpdate, toRemove) = InvokeBuildDealsDiff(remote, source, forceUpdate: false);

        Assert.Multiple(() =>
        {
            Assert.That(toCreate, Has.Count.EqualTo(1));
            Assert.That(toUpdate, Has.Count.EqualTo(1));
            Assert.That(toRemove, Has.Count.EqualTo(1));
        });
    }

    // =====================================================================
    // CollectDistinctCodeNames (private static)
    // =====================================================================

    [Test]
    public void CollectDistinctCodeNames_WhenFieldSelectorReturnsNull_ReturnsEmpty()
    {
        var deals = new[] { CreateDeal(Guid.NewGuid()) };

        var result = InvokeCollectDistinctCodeNames<DealCardIndustry>(
            deals,
            _ => null,
            i => i.CodeName);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void CollectDistinctCodeNames_FiltersBlankAndNullNames()
    {
        var deals = new[] { CreateDeal(Guid.NewGuid()) };
        var industries = new List<DealCardIndustry>
        {
            new() { CodeName = "  " },
            new() { CodeName = string.Empty },
            new() { CodeName = null! },
            new() { CodeName = "Valid" },
        };

        var result = InvokeCollectDistinctCodeNames<DealCardIndustry>(
            deals,
            _ => industries,
            i => i.CodeName);

        Assert.That(result, Is.EquivalentTo(new[] { "Valid" }));
    }

    [Test]
    public void CollectDistinctCodeNames_TrimsAndDeduplicatesIgnoringCase()
    {
        var deals = new[] { CreateDeal(Guid.NewGuid()), CreateDeal(Guid.NewGuid()) };
        var industries = new List<DealCardIndustry>
        {
            new() { CodeName = " Retail " },
            new() { CodeName = "retail" },
            new() { CodeName = "RETAIL" },
            new() { CodeName = "Banking" },
        };

        var result = InvokeCollectDistinctCodeNames<DealCardIndustry>(
            deals,
            _ => industries,
            i => i.CodeName);

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result, Does.Contain("Retail"));
        Assert.That(result, Does.Contain("Banking"));
    }

    // =====================================================================
    // ResolveReferences (private static)
    // =====================================================================

    [Test]
    public void ResolveReferences_WhenSourceItemsNull_ReturnsEmpty()
    {
        var dict = new Dictionary<string, ContentItemReference>();

        var result = InvokeResolveReferences<DealCardIndustry>(
            sourceItems: null,
            codeNameSelector: i => i.CodeName,
            referenceByCodeName: dict);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void ResolveReferences_SkipsUnknownCodeNames()
    {
        var known = new ContentItemReference { Identifier = Guid.NewGuid() };
        var dict = new Dictionary<string, ContentItemReference>(StringComparer.OrdinalIgnoreCase)
        {
            { "Known", known },
        };
        var items = new List<DealCardIndustry>
        {
            new() { CodeName = "Known" },
            new() { CodeName = "Unknown" },
        };

        var result = InvokeResolveReferences<DealCardIndustry>(items, i => i.CodeName, dict);

        Assert.That(result, Is.EquivalentTo(new[] { known }));
    }

    [Test]
    public void ResolveReferences_DistinctReferencesAreReturnedOnce()
    {
        var shared = new ContentItemReference { Identifier = Guid.NewGuid() };
        var dict = new Dictionary<string, ContentItemReference>(StringComparer.OrdinalIgnoreCase)
        {
            { "A", shared },
            { "B", shared },
        };
        var items = new List<DealCardIndustry>
        {
            new() { CodeName = "A" },
            new() { CodeName = "B" },
        };

        var result = InvokeResolveReferences<DealCardIndustry>(items, i => i.CodeName, dict);

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0], Is.SameAs(shared));
    }

    // =====================================================================
    // DeleteRemovedDealsAsync (private instance)
    // =====================================================================

    [Test]
    public async Task DeleteRemovedDealsAsync_WhenListIsEmpty_DoesNotInvokeManager()
    {
        var managerMock = new Mock<IWebPageManager>();

        await InvokeDeleteRemovedDealsAsync(managerMock.Object, new List<RemoteDeal>());

        managerMock.VerifyNoOtherCalls();
        VerifyLog(LogLevel.Error, Times.Never());
    }

    [Test]
    public async Task DeleteRemovedDealsAsync_WhenDeleteSucceeds_CallsDeleteOncePerItem()
    {
        var managerMock = new Mock<IWebPageManager>();
        managerMock
            .Setup(m => m.Delete(It.IsAny<DeleteWebPageParameters>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var deals = new List<RemoteDeal>
        {
            CreateRemoteDeal(Guid.NewGuid(), webPageItemID: 11),
            CreateRemoteDeal(Guid.NewGuid(), webPageItemID: 22),
        };

        await InvokeDeleteRemovedDealsAsync(managerMock.Object, deals);

        managerMock.Verify(
            m => m.Delete(It.IsAny<DeleteWebPageParameters>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
        VerifyLog(LogLevel.Error, Times.Never());
    }

    [Test]
    public async Task DeleteRemovedDealsAsync_WhenDeleteThrows_LogsErrorAndContinues()
    {
        var managerMock = new Mock<IWebPageManager>();
        managerMock
            .Setup(m => m.Delete(It.IsAny<DeleteWebPageParameters>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        var deals = new List<RemoteDeal>
        {
            CreateRemoteDeal(Guid.NewGuid(), webPageItemID: 11),
            CreateRemoteDeal(Guid.NewGuid(), webPageItemID: 22),
        };

        await InvokeDeleteRemovedDealsAsync(managerMock.Object, deals);

        managerMock.Verify(
            m => m.Delete(It.IsAny<DeleteWebPageParameters>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
        VerifyLog(LogLevel.Error, Times.Exactly(2));
    }

    [Test]
    public void DeleteRemovedDealsAsync_WhenCancellationRequested_PropagatesOperationCanceled()
    {
        var managerMock = new Mock<IWebPageManager>();
        managerMock
            .Setup(m => m.Delete(It.IsAny<DeleteWebPageParameters>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var deals = new List<RemoteDeal> { CreateRemoteDeal(Guid.NewGuid(), webPageItemID: 5) };

        Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await InvokeDeleteRemovedDealsAsync(managerMock.Object, deals));
    }

    // =====================================================================
    // UpdateOutdatedDealsAsync (private instance)
    // =====================================================================

    [Test]
    public async Task UpdateOutdatedDealsAsync_WhenListIsEmpty_DoesNotInvokeManagerOrLog()
    {
        var managerMock = new Mock<IWebPageManager>();

        await InvokeUpdateOutdatedDealsAsync(
            managerMock.Object,
            new List<(Deal Deal, Guid ChannelGuid, RemoteDeal Existing)>(),
            CreateEmptyReferenceMaps());

        managerMock.VerifyNoOtherCalls();
        VerifyLog(LogLevel.Error, Times.Never());
    }

    [Test]
    public async Task UpdateOutdatedDealsAsync_WhenBuildOrManagerFails_LogsErrorAndContinues()
    {
        // BuildRemoteDealItemData relies on Kentico's GetUrl() extension which is not
        // available in unit-test context; when it (or a manager call) fails the service
        // must log the error and continue processing the rest of the batch.
        var managerMock = new Mock<IWebPageManager>();
        managerMock
            .Setup(m => m.TryCreateDraft(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        managerMock
            .Setup(m => m.TryUpdateDraft(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<UpdateDraftData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var entries = new List<(Deal Deal, Guid ChannelGuid, RemoteDeal Existing)>
        {
            (CreateDeal(Guid.NewGuid()), Guid.NewGuid(), CreateRemoteDeal(Guid.NewGuid(), webPageItemID: 42)),
        };

        await InvokeUpdateOutdatedDealsAsync(managerMock.Object, entries, CreateEmptyReferenceMaps());

        VerifyLog(LogLevel.Error, Times.AtLeastOnce());
    }

    // =====================================================================
    // CreateMissingDealsAsync (private instance)
    // =====================================================================

    [Test]
    public async Task CreateMissingDealsAsync_WhenListIsEmpty_DoesNotInvokeManagerOrLog()
    {
        var managerMock = new Mock<IWebPageManager>();

        await InvokeCreateMissingDealsAsync(
            managerMock.Object,
            sectionDealsWebPageItemID: 123,
            dealsToCreate: new List<(Deal Deal, Guid ChannelGuid)>(),
            references: CreateEmptyReferenceMaps());

        managerMock.VerifyNoOtherCalls();
        VerifyLog(LogLevel.Error, Times.Never());
    }

    [Test]
    public async Task CreateMissingDealsAsync_WhenBuildOrManagerFails_LogsErrorAndContinues()
    {
        var managerMock = new Mock<IWebPageManager>();
        managerMock
            .Setup(m => m.Create(It.IsAny<CreateWebPageParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(999);
        managerMock
            .Setup(m => m.TryPublish(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var entries = new List<(Deal Deal, Guid ChannelGuid)>
        {
            (CreateDeal(Guid.NewGuid()), Guid.NewGuid()),
            (CreateDeal(Guid.NewGuid()), Guid.NewGuid()),
        };

        await InvokeCreateMissingDealsAsync(
            managerMock.Object,
            sectionDealsWebPageItemID: 100,
            dealsToCreate: entries,
            references: CreateEmptyReferenceMaps());

        VerifyLog(LogLevel.Error, Times.AtLeastOnce());
    }

    // =====================================================================
    // Reflection helpers
    // =====================================================================

    private static (
        IList<object> ToCreate,
        IList<object> ToUpdate,
        IList<RemoteDeal> ToRemove) InvokeBuildDealsDiff(
            List<RemoteDeal> remoteDeals,
            List<(Deal Deal, Guid ChannelGuid)> sourceDeals,
            bool forceUpdate)
    {
        var method = ServiceType.GetMethod(
            "BuildDealsDiff",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("BuildDealsDiff not found.");

        var tuple = method.Invoke(null, new object[] { remoteDeals, sourceDeals, forceUpdate })!;
        var tupleType = tuple.GetType();

        var toCreate = ((System.Collections.IEnumerable)tupleType.GetField("Item1")!.GetValue(tuple)!)
            .Cast<object>().ToList();
        var toUpdate = ((System.Collections.IEnumerable)tupleType.GetField("Item2")!.GetValue(tuple)!)
            .Cast<object>().ToList();
        var toRemove = ((System.Collections.IEnumerable)tupleType.GetField("Item3")!.GetValue(tuple)!)
            .Cast<RemoteDeal>().ToList();

        return (toCreate, toUpdate, toRemove);
    }

    private static List<string> InvokeCollectDistinctCodeNames<T>(
        IEnumerable<Deal> deals,
        Func<Deal, IEnumerable<T>?> fieldSelector,
        Func<T, string> codeNameSelector)
    {
        var method = ServiceType
            .GetMethod("CollectDistinctCodeNames", BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(typeof(T));
        return (List<string>)method.Invoke(null, new object[] { deals, fieldSelector, codeNameSelector })!;
    }

    private static List<ContentItemReference> InvokeResolveReferences<T>(
        IEnumerable<T>? sourceItems,
        Func<T, string> codeNameSelector,
        IReadOnlyDictionary<string, ContentItemReference> referenceByCodeName)
    {
        var method = ServiceType
            .GetMethod("ResolveReferences", BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(typeof(T));
        return (List<ContentItemReference>)method.Invoke(
            null,
            new object?[] { sourceItems, codeNameSelector, referenceByCodeName })!;
    }

    private Task InvokeDeleteRemovedDealsAsync(IWebPageManager manager, List<RemoteDeal> deals)
    {
        var method = ServiceType.GetMethod(
            "DeleteRemovedDealsAsync",
            BindingFlags.NonPublic | BindingFlags.Instance)!;
        return (Task)method.Invoke(_service, new object[] { manager, deals, CancellationToken.None })!;
    }

    private Task InvokeUpdateOutdatedDealsAsync(
        IWebPageManager manager,
        List<(Deal Deal, Guid ChannelGuid, RemoteDeal Existing)> entries,
        object referenceMaps)
    {
        var method = ServiceType.GetMethod(
            "UpdateOutdatedDealsAsync",
            BindingFlags.NonPublic | BindingFlags.Instance)!;
        return (Task)method.Invoke(_service, new object[] { manager, entries, referenceMaps, CancellationToken.None })!;
    }

    private Task InvokeCreateMissingDealsAsync(
        IWebPageManager manager,
        int sectionDealsWebPageItemID,
        List<(Deal Deal, Guid ChannelGuid)> dealsToCreate,
        object references)
    {
        var method = ServiceType.GetMethod(
            "CreateMissingDealsAsync",
            BindingFlags.NonPublic | BindingFlags.Instance)!;
        return (Task)method.Invoke(
            _service,
            new object[] { manager, sectionDealsWebPageItemID, dealsToCreate, references, CancellationToken.None })!;
    }

    private static object CreateEmptyReferenceMaps()
    {
        var empty = new Dictionary<string, ContentItemReference>(StringComparer.OrdinalIgnoreCase);
        return Activator.CreateInstance(DealCardReferenceMapsType, empty, empty, empty)!;
    }

    // =====================================================================
    // Test data & log verification helpers
    // =====================================================================

#pragma warning disable CA1873 // Moq Verify expression, not a real log call
    private void VerifyLog(LogLevel level, Times times) =>
        _loggerMock.Verify(
            l => l.Log(
                level,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);
#pragma warning restore CA1873

    private static ChannelOutput CreateChannel(Guid guid, string codeName) => new()
    {
        General = new SiteGeneralOutput
        {
            ID = 1,
            GUID = guid,
            DisplayName = codeName,
            CodeName = codeName,
            WebsitedomainresentationURL = $"https://{codeName}.example.com",
        },
    };

    private static Deal CreateDeal(Guid pageGuid, DateTime? lastPublishedWhen = null) => new()
    {
        SystemFields = new WebPageFields
        {
            WebPageItemGUID = pageGuid,
            ContentItemCommonDataLastPublishedWhen = lastPublishedWhen ?? new DateTime(2024, 1, 1),
        },
    };

    private static RemoteDeal CreateRemoteDeal(
        Guid originalPageGuid,
        DateTime? originalModifiedWhen = null,
        bool forceUpdateOnNextSync = false,
        int webPageItemID = 0) => new()
    {
        SystemFields = new WebPageFields
        {
            WebPageItemGUID = Guid.NewGuid(),
            WebPageItemID = webPageItemID,
        },
        OriginalPageGUID = originalPageGuid,
        OriginalModifiedWhen = originalModifiedWhen ?? new DateTime(2024, 6, 1),
        ForceUpdateOnNextSync = forceUpdateOnNextSync,
    };
}
