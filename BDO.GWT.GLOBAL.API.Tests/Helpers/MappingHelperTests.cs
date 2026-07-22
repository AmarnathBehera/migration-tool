using BDO.GWT.Core;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Helpers;
using CMS.ContentEngine;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Moq;
using System.Reflection;

namespace BDO.GWT.GLOBAL.API.Tests.Helpers;

public class MappingHelperTests
{
    private Mock<IContentService> _contentServiceMock = null!;
    private MappingHelper _mappingHelper = null!;

    [SetUp]
    public void Setup()
    {
        _contentServiceMock = new Mock<IContentService>();
        _mappingHelper = new MappingHelper(_contentServiceMock.Object);
    }

    private static MethodInfo GetPrivateStaticMethod(string name) =>
        typeof(MappingHelper).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new MissingMethodException(nameof(MappingHelper), name);

    private static bool InvokeUrlsMatch(string? assetUrl, string? requestedUrl)
    {
        var method = GetPrivateStaticMethod("UrlsMatch");
        return (bool)method.Invoke(null, new object?[] { assetUrl, requestedUrl })!;
    }

    [TestCase(null, "foo.jpg")]
    [TestCase("foo.jpg", null)]
    [TestCase("", "foo.jpg")]
    [TestCase("foo.jpg", "")]
    [TestCase("   ", "foo.jpg")]
    public void UrlsMatch_WithNullOrWhitespaceInput_ReturnsFalse(string? assetUrl, string? requestedUrl)
    {
        Assert.That(InvokeUrlsMatch(assetUrl, requestedUrl), Is.False);
    }

    [Test]
    public void UrlsMatch_WithIdenticalUrls_ReturnsTrue()
    {
        Assert.That(InvokeUrlsMatch("/images/foo.jpg", "/images/foo.jpg"), Is.True);
    }

    [Test]
    public void UrlsMatch_IsCaseInsensitive()
    {
        Assert.That(InvokeUrlsMatch("/Images/Foo.JPG", "/images/foo.jpg"), Is.True);
    }

    [Test]
    public void UrlsMatch_IgnoresLeadingTilde()
    {
        Assert.That(InvokeUrlsMatch("~/images/foo.jpg", "/images/foo.jpg"), Is.True);
    }

    [Test]
    public void UrlsMatch_IgnoresQueryString()
    {
        Assert.That(InvokeUrlsMatch("/images/foo.jpg?v=2", "/images/foo.jpg"), Is.True);
    }

    [Test]
    public void UrlsMatch_IgnoresTrailingSlash()
    {
        Assert.That(InvokeUrlsMatch("/images/foo.jpg/", "/images/foo.jpg"), Is.True);
    }

    [Test]
    public void UrlsMatch_FallsBackToFileNameComparison()
    {
        Assert.That(InvokeUrlsMatch("https://cdn.example.com/a/b/foo.jpg", "/local/path/foo.jpg"), Is.True);
    }

