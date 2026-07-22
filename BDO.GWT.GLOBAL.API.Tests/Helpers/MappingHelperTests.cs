using BDO.GWT.Core;
using BDO.GWT.GLOBAL.API.Helpers;
using CMS.ContentEngine;
using Kentico.Content.Web.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.API.Tests.Helpers;

public class MappingHelperTests
{
    private Mock<IContentService> _contentServiceMock = null!;
    private MappingHelper _helper = null!;

    [SetUp]
    public void Setup()
    {
        _contentServiceMock = new Mock<IContentService>();
        _helper = new MappingHelper(_contentServiceMock.Object);
    }

    [Test]
    public void Constructor_ImplementsIMappingHelper()
    {
        Assert.That(_helper, Is.InstanceOf<IMappingHelper>());
    }

    #region BuildContentAttachmentImageReferences

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public async Task BuildContentAttachmentImageReferences_WithNullOrWhitespace_ReturnsEmptyList(string? url)
    {
        var result = await _helper.BuildContentAttachmentImageReferences(url, CancellationToken.None);

        Assert.That(result, Is.Empty);
        _contentServiceMock.Verify(s => s.GetItemsAsync<ContentAttachment>(
            It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task BuildContentAttachmentImageReferences_WithGuidString_ReturnsSingleReference_WithoutQueryingProvider()
    {
        var guid = Guid.NewGuid();

        var result = await _helper.BuildContentAttachmentImageReferences(guid.ToString(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Identifier, Is.EqualTo(guid));
        });
        _contentServiceMock.Verify(s => s.GetItemsAsync<ContentAttachment>(
            It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task BuildContentAttachmentImageReferences_WithUrl_NoMatchingAttachment_ReturnsEmptyList()
    {
        _contentServiceMock
            .Setup(s => s.GetItemsAsync<ContentAttachment>(
                It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ContentAttachment>());

        var result = await _helper.BuildContentAttachmentImageReferences("/images/teaser.jpg", CancellationToken.None);

        Assert.That(result, Is.Empty);
    }

    #endregion

    #region BuildContentAttachmentPersonPageReferences

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public async Task BuildContentAttachmentPersonPageReferences_WithNullOrWhitespace_ReturnsEmptyList(string? pagePath)
    {
        var result = await _helper.BuildContentAttachmentPersonPageReferences(pagePath, "Global", "en-gb", CancellationToken.None);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task BuildContentAttachmentPersonPageReferences_WithGuidString_ReturnsSingleReference_WithoutQueryingProvider()
    {
        var guid = Guid.NewGuid();

        var result = await _helper.BuildContentAttachmentPersonPageReferences(guid.ToString(), "Global", "en-gb", CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Identifier, Is.EqualTo(guid));
        });
        _contentServiceMock.Verify(s => s.GetPagesAsync<Person>(
            It.IsAny<RetrievePagesParameters>(),
            It.IsAny<Action<RetrievePagesQueryParameters>>(),
            It.IsAny<RetrievalCacheSettings>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task BuildContentAttachmentPersonPageReferences_WithPath_NoMatchingPage_ReturnsEmptyList()
    {
        _contentServiceMock
            .Setup(s => s.GetPagesAsync<Person>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Person>());

        var result = await _helper.BuildContentAttachmentPersonPageReferences("/people/jane-doe", "Global", "en-gb", CancellationToken.None);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task BuildContentAttachmentPersonPageReferences_WithNullCulture_UsesDefaultLanguage()
    {
        RetrievePagesParameters? captured = null;
        _contentServiceMock
            .Setup(s => s.GetPagesAsync<Person>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .Callback<RetrievePagesParameters, Action<RetrievePagesQueryParameters>, RetrievalCacheSettings, CancellationToken>(
                (p, _, _, _) => captured = p)
            .ReturnsAsync(Enumerable.Empty<Person>());

        _ = await _helper.BuildContentAttachmentPersonPageReferences("/people/jane-doe", "Global", null, CancellationToken.None);

        Assert.That(captured, Is.Not.Null);
        Assert.That(captured!.LanguageName, Is.Not.Null.And.Not.Empty);
    }

    #endregion

    #region BuildReferencesAsync

    [TestCase(null)]
    public async Task BuildReferencesAsync_WithNullIdentifiers_ReturnsEmptyList(IEnumerable<string>? identifiers)
    {
        var result = await _helper.BuildReferencesAsync<Person>(identifiers, p => p.PersonFirstName ?? string.Empty, CancellationToken.None);

        Assert.That(result, Is.Empty);
        _contentServiceMock.Verify(s => s.GetItemsAsync<Person>(
            It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task BuildReferencesAsync_WithEmptyIdentifiers_ReturnsEmptyList()
    {
        var result = await _helper.BuildReferencesAsync<Person>(new List<string>(), p => p.PersonFirstName ?? string.Empty, CancellationToken.None);

        Assert.That(result, Is.Empty);
        _contentServiceMock.Verify(s => s.GetItemsAsync<Person>(
            It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task BuildReferencesAsync_WithOnlyWhitespaceIdentifiers_ReturnsEmptyList()
    {
        var result = await _helper.BuildReferencesAsync<Person>(
            new List<string> { "", "   ", null! }, p => p.PersonFirstName ?? string.Empty, CancellationToken.None);

        Assert.That(result, Is.Empty);
        _contentServiceMock.Verify(s => s.GetItemsAsync<Person>(
            It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task BuildReferencesAsync_WithNoMatchingItems_ReturnsEmptyList()
    {
        _contentServiceMock
            .Setup(s => s.GetItemsAsync<Person>(
                It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Person>());

        var result = await _helper.BuildReferencesAsync<Person>(
            new List<string> { "Jane" }, p => p.PersonFirstName ?? string.Empty, CancellationToken.None);

        Assert.That(result, Is.Empty);
    }

    #endregion

    #region BuildReferencesPageAsync

    [TestCase(null)]
    public async Task BuildReferencesPageAsync_WithNullIdentifiers_ReturnsEmptyList(IEnumerable<string>? identifiers)
    {
        var result = await _helper.BuildReferencesPageAsync<Person>(identifiers, p => p.MetadataTitle ?? string.Empty, CancellationToken.None);

        Assert.That(result, Is.Empty);
        _contentServiceMock.Verify(s => s.GetItemsAsync<Person>(
            It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task BuildReferencesPageAsync_WithEmptyIdentifiers_ReturnsEmptyList()
    {
        var result = await _helper.BuildReferencesPageAsync<Person>(new List<string>(), p => p.MetadataTitle ?? string.Empty, CancellationToken.None);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task BuildReferencesPageAsync_WithOnlyWhitespaceIdentifiers_ReturnsEmptyList()
    {
        var result = await _helper.BuildReferencesPageAsync<Person>(
            new List<string> { "", "   " }, p => p.MetadataTitle ?? string.Empty, CancellationToken.None);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task BuildReferencesPageAsync_WithNoMatchingItems_ReturnsEmptyList()
    {
        _contentServiceMock
            .Setup(s => s.GetItemsAsync<Person>(
                It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Person>());

        var result = await _helper.BuildReferencesPageAsync<Person>(
            new List<string> { "Some Title" }, p => p.MetadataTitle ?? string.Empty, CancellationToken.None);

        Assert.That(result, Is.Empty);
    }

    #endregion
}
