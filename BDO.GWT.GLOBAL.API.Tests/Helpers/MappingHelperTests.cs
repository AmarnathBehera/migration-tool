using BDO.GWT.Core;
using BDO.GWT.Core.Services;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Helpers;
using BDO.GWT.GLOBAL.API.Services;
using CMS.ContentEngine;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Moq;
using System.Reflection;

namespace BDO.GWT.GLOBAL.API.Tests.Helpers;

public class MappingHelperTests
{
    private Mock<IContentService> _contentServiceMock = null!;
    private Mock<ICultureService> _cultureServiceMock = null!;
    private Mock<IWebsiteChannelContext> _websiteChannelContextMock = null!;
    private Mock<IValidationState> _validationStateMock = null!;
    private MappingHelper _mappingHelper = null!;

    private static readonly string[] WhitespaceIdentifiersWithNull = new[] { "", "   ", null! };
    private static readonly string[] WhitespaceIdentifiers = new[] { "", "   " };
    private static readonly string[] SampleIdentifiers = new[] { "a", "b" };
    private static readonly int[] SampleWebPageIds = new[] { 1, 2 };
    private static readonly int[] SingleWebPageId = new[] { 1 };

    [SetUp]
    public void Setup()
    {
        _contentServiceMock = new Mock<IContentService>();
        _cultureServiceMock = new Mock<ICultureService>();
        _websiteChannelContextMock = new Mock<IWebsiteChannelContext>();
        _validationStateMock = new Mock<IValidationState>();
        _mappingHelper = new MappingHelper(
            _contentServiceMock.Object,
            _cultureServiceMock.Object,
            _websiteChannelContextMock.Object,
            _validationStateMock.Object);
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
        var result = await _mappingHelper.BuildReferencesAsync<Person>(WhitespaceIdentifiersWithNull, p => p.MetadataTitle ?? string.Empty, CancellationToken.None);

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
        var result = await _mappingHelper.BuildReferencesPageAsync<Person>(WhitespaceIdentifiers, p => p.MetadataTitle ?? string.Empty, CancellationToken.None);

        Assert.That(result, Is.Empty);
        _contentServiceMock.Verify(
            x => x.GetItemsAsync<Person>(It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ---------------------------------------------------------------------
    // BuildContentAttachmentImageReferencesExtGuid
    // ---------------------------------------------------------------------

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public async Task BuildImageReferencesExtGuid_WithNullOrWhitespace_ReturnsEmpty_AndNeverQueries(string? url)
    {
        var result = await _mappingHelper.BuildContentAttachmentImageReferencesExtGuid(url, CancellationToken.None);

        Assert.That(result, Is.Empty);
        _contentServiceMock.Verify(
            x => x.GetItemsAsync<ContentAttachment>(It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task BuildImageReferencesExtGuid_WithRawGuidThatExists_ReturnsThatGuid()
    {
        var guid = Guid.NewGuid();
        _contentServiceMock
            .Setup(x => x.GetItemsAsync<ContentAttachment>(It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ContentAttachment> { new ContentAttachment() }.AsReadOnly());

        var result = await _mappingHelper.BuildContentAttachmentImageReferencesExtGuid($"  {guid}  ", CancellationToken.None);

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Identifier, Is.EqualTo(guid));
    }

    [Test]
    public async Task BuildImageReferencesExtGuid_WithRawGuidThatDoesNotExist_ReturnsEmpty()
    {
        var guid = Guid.NewGuid();
        _contentServiceMock
            .Setup(x => x.GetItemsAsync<ContentAttachment>(It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ContentAttachment>().AsReadOnly());

        var result = await _mappingHelper.BuildContentAttachmentImageReferencesExtGuid(guid.ToString(), CancellationToken.None);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task BuildImageReferencesExtGuid_WithGetContentAssetUrlThatExists_ReturnsExtractedGuid()
    {
        var guid = Guid.NewGuid();
        _contentServiceMock
            .Setup(x => x.GetItemsAsync<ContentAttachment>(It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ContentAttachment> { new ContentAttachment() }.AsReadOnly());

        var url = $"~/getContentAsset/{guid}/abc/foo.jpg?width=100";
        var result = await _mappingHelper.BuildContentAttachmentImageReferencesExtGuid(url, CancellationToken.None);

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Identifier, Is.EqualTo(guid));
    }

    [Test]
    public async Task BuildImageReferencesExtGuid_WithUnmatchedUrl_ReturnsEmpty()
    {
        _contentServiceMock
            .Setup(x => x.GetItemsAsync<ContentAttachment>(It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ContentAttachment>().AsReadOnly());

        var result = await _mappingHelper.BuildContentAttachmentImageReferencesExtGuid("no-match.jpg", CancellationToken.None);

        Assert.That(result, Is.Empty);
    }

    // ---------------------------------------------------------------------
    // BuildWebPageIdReferencesAsync
    // ---------------------------------------------------------------------

    [Test]
    public async Task BuildWebPageIdReferences_WithNullIds_ReturnsEmpty_AndNeverQueries()
    {
        var result = await _mappingHelper.BuildWebPageIdReferencesAsync<Person>(null, "ChannelName", "en-gb", CancellationToken.None);

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
    public async Task BuildWebPageIdReferences_WithEmptyArray_ReturnsEmpty_AndNeverQueries()
    {
        var result = await _mappingHelper.BuildWebPageIdReferencesAsync<Person>(Array.Empty<int>(), "ChannelName", "en-gb", CancellationToken.None);

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
    public async Task BuildWebPageIdReferences_WhenNoPagesFound_ReturnsEmpty_AndQueriesOnce()
    {
        _contentServiceMock
            .Setup(x => x.GetPagesAsync<Person>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Person>());

        var result = await _mappingHelper.BuildWebPageIdReferencesAsync<Person>(SampleWebPageIds, "ChannelName", "en-gb", CancellationToken.None);

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
    public async Task BuildWebPageIdReferences_WithNullCulture_UsesDefaultCulture()
    {
        RetrievePagesParameters? captured = null;
        _contentServiceMock
            .Setup(x => x.GetPagesAsync<Person>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .Callback<RetrievePagesParameters, Action<RetrievePagesQueryParameters>, RetrievalCacheSettings, CancellationToken>(
                (parameters, _, _, _) => captured = parameters)
            .ReturnsAsync(new List<Person>());

        await _mappingHelper.BuildWebPageIdReferencesAsync<Person>(SingleWebPageId, "ChannelName", null, CancellationToken.None);

        Assert.That(captured, Is.Not.Null);
        Assert.That(captured!.LanguageName, Is.EqualTo("en"));
    }

    // ---------------------------------------------------------------------
    // BuildWebPageSingleIdReferencesAsync
    // ---------------------------------------------------------------------

    [TestCase(null)]
    [TestCase(0)]
    public async Task BuildWebPageSingleIdReferences_WithNullOrZero_ReturnsEmpty_AndNeverQueries(int? id)
    {
        var result = await _mappingHelper.BuildWebPageSingleIdReferencesAsync<Person>(id, "ChannelName", "en-gb", CancellationToken.None);

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
    public async Task BuildWebPageSingleIdReferences_WhenNoPageFound_ReturnsEmpty_AndQueriesOnce()
    {
        _contentServiceMock
            .Setup(x => x.GetPagesAsync<Person>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Person>());

        var result = await _mappingHelper.BuildWebPageSingleIdReferencesAsync<Person>(5, "ChannelName", "en-gb", CancellationToken.None);

        Assert.That(result, Is.Empty);
        _contentServiceMock.Verify(
            x => x.GetPagesAsync<Person>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ---------------------------------------------------------------------
    // BuildReusableContentItemNameReferenceAsync
    // ---------------------------------------------------------------------

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public async Task BuildReusableContentItemNameReference_WithNullOrWhitespace_ReturnsEmpty_AndNeverQueries(string? name)
    {
        var result = await _mappingHelper.BuildReusableContentItemNameReferenceAsync<ContentAttachment>(name, CancellationToken.None);

        Assert.That(result, Is.Empty);
        _contentServiceMock.Verify(
            x => x.GetItemsAsync<ContentAttachment>(It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task BuildReusableContentItemNameReference_WithRawGuid_ReturnsThatGuid_AndNeverQueries()
    {
        var guid = Guid.NewGuid();

        var result = await _mappingHelper.BuildReusableContentItemNameReferenceAsync<ContentAttachment>($"  {guid}  ", CancellationToken.None);

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Identifier, Is.EqualTo(guid));
        _contentServiceMock.Verify(
            x => x.GetItemsAsync<ContentAttachment>(It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task BuildReusableContentItemNameReference_WhenNoItemMatches_ReturnsEmpty_AndQueriesOnce()
    {
        _contentServiceMock
            .Setup(x => x.GetItemsAsync<ContentAttachment>(It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ContentAttachment>().AsReadOnly());

        var result = await _mappingHelper.BuildReusableContentItemNameReferenceAsync<ContentAttachment>("SomeName", CancellationToken.None);

        Assert.That(result, Is.Empty);
        _contentServiceMock.Verify(
            x => x.GetItemsAsync<ContentAttachment>(It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ---------------------------------------------------------------------
    // BuildReferencesCarAsync / BuildReferencesPageCarAsync
    // ---------------------------------------------------------------------

    [TestCase(null)]
    public async Task BuildReferencesCarAsync_WithNullIdentifiers_ReturnsEmpty_AndNeverQueries(string[]? identifiers)
    {
        var result = await _mappingHelper.BuildReferencesCarAsync<Person>(identifiers, p => p.MetadataTitle ?? string.Empty, CancellationToken.None);

        Assert.That(result, Is.Empty);
        _contentServiceMock.Verify(
            x => x.GetItemsAsync<Person>(It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task BuildReferencesCarAsync_WithOnlyWhitespaceIdentifiers_ReturnsEmpty_AndNeverQueries()
    {
        var result = await _mappingHelper.BuildReferencesCarAsync<Person>(WhitespaceIdentifiersWithNull, p => p.MetadataTitle ?? string.Empty, CancellationToken.None);

        Assert.That(result, Is.Empty);
        _contentServiceMock.Verify(
            x => x.GetItemsAsync<Person>(It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task BuildReferencesCarAsync_WhenNoItemsMatch_ReturnsEmpty_AndQueriesOnce()
    {
        _contentServiceMock
            .Setup(x => x.GetItemsAsync<Person>(It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Person>().AsReadOnly());

        var result = await _mappingHelper.BuildReferencesCarAsync<Person>(SampleIdentifiers, p => p.MetadataTitle ?? string.Empty, CancellationToken.None);

        Assert.That(result, Is.Empty);
        _contentServiceMock.Verify(
            x => x.GetItemsAsync<Person>(It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [TestCase(null)]
    public async Task BuildReferencesPageCarAsync_WithNullIdentifiers_ReturnsEmpty_AndNeverQueries(string[]? identifiers)
    {
        var result = await _mappingHelper.BuildReferencesPageCarAsync<Person>(identifiers, p => p.MetadataTitle ?? string.Empty, CancellationToken.None);

        Assert.That(result, Is.Empty);
        _contentServiceMock.Verify(
            x => x.GetItemsAsync<Person>(It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task BuildReferencesPageCarAsync_WithOnlyWhitespaceIdentifiers_ReturnsEmpty_AndNeverQueries()
    {
        var result = await _mappingHelper.BuildReferencesPageCarAsync<Person>(WhitespaceIdentifiers, p => p.MetadataTitle ?? string.Empty, CancellationToken.None);

        Assert.That(result, Is.Empty);
        _contentServiceMock.Verify(
            x => x.GetItemsAsync<Person>(It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}