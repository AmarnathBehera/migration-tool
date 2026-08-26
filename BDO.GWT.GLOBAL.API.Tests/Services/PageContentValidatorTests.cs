using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Services;
using System.Collections.Immutable;
using System.Reflection;

namespace BDO.GWT.GLOBAL.API.Tests.Services;

public class PageContentValidatorTests
{
    [Test]
    public void GetClassNameFromPageContentType_ResolvesGeneratedXbyKContentType()
    {
        var method = GetPrivateStaticMethod(nameof(PageContentValidator), "GetClassNameFromPageContentType");

        var className = method.Invoke(null, [typeof(DealPageContent)]);

        Assert.That(className, Is.EqualTo(Deal.CONTENT_TYPE_NAME));
    }

    [TestCase("MetadataShowTitle", nameof(DealPageContent.ShowTitle))]
    [TestCase("DealMetadataRssCategory", nameof(DealPageContent.MetadataRssCategory))]
    [TestCase("DealShowSocialMediaButtons", nameof(DealPageContent.ShowSocialMediaButtons))]
    [TestCase("RelevantContactPersonDeal", nameof(DealPageContent.RelevantContactPerson))]
    public void TryGetContentProperty_ResolvesXbyKFieldAliases(string kenticoFieldName, string expectedPropertyName)
    {
        var properties = typeof(DealPageContent)
            .GetProperties()
            .ToImmutableDictionary(property => property.Name, property => property, StringComparer.OrdinalIgnoreCase);

        var method = GetPrivateStaticMethod(nameof(PageContentValidator), "TryGetContentProperty");
        object?[] args = [properties, "Deal", kenticoFieldName, null];

        var resolved = method.Invoke(null, args);

        Assert.Multiple(() =>
        {
            Assert.That(resolved, Is.True);
            Assert.That(args[3], Is.AssignableTo<PropertyInfo>());
            Assert.That(((PropertyInfo)args[3]!).Name, Is.EqualTo(expectedPropertyName));
        });
    }

    [Test]
    public void TryGetContentProperty_ReturnsFalseForUnmappedField()
    {
        var properties = typeof(DealPageContent)
            .GetProperties()
            .ToImmutableDictionary(property => property.Name, property => property, StringComparer.OrdinalIgnoreCase);

        var method = GetPrivateStaticMethod(nameof(PageContentValidator), "TryGetContentProperty");
        object?[] args = [properties, "Deal", "FieldThatDoesNotExist", null];

        var resolved = method.Invoke(null, args);

        Assert.Multiple(() =>
        {
            Assert.That(resolved, Is.False);
            Assert.That(args[3], Is.Null);
        });
    }

    [Test]
    public void GetClassNameFromPageContentType_ThrowsForTypeWithoutContentTypeName()
    {
        var method = GetPrivateStaticMethod(nameof(PageContentValidator), "GetClassNameFromPageContentType");

        var ex = Assert.Throws<TargetInvocationException>(
            () => method.Invoke(null, [typeof(string)]));

        Assert.That(ex!.InnerException, Is.InstanceOf<InvalidOperationException>());
    }

    [TestCase("BDO.GWT.Deal", "Deal")]
    [TestCase("BDO.Insight", "Insight")]
    [TestCase("Deal", "Deal")]
    public void GetContentTypeCodeNameSegment_ReturnsLastDotSegment(string className, string expectedSegment)
    {
        var method = GetPrivateStaticMethod(nameof(PageContentValidator), "GetContentTypeCodeNameSegment");

        var segment = method.Invoke(null, [className]);

        Assert.That(segment, Is.EqualTo(expectedSegment));
    }

    [Test]
    public void TryGetContentProperty_ResolvesDirectMatch()
    {
        var properties = typeof(DealPageContent)
            .GetProperties()
            .ToImmutableDictionary(property => property.Name, property => property, StringComparer.OrdinalIgnoreCase);

        var method = GetPrivateStaticMethod(nameof(PageContentValidator), "TryGetContentProperty");
        object?[] args = [properties, "Deal", nameof(DealPageContent.DealContent), null];

        var resolved = method.Invoke(null, args);

        Assert.Multiple(() =>
        {
            Assert.That(resolved, Is.True);
            Assert.That(args[3], Is.AssignableTo<PropertyInfo>());
            Assert.That(((PropertyInfo)args[3]!).Name, Is.EqualTo(nameof(DealPageContent.DealContent)));
        });
    }

    [TestCase("MetadataTitle", nameof(InsightPageContent.MetadataTitle))]
    [TestCase("InsightMetadataRssCategory", nameof(InsightPageContent.MetadataRssCategory))]
    [TestCase("InsightShowSocialMediaButtons", nameof(InsightPageContent.ShowSocialMediaButtons))]
    [TestCase("RelevantContactPersonInsight", nameof(InsightPageContent.RelevantContactPerson))]
    public void TryGetContentProperty_ResolvesInsightXbyKFieldAliases(string kenticoFieldName, string expectedPropertyName)
    {
        var properties = typeof(InsightPageContent)
            .GetProperties()
            .ToImmutableDictionary(property => property.Name, property => property, StringComparer.OrdinalIgnoreCase);

        var method = GetPrivateStaticMethod(nameof(PageContentValidator), "TryGetContentProperty");
        object?[] args = [properties, "Insight", kenticoFieldName, null];

        var resolved = method.Invoke(null, args);

        Assert.Multiple(() =>
        {
            Assert.That(resolved, Is.True);
            Assert.That(args[3], Is.AssignableTo<PropertyInfo>());
            Assert.That(((PropertyInfo)args[3]!).Name, Is.EqualTo(expectedPropertyName));
        });
    }

    [Test]
    public void TryGetContentProperty_ReturnsFalseForUnmappedInsightField()
    {
        var properties = typeof(InsightPageContent)
            .GetProperties()
            .ToImmutableDictionary(property => property.Name, property => property, StringComparer.OrdinalIgnoreCase);

        var method = GetPrivateStaticMethod(nameof(PageContentValidator), "TryGetContentProperty");
        object?[] args = [properties, "Insight", "FieldThatDoesNotExist", null];

        var resolved = method.Invoke(null, args);

        Assert.Multiple(() =>
        {
            Assert.That(resolved, Is.False);
            Assert.That(args[3], Is.Null);
        });
    }

    private static MethodInfo GetPrivateStaticMethod(string typeName, string methodName)
    {
        return typeof(PageContentValidator).GetMethod(
            methodName,
            BindingFlags.NonPublic | BindingFlags.Static) ??
            throw new AssertionException($"Could not find {typeName}.{methodName}.");
    }
}