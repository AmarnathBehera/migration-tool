using BDO.GWT.Core.Services;
using CMS.ContentEngine;
using CMS.Helpers;
using Moq;
using NUnit.Framework;

namespace BDO.GWT.Tests.Core.Services;

[TestFixture]
public class InsightCarouselStateStoreTests
{
    private static readonly HashSet<ContentItemReference> EmptySet = new();

    private static HashSet<ContentItemReference> MakeRefs(params Guid[] guids) =>
        guids.Select(g => new ContentItemReference { Identifier = g }).ToHashSet();

    private static (Mock<IProgressiveCache> mock, List<CacheSettings> capturedSettings) BuildPassThroughCacheMock()
    {
        var captured = new List<CacheSettings>();
        var mock = new Mock<IProgressiveCache>();

        mock.Setup(c => c.Load(It.IsAny<Func<CacheSettings, object>>(), It.IsAny<CacheSettings>()))
            .Callback<Func<CacheSettings, object>, CacheSettings>((fn, cs) =>
            {
                captured.Add(cs);
                fn(cs);
            })
            .Returns<Func<CacheSettings, object>, CacheSettings>((fn, cs) => fn(cs));

        return (mock, captured);
    }

    [Test]
    public void Save_WhenStateGuidIsEmpty_DoesNotCallProgressiveCache()
    {
        var (mock, _) = BuildPassThroughCacheMock();
        var sut = new InsightCarouselStateStore(mock.Object);

        sut.Save(Guid.Empty, MakeRefs(Guid.NewGuid()));

        mock.Verify(
            c => c.Load(It.IsAny<Func<CacheSettings, object>>(), It.IsAny<CacheSettings>()),
            Times.Never);
    }

    [Test]
    public void Save_WithValidGuid_CallsProgressiveCacheLoadOnce()
    {
        var (mock, _) = BuildPassThroughCacheMock();
        var sut = new InsightCarouselStateStore(mock.Object);

        sut.Save(Guid.NewGuid(), EmptySet);

        mock.Verify(
            c => c.Load(It.IsAny<Func<CacheSettings, object>>(), It.IsAny<CacheSettings>()),
            Times.Once);
    }

    [Test]
    public void Save_WithValidGuid_SetsCacheDurationTo1440Minutes()
    {
        var (mock, captured) = BuildPassThroughCacheMock();
        var sut = new InsightCarouselStateStore(mock.Object);

        sut.Save(Guid.NewGuid(), EmptySet);

        Assert.That(captured[0].CacheMinutes, Is.EqualTo(24 * 60));
    }

    [Test]
    public void Save_WithValidGuid_EnablesProgressiveCaching()
    {
        var (mock, captured) = BuildPassThroughCacheMock();
        var sut = new InsightCarouselStateStore(mock.Object);

        sut.Save(Guid.NewGuid(), EmptySet);

        Assert.That(captured[0].AllowProgressiveCaching, Is.True);
    }

    [Test]
    public void Save_WithFeaturedGuids_DelegateReturnsCorrectState()
    {
        var featured = new[] { Guid.NewGuid(), Guid.NewGuid() };

        object? capturedState = null;
        var mock = new Mock<IProgressiveCache>();
        mock.Setup(c => c.Load(It.IsAny<Func<CacheSettings, object>>(), It.IsAny<CacheSettings>()))
            .Callback<Func<CacheSettings, object>, CacheSettings>((fn, cs) => capturedState = fn(cs))
            .Returns<Func<CacheSettings, object>, CacheSettings>((fn, cs) => fn(cs));

        var sut = new InsightCarouselStateStore(mock.Object);
        sut.Save(Guid.NewGuid(), MakeRefs(featured));

        Assert.That(capturedState, Is.Not.Null);
        var type = capturedState!.GetType();

        var featuredGuids = (Guid[])type
            .GetProperty("FeaturedGuids", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)!
            .GetValue(capturedState)!;

        Assert.That(featuredGuids, Is.EquivalentTo(featured));
    }

    [Test]
    public void TryGet_WhenStateGuidIsEmpty_ReturnsFalseAndEmptySet()
    {
        var mock = new Mock<IProgressiveCache>();
        var sut = new InsightCarouselStateStore(mock.Object);

        var result = sut.TryGet(Guid.Empty, out var featuredGuids);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.False);
            Assert.That(featuredGuids, Is.Not.Null.And.Empty);
        });
    }
}
