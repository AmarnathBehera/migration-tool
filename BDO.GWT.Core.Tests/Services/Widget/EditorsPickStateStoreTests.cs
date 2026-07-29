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

        Assert.Multiple(() =>
        {
            Assert.That(featuredGuids, Is.EquivalentTo(featured));
            Assert.That(selectedGuids, Is.EquivalentTo(selected));
        });
        
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

        Assert.Multiple(() =>
        {
            Assert.That(featuredGuids, Is.Not.Null.And.Empty);
            Assert.That(selectedGuids, Is.Not.Null.And.Empty);

        });
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

        Assert.Multiple(() =>
        {
            Assert.That(featuredGuids, Is.Not.Null.And.Empty);
            Assert.That(selectedGuids, Is.Not.Null.And.Empty);
        });
    }


    
}