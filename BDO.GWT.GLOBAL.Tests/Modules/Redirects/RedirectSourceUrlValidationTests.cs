using System.Collections;
using System.Runtime.CompilerServices;
using BDO;
using CMS.DataEngine;
using Moq;
using XperienceCommunity.Redirects.UIPages;

namespace BDO.GWT.GLOBAL.Tests.Modules.Redirects;

public class RedirectSourceUrlValidationTests
{
    [Test]
    public void SourceUrlExists_WhenSourceUrlIsNullOrWhitespace_ReturnsFalse()
    {
        var provider = new Mock<IInfoProvider<RedirectInfo>>();

        Assert.Multiple(() =>
        {
            Assert.That(RedirectSourceUrlValidation.SourceUrlExists(provider.Object, null), Is.False);
            Assert.That(RedirectSourceUrlValidation.SourceUrlExists(provider.Object, string.Empty), Is.False);
            Assert.That(RedirectSourceUrlValidation.SourceUrlExists(provider.Object, "   "), Is.False);
        });

        provider.Verify(x => x.Get(), Times.Never);
    }

    [Test]
    public void SourceUrlExists_WhenProviderThrows_ReturnsFalse()
    {
        var provider = new Mock<IInfoProvider<RedirectInfo>>();
        provider.Setup(x => x.Get()).Throws(new InvalidOperationException("query failed"));

        var result = RedirectSourceUrlValidation.SourceUrlExists(provider.Object, "/source");

        Assert.That(result, Is.False);
    }

    [Test]
    public void SourceUrlExists_WhenMatchExists_ReturnsTrue()
    {
        var provider = new Mock<IInfoProvider<RedirectInfo>>();
        var query = QueryReturning([CreateRedirectInfo()]);
        provider.Setup(x => x.Get()).Returns(query.Object);

        var result = RedirectSourceUrlValidation.SourceUrlExists(provider.Object, "/SOURCE");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            query.Verify(x => x.WhereEquals(nameof(RedirectInfo.RedirectSourceUrl), "/source"), Times.Once);
            query.Verify(x => x.TopN(1), Times.Once);
        });
    }

    [Test]
    public void SourceUrlExists_WhenExcludedRedirectIdProvided_AppliesExclusionFilter()
    {
        var provider = new Mock<IInfoProvider<RedirectInfo>>();
        var query = QueryReturning(Array.Empty<RedirectInfo>());
        provider.Setup(x => x.Get()).Returns(query.Object);

        var result = RedirectSourceUrlValidation.SourceUrlExists(provider.Object, "/source", excludedRedirectId: 99);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.False);
            query.Verify(x => x.WhereNotEquals(nameof(RedirectInfo.RedirectID), 99), Times.Once);
        });
    }

    private static Mock<ObjectQuery<RedirectInfo>> QueryReturning(IEnumerable<RedirectInfo> rows)
    {
        var data = rows.ToArray();
        var query = new Mock<ObjectQuery<RedirectInfo>>();

        query.Setup(x => x.WhereEquals(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(query.Object);
        query.Setup(x => x.WhereNotEquals(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(query.Object);
        query.Setup(x => x.TopN(It.IsAny<int>()))
            .Returns(query.Object);

        query.As<IEnumerable<RedirectInfo>>()
            .Setup(x => x.GetEnumerator())
            .Returns(() => ((IEnumerable<RedirectInfo>)data).GetEnumerator());
        query.As<IEnumerable>()
            .Setup(x => x.GetEnumerator())
            .Returns(() => data.GetEnumerator());

        return query;
    }

    private static RedirectInfo CreateRedirectInfo() =>
        (RedirectInfo)RuntimeHelpers.GetUninitializedObject(typeof(RedirectInfo));
}
