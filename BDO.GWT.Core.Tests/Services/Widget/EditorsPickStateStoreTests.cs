using BDO.GWT.Core.Services;
using CMS.ContentEngine;
using CMS.Helpers;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BDO.GWT.Tests.Core.Services;


/// </remarks>
[TestFixture]
public class EditorsPickStateStoreTests
{


    private static readonly HashSet<ContentItemReference> EmptySet =
        new HashSet<ContentItemReference>();

    private static HashSet<ContentItemReference> MakeRefs(params Guid[] guids) =>
        guids.Select(g => new ContentItemReference { Identifier = g })
             .ToHashSet();


    private static (Mock<IProgressiveCache> mock, List<CacheSettings> capturedSettings)
        BuildPassThroughCacheMock()
    {
        var captured = new List<CacheSettings>();
        var mock = new Mock<IProgressiveCache>();

        mock.Setup(c => c.Load(
                It.IsAny<Func<CacheSettings, object>>(),
                It.IsAny<CacheSettings>()))
            .Callback<Func<CacheSettings, object>, CacheSettings>(
                (fn, cs) =>
                {
                    captured.Add(cs);
                    fn(cs); // invoke delegate so state is returned (but not stored in real cache)
                })
            .Returns<Func<CacheSettings, object>, CacheSettings>((fn, cs) => fn(cs));

        return (mock, captured);
    }


    [Test]
    public void Save_WhenStateGuidIsEmpty_DoesNotCallProgressiveCache()
    {
        var (mock, _) = BuildPassThroughCacheMock();
        var sut = new EditorsPickStateStore(mock.Object);

        sut.Save(Guid.Empty, MakeRefs(Guid.NewGuid()), MakeRefs(Guid.NewGuid()));

        mock.Verify(
            c => c.Load(It.IsAny<Func<CacheSettings, object>>(), It.IsAny<CacheSettings>()),
            Times.Never);
    }

    [Test]
    public void Save_WithValidGuid_CallsProgressiveCacheLoadOnce()
    {
        var (mock, _) = BuildPassThroughCacheMock();
        var sut = new EditorsPickStateStore(mock.Object);

        sut.Save(Guid.NewGuid(), EmptySet, EmptySet);

        mock.Verify(
            c => c.Load(It.IsAny<Func<CacheSettings, object>>(), It.IsAny<CacheSettings>()),
            Times.Once);
    }


    //[Test]
    //public void Save_WithValidGuid_UsesCacheKeyContainingGuid()
    //{
    //    var (mock, captured) = BuildPassThroughCacheMock();
    //    var sut = new EditorsPickStateStore(mock.Object);
    //    var guid = Guid.NewGuid();

    //    sut.Save(guid, EmptySet, EmptySet);

    //    Assert.That(captured, Has.Count.EqualTo(1));
    //    Assert.That(captured[0].CacheItemName, Does.Contain("BDO|EditorsPickState|"));
    //    Assert.That(captured[0].CacheItemName, Does.Contain(guid.ToString()));
    //}


    [Test]
    public void Save_WithValidGuid_SetsCacheDurationTo1440Minutes()
    {
        var (mock, captured) = BuildPassThroughCacheMock();
        var sut = new EditorsPickStateStore(mock.Object);

        sut.Save(Guid.NewGuid(), EmptySet, EmptySet);

        Assert.That(captured[0].CacheMinutes, Is.EqualTo(24 * 60));
    }


    [Test]
    public void Save_WithValidGuid_EnablesProgressiveCaching()
    {
        var (mock, captured) = BuildPassThroughCacheMock();
        var sut = new EditorsPickStateStore(mock.Object);

        sut.Save(Guid.NewGuid(), EmptySet, EmptySet);

        Assert.That(captured[0].AllowProgressiveCaching, Is.True);
    }


