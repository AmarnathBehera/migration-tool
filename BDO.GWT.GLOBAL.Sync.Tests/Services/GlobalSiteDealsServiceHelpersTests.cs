using System.Reflection;
using BDO.GWT.GLOBAL.Sync.Services;
using CMS.ContentEngine;
using CMS.Websites;

namespace BDO.GWT.GLOBAL.Sync.Tests.Services;

/// <summary>
/// Covers the private static helpers of <see cref="GlobalSiteDealsService"/>
/// (BuildDealsDiff, CollectDistinctCodeNames, ResolveReferences) via reflection.
/// These methods contain the bulk of the branching logic in the service and
/// are otherwise uncovered because the orchestration path depends on the
/// non-mockable <c>GetMappedResult&lt;T&gt;</c> extension method.
/// </summary>
[TestFixture]
public class GlobalSiteDealsServiceHelpersTests
{
    private static readonly Type ServiceType = typeof(GlobalSiteDealsService);

    // ---------------------------------------------------------------------
    // BuildDealsDiff
    // ---------------------------------------------------------------------

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

    // ---------------------------------------------------------------------
    // CollectDistinctCodeNames
    // ---------------------------------------------------------------------

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

    // ---------------------------------------------------------------------
    // ResolveReferences
    // ---------------------------------------------------------------------

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

    // ---------------------------------------------------------------------
    // Reflection helpers
    // ---------------------------------------------------------------------

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
        bool forceUpdateOnNextSync = false) => new()
    {
        SystemFields = new WebPageFields
        {
            WebPageItemGUID = Guid.NewGuid(),
        },
        OriginalPageGUID = originalPageGuid,
        OriginalModifiedWhen = originalModifiedWhen ?? new DateTime(2024, 6, 1),
        ForceUpdateOnNextSync = forceUpdateOnNextSync,
    };
}