    [Test]
    public void UrlsMatch_WithDifferentFileNames_ReturnsFalse()
    {
        Assert.That(InvokeUrlsMatch("/images/foo.jpg", "/images/bar.jpg"), Is.False);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public async Task BuildContentAttachmentImageReferences_WithNullOrWhitespaceUrl_ReturnsEmptyList_AndNeverQueries(string? url)
    {
        var result = await _mappingHelper.BuildContentAttachmentImageReferences(url, CancellationToken.None);

        Assert.That(result, Is.Empty);
        _contentServiceMock.Verify(
            x => x.GetItemsAsync<ContentAttachment>(It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task BuildContentAttachmentImageReferences_WithRawGuid_ReturnsThatGuid_AndNeverQueries()
    {
        var guid = Guid.NewGuid();

        var result = await _mappingHelper.BuildContentAttachmentImageReferences(guid.ToString(), CancellationToken.None);

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Identifier, Is.EqualTo(guid));
        _contentServiceMock.Verify(
            x => x.GetItemsAsync<ContentAttachment>(It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task BuildContentAttachmentImageReferences_WithGuidSurroundedByWhitespace_IsTrimmedAndParsed()
    {
        var guid = Guid.NewGuid();

        var result = await _mappingHelper.BuildContentAttachmentImageReferences($"  {guid}  ", CancellationToken.None);

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Identifier, Is.EqualTo(guid));
    }

    [Test]
    public async Task BuildContentAttachmentImageReferences_WhenNoAttachmentMatches_ReturnsEmptyList()
    {
        _contentServiceMock
            .Setup(x => x.GetItemsAsync<ContentAttachment>(It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ContentAttachment>().AsReadOnly());

        var result = await _mappingHelper.BuildContentAttachmentImageReferences("not-a-guid-and-not-found.jpg", CancellationToken.None);

        Assert.That(result, Is.Empty);
        _contentServiceMock.Verify(
            x => x.GetItemsAsync<ContentAttachment>(It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public async Task BuildContentAttachmentPersonPageReferences_WithNullOrWhitespacePath_ReturnsEmptyList_AndNeverQueries(string? path)
    {
        var result = await _mappingHelper.BuildContentAttachmentPersonPageReferences(path, "ChannelName", "en-gb", CancellationToken.None);

        Assert.That(result, Is.Empty);
        _contentServiceMock.Verify(
            x => x.GetPagesAsync<Person>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task BuildContentAttachmentPersonPageReferences_WithRawGuid_ReturnsThatGuid_AndNeverQueries()
    {
        var guid = Guid.NewGuid();

        var result = await _mappingHelper.BuildContentAttachmentPersonPageReferences(guid.ToString(), "ChannelName", "en-gb", CancellationToken.None);

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Identifier, Is.EqualTo(guid));
        _contentServiceMock.Verify(
            x => x.GetPagesAsync<Person>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task BuildContentAttachmentPersonPageReferences_WhenNoPageMatches_ReturnsEmptyList()
    {
        _contentServiceMock
            .Setup(x => x.GetPagesAsync<Person>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Person>());

        var result = await _mappingHelper.BuildContentAttachmentPersonPageReferences("/people/unknown", "ChannelName", "en-gb", CancellationToken.None);

        Assert.That(result, Is.Empty);
        _contentServiceMock.Verify(
            x => x.GetPagesAsync<Person>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task BuildContentAttachmentPersonPageReferences_WithNullCulture_UsesDefaultCulture()
    {
        RetrievePagesParameters? capturedParameters = null;

        _contentServiceMock
            .Setup(x => x.GetPagesAsync<Person>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .Callback<RetrievePagesParameters, Action<RetrievePagesQueryParameters>, RetrievalCacheSettings, CancellationToken>(
                (parameters, _, _, _) => capturedParameters = parameters)
            .ReturnsAsync(new List<Person>());

        await _mappingHelper.BuildContentAttachmentPersonPageReferences("/people/jane-doe", "ChannelName", null, CancellationToken.None);

        Assert.That(capturedParameters, Is.Not.Null);
        Assert.That(capturedParameters!.LanguageName, Is.EqualTo("en"));
    }

    [TestCase(null)]
    public async Task BuildReferencesAsync_WithNullOrEmptyIdentifiers_ReturnsEmptyList_AndNeverQueries(string[]? identifiers)
    {
        var result = await _mappingHelper.BuildReferencesAsync<Person>(identifiers, p => p.MetadataTitle ?? string.Empty, CancellationToken.None);

        Assert.That(result, Is.Empty);
        _contentServiceMock.Verify(
            x => x.GetItemsAsync<Person>(It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task BuildReferencesAsync_WithOnlyWhitespaceIdentifiers_ReturnsEmptyList_AndNeverQueries()
    {
        var result = await _mappingHelper.BuildReferencesAsync<Person>(new[] { "", "   ", null! }, p => p.MetadataTitle ?? string.Empty, CancellationToken.None);

        Assert.That(result, Is.Empty);
        _contentServiceMock.Verify(
            x => x.GetItemsAsync<Person>(It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [TestCase(null)]
    public async Task BuildReferencesPageAsync_WithNullOrEmptyIdentifiers_ReturnsEmptyList_AndNeverQueries(string[]? identifiers)
    {
        var result = await _mappingHelper.BuildReferencesPageAsync<Person>(identifiers, p => p.MetadataTitle ?? string.Empty, CancellationToken.None);

        Assert.That(result, Is.Empty);
        _contentServiceMock.Verify(
            x => x.GetItemsAsync<Person>(It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task BuildReferencesPageAsync_WithOnlyWhitespaceIdentifiers_ReturnsEmptyList_AndNeverQueries()
    {
        var result = await _mappingHelper.BuildReferencesPageAsync<Person>(new[] { "", "   " }, p => p.MetadataTitle ?? string.Empty, CancellationToken.None);

        Assert.That(result, Is.Empty);
        _contentServiceMock.Verify(
            x => x.GetItemsAsync<Person>(It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}