    [Test]
    public void Save_WithFeaturedAndSelectedGuids_DelegateReturnsCorrectState()
    {
        var featured = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var selected = new[] { Guid.NewGuid() };

        object? capturedState = null;
        var mock = new Mock<IProgressiveCache>();
        mock.Setup(c => c.Load(
                It.IsAny<Func<CacheSettings, object>>(),
                It.IsAny<CacheSettings>()))
            .Callback<Func<CacheSettings, object>, CacheSettings>(
                (fn, cs) => capturedState = fn(cs))
            .Returns<Func<CacheSettings, object>, CacheSettings>((fn, cs) => fn(cs));

        var sut = new EditorsPickStateStore(mock.Object);
        sut.Save(Guid.NewGuid(), MakeRefs(featured), MakeRefs(selected));

        // Use reflection to read the private EditorsPickState fields
        Assert.That(capturedState, Is.Not.Null);
        var type = capturedState!.GetType();

        var featuredGuids = (Guid[])type
            .GetProperty("FeaturedGuids",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)!
            .GetValue(capturedState)!;

        var selectedGuids = (Guid[])type
            .GetProperty("SelectedGuids",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)!
            .GetValue(capturedState)!;

        Assert.That(featuredGuids, Is.EquivalentTo(featured));
        Assert.That(selectedGuids, Is.EquivalentTo(selected));
    }


    [Test]
    public void Save_WithEmptySets_StoresEmptyArraysNotNull()
    {
        object? capturedState = null;
        var mock = new Mock<IProgressiveCache>();
        mock.Setup(c => c.Load(
                It.IsAny<Func<CacheSettings, object>>(),
                It.IsAny<CacheSettings>()))
            .Callback<Func<CacheSettings, object>, CacheSettings>(
                (fn, cs) => capturedState = fn(cs))
            .Returns<Func<CacheSettings, object>, CacheSettings>((fn, cs) => fn(cs));

        var sut = new EditorsPickStateStore(mock.Object);
        sut.Save(Guid.NewGuid(), EmptySet, EmptySet);

        var type = capturedState!.GetType();
        var featuredGuids = (Guid[])type
            .GetProperty("FeaturedGuids",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)!
            .GetValue(capturedState)!;
        var selectedGuids = (Guid[])type
            .GetProperty("SelectedGuids",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)!
            .GetValue(capturedState)!;

        Assert.That(featuredGuids, Is.Not.Null.And.Empty);
        Assert.That(selectedGuids, Is.Not.Null.And.Empty);
    }


    [Test]
    public void TryGet_WhenStateGuidIsEmpty_ReturnsFalse()
    {
        var mock = new Mock<IProgressiveCache>();
        var sut = new EditorsPickStateStore(mock.Object);

        var result = sut.TryGet(Guid.Empty, out _, out _);

        Assert.That(result, Is.False);
    }


    [Test]
    public void TryGet_WhenStateGuidIsEmpty_OutParamsAreEmptySets()
    {
        var mock = new Mock<IProgressiveCache>();
        var sut = new EditorsPickStateStore(mock.Object);

        sut.TryGet(Guid.Empty,
            out var featuredGuids,
            out var selectedGuids);

        Assert.That(featuredGuids, Is.Not.Null.And.Empty);
        Assert.That(selectedGuids, Is.Not.Null.And.Empty);
    }


    //[Test]
    //public void TryGet_WhenKeyNotInCache_ReturnsFalse()
    //{
    //    var mock = new Mock<IProgressiveCache>();
    //    var sut = new EditorsPickStateStore(mock.Object);

    //    // A freshly generated GUID will never exist in the cache
    //    var result = sut.TryGet(Guid.NewGuid(), out _, out _);

    //    Assert.That(result, Is.False);
    //}


    //[Test]
    //public void TryGet_WhenKeyNotInCache_OutParamsAreEmptySets()
    //{
    //    var mock = new Mock<IProgressiveCache>();
    //    var sut = new EditorsPickStateStore(mock.Object);

    //    sut.TryGet(Guid.NewGuid(), out var featuredGuids, out var selectedGuids);

    //    Assert.That(featuredGuids, Is.Not.Null.And.Empty);
    //    Assert.That(selectedGuids, Is.Not.Null.And.Empty);
    //}



    //[Test]
    //[Category("Integration")]
    //public void SaveThenTryGet_WithValidGuids_ReturnsTrueAndCorrectReferences()
    //{
    //    // Arrange — real IProgressiveCache wired to the in-process cache store
    //    var realCache = Service.Resolve<IProgressiveCache>();
    //    var sut = new EditorsPickStateStore(realCache);

    //    var stateGuid = Guid.NewGuid();
    //    var featured = new[] { Guid.NewGuid(), Guid.NewGuid() };
    //    var selected = new[] { Guid.NewGuid() };

    //    // Act
    //    sut.Save(Guid.NewGuid(), MakeRefs(featured), MakeRefs(selected));
    //    var found = sut.TryGet(stateGuid, out var outFeatured, out var outSelected);

    //    // Assert
    //    Assert.That(found, Is.True);
    //    Assert.That(
    //        outFeatured.Select(r => r.Identifier),
    //        Is.EquivalentTo(featured));
    //    Assert.That(
    //        outSelected.Select(r => r.Identifier),
    //        Is.EquivalentTo(selected));
    //}

    //// -----------------------------------------------------------------------
    //// Round-trip — TC-13  TryGet after Save with empty sets returns true
    ////                     and empty out params (not null)
    //// -----------------------------------------------------------------------
    //[Test]
    //[Category("Integration")]
    //public void SaveThenTryGet_WithEmptySets_ReturnsTrueAndEmptyOutParams()
    //{
    //    var realCache = Service.Resolve<IProgressiveCache>();
    //    var sut = new EditorsPickStateStore(realCache);
    //    var stateGuid = Guid.NewGuid();

    //    sut.Save(stateGuid, EmptySet, EmptySet);
    //    var found = sut.TryGet(stateGuid, out var outFeatured, out var outSelected);

    //    Assert.That(found, Is.True);
    //    Assert.That(outFeatured, Is.Not.Null.And.Empty);
    //    Assert.That(outSelected, Is.Not.Null.And.Empty);
    //}

    //// -----------------------------------------------------------------------
    //// Round-trip — TC-14  Two different stateGuids do not share state
    //// -----------------------------------------------------------------------
    //[Test]
    //[Category("Integration")]
    //public void Save_TwoDifferentGuids_DoNotShareCachedState()
    //{
    //    var realCache = Service.Resolve<IProgressiveCache>();
    //    var sut = new EditorsPickStateStore(realCache);

    //    var guid1 = Guid.NewGuid();
    //    var guid2 = Guid.NewGuid();
    //    var refForG1 = Guid.NewGuid();

    //    sut.Save(guid1, MakeRefs(refForG1), EmptySet);
    //    // guid2 is never saved

    //    sut.TryGet(guid2, out var outFeatured, out _);

    //    Assert.That(outFeatured, Is.Empty,
    //        "State saved under guid1 must not bleed into guid2's lookup.");
    //}

    //// -----------------------------------------------------------------------
    //// ToReferences — TC-15  ContentItemReferences are rebuilt with correct Identifiers
    ////                (tested indirectly via a Save → TryGet round-trip)
    //// -----------------------------------------------------------------------
    //[Test]
    //[Category("Integration")]
    //public void TryGet_ReturnsContentItemReferencesWithCorrectIdentifiers()
    //{
    //    var realCache = Service.Resolve<IProgressiveCache>();
    //    var sut = new EditorsPickStateStore(realCache);

    //    var stateGuid = Guid.NewGuid();
    //    var expectedGuid = Guid.NewGuid();

    //    sut.Save(stateGuid, MakeRefs(expectedGuid), EmptySet);
    //    sut.TryGet(stateGuid, out var outFeatured, out _);

    //    var reference = outFeatured.Single();
    //    Assert.That(reference.Identifier, Is.EqualTo(expectedGuid));
    //}
